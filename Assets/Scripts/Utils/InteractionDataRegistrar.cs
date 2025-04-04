using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// ScriptableObject로 만든 상호작용 데이터를 런타임에 InteractionManager에 등록하는 유틸리티 클래스
/// </summary>
public class InteractionDataRegistrar : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static InteractionDataRegistrar instance;
    public static InteractionDataRegistrar Instance => instance;
    
    [Header("Nursing Actions")]
    [SerializeField] private List<NursingActionData> nursingActions = new List<NursingActionData>();
    
    [Header("Procedure Steps")]
    [SerializeField] private List<ProcedureStepData> procedureSteps = new List<ProcedureStepData>();
    
    
    
    [Header("Generic Interaction Data")]
    [SerializeField] private List<GenericInteractionData> genericInteractions = new List<GenericInteractionData>();
    
    // 등록된 모든 상호작용 데이터 캐시
    private Dictionary<string, GenericInteractionData> cachedInteractions = new Dictionary<string, GenericInteractionData>();
    
    private void Awake()
    {
        // 싱글톤 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // 상호작용 데이터 로드 및 등록
        LoadInteractionsFromResources();
        RegisterActions();
    }
    
    /// <summary>
    /// Resources 폴더에서 상호작용 데이터를 로드합니다.
    /// </summary>
    private void LoadInteractionsFromResources()
    {
        try
        {
            // 디렉토리에서 모든 상호작용 데이터 로드
            GenericInteractionData[] interactions = Resources.LoadAll<GenericInteractionData>("Interactions");
            
            if (interactions != null && interactions.Length > 0)
            {
                foreach (var data in interactions)
                {
                    if (data != null && !string.IsNullOrEmpty(data.interactionId))
                    {
                        // 캐시에 저장
                        cachedInteractions[data.interactionId] = data;
                        
                        // 인스펙터에도 추가 (에디터에서 볼 수 있도록)
                        if (!genericInteractions.Contains(data))
                        {
                            genericInteractions.Add(data);
                        }
                        
                        Debug.Log($"Loaded interaction data from Resources: {data.interactionName} (ID: {data.interactionId})");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading interactions from Resources: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 모든 액션과 상호작용을 매니저에 등록합니다.
    /// </summary>
    private void RegisterActions()
    {
        if (InteractionManager.Instance == null)
        {
            Debug.LogError("InteractionManager.Instance is null. Make sure it's initialized before this component.");
            return;
        }
        
        // 일반 상호작용 데이터 등록 (모든 상호작용을 범용화)
        foreach (var interaction in genericInteractions)
        {
            if (interaction == null) continue;
            
            // 상호작용 단계 변환 및 등록
            var steps = ConvertGenericSteps(interaction);
            InteractionManager.Instance.RegisterItemInteraction(
                interaction.interactionId,
                steps
            );
            
            // 캐시에 저장
            cachedInteractions[interaction.interactionId] = interaction;
            
            Debug.Log($"Registered generic interaction: {interaction.interactionName} (ID: {interaction.interactionId})");
        }
    }
    
    /// <summary>
    /// 고급 기능
    /// </summary>
    private void RegisterAdvancedInteraction(string interactionId)
    {
        // 상호작용 데이터가 있는지 확인
        GenericInteractionData interactionData = GetInteractionData(interactionId);
        
        if (interactionData == null)
        {
            // Resources에서 찾기
            interactionData = Resources.Load<GenericInteractionData>($"Interactions/{interactionId}");
            
            if (interactionData == null)
            {
                Debug.LogWarning($"상호작용 데이터 '{interactionId}'를 찾을 수 없습니다.");
                return;
            }
        }
        
        // 상호작용 등록
        var steps = ConvertGenericSteps(interactionData);
        InteractionManager.Instance.RegisterItemInteraction(interactionId, steps);
        
        // 캐시에 저장
        cachedInteractions[interactionId] = interactionData;
        
        Debug.Log($"고급 상호작용이 등록되었습니다: {interactionId}");
    }
    
    /// <summary>
    /// GenericInteractionData의 단계를 InteractionStep으로 변환합니다.
    /// </summary>
    public List<InteractionStep> ConvertGenericSteps(GenericInteractionData data)
    {
        List<InteractionStep> result = new List<InteractionStep>();
        
        if (data == null || data.steps == null)
            return result;
            
        foreach (var genericStep in data.steps)
        {
            InteractionStep step = new InteractionStep
            {
                interactionType = genericStep.interactionType,
                guideText = genericStep.guideText,
                
                // 드래그 관련 필드
                requiredDragAngle = genericStep.requiredDragAngle,
                dragAngleTolerance = genericStep.dragAngleTolerance,
                dragDistance = genericStep.dragDistance,
                
                // 클릭 관련 필드
                validClickArea = genericStep.validClickArea,
                
                // 퀴즈 관련 필드
                quizQuestion = genericStep.quizQuestion,
                quizOptions = genericStep.quizOptions,
                correctOptionIndex = genericStep.correctOptionIndex,
                
                // 튜토리얼 관련 필드
                tutorialArrowSprite = genericStep.tutorialArrowSprite,
                tutorialArrowPosition = genericStep.tutorialArrowPosition,
                tutorialArrowRotation = genericStep.tutorialArrowRotation,
                
                // 피드백 관련 필드
                successMessage = genericStep.successMessage,
                errorMessage = genericStep.errorMessage,
                
                // 초기 오브젝트 관련 필드
                createInitialObjects = genericStep.createInitialObjects,
                
                // 다중 단계 드래그 관련 필드
                useMultiStageDrag = genericStep.useMultiStageDrag,
                totalDragStages = genericStep.useMultiStageDrag ? genericStep.multiStageDragSteps.Count : 0,
                
                // 조건부 터치 관련 필드
                useConditionalTouch = genericStep.useConditionalTouch,
                
                // 오류 피드백 관련 필드
                showErrorBorderFlash = genericStep.showErrorBorderFlash,
                disableTouchDuration = genericStep.disableTouchDuration,
                errorEntryText = genericStep.errorEntryText,
                
                // 물 효과 관련 필드
                createWaterEffect = genericStep.createWaterEffect,
                waterEffectPosition = genericStep.waterEffectPosition,
                createWaterImageOnObject = genericStep.createWaterImageOnObject,
            };
            
            // 조건부 터치 태그 설정
            if (genericStep.useConditionalTouch && genericStep.touchOptions != null && genericStep.touchOptions.Count > 0)
            {
                List<string> allTags = new List<string>();
                List<string> correctTags = new List<string>();
                
                foreach (var option in genericStep.touchOptions)
                {
                    if (!string.IsNullOrEmpty(option.targetTag))
                    {
                        allTags.Add(option.targetTag);
                        
                        if (option.isCorrectOption)
                        {
                            correctTags.Add(option.targetTag);
                        }
                    }
                }
                
                step.validTouchTags = allTags.ToArray();
                step.correctTouchTags = correctTags.ToArray();
            }
            
            result.Add(step);
        }
        
        return result;
    }
    
    /// <summary>
    /// ID로 상호작용 데이터를 가져옵니다.
    /// </summary>
    public GenericInteractionData GetInteractionData(string interactionId)
    {
        if (string.IsNullOrEmpty(interactionId))
            return null;
            
        // 캐시에서 찾기
        if (cachedInteractions.TryGetValue(interactionId, out GenericInteractionData data))
            return data;
            
        // 캐시에 없으면 Resources에서 다시 로드 시도
        try
        {
            data = Resources.Load<GenericInteractionData>($"Interactions/{interactionId}");
            if (data != null)
            {
                cachedInteractions[interactionId] = data;
                return data;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading interaction data for ID '{interactionId}': {ex.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// 런타임에 새 상호작용 데이터를 추가합니다.
    /// </summary>
    public void AddInteractionData(GenericInteractionData data)
    {
        if (data == null || string.IsNullOrEmpty(data.interactionId))
            return;
            
        // 캐시와 리스트에 추가
        cachedInteractions[data.interactionId] = data;
        
        // 콜렉션에 없으면 추가
        if (!genericInteractions.Contains(data))
        {
            genericInteractions.Add(data);
        }
        
        // InteractionManager에 등록
        if (InteractionManager.Instance != null)
        {
            var steps = ConvertGenericSteps(data);
            InteractionManager.Instance.RegisterItemInteraction(
                data.interactionId,
                steps
            );
            Debug.Log($"Runtime added interaction data: {data.interactionName} (ID: {data.interactionId})");
        }
    }
    
    /// <summary>
    /// 에디터에서 버튼으로 등록 테스트를 위한 메소드
    /// </summary>
    [ContextMenu("Test Register Actions")]
    private void TestRegisterActions()
    {
        LoadInteractionsFromResources();
        RegisterActions();
    }
}
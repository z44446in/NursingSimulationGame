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
    
    [Header("Interaction Data")]
    [SerializeField] private List<InteractionData> interactionAssets = new List<InteractionData>();
    
    // 등록된 모든 상호작용 데이터 캐시
    private Dictionary<string, InteractionData> cachedInteractions = new Dictionary<string, InteractionData>();
    
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
            InteractionData[] interactions = Resources.LoadAll<InteractionData>("Interactions");
            
            if (interactions != null && interactions.Length > 0)
            {
                foreach (var data in interactions)
                {
                    if (data != null)
                    {
                        // ID 유효성 확인 및 이전 필드에서 값 복사
                        #if UNITY_EDITOR
                        if (string.IsNullOrEmpty(data.id) && !string.IsNullOrEmpty(GetOldFieldValue(data, "interactionId")))
                        {
                            data.id = GetOldFieldValue(data, "interactionId");
                            data.displayName = GetOldFieldValue(data, "interactionName");
                            UnityEditor.EditorUtility.SetDirty(data);
                            UnityEditor.AssetDatabase.SaveAssets();
                            Debug.Log($"Updated old field values for: {data.displayName} (ID: {data.id})");
                        }
                        #endif
                        
                        if (!string.IsNullOrEmpty(data.id))
                        {
                            // 캐시에 저장
                            cachedInteractions[data.id] = data;
                            
                            // 인스펙터에도 추가 (에디터에서 볼 수 있도록)
                            if (!interactionAssets.Contains(data))
                            {
                                interactionAssets.Add(data);
                            }
                            
                            Debug.Log($"Loaded interaction data from Resources: {data.displayName} (ID: {data.id})");
                        }
                        else
                        {
                            Debug.LogWarning($"Skipped interaction data with empty ID: {data.name}");
                        }
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
        
        // 모든 상호작용 데이터 등록
        foreach (var interaction in interactionAssets)
        {
            if (interaction == null) continue;
            
            // 상호작용 단계 변환 및 등록
            var steps = ConvertToInteractionSteps(interaction);
            InteractionManager.Instance.RegisterItemInteraction(
                interaction.id,
                steps
            );
            
            // 캐시에 저장
            cachedInteractions[interaction.id] = interaction;
            
            Debug.Log($"Registered interaction: {interaction.displayName} (ID: {interaction.id})");
        }
    }
    
    /// <summary>
    /// 고급 기능 - 특정 ID의 상호작용 데이터 등록
    /// </summary>
    private void RegisterAdvancedInteraction(string interactionId)
    {
        // 상호작용 데이터가 있는지 확인
        InteractionData interactionData = GetInteractionData(interactionId);
        
        if (interactionData == null)
        {
            // Resources에서 찾기
            interactionData = Resources.Load<InteractionData>($"Interactions/{interactionId}");
            
            if (interactionData == null)
            {
                Debug.LogWarning($"상호작용 데이터 '{interactionId}'를 찾을 수 없습니다.");
                return;
            }
        }
        
        // 상호작용 등록
        var steps = ConvertToInteractionSteps(interactionData);
        InteractionManager.Instance.RegisterItemInteraction(interactionId, steps);
        
        // 캐시에 저장
        cachedInteractions[interactionId] = interactionData;
        
        Debug.Log($"고급 상호작용이 등록되었습니다: {interactionId}");
    }
    
    /// <summary>
    /// InteractionData를 BaseInteractionSystem에서 사용하는 InteractionStep 목록으로 변환합니다.
    /// </summary>
    public List<InteractionStep> ConvertToInteractionSteps(InteractionData data)
    {
        List<InteractionStep> result = new List<InteractionStep>();
        
        if (data == null || data.steps == null)
            return result;
            
        foreach (var stepData in data.steps)
        {
            InteractionStep step = new InteractionStep
            {
                interactionType = stepData.interactionType,
                guideText = stepData.guideText,
                
                // 드래그 관련 필드
                requiredDragAngle = stepData.requiredDragAngle,
                dragAngleTolerance = stepData.dragAngleTolerance,
                dragDistance = stepData.dragDistance,
                
                // 클릭 관련 필드
                validClickArea = stepData.validClickArea,
                
                // 퀴즈 관련 필드
                quizQuestion = stepData.quizQuestion,
                quizOptions = stepData.quizOptions,
                correctOptionIndex = stepData.correctOptionIndex,
                
                // 튜토리얼 관련 필드
                tutorialArrowSprite = stepData.tutorialArrowSprite,
                tutorialArrowPosition = stepData.tutorialArrowPosition,
                tutorialArrowRotation = stepData.tutorialArrowRotation,
                
                // 피드백 관련 필드
                successMessage = stepData.successMessage,
                errorMessage = stepData.errorMessage,
                
                // 초기 오브젝트 관련 필드
                createInitialObjects = stepData.createInitialObjects,
                
                // 다중 단계 드래그 관련 필드
                useMultiStageDrag = stepData.useMultiStageDrag,
                totalDragStages = stepData.useMultiStageDrag ? stepData.multiStageDragSteps.Count : 0,
                
                // 조건부 터치 관련 필드
                useConditionalTouch = stepData.useConditionalTouch,
                
                // 오류 피드백 관련 필드
                showErrorBorderFlash = stepData.showErrorBorderFlash,
                disableTouchDuration = stepData.disableTouchDuration,
                
                // 물 효과 관련 필드
                createWaterEffect = stepData.createWaterEffect,
                waterEffectPosition = stepData.waterEffectPosition,
                createWaterImageOnObject = stepData.createWaterImageOnObject,
            };
            
            // 조건부 터치 태그 설정
            if (stepData.useConditionalTouch && stepData.touchOptions != null && stepData.touchOptions.Count > 0)
            {
                List<string> allTags = new List<string>();
                List<string> correctTags = new List<string>();
                
                foreach (var option in stepData.touchOptions)
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
    public InteractionData GetInteractionData(string interactionId)
    {
        if (string.IsNullOrEmpty(interactionId))
            return null;
            
        // 캐시에서 찾기
        if (cachedInteractions.TryGetValue(interactionId, out InteractionData data))
            return data;
            
        // 캐시에 없으면 Resources에서 다시 로드 시도
        try
        {
            data = Resources.Load<InteractionData>($"Interactions/{interactionId}");
            if (data != null)
            {
                // ID 유효성 확인 및 이전 필드에서 값 복사
                #if UNITY_EDITOR
                if (string.IsNullOrEmpty(data.id) && !string.IsNullOrEmpty(GetOldFieldValue(data, "interactionId")))
                {
                    data.id = GetOldFieldValue(data, "interactionId");
                    data.displayName = GetOldFieldValue(data, "interactionName");
                    UnityEditor.EditorUtility.SetDirty(data);
                    UnityEditor.AssetDatabase.SaveAssets();
                    Debug.Log($"Updated old field values for: {data.displayName} (ID: {data.id})");
                }
                #endif
                
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
    /// 직렬화된 필드 값을 가져오는 유틸리티 메서드
    /// </summary>
    private string GetOldFieldValue(InteractionData data, string fieldName)
    {
        #if UNITY_EDITOR
        if (data == null)
            return string.Empty;
            
        UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(data);
        UnityEditor.SerializedProperty property = serializedObject.FindProperty(fieldName);
        
        if (property != null && property.propertyType == UnityEditor.SerializedPropertyType.String)
        {
            return property.stringValue;
        }
        #endif
        
        return string.Empty;
    }
    
    /// <summary>
    /// 런타임에 새 상호작용 데이터를 추가합니다.
    /// </summary>
    public void AddInteractionData(InteractionData data)
    {
        if (data == null || string.IsNullOrEmpty(data.id))
            return;
            
        // 캐시와 리스트에 추가
        cachedInteractions[data.id] = data;
        
        // 콜렉션에 없으면 추가
        if (!interactionAssets.Contains(data))
        {
            interactionAssets.Add(data);
        }
        
        // InteractionManager에 등록
        if (InteractionManager.Instance != null)
        {
            var steps = ConvertToInteractionSteps(data);
            InteractionManager.Instance.RegisterItemInteraction(
                data.id,
                steps
            );
            Debug.Log($"Runtime added interaction data: {data.displayName} (ID: {data.id})");
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
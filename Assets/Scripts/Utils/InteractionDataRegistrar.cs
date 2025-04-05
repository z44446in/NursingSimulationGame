using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Need to access ScoringSystem for PenaltyType
using System;

/// <summary>
/// ScriptableObject로 만든 상호작용 데이터를 런타임에 InteractionManager에 등록하는 유틸리티 클래스
/// </summary>
public class InteractionDataRegistrar : MonoBehaviour
{
    [Header("Nursing Actions")]
    [SerializeField] private List<NursingActionData> nursingActions = new List<NursingActionData>();
    
    [Header("Procedure Steps")]
    [SerializeField] private List<ProcedureStepData> procedureSteps = new List<ProcedureStepData>();
    
    
    [Header("Generic Interaction Data")]
    [SerializeField] private List<GenericInteractionData> genericInteractions = new List<GenericInteractionData>();
    
    private void Awake()
    {
        RegisterActions();
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
        
        // 개별 간호 액션 등록
        foreach (var action in nursingActions)
        {
            if (action == null) continue;
            
            // 여기서 액션 등록 로직 구현
            // 예: InteractionManager.Instance.RegisterNursingAction(action);
            Debug.Log($"Registered nursing action: {action.actionName}");
        }
        
        // 시술 단계 등록
        foreach (var step in procedureSteps)
        {
            if (step == null) continue;
            
            // 여기서 단계 등록 로직 구현
            // 예: ProcedureManager.Instance.RegisterProcedureStep(step);
            Debug.Log($"Registered procedure step: {step.stepName}");
        }
        
        // 멸균증류수 상호작용 데이터 등록
        if (distilledWaterData != null)
        {
            InteractionManager.Instance.RegisterItemInteraction(
                distilledWaterData.itemId, 
                distilledWaterData.interactionSteps
            );
            Debug.Log($"Registered distilled water interaction data");
        }
        
        // 범용 상호작용 데이터 등록
        foreach (var interaction in genericInteractions)
        {
            if (interaction == null) continue;
            
            // GenericInteractionData를 InteractionStep 리스트로 변환
            List<InteractionStep> steps = ConvertGenericToInteractionSteps(interaction);
            
            // InteractionManager에 등록
            InteractionManager.Instance.RegisterItemInteraction(
                interaction.interactionId, 
                steps
            );
            Debug.Log($"Registered generic interaction: {interaction.interactionName} (ID: {interaction.interactionId})");
        }
    }
    
    /// <summary>
    /// GenericInteractionData를 InteractionStep 리스트로 변환
    /// </summary>
    private List<InteractionStep> ConvertGenericToInteractionSteps(GenericInteractionData data)
    {
        List<InteractionStep> result = new List<InteractionStep>();
        
        foreach (var genericStep in data.steps)
        {
            InteractionStep step = new InteractionStep
            {
                interactionType = genericStep.interactionType,
                guideText = genericStep.guideText,
                requiredDragAngle = genericStep.requiredDragAngle,
                dragAngleTolerance = genericStep.dragAngleTolerance,
                dragDistance = genericStep.dragDistance,
                validClickArea = genericStep.validClickArea,
                quizQuestion = genericStep.quizQuestion,
                quizOptions = genericStep.quizOptions,
                correctOptionIndex = genericStep.correctOptionIndex,
                tutorialArrowSprite = genericStep.tutorialArrowSprite,
                tutorialArrowPosition = genericStep.tutorialArrowPosition,
                tutorialArrowRotation = genericStep.tutorialArrowRotation,
                successMessage = genericStep.successMessage,
                errorMessage = genericStep.errorMessage,
                
                // 추가 설정
                createInitialObjects = genericStep.createInitialObjects,
                useMultiStageDrag = genericStep.useMultiStageDrag,
                totalDragStages = genericStep.useMultiStageDrag ? genericStep.multiStageDragSteps.Count : 0,
                useConditionalTouch = genericStep.useConditionalTouch,
                showErrorBorderFlash = genericStep.showErrorBorderFlash,
                disableTouchDuration = genericStep.disableTouchDuration,
                errorEntryText = genericStep.errorEntryText,
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
    /// 에디터에서 버튼으로 등록 테스트를 위한 메소드
    /// </summary>
    [ContextMenu("Test Register Actions")]
    private void TestRegisterActions()
    {
        RegisterActions();
    }
    
    /// <summary>
    /// 코드에서 직접 상호작용 데이터를 추가하기 위한 메소드
    /// </summary>
    public void AddGenericInteraction(GenericInteractionData interaction)
    {
        if (interaction == null) return;
        
        if (genericInteractions == null)
        {
            genericInteractions = new List<GenericInteractionData>();
        }
        
        // 이미 추가되어 있는지 확인
        if (!genericInteractions.Contains(interaction))
        {
            genericInteractions.Add(interaction);
            
            // InteractionManager가 초기화되어 있으면 바로 등록
            if (InteractionManager.Instance != null)
            {
                List<InteractionStep> steps = ConvertGenericToInteractionSteps(interaction);
                InteractionManager.Instance.RegisterItemInteraction(
                    interaction.interactionId, 
                    steps
                );
                Debug.Log($"Runtime added generic interaction: {interaction.interactionName} (ID: {interaction.interactionId})");
            }
        }
    }
}
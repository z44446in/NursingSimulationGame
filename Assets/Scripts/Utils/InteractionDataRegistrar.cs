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
    
    [Header("Item Interaction Data")]
    [SerializeField] private DistilledWaterInteractionData distilledWaterData;
    
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
    }
    
    /// <summary>
    /// 에디터에서 버튼으로 등록 테스트를 위한 메소드
    /// </summary>
    [ContextMenu("Test Register Actions")]
    private void TestRegisterActions()
    {
        RegisterActions();
    }
}
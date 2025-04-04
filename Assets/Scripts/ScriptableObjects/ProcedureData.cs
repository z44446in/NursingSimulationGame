using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 간호 시술 전체 데이터 - 시술 과정, 단계, 요구 조건 등을 정의
/// </summary>
[CreateAssetMenu(fileName = "New Procedure", menuName = "Nursing/Procedure Data")]
public class ProcedureData : BaseData, IRequiresItems
{
    [Header("시술 정보")]
    public NursingProcedureType procedureType;
    
    [Header("시술 단계")]
    public List<ProcedureStep> steps = new List<ProcedureStep>();
    
    [Header("필요 아이템")]
    public List<ItemRequirement> requiredItems = new List<ItemRequirement>();
    
    [Header("UI 설정")]
    public Sprite backgroundImage;
    public string procedureTitle;
    public Color titleColor = Color.white;
    
    [Header("평가 설정")]
    public int maxScore = 100;
    public int errorPenalty = 5;
    public bool trackErrors = true;
    
    /// <summary>
    /// 모든 필수 아이템 목록을 반환합니다
    /// </summary>
    public List<Item> GetRequiredItems(bool includeOptional = false)
    {
        List<Item> items = new List<Item>();
        
        // 직접 지정된 필수 아이템
        foreach (var req in requiredItems)
        {
            if (req.item != null && (includeOptional || !req.isOptional))
            {
                if (!items.Contains(req.item))
                {
                    items.Add(req.item);
                }
            }
        }
        
        // 각 단계에서 필요한 아이템 추가
        foreach (var step in steps)
        {
            List<Item> stepItems = step.GetRequiredItems(includeOptional);
            foreach (var item in stepItems)
            {
                if (!items.Contains(item))
                {
                    items.Add(item);
                }
            }
        }
        
        return items;
    }
    
    /// <summary>
    /// 시술의 모든 대화 단계를 반환합니다
    /// </summary>
    public List<ProcedureStep> GetDialogueSteps()
    {
        return steps.Where(s => s.stepType == StepType.Dialogue).ToList();
    }
    
    /// <summary>
    /// 시술의 모든 상호작용 단계를 반환합니다
    /// </summary>
    public List<ProcedureStep> GetInteractionSteps()
    {
        return steps.Where(s => s.stepType == StepType.Interaction).ToList();
    }
    
    /// <summary>
    /// 시술의 모든 준비 단계를 반환합니다
    /// </summary>
    public List<ProcedureStep> GetPreparationSteps()
    {
        return steps.Where(s => s.stepType == StepType.Preparation).ToList();
    }
}

/// <summary>
/// 간호 시술의 단계 정의
/// </summary>
[Serializable]
public class ProcedureStep : IRequiresItems
{
    public string stepId;
    public string stepName;
    [TextArea(2, 4)] public string description;
    
    [Header("단계 유형")]
    public StepType stepType;
    
    [Header("단계 설정")]
    public bool isRequired = true;
    public int scoreWeight = 10;
    public float timeLimit = 0f; // 0은 제한 없음
    
    [Header("대화 데이터 - 대화 단계용")]
    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();
    
    [Header("상호작용 데이터 - 상호작용 단계용")]
    public string interactionDataId; // 관련 InteractionData ID
    
    [Header("필요 아이템")]
    public List<ItemRequirement> requiredItems = new List<ItemRequirement>();
    
    [Header("UI 설정")]
    public Sprite backgroundImage;
    public Vector2 cameraPosition;
    public float cameraZoom = 1f;
    
    [Header("완료 조건")]
    public bool requireAllItems = true; // 모든 필수 아이템 필요
    public bool requireCorrectResponses = true; // 대화에서 올바른 응답 필요
    
    /// <summary>
    /// 이 단계에 필요한 모든 아이템을 반환합니다
    /// </summary>
    public List<Item> GetRequiredItems(bool includeOptional = false)
    {
        List<Item> items = new List<Item>();
        
        foreach (var req in requiredItems)
        {
            if (req.item != null && (includeOptional || !req.isOptional))
            {
                if (!items.Contains(req.item))
                {
                    items.Add(req.item);
                }
            }
        }
        
        return items;
    }
}
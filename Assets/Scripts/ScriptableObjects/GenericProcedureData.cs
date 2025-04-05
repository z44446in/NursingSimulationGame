using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범용 간호 절차 데이터를 정의하는 ScriptableObject
/// 여러 상호작용을 하나의 간호 절차로 묶을 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "ProcedureData", menuName = "Nursing/Generic/Procedure Data")]
public class GenericProcedureData : ScriptableObject
{
    [Header("기본 정보")]
    public string procedureId;
    public string procedureName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    
    [Header("절차 단계")]
    public List<GenericProcedureStep> steps = new List<GenericProcedureStep>();
    
    [Header("UI 및 배경")]
    public Sprite backgroundImage;
    public AudioClip backgroundMusic;
    public Color themeColor = Color.white;
    
    [Header("설정")]
    public bool isOrderImportant = true;
    public bool allowSkipNonRequiredSteps = true;
    public float timeLimit = 0f; // 0이면 시간 제한 없음
    public int maxScore = 100;
    
    [Header("평가 기준")]
    public int perfectScoreThreshold = 95;
    public int goodScoreThreshold = 80;
    public int passScoreThreshold = 60;
    
    /// <summary>
    /// 모든 필수 아이템 목록을 반환합니다.
    /// </summary>
    public List<Item> GetAllRequiredItems()
    {
        HashSet<Item> items = new HashSet<Item>();
        
        foreach (var step in steps)
        {
            if (step.requiredItems != null)
            {
                foreach (var item in step.requiredItems)
                {
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }
        }
        
        return new List<Item>(items);
    }
}

/// <summary>
/// 범용 간호 절차 단계를 정의하는 클래스
/// </summary>
[System.Serializable]
public class GenericProcedureStep
{
    [Header("기본 정보")]
    public string stepId;
    public string stepName;
    [TextArea(2, 4)]
    public string description;
    public Sprite stepIcon;
    
    [Header("단계 설정")]
    public bool isRequired = true;
    public int scoreWeight = 1;
    public List<Item> requiredItems = new List<Item>();
    public List<GenericInteractionData> interactions = new List<GenericInteractionData>();
    
    [Header("UI 및 가이드")]
    [TextArea(2, 4)]
    public string guideText;
    public Sprite backgroundOverlay;
    public AudioClip stepAudio;
    
    [Header("설정")]
    public bool waitForAllInteractions = true;
    public float autoAdvanceDelay = 1f;
    public string[] requiredCompletedStepIds;
    
    /// <summary>
    /// 현재 단계의 상호작용 중 필수 상호작용만 반환합니다.
    /// </summary>
    public List<GenericInteractionData> GetRequiredInteractions()
    {
        List<GenericInteractionData> requiredInteractions = new List<GenericInteractionData>();
        
        foreach (var interaction in interactions)
        {
            if (interaction != null)
            {
                requiredInteractions.Add(interaction);
            }
        }
        
        return requiredInteractions;
    }
}
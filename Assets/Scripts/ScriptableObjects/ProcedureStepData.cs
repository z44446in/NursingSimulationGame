using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간호 시술 단계를 정의하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "ProcedureStep", menuName = "Nursing/Procedure Step")]
public class ProcedureStepData : ScriptableObject
{
    [Header("Step Info")]
    public string stepId;
    public string stepName;
    [TextArea(3, 5)]
    public string description;
    
    [Header("Step Settings")]
    public bool isOrderImportant = true;
    public bool isRequired = true;
    public int scoreWeight = 1;
    
    [Header("Step Actions")]
    public List<NursingActionData> actions = new List<NursingActionData>();
    
    [Header("Step UI")]
    public Sprite stepIcon;
    [TextArea(2, 4)]
    public string guideText;
    
    [Header("Step Completion")]
    public bool waitForAllActions = true;
    public float autoAdvanceDelay = 1f;
    
    [Header("Step Background")]
    public Sprite backgroundImage;
    public AudioClip backgroundSound;
    
    /// <summary>
    /// 단계에 필요한 모든 아이템 목록을 반환합니다.
    /// </summary>
    public List<Item> GetAllRequiredItems()
    {
        HashSet<Item> items = new HashSet<Item>();
        
        foreach (var action in actions)
        {
            if (action == null)
                continue;
                
            foreach (var item in action.requiredItems)
            {
                if (item != null)
                {
                    items.Add(item);
                }
            }
        }
        
        return new List<Item>(items);
    }
    
    /// <summary>
    /// 단계에서 필수 행동들만 반환합니다.
    /// </summary>
    public List<NursingActionData> GetRequiredActions()
    {
        List<NursingActionData> requiredActions = new List<NursingActionData>();
        
        foreach (var action in actions)
        {
            if (action != null && action.isRequired)
            {
                requiredActions.Add(action);
            }
        }
        
        return requiredActions;
    }
    
    /// <summary>
    /// 다음 행동을 반환합니다. isOrderImportant가 true인 경우에만 의미가 있습니다.
    /// </summary>
    public NursingActionData GetNextAction(int currentIndex)
    {
        if (!isOrderImportant)
            return null;
            
        if (currentIndex < 0 || currentIndex >= actions.Count - 1)
            return null;
            
        return actions[currentIndex + 1];
    }
}
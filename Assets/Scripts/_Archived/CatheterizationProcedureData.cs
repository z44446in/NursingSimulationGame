using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 유치도뇨 시술에 대한 데이터를 정의하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "CatheterizationProcedureData", menuName = "Nursing/Catheterization Procedure")]
public class CatheterizationProcedureData : ScriptableObject
{
    [Header("Procedure Info")]
    public NursingProcedureType procedureType = NursingProcedureType.UrinaryCatheterization;
    public string procedureName = "유치도뇨(Catheterization)";
    [TextArea(3, 5)]
    public string description = "환자의 방광에 도뇨관을 삽입하여 소변을 배출하는 시술입니다.";
    
    [Header("Procedure Steps")]
    public List<ProcedureStepData> steps = new List<ProcedureStepData>();
    
    [Header("Procedure Settings")]
    public float timeLimit = 600f; // 10분 제한시간
    public int maxScore = 100;
    public float timeBonus = 0.1f; // 남은 시간 비율당 보너스 점수 계수
    public int maxTimeBonus = 20;  // 최대 시간 보너스
    
    [Header("UI Settings")]
    public Sprite procedureIcon;
    public Sprite procedureBanner;
    public Color procedureColor = new Color(0.2f, 0.6f, 1f);
    
    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    public AudioClip completionSound;
    
    /// <summary>
    /// 시술에 필요한 모든 아이템 목록을 반환합니다.
    /// </summary>
    public List<Item> GetAllRequiredItems()
    {
        HashSet<Item> items = new HashSet<Item>();
        
        foreach (var step in steps)
        {
            if (step == null)
                continue;
                
            foreach (var item in step.GetAllRequiredItems())
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
    /// 모든 필수 단계만 반환합니다.
    /// </summary>
    public List<ProcedureStepData> GetRequiredSteps()
    {
        List<ProcedureStepData> requiredSteps = new List<ProcedureStepData>();
        
        foreach (var step in steps)
        {
            if (step != null && step.isRequired)
            {
                requiredSteps.Add(step);
            }
        }
        
        return requiredSteps;
    }
    
    /// <summary>
    /// 특정 단계가 시술에 포함되어 있는지 확인합니다.
    /// </summary>
    public bool ContainsStep(ProcedureStepData step)
    {
        return steps.Contains(step);
    }
    
    /// <summary>
    /// 특정 단계의 인덱스를 반환합니다.
    /// </summary>
    public int GetStepIndex(ProcedureStepData step)
    {
        return steps.IndexOf(step);
    }
    
    /// <summary>
    /// 다음 단계를 반환합니다.
    /// </summary>
    public ProcedureStepData GetNextStep(ProcedureStepData currentStep)
    {
        int currentIndex = steps.IndexOf(currentStep);
        
        if (currentIndex < 0 || currentIndex >= steps.Count - 1)
            return null;
            
        return steps[currentIndex + 1];
    }
    
    /// <summary>
    /// 이전 단계를 반환합니다.
    /// </summary>
    public ProcedureStepData GetPreviousStep(ProcedureStepData currentStep)
    {
        int currentIndex = steps.IndexOf(currentStep);
        
        if (currentIndex <= 0)
            return null;
            
        return steps[currentIndex - 1];
    }
}
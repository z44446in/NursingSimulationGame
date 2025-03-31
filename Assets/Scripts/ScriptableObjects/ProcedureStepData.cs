using System.Collections;
using System.Collections.Generic;
// ProcedureStepData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "ProcedureStep", menuName = "Nursing/Procedure Step")]
public class ProcedureStepData : ScriptableObject
{
    public NursingActionData[] actions;       // 단계별 행동들
    public bool requiresSequentialOrder;       // 순서 중요 여부
    public string stepName;                    // 단계 이름
    public string description;                 // 단계 설명
}
using System.Collections;
using System.Collections.Generic;
// CatheterizationProcedureData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "CatheterizationProcedure", menuName = "Nursing/Catheterization")]
public class CatheterizationProcedureData : ScriptableObject
{
    public ProcedureStepData[] steps;         // 전체 단계
    public float timeLimit;                    // 제한 시간
    public int maxScore;                      // 최대 점수
    public float bonusTimeThreshold;          // 보너스 점수 시간 기준
    public int bonusScore;                    // 보너스 점수
}

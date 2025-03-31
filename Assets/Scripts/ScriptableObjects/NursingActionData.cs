using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// NursingActionData.cs

[CreateAssetMenu(fileName = "NursingAction", menuName = "Nursing/Action Data")]
public class NursingActionData : ScriptableObject
{
    public string actionId;           // 행동 고유 ID
    public string actionName;         // 행동 이름
    public string description;        // 행동 설명
    public bool isEssential;          // 필수 단계 여부
    public int score;                 // 기본 점수
    public string[] requiredItems;    // 필요한 준비물 목록
    
    [TextArea(3, 10)]
    public string practiceHint;       // 연습 모드 힌트
    
    [TextArea(3, 10)]
    public string feedbackMessage;    // 피드백 메시지
}


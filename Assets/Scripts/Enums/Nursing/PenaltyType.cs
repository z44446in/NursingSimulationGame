using System;
using UnityEngine;


namespace Nursing.Penalty
{
    public delegate void UndoActionDelegate();

    [Serializable]
    public enum PenaltyType
    {
        [Tooltip("경미한 오류 - 작은 감점")]
        Minor,
        [Tooltip("중요한 오류 - 중간 감점")]
        Major,
        [Tooltip("치명적 오류 - 큰 감점")]
        Critical
    }


    [Serializable]
    public class PenaltyData
    {
        [Header("패널티 기본 정보")]
        [Tooltip("패널티의 심각도 유형")]
        public PenaltyType penaltyType;

        [Space(5)]
        [Tooltip("이 패널티로 감점될 점수")]
        public int penaltyScore;

        [Header("패널티 메시지")]
        [Space(5)]
        [Tooltip("패널티 메시지를 말하는 화자")]
        public Nursing.Managers.DialogueManager.Speaker speaker = Nursing.Managers.DialogueManager.Speaker.Character;

        [Space(5)]
        [Tooltip("사용자에게 표시할 패널티 메시지")]
        public string penaltyMessage;

        [Space(5)]
        [Tooltip("데이터베이스에 기록할 패널티 메시지")]
        public string databaseMessage;

        [Header("패널티 시각 효과")]
        [Space(5)]
        [Tooltip("패널티 발생 시 화면 가장자리를 빨간색으로 깜빡일지 여부")]
        public bool flashRedScreen = true;

        [Space(5)]
        [Tooltip("화면을 깜빡일 횟수")]
        public int flashCount = 2;

        [NonSerialized]
        public UndoActionDelegate undoAction;
    }
}
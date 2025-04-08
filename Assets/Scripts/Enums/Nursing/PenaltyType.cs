using System;
using UnityEngine;

namespace Nursing.Penalty
{
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
        public PenaltyType penaltyType;
        public int penaltyScore;
        
        [Header("패널티 메시지")]
        public string speaker;
        public string penaltyMessage;
        public string databaseMessage;
        
        [Header("패널티 시각 효과")]
        public bool flashRedScreen = true;
        public int flashCount = 2;
    }
}
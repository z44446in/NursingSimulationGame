using System;
using UnityEngine;

namespace Nursing.Penalty
{
    /// <summary>
    /// 패널티 데이터 클래스
    /// 패널티 정보, 메시지, 시각적 효과 정의
    /// </summary>
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
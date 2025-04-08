using System;
using System.Collections.Generic;
using UnityEngine;
using Nursing.Penalty;

namespace Nursing.Managers
{
    public class PenaltyDatabase : MonoBehaviour
    {
        [Serializable]
        public class PenaltyRecord
        {
            public PenaltyType penaltyType;
            public string message;
            public string timestamp;
            public int penaltyScore;
        }
        
        [Header("패널티 기록")]
        [SerializeField] private List<PenaltyRecord> penaltyRecords = new List<PenaltyRecord>();
        
        [Header("총 패널티 점수")]
        [SerializeField] private int totalPenaltyScore = 0;
        
        /// <summary>
        /// 패널티를 데이터베이스에 기록합니다.
        /// </summary>
        /// <param name="penaltyData">기록할 패널티 데이터</param>
        public void RecordPenalty(PenaltyData penaltyData)
        {
            if (penaltyData == null || string.IsNullOrEmpty(penaltyData.databaseMessage))
                return;
            
            PenaltyRecord record = new PenaltyRecord
            {
                penaltyType = penaltyData.penaltyType,
                message = penaltyData.databaseMessage,
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                penaltyScore = penaltyData.penaltyScore
            };
            
            penaltyRecords.Add(record);
            totalPenaltyScore += penaltyData.penaltyScore;
            
            Debug.Log($"[Penalty Recorded] {record.message} (Score: {record.penaltyScore})");
        }
        
        /// <summary>
        /// 저장된 모든 패널티 기록을 가져옵니다.
        /// </summary>
        public List<PenaltyRecord> GetAllPenaltyRecords()
        {
            return penaltyRecords;
        }
        
        /// <summary>
        /// 특정 타입의 패널티 기록을 가져옵니다.
        /// </summary>
        public List<PenaltyRecord> GetPenaltyRecordsByType(PenaltyType type)
        {
            return penaltyRecords.FindAll(record => record.penaltyType == type);
        }
        
        /// <summary>
        /// 총 패널티 점수를 가져옵니다.
        /// </summary>
        public int GetTotalPenaltyScore()
        {
            return totalPenaltyScore;
        }
        
        /// <summary>
        /// 모든 패널티 기록을 초기화합니다.
        /// </summary>
        public void ClearAllPenaltyRecords()
        {
            penaltyRecords.Clear();
            totalPenaltyScore = 0;
            Debug.Log("All penalty records cleared.");
        }
    }
}
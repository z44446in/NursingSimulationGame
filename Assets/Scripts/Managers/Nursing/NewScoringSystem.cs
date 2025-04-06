using System;
using System.Collections.Generic;
using UnityEngine;
using Nursing.Penalty;
using Nursing.Managers;

namespace Nursing.Scoring
{
    public class NewScoringSystem : MonoBehaviour
    {
        [Header("점수 설정")]
        [SerializeField] private int maximumScore = 100;
        [SerializeField] private int passingScore = 70;
        
        [Header("패널티 가중치")]
        [SerializeField] private int minorPenaltyWeight = 1;
        [SerializeField] private int majorPenaltyWeight = 5;
        [SerializeField] private int criticalPenaltyWeight = 10;
        
        [Header("점수 상황")]
        [SerializeField] private int currentScore;
        [SerializeField] private int totalPenaltyPoints;
        
        private PenaltyManager penaltyManager;
        private PenaltyDatabase penaltyDatabase;
        
        // 평가 카테고리별 점수
        private Dictionary<string, int> categoryScores = new Dictionary<string, int>();
        
        private void Awake()
        {
            penaltyManager = FindObjectOfType<PenaltyManager>();
            penaltyDatabase = FindObjectOfType<PenaltyDatabase>();
            
            ResetNewScore();
        }
        
        /// <summary>
        /// 점수를 초기화합니다.
        /// </summary>
        public void ResetNewScore()
        {
            currentScore = maximumScore;
            totalPenaltyPoints = 0;
            categoryScores.Clear();
            
            if (penaltyDatabase != null)
            {
                penaltyDatabase.ClearAllPenaltyRecords();
            }
        }
        
        /// <summary>
        /// 패널티를 적용하고 점수를 감점합니다.
        /// </summary>
        /// <param name="penaltyData">패널티 데이터</param>
        /// <param name="category">평가 카테고리 (선택사항)</param>
        /// <returns>패널티가 성공적으로 적용되었는지 여부</returns>
        public bool ApplyPenalty(PenaltyData penaltyData, string category = "")
        {
            if (penaltyData == null)
                return false;
            
            // 패널티 점수 계산
            int penaltyPoints = CalculatePenaltyPoints(penaltyData);
            
            // 점수 감점
            DeductPoints(penaltyPoints, category);
            
            // 패널티 적용
            if (penaltyManager != null)
            {
                return penaltyManager.ApplyPenalty(penaltyData);
            }
            
            return true;
        }
        
        /// <summary>
        /// 패널티 데이터를 기반으로 패널티 점수를 계산합니다.
        /// </summary>
        private int CalculatePenaltyPoints(PenaltyData penaltyData)
        {
            // 패널티 데이터에 명시적 점수가 있으면 해당 점수 사용
            if (penaltyData.penaltyScore > 0)
            {
                return penaltyData.penaltyScore;
            }
            
            // 패널티 타입에 따른 가중치 사용
            switch (penaltyData.penaltyType)
            {
                case PenaltyType.Minor:
                    return minorPenaltyWeight;
                case PenaltyType.Major:
                    return majorPenaltyWeight;
                case PenaltyType.Critical:
                    return criticalPenaltyWeight;
                default:
                    return 0;
            }
        }
        
        /// <summary>
        /// 점수를 감점합니다.
        /// </summary>
        private void DeductPoints(int points, string category)
        {
            if (points <= 0)
                return;
            
            // 총 패널티 점수 추가
            totalPenaltyPoints += points;
            
            // 카테고리별 패널티 점수 추가 (지정된 경우)
            if (!string.IsNullOrEmpty(category))
            {
                if (!categoryScores.ContainsKey(category))
                {
                    categoryScores[category] = 0;
                }
                
                categoryScores[category] += points;
            }
            
            // 현재 점수 감점 (최소 0점)
            currentScore = Mathf.Max(0, currentScore - points);
            
            Debug.Log($"[Score] {points} points deducted. Current score: {currentScore}/{maximumScore}");
        }
        
        /// <summary>
        /// 현재 점수를 가져옵니다.
        /// </summary>
        public int GetNewScore()
        {
            return currentScore;
        }
        
        /// <summary>
        /// 최대 가능 점수를 가져옵니다.
        /// </summary>
        public int GetMaximumScore()
        {
            return maximumScore;
        }
        
        /// <summary>
        /// 통과 점수를 가져옵니다.
        /// </summary>
        public int GetPassingScore()
        {
            return passingScore;
        }
        
        /// <summary>
        /// 총 패널티 점수를 가져옵니다.
        /// </summary>
        public int GetTotalPenaltyPoints()
        {
            return totalPenaltyPoints;
        }
        
        /// <summary>
        /// 특정 카테고리의 패널티 점수를 가져옵니다.
        /// </summary>
        public int GetCategoryPenaltyPoints(string category)
        {
            if (string.IsNullOrEmpty(category) || !categoryScores.ContainsKey(category))
            {
                return 0;
            }
            
            return categoryScores[category];
        }
        
        /// <summary>
        /// 합격 여부를 확인합니다.
        /// </summary>
        public bool IsPassing()
        {
            return currentScore >= passingScore;
        }
        
        /// <summary>
        /// 점수 등급을 가져옵니다 (A, B, C, D, F).
        /// </summary>
        public string GetGrade()
        {
            if (currentScore >= 90) return "A";
            if (currentScore >= 80) return "B";
            if (currentScore >= 70) return "C";
            if (currentScore >= 60) return "D";
            return "F";
        }
    }
}
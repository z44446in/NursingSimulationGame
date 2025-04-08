using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 점수 시스템 클래스
/// 간호 시뮬레이션의 점수 계산, 관리, 감점 등을 담당합니다.
/// </summary>
public class ScoringSystem : MonoBehaviour
{
    private static ScoringSystem instance;
    public static ScoringSystem Instance => instance;

    [Header("Score Settings")]
    [SerializeField] private int initialScore = 100;
    [SerializeField] private int minScore = 0;
    [SerializeField] private int maxScore = 100;

    [Header("Penalty Settings")]
    [SerializeField] private int minorPenalty = 5;  // 경미한 오류
    [SerializeField] private int majorPenalty = 10; // 중요한 오류
    [SerializeField] private int criticalPenalty = 20; // 치명적 오류

    [Header("Time Bonus Settings")]
    [SerializeField] private bool useTimeBonus = true;
    [SerializeField] private int maxTimeBonus = 20;
    [SerializeField] private float timeBonusFactor = 0.1f; // 남은 시간 비율당 보너스 점수

    // 현재 점수
    private int currentScore;
    
    // 시간 정보
    private float totalTimeLimit;
    private float remainingTime;
    
    // 시술 추적
    private NursingProcedureType currentProcedure;
    
    // 오류 로그
    private List<PenaltyRecord> penaltyRecords = new List<PenaltyRecord>();
    
    // 이벤트
    public event Action<int> OnScoreChanged;
    public event Action<PenaltyRecord> OnPenaltyApplied;
    public event Action<int> OnBonusApplied;
    public event Action<int> OnFinalScoreCalculated;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 새 시술을 시작합니다.
    /// </summary>
    public void StartNewProcedure(NursingProcedureType procedureType, float timeLimit)
    {
        currentProcedure = procedureType;
        totalTimeLimit = timeLimit;
        remainingTime = timeLimit;
        
        ResetScore();
        ClearPenaltyRecords();
    }

    /// <summary>
    /// 점수를 초기화합니다.
    /// </summary>
    public void ResetScore()
    {
        currentScore = initialScore;
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// 오류 기록을 초기화합니다.
    /// </summary>
    public void ClearPenaltyRecords()
    {
        penaltyRecords.Clear();
    }

    /// <summary>
    /// 오류에 따른 감점을 적용합니다.
    /// </summary>
    public void ApplyPenalty(string errorMessage, PenaltyType penaltyType, string itemName = "", string stepName = "")
    {
        int penaltyPoints = GetPenaltyPoints(penaltyType);
        
        // 점수 감점
        currentScore = Mathf.Max(minScore, currentScore - penaltyPoints);
        
        // 오류 기록
        PenaltyRecord record = new PenaltyRecord
        {
            timestamp = DateTime.Now,
            procedureType = currentProcedure,
            penaltyType = penaltyType,
            penaltyPoints = penaltyPoints,
            errorMessage = errorMessage,
            itemName = itemName,
            stepName = stepName
        };
        
        penaltyRecords.Add(record);
        
        // 이벤트 발생
        OnPenaltyApplied?.Invoke(record);
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// 남은 시간을 업데이트합니다.
    /// </summary>
    public void UpdateRemainingTime(float newRemainingTime)
    {
        remainingTime = Mathf.Max(0, newRemainingTime);
    }

    /// <summary>
    /// 최종 점수를 계산합니다.
    /// </summary>
    public int CalculateFinalScore()
    {
        int finalScore = currentScore;
        
        // 시간 보너스 계산
        if (useTimeBonus && totalTimeLimit > 0)
        {
            float timeRatio = remainingTime / totalTimeLimit;
            int timeBonus = Mathf.RoundToInt(maxTimeBonus * timeRatio * timeBonusFactor);
            
            // 최대 점수 한도 확인
            finalScore = Mathf.Min(maxScore, finalScore + timeBonus);
            
            // 시간 보너스 이벤트
            OnBonusApplied?.Invoke(timeBonus);
        }
        
        // 최종 점수 이벤트
        OnFinalScoreCalculated?.Invoke(finalScore);
        
        return finalScore;
    }

    /// <summary>
    /// 현재 점수를 반환합니다.
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    /// <summary>
    /// 오류 기록 목록을 반환합니다.
    /// </summary>
    public List<PenaltyRecord> GetPenaltyRecords()
    {
        return new List<PenaltyRecord>(penaltyRecords);
    }

    /// <summary>
    /// 페널티 유형에 따른 감점 점수를 반환합니다.
    /// </summary>
    private int GetPenaltyPoints(PenaltyType penaltyType)
    {
        switch (penaltyType)
        {
            case PenaltyType.Minor:
                return minorPenalty;
            case PenaltyType.Major:
                return majorPenalty;
            case PenaltyType.Critical:
                return criticalPenalty;
            default:
                return minorPenalty;
        }
    }
}

/// <summary>
/// 페널티 유형 열거형
/// </summary>
public enum PenaltyType
{
    Minor,    // 경미한 오류 (5점)
    Major,    // 중요한 오류 (10점)
    Critical  // 치명적 오류 (20점)
}

/// <summary>
/// 페널티 기록 클래스
/// </summary>
[System.Serializable]
public class PenaltyRecord
{
    public DateTime timestamp;
    public NursingProcedureType procedureType;
    public PenaltyType penaltyType;
    public int penaltyPoints;
    public string errorMessage;
    public string itemName;
    public string stepName;
}
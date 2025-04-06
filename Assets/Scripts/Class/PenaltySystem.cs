using System;
using System.Collections.Generic;
using UnityEngine;
using Nursing.Penalty;

/// <summary>
/// 페널티 시스템 클래스
/// 간호 시뮬레이션의 페널티 관리, 메시지 표시, 데이터베이스 기록을 담당합니다.
/// </summary>
public class PenaltySystem : MonoBehaviour
{
    private static PenaltySystem instance;
    public static PenaltySystem Instance => instance;

    [Header("페널티 설정")]
    [SerializeField] private int minorPenaltyScore = 5;  // 경미한 오류
    [SerializeField] private int majorPenaltyScore = 10; // 중요한 오류
    [SerializeField] private int criticalPenaltyScore = 20; // 치명적 오류

    [Header("UI 참조")]
    [SerializeField] private GameObject errorBorderFlash; // 화면 가장자리 빨간색 깜빡임 효과
    [SerializeField] private float flashDuration = 0.3f; // 깜빡임 지속 시간

    // 페널티 이벤트
    public event Action<NewPenaltyRecord> OnPenaltyApplied;
    public event Action<int> OnScoreDeducted;

    // 페널티 데이터베이스
    private List<NewPenaltyRecord> penaltyDatabase = new List<NewPenaltyRecord>();

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
    /// 페널티를 적용하고 메시지 표시, 데이터베이스 기록을 수행
    /// </summary>
    public void ApplyPenalty(PenaltyData penaltyData)
    {
        if (penaltyData == null)
            return;

        // 페널티 점수 계산
        int penaltyScore = GetPenaltyScore(penaltyData.penaltyType);

        // 점수 감점 이벤트 발생
        OnScoreDeducted?.Invoke(penaltyScore);

        // 메시지가 있을 경우 데이터베이스에 기록
        if (!string.IsNullOrEmpty(penaltyData.databaseMessage))
        {
            NewPenaltyRecord record = new NewPenaltyRecord
            {
                timestamp = DateTime.Now,
                penaltyType = penaltyData.penaltyType,
                penaltyScore = penaltyScore,
                message = penaltyData.databaseMessage
            };

            penaltyDatabase.Add(record);
            OnPenaltyApplied?.Invoke(record);
        }

        // 메시지 표시
        ShowPenaltyMessage(penaltyData);

        // 화면 효과 표시
        FlashErrorBorder();
    }

    /// <summary>
    /// 페널티 유형에 따른 점수 반환
    /// </summary>
    private int GetPenaltyScore(PenaltyType penaltyType)
    {
        switch (penaltyType)
        {
            case PenaltyType.Minor:
                return minorPenaltyScore;
            case PenaltyType.Major:
                return majorPenaltyScore;
            case PenaltyType.Critical:
                return criticalPenaltyScore;
            default:
                return minorPenaltyScore;
        }
    }

    /// <summary>
    /// 화면 테두리 오류 표시 효과
    /// </summary>
    private void FlashErrorBorder()
    {
        if (errorBorderFlash == null)
            return;

        // 코루틴으로 깜빡임 효과 구현
        StartCoroutine(FlashCoroutine());
    }

    /// <summary>
    /// 깜빡임 코루틴
    /// </summary>
    private System.Collections.IEnumerator FlashCoroutine()
    {
        // 시작
        errorBorderFlash.SetActive(true);
        
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(flashDuration);
        
        // 꺼짐
        errorBorderFlash.SetActive(false);
        
        // 잠시 대기
        yield return new WaitForSeconds(flashDuration);
        
        // 다시 켜짐
        errorBorderFlash.SetActive(true);
        
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(flashDuration);
        
        // 완전 꺼짐
        errorBorderFlash.SetActive(false);
    }

    /// <summary>
    /// 페널티 메시지 표시
    /// </summary>
    private void ShowPenaltyMessage(PenaltyData penaltyData)
    {
        if (string.IsNullOrEmpty(penaltyData.penaltyMessage))
            return;

        // DialogueManager가 있으면 대화창으로 표시
        var dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.ShowMessage("간호사", penaltyData.penaltyMessage);
        }
        else
        {
            // 없으면 디버그 메시지로만 표시
            Debug.Log($"Penalty: {penaltyData.penaltyMessage}");
        }
    }

    /// <summary>
    /// 페널티 기록 목록 반환
    /// </summary>
    public List<NewPenaltyRecord> GetPenaltyRecords()
    {
        return new List<NewPenaltyRecord>(penaltyDatabase);
    }

    /// <summary>
    /// 모든 페널티 기록 지우기
    /// </summary>
    public void ClearPenaltyRecords()
    {
        penaltyDatabase.Clear();
    }
}

/// <summary>
/// 페널티 기록 클래스
/// </summary>
[System.Serializable]
public class NewPenaltyRecord
{
    public DateTime timestamp;
    public PenaltyType penaltyType;
    public int penaltyScore;
    public string message;
    public string context; // 추가 맥락 정보 (예: 어떤 아이템, 환자 상태 등)
}
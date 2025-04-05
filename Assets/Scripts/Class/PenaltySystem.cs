using System;
using System.Collections.Generic;
using UnityEngine;

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
    public event Action<PenaltyRecord> OnPenaltyApplied;
    public event Action<int> OnScoreDeducted;

    // 페널티 데이터베이스
    private List<PenaltyRecord> penaltyDatabase = new List<PenaltyRecord>();

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

        // 에러 보더 초기화
        if (errorBorderFlash != null)
        {
            errorBorderFlash.SetActive(false);
        }
    }

    /// <summary>
    /// 페널티를 적용합니다.
    /// </summary>
    /// <param name="penaltyData">페널티 데이터</param>
    public void ApplyPenalty(PenaltyData penaltyData)
    {
        if (penaltyData == null) return;

        // 점수 계산
        int penaltyScore = GetPenaltyScore(penaltyData.penaltyType);

        // 페널티 레코드 생성
        PenaltyRecord record = new PenaltyRecord
        {
            timestamp = DateTime.Now,
            penaltyType = penaltyData.penaltyType,
            penaltyPoints = penaltyScore,
            penaltyMessage = penaltyData.penaltyMessage,
            databaseMessage = penaltyData.databaseMessage,
            speaker = penaltyData.speaker
        };

        // 데이터베이스 메시지가 있는 경우에만 레코드에 기록
        if (!string.IsNullOrEmpty(penaltyData.databaseMessage))
        {
            // 데이터베이스에 기록
            penaltyDatabase.Add(record);
        }

        // 이벤트 발생
        OnPenaltyApplied?.Invoke(record);
        OnScoreDeducted?.Invoke(penaltyScore);

        // 오류 메시지 표시
        ShowPenaltyMessage(penaltyData);

        // 에러 보더 깜빡임 효과
        FlashErrorBorder();
    }

    /// <summary>
    /// 페널티 유형에 따라 감점 점수를 반환합니다.
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
    /// 페널티 메시지를 표시합니다.
    /// </summary>
    private void ShowPenaltyMessage(PenaltyData penaltyData)
    {
        if (string.IsNullOrEmpty(penaltyData.penaltyMessage)) return;

        // DialogueManager를 사용하여 작은 대화창 표시
        if (DialogueManager.Instance != null)
        {
            string speakerName = GetSpeakerName(penaltyData.speaker);
            DialogueManager.Instance.ShowSmallDialogue(speakerName, penaltyData.penaltyMessage);
        }
    }

    /// <summary>
    /// 화자 이름을 가져옵니다.
    /// </summary>
    private string GetSpeakerName(DialogueManager.Speaker speaker)
    {
        if (DialogueManager.Instance != null)
        {
            return DialogueManager.Instance.GetSpeakerName(speaker);
        }
        
        return "간호사";
    }

    /// <summary>
    /// 화면 가장자리 에러 표시를 깜빡입니다.
    /// </summary>
    private void FlashErrorBorder()
    {
        if (errorBorderFlash == null) return;

        // 코루틴으로 깜빡임 효과 구현
        StartCoroutine(FlashErrorBorderCoroutine());
    }

    /// <summary>
    /// 에러 보더 깜빡임 코루틴
    /// </summary>
    private System.Collections.IEnumerator FlashErrorBorderCoroutine()
    {
        // 첫 번째 깜빡임
        errorBorderFlash.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        errorBorderFlash.SetActive(false);
        yield return new WaitForSeconds(flashDuration);

        // 두 번째 깜빡임
        errorBorderFlash.SetActive(true);
        yield return new WaitForSeconds(flashDuration);
        errorBorderFlash.SetActive(false);
    }

    /// <summary>
    /// 페널티 데이터베이스 전체를 반환합니다.
    /// </summary>
    public List<PenaltyRecord> GetPenaltyDatabase()
    {
        return new List<PenaltyRecord>(penaltyDatabase);
    }

    /// <summary>
    /// 페널티 데이터베이스를 초기화합니다.
    /// </summary>
    public void ClearPenaltyDatabase()
    {
        penaltyDatabase.Clear();
    }
}

/// <summary>
/// 페널티 데이터 구조체 - 페널티 적용을 위한 정보를 담습니다.
/// </summary>
[System.Serializable]
public class PenaltyData
{
    public PenaltyType penaltyType = PenaltyType.Minor;
    public DialogueManager.Speaker speaker = DialogueManager.Speaker.Character;
    public string penaltyMessage = ""; // 사용자에게 표시할 메시지
    public string databaseMessage = ""; // 데이터베이스에 기록할 메시지
}

/// <summary>
/// 페널티 기록 - 적용된 페널티의 정보를 저장합니다.
/// </summary>
[System.Serializable]
public class PenaltyRecord
{
    public DateTime timestamp;
    public PenaltyType penaltyType;
    public int penaltyPoints;
    public string penaltyMessage;
    public string databaseMessage;
    public DialogueManager.Speaker speaker;

    // 옵션 정보
    public string itemName = "";
    public string stepName = "";
    public string procedureName = "";
}
using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// ProcedureManager: 시술 데이터를 관리하고 진행 상태를 제어하는 클래스.
/// </summary>
public class ProcedureManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static ProcedureManager instance;

    /// <summary>
    /// 전역 ProcedureManager 인스턴스에 접근.
    /// </summary>
    public static ProcedureManager Instance => instance;

    // 현재 진행 중인 시술 데이터
    private CatheterizationProcedureData currentProcedure;

    // 단계 및 액션 진행 상태
    private int currentStepIndex = -1;
    private int currentActionIndex = -1;

    // 점수 및 완료된 액션
    private int currentScore;
    private List<string> completedActions = new List<string>();

    // 시작 시간
    private float startTime;

    // 이벤트
    public event Action<int> OnScoreChanged;
    public event Action<int> OnStepChanged;
    public event Action<string> OnActionCompleted;
    public event Action OnProcedureCompleted;

    /// <summary>
    /// 싱글톤 초기화.
    /// </summary>
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
    /// 시술 데이터를 초기화하고 첫 번째 단계로 설정.
    /// </summary>
    public void InitializeProcedure(CatheterizationProcedureData procedureData)
    {
        if (procedureData == null)
        {
            Debug.LogError("Procedure data is null. Initialization failed.");
            return;
        }

        currentProcedure = procedureData;
        currentStepIndex = 0;
        currentActionIndex = 0;
        currentScore = procedureData.maxScore;
        completedActions.Clear();
        startTime = Time.time;

        // GameManager에 준비 상태 알림
        GameManager.Instance?.ChangeGameState(GameManager.GameState.READY);

        Debug.Log("Procedure initialized. Starting at step 0.");
    }

    /// <summary>
    /// 주어진 액션 ID를 수행하려고 시도.
    /// </summary>
    public bool TryPerformAction(string actionId)
    {
        if (currentProcedure == null || string.IsNullOrEmpty(actionId))
        {
            Debug.LogWarning("Invalid procedure or action ID.");
            return false;
        }

        var currentStep = currentProcedure.steps[currentStepIndex];
        var currentAction = currentStep.actions[currentActionIndex];

        // 현재 단계에서 수행 가능한 액션인지 확인
        if (currentAction.actionId != actionId && currentAction.isEssential)
        {
            ApplyPenalty("필수 단계를 건너뛸 수 없습니다.");
            return false;
        }

        // 액션 성공 처리
        completedActions.Add(actionId);
        OnActionCompleted?.Invoke(actionId);

        // 다음 단계로 진행
        MoveToNextAction();

        return true;
    }

    /// <summary>
    /// 다음 액션 또는 단계로 이동.
    /// </summary>
    private void MoveToNextAction()
    {
        currentActionIndex++;

        var currentStep = currentProcedure.steps[currentStepIndex];

        // 현재 단계의 모든 액션을 완료했는지 확인
        if (currentActionIndex >= currentStep.actions.Length)
        {
            currentStepIndex++;
            currentActionIndex = 0;

            // 모든 단계를 완료했는지 확인
            if (currentStepIndex >= currentProcedure.steps.Length)
            {
                CompleteProcedure();
                return;
            }

            OnStepChanged?.Invoke(currentStepIndex);
        }
    }

    /// <summary>
    /// 패널티를 적용하고 점수 감소.
    /// </summary>
    private void ApplyPenalty(string reason)
    {
        currentScore = Mathf.Max(0, currentScore - 5); // 최소 점수 0 유지
        OnScoreChanged?.Invoke(currentScore);

        Debug.LogWarning($"Penalty applied: {reason}");
    }

    /// <summary>
    /// 시술 완료 처리 및 보너스 점수 계산.
    /// </summary>
    private void CompleteProcedure()
    {
        float completionTime = Time.time - startTime;

        // 보너스 시간 이내 완료 시 보너스 점수 추가
        if (completionTime < currentProcedure.bonusTimeThreshold)
        {
            currentScore += currentProcedure.bonusScore;
        }

        OnProcedureCompleted?.Invoke();
        GameManager.Instance?.EndGame();

        Debug.Log("Procedure completed successfully.");
    }

    /// <summary>
    /// 현재 액션 데이터를 가져옴.
    /// </summary>
    public NursingActionData GetCurrentAction()
    {
        if (currentProcedure == null || currentStepIndex < 0 || currentActionIndex < 0)
        {
            return null;
        }

        return currentProcedure.steps[currentStepIndex].actions[currentActionIndex];
    }

    /// <summary>
    /// 경과 시간을 반환.
    /// </summary>
    public float GetElapsedTime()
    {
        return Time.time - startTime;
    }

    /// <summary>
    /// 현재 점수를 반환.
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void ApplyScorePenalty(int penaltyAmount)
    {
        currentScore = Mathf.Max(0, currentScore - penaltyAmount);
        OnScoreChanged?.Invoke(currentScore);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 간호 시술 진행을 관리하는 매니저 클래스
/// 단계 진행, 행동 유효성 검사, 점수 계산 등을 담당합니다.
/// </summary>
public class ProcedureManager : MonoBehaviour
{
    private static ProcedureManager instance;
    public static ProcedureManager Instance => instance;
    
   
    [Header("UI References")]
    [SerializeField] private Transform popupContainer;
    
    [Header("Audio")]
    [SerializeField] private AudioSource backgroundAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;
    
    // 현재 시술 관련 데이터
    private NursingProcedureType currentProcedureType;
    
    private ProcedureStepData currentStep;
    private NursingActionData currentAction;
    private int currentStepIndex = -1;
    private int currentActionIndex = -1;
    
    // 점수 및 시간 관련
    private float remainingTime;
    private int currentScore;
    private List<PenaltyRecord> penaltyRecords = new List<PenaltyRecord>();
    
    // 진행 상태
    private bool isProcedureActive = false;
    private bool isPaused = false;
    private Dictionary<string, bool> completedActions = new Dictionary<string, bool>();
    private Dictionary<string, bool> completedSteps = new Dictionary<string, bool>();
    
    // 이벤트
    public event Action<NursingProcedureType> OnProcedureStarted;
    public event Action<NursingProcedureType> OnProcedureCompleted;
    public event Action<ProcedureStepData, int> OnStepStarted;
    public event Action<ProcedureStepData, int> OnStepCompleted;
    public event Action<NursingActionData, int> OnActionStarted;
    public event Action<NursingActionData, int> OnActionCompleted;
    public event Action<string, PenaltyType, int> OnPenaltyApplied;
    public event Action<int> OnScoreChanged;
    public event Action<float> OnTimeUpdated;
    
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
    
    private void Start()
    {
        // 인스턴스 초기화 및 참조 설정
        if (ScoringSystem.Instance != null)
        {
            ScoringSystem.Instance.OnPenaltyApplied += HandlePenaltyApplied;
            ScoringSystem.Instance.OnScoreChanged += HandleScoreChanged;
        }
    }
    
    private void Update()
    {
        if (isProcedureActive && !isPaused)
        {
            // 시간 업데이트
            if (remainingTime > 0)
            {
                remainingTime -= Time.deltaTime;
                
                if (remainingTime <= 0)
                {
                    remainingTime = 0;
                    HandleTimeExpired();
                }
                
                OnTimeUpdated?.Invoke(remainingTime);
                
                if (ScoringSystem.Instance != null)
                {
                    ScoringSystem.Instance.UpdateRemainingTime(remainingTime);
                }
            }
        }
    }
    
    /// <summary>
    /// 지정된 유형의 시술을 시작합니다.
    /// </summary>
    public bool StartProcedure(NursingProcedureType procedureType)
    {
        if (isProcedureActive)
        {
            Debug.LogWarning("다른 시술이 이미 진행 중입니다.");
            return false;
        }
        
        // 절차 데이터 가져오기
        CatheterizationProcedureData procedureData = GetProcedureData(procedureType);
        if (procedureData == null)
        {
            Debug.LogError($"시술 유형 {procedureType}에 대한 데이터를 찾을 수 없습니다.");
            return false;
        }
        
        currentProcedureType = procedureType;
        currentProcedureData = procedureData;
        
        // 초기화
        ResetProcedureState();
        
        // 시간 및 점수 설정
        remainingTime = procedureData.timeLimit;
        currentScore = procedureData.maxScore;
        
        if (ScoringSystem.Instance != null)
        {
            ScoringSystem.Instance.StartNewProcedure(procedureType, procedureData.timeLimit);
        }
        
        // 배경 음악 설정 (있는 경우)
        if (backgroundAudioSource != null && procedureData.backgroundMusic != null)
        {
            backgroundAudioSource.clip = procedureData.backgroundMusic;
            backgroundAudioSource.Play();
        }
        
        // 첫 단계 시작
        isProcedureActive = true;
        OnProcedureStarted?.Invoke(procedureType);
        
        StartNextStep();
        
        return true;
    }
    
    /// <summary>
    /// 시술을 일시 중지합니다.
    /// </summary>
    public void PauseProcedure()
    {
        if (!isProcedureActive)
            return;
            
        isPaused = true;
        
        // 배경 음악 일시 중지
        if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
        {
            backgroundAudioSource.Pause();
        }
    }
    
    /// <summary>
    /// 시술을 재개합니다.
    /// </summary>
    public void ResumeProcedure()
    {
        if (!isProcedureActive || !isPaused)
            return;
            
        isPaused = false;
        
        // 배경 음악 재개
        if (backgroundAudioSource != null && !backgroundAudioSource.isPlaying)
        {
            backgroundAudioSource.UnPause();
        }
    }
    
    /// <summary>
    /// 시술을 종료합니다.
    /// </summary>
    public void EndProcedure(bool success = true)
    {
        if (!isProcedureActive)
            return;
            
        isProcedureActive = false;
        isPaused = false;
        
        // 배경 음악 정지
        if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
        {
            backgroundAudioSource.Stop();
        }
        
        // 완료 효과음 재생 (성공한 경우)
        if (success && sfxAudioSource != null && currentProcedureData.completionSound != null)
        {
            sfxAudioSource.PlayOneShot(currentProcedureData.completionSound);
        }
        
        // 최종 점수 계산
        int finalScore = currentScore;
        if (success && ScoringSystem.Instance != null)
        {
            finalScore = ScoringSystem.Instance.CalculateFinalScore();
        }
        
        OnProcedureCompleted?.Invoke(currentProcedureType);
        
        // 결과 기록 (추후 리뷰를 위해 저장)
        SaveProcedureResults(finalScore);
    }
    
    /// <summary>
    /// 현재 단계를 완료하고 다음 단계로 이동합니다.
    /// </summary>
    public void CompleteCurrentStep()
    {
        if (!isProcedureActive || currentStep == null)
            return;
            
        // 단계 완료 표시
        completedSteps[currentStep.stepId] = true;
        
        OnStepCompleted?.Invoke(currentStep, currentStepIndex);
        
        // 다음 단계로 진행
        StartNextStep();
    }
    
    /// <summary>
    /// 다음 단계를 시작합니다.
    /// </summary>
    private void StartNextStep()
    {
        if (currentProcedureData == null || !isProcedureActive)
            return;
            
        // 다음 단계 인덱스 계산
        int nextStepIndex = (currentStepIndex < 0) ? 0 : currentStepIndex + 1;
        
        // 모든 단계 완료 확인
        if (nextStepIndex >= currentProcedureData.steps.Count)
        {
            EndProcedure(true);
            return;
        }
        
        // 다음 단계 설정
        currentStepIndex = nextStepIndex;
        currentStep = currentProcedureData.steps[currentStepIndex];
        currentActionIndex = -1;
        
        if (currentStep == null)
        {
            Debug.LogError($"단계 {currentStepIndex}에 대한 데이터가 누락되었습니다.");
            StartNextStep(); // 누락된 단계 건너뛰기
            return;
        }
        
        OnStepStarted?.Invoke(currentStep, currentStepIndex);
        
        // 단계에 행동이 있는 경우 첫 행동 시작
        if (currentStep.actions.Count > 0)
        {
            StartNextAction();
        }
    }
    
    /// <summary>
    /// 다음 행동을 시작합니다.
    /// </summary>
    private void StartNextAction()
    {
        if (currentStep == null || !isProcedureActive)
            return;
            
        // 다음 행동 인덱스 계산
        int nextActionIndex = (currentActionIndex < 0) ? 0 : currentActionIndex + 1;
        
        // 모든 행동 완료 확인
        if (nextActionIndex >= currentStep.actions.Count)
        {
            CompleteCurrentStep();
            return;
        }
        
        // 다음 행동 설정
        currentActionIndex = nextActionIndex;
        currentAction = currentStep.actions[currentActionIndex];
        
        if (currentAction == null)
        {
            Debug.LogError($"행동 {currentActionIndex}에 대한 데이터가 누락되었습니다.");
            StartNextAction(); // 누락된 행동 건너뛰기
            return;
        }
        
        OnActionStarted?.Invoke(currentAction, currentActionIndex);
    }
    
    /// <summary>
    /// 행동을 완료합니다.
    /// </summary>
    public void CompleteAction(string actionId)
    {
        if (!isProcedureActive)
            return;
            
        // 행동 ID로 완료 표시
        completedActions[actionId] = true;
        
        // 현재 행동이 완료된 경우
        if (currentAction != null && currentAction.actionId == actionId)
        {
            OnActionCompleted?.Invoke(currentAction, currentActionIndex);
            
            // 다음 행동으로 진행
            StartNextAction();
        }
        else
        {
            // 단계가 정해진 순서를 따르지 않는 경우
            if (currentStep != null && !currentStep.isOrderImportant)
            {
                // 모든 필수 행동이 완료되었는지 확인
                CheckStepCompletion();
            }
        }
    }
    
    /// <summary>
    /// 단계 완료 여부를 확인합니다. (순서를 따르지 않는 단계용)
    /// </summary>
    private void CheckStepCompletion()
    {
        if (currentStep == null || currentStep.isOrderImportant)
            return;
            
        // 모든 필수 행동 완료 확인
        bool allRequiredActionsCompleted = true;
        
        foreach (var action in currentStep.GetRequiredActions())
        {
            if (action == null)
                continue;
                
            if (!completedActions.ContainsKey(action.actionId) || !completedActions[action.actionId])
            {
                allRequiredActionsCompleted = false;
                break;
            }
        }
        
        // 모든 필수 행동 완료 시 단계 완료
        if (allRequiredActionsCompleted)
        {
            CompleteCurrentStep();
        }
    }
    
    /// <summary>
    /// 시술 상태를 리셋합니다.
    /// </summary>
    private void ResetProcedureState()
    {
        currentStepIndex = -1;
        currentActionIndex = -1;
        currentStep = null;
        currentAction = null;
        
        completedActions.Clear();
        completedSteps.Clear();
        penaltyRecords.Clear();
        
        isPaused = false;
    }
    
    /// <summary>
    /// 시간 만료를 처리합니다.
    /// </summary>
    private void HandleTimeExpired()
    {
        // 시간 초과로 인한 실패 처리
        ApplyPenalty("시간이 초과되었습니다.", PenaltyType.Major);
        EndProcedure(false);
    }
    
    /// <summary>
    /// 오류를 기록하고 감점을 적용합니다.
    /// </summary>
    public void ApplyPenalty(string errorMessage, PenaltyType penaltyType)
    {
        int penaltyPoints = GetPenaltyPoints(penaltyType);
        
        // 점수 감점
        currentScore = Mathf.Max(0, currentScore - penaltyPoints);
        
        // 오류 기록
        PenaltyRecord record = new PenaltyRecord
        {
            timestamp = DateTime.Now,
            procedureType = currentProcedureType,
            penaltyType = penaltyType,
            penaltyPoints = penaltyPoints,
            errorMessage = errorMessage,
            itemName = (currentAction != null && currentAction.requiredItems.Count > 0) 
                ? currentAction.requiredItems[0].itemName : "",
            stepName = currentStep?.stepName
        };
        
        penaltyRecords.Add(record);
        
        // 이벤트 발생
        OnPenaltyApplied?.Invoke(errorMessage, penaltyType, penaltyPoints);
        OnScoreChanged?.Invoke(currentScore);
        
        // 점수 시스템에 오류 전달
        if (ScoringSystem.Instance != null)
        {
            ScoringSystem.Instance.ApplyPenalty(
                errorMessage, 
                penaltyType,
                record.itemName, 
                record.stepName
            );
        }
    }
    
    /// <summary>
    /// 점수 시스템의 페널티 이벤트를 처리합니다.
    /// </summary>
    private void HandlePenaltyApplied(PenaltyRecord record)
    {
        if (!isProcedureActive)
            return;
            
        // 동일한 처리를 방지하기 위해 기록만 추가
        penaltyRecords.Add(record);
    }
    
    /// <summary>
    /// 점수 시스템의 점수 변경 이벤트를 처리합니다.
    /// </summary>
    private void HandleScoreChanged(int newScore)
    {
        if (!isProcedureActive)
            return;
            
        currentScore = newScore;
        OnScoreChanged?.Invoke(currentScore);
    }
    
    /// <summary>
    /// 페널티 유형에 따른 감점 점수를 반환합니다.
    /// </summary>
    private int GetPenaltyPoints(PenaltyType penaltyType)
    {
        switch (penaltyType)
        {
            case PenaltyType.Minor:
                return 5;  // 경미한 오류
            case PenaltyType.Major:
                return 10; // 중요한 오류
            case PenaltyType.Critical:
                return 20; // 치명적 오류
            default:
                return 5;
        }
    }
    
    /// <summary>
    /// 시술 유형에 따른 시술 데이터를 반환합니다.
    /// </summary>
    
    
    /// <summary>
    /// 시술 결과를 저장합니다.
    /// </summary>
    private void SaveProcedureResults(int finalScore)
    {
        // 결과 저장 로직 (PlayerPrefs, 파일 또는 서버)
        // 여기서는 간단히 로그로 출력
        Debug.Log($"시술 {currentProcedureType} 완료: 점수 {finalScore}, 페널티 {penaltyRecords.Count}개");
        
        // TODO: 결과 저장 구현
    }
    
    /// <summary>
    /// 현재 단계 인덱스를 반환합니다.
    /// </summary>
    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }
    
    /// <summary>
    /// 현재 행동 인덱스를 반환합니다.
    /// </summary>
    public int GetCurrentActionIndex()
    {
        return currentActionIndex;
    }
    
    /// <summary>
    /// 현재 점수를 반환합니다.
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// 남은 시간을 반환합니다.
    /// </summary>
    public float GetRemainingTime()
    {
        return remainingTime;
    }
    
    /// <summary>
    /// 페널티 기록 목록을 반환합니다.
    /// </summary>
    public List<PenaltyRecord> GetPenaltyRecords()
    {
        return new List<PenaltyRecord>(penaltyRecords);
    }
    
    /// <summary>
    /// 현재 시술 데이터를 반환합니다.
    /// </summary>
  
    
    /// <summary>
    /// 현재 단계를 반환합니다.
    /// </summary>
    public ProcedureStepData GetCurrentStep()
    {
        return currentStep;
    }
    
    /// <summary>
    /// 현재 행동을 반환합니다.
    /// </summary>
    public NursingActionData GetCurrentAction()
    {
        return currentAction;
    }
    
    /// <summary>
    /// 행동이 완료되었는지 확인합니다.
    /// </summary>
    public bool IsActionCompleted(string actionId)
    {
        return completedActions.ContainsKey(actionId) && completedActions[actionId];
    }
    
    /// <summary>
    /// 단계가 완료되었는지 확인합니다.
    /// </summary>
    public bool IsStepCompleted(string stepId)
    {
        return completedSteps.ContainsKey(stepId) && completedSteps[stepId];
    }
    
    /// <summary>
    /// 시술이 활성화되어 있는지 확인합니다.
    /// </summary>
    public bool IsProcedureActive()
    {
        return isProcedureActive;
    }
    
    /// <summary>
    /// 시술이 일시 중지되었는지 확인합니다.
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (ScoringSystem.Instance != null)
        {
            ScoringSystem.Instance.OnPenaltyApplied -= HandlePenaltyApplied;
            ScoringSystem.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
// DialogueManager 참조 추가
using UnityEngine.SceneManagement;

/// <summary>
/// 모든 상호작용을 처리하는 범용 베이스 클래스
/// 다양한 간호 절차 및 아이템에 적용 가능한 상호작용 기능 제공
/// </summary>
public class BaseInteractionSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] protected CanvasGroup feedbackPanel;
    [SerializeField] protected Image tutorialArrow;
    [SerializeField] protected Transform popupContainer;
    
    [Header("Feedback Settings")]
    [SerializeField] protected AudioClip successSound;
    [SerializeField] protected AudioClip errorSound;
    [SerializeField] protected float feedbackDuration = 0.5f;
    
    // 이벤트
    public event Action<string, InteractionEventData> OnInteractionStarted;
    public event Action<string, InteractionEventData> OnInteractionCompleted;
    public event Action<string, InteractionEventData, string> OnInteractionError;
    
    // 오디오 컴포넌트
    protected AudioSource audioSource;
    
    // 상호작용 데이터
    protected Dictionary<string, InteractionData> interactionsDatabase = new Dictionary<string, InteractionData>();
    
    // 현재 진행 중인 상호작용
    protected string currentInteractionId;
    protected int currentStepIndex = 0;
    protected bool isInteractionActive = false;
    
    protected virtual void Awake()
    {
        // 오디오 소스 확인
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // UI 컴포넌트 초기화
        if (feedbackPanel != null)
        {
            feedbackPanel.alpha = 0f;
        }
        
        if (tutorialArrow != null && tutorialArrow.gameObject.activeSelf)
        {
            tutorialArrow.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 상호작용 데이터를 등록합니다.
    /// </summary>
    /// <param name="id">상호작용 ID</param>
    /// <param name="data">상호작용 데이터</param>
    public virtual void RegisterInteraction(string id, InteractionData data)
    {
        if (interactionsDatabase.ContainsKey(id))
        {
            Debug.LogWarning($"상호작용 ID '{id}'는 이미 등록되어 있습니다. 덮어씁니다.");
        }
        
        interactionsDatabase[id] = data;
        Debug.Log($"상호작용 '{id}'가 등록되었습니다. 단계 수: {data.steps.Count}");
    }
    
    /// <summary>
    /// 상호작용 시작
    /// </summary>
    /// <param name="interactionId">시작할 상호작용의 ID</param>
    /// <param name="eventData">상호작용 이벤트 데이터</param>
    public virtual bool StartInteraction(string interactionId, InteractionEventData eventData = null)
    {
        if (!interactionsDatabase.ContainsKey(interactionId))
        {
            Debug.LogWarning($"상호작용 ID '{interactionId}'가 등록되어 있지 않습니다.");
            return false;
        }
        
        // 이미 진행 중인 상호작용이 있으면 중지
        if (isInteractionActive)
        {
            StopInteraction();
        }
        
        currentInteractionId = interactionId;
        currentStepIndex = 0;
        isInteractionActive = true;
        
        // 이벤트 발생
        OnInteractionStarted?.Invoke(interactionId, eventData);
        
        // 첫 단계 가이드 표시
        ShowCurrentStepGuide();
        
        return true;
    }
    
    /// <summary>
    /// 현재 진행 중인 상호작용을 중지합니다.
    /// </summary>
    public virtual void StopInteraction()
    {
        if (!isInteractionActive)
            return;
            
        HideGuide();
        isInteractionActive = false;
        currentInteractionId = null;
        currentStepIndex = 0;
    }
    
    /// <summary>
    /// 현재 단계의 가이드를 표시합니다.
    /// </summary>
    protected virtual void ShowCurrentStepGuide()
    {
        if (!isInteractionActive || !interactionsDatabase.ContainsKey(currentInteractionId))
            return;
            
        var data = interactionsDatabase[currentInteractionId];
        if (currentStepIndex >= data.steps.Count)
            return;
            
        var step = data.steps[currentStepIndex];
        
        // 가이드 텍스트 업데이트
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGuideText(step.guideText);
        }
        
        // 튜토리얼 화살표 표시
        ShowTutorialArrow(step);
    }
    
    /// <summary>
    /// 튜토리얼 화살표를 표시합니다.
    /// </summary>
    protected virtual void ShowTutorialArrow(InteractionStep step)
    {
        if (tutorialArrow == null || step.tutorialArrowSprite == null)
            return;
            
        tutorialArrow.sprite = step.tutorialArrowSprite;
        tutorialArrow.rectTransform.anchoredPosition = step.tutorialArrowPosition;
        tutorialArrow.rectTransform.eulerAngles = new Vector3(0, 0, step.tutorialArrowRotation);
        tutorialArrow.gameObject.SetActive(true);
        
        // 깜빡임 효과
        CanvasGroup canvasGroup = tutorialArrow.gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tutorialArrow.gameObject.AddComponent<CanvasGroup>();
        }
        
        // 기존 애니메이션 제거
        DOTween.Kill(canvasGroup);
        
        // 깜빡임 애니메이션
        canvasGroup.DOFade(0.4f, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }
    
    /// <summary>
    /// 가이드를 숨깁니다.
    /// </summary>
    protected virtual void HideGuide()
    {
        if (tutorialArrow != null)
        {
            tutorialArrow.gameObject.SetActive(false);
            
            // 애니메이션 제거
            CanvasGroup canvasGroup = tutorialArrow.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                DOTween.Kill(canvasGroup);
                canvasGroup.alpha = 1f;
            }
        }
    }
    
    /// <summary>
    /// 드래그 상호작용을 처리합니다.
    /// </summary>
    public virtual void HandleDragInteraction(Vector2 startPosition, Vector2 endPosition, Vector2 dragVector)
    {
        if (!isInteractionActive || !interactionsDatabase.ContainsKey(currentInteractionId))
            return;
            
        var data = interactionsDatabase[currentInteractionId];
        if (currentStepIndex >= data.steps.Count)
            return;
            
        var step = data.steps[currentStepIndex];
        
        // 현재 단계가 드래그 상호작용인지 확인
        if (step.interactionType != InteractionType.Drag)
        {
            ShowError("이 단계에서는 드래그를 사용할 수 없습니다.");
            return;
        }
        
        // 드래그 각도 계산
        float dragAngle = Mathf.Atan2(dragVector.y, dragVector.x) * Mathf.Rad2Deg;
        if (dragAngle < 0) dragAngle += 360f;
        
        // 허용 오차 범위 확인
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(dragAngle, step.requiredDragAngle));
        
        if (angleDifference <= step.dragAngleTolerance)
        {
            // 성공
            CompleteCurrentStep();
        }
        else
        {
            // 오류: 잘못된 드래그 방향
            ShowError(step.errorMessage);
        }
    }
    
    /// <summary>
    /// 클릭 상호작용을 처리합니다.
    /// </summary>
    public virtual void HandleClickInteraction(Vector2 clickPosition)
    {
        if (!isInteractionActive || !interactionsDatabase.ContainsKey(currentInteractionId))
            return;
            
        var data = interactionsDatabase[currentInteractionId];
        if (currentStepIndex >= data.steps.Count)
            return;
            
        var step = data.steps[currentStepIndex];
        
        // 현재 단계가 클릭 상호작용인지 확인
        if (step.interactionType != InteractionType.SingleClick)
        {
            ShowError("이 단계에서는 클릭을 사용할 수 없습니다.");
            return;
        }
        
        // 클릭 영역 확인
        if (step.validClickArea.Contains(clickPosition))
        {
            // 성공
            CompleteCurrentStep();
        }
        else
        {
            // 오류: 잘못된 클릭 위치
            ShowError(step.errorMessage);
        }
    }
    
    /// <summary>
    /// 퀴즈 상호작용을 처리합니다.
    /// </summary>
    public virtual void HandleQuizInteraction(int selectedOptionIndex)
    {
        if (!isInteractionActive || !interactionsDatabase.ContainsKey(currentInteractionId))
            return;
            
        var data = interactionsDatabase[currentInteractionId];
        if (currentStepIndex >= data.steps.Count)
            return;
            
        var step = data.steps[currentStepIndex];
        
        // 현재 단계가 퀴즈 상호작용인지 확인
        if (step.interactionType != InteractionType.Quiz)
        {
            ShowError("이 단계에서는 퀴즈를 사용할 수 없습니다.");
            return;
        }
        
        // 정답 확인
        if (selectedOptionIndex == step.correctOptionIndex)
        {
            // 성공
            CompleteCurrentStep();
        }
        else
        {
            // 오류: 잘못된 선택
            ShowError(step.errorMessage);
        }
    }
    
    /// <summary>
    /// 현재 단계를 완료합니다.
    /// </summary>
    protected virtual void CompleteCurrentStep()
    {
        if (!isInteractionActive || !interactionsDatabase.ContainsKey(currentInteractionId))
            return;
            
        var data = interactionsDatabase[currentInteractionId];
        if (currentStepIndex >= data.steps.Count)
            return;
            
        // 성공 효과음
        PlaySound(successSound);
        
        // 성공 피드백
        ShowSuccessFeedback();
        
        // 다음 단계로 진행
        currentStepIndex++;
        
        // 모든 단계 완료 확인
        if (currentStepIndex >= data.steps.Count)
        {
            CompleteInteraction();
        }
        else
        {
            // 다음 단계 가이드 표시
            ShowCurrentStepGuide();
        }
    }
    
    /// <summary>
    /// 상호작용을 완료합니다.
    /// </summary>
    protected virtual void CompleteInteraction()
    {
        if (!isInteractionActive)
            return;
            
        var eventData = new InteractionEventData
        {
            interactionId = currentInteractionId,
            completedSteps = currentStepIndex
        };
        
        // 이벤트 발생
        OnInteractionCompleted?.Invoke(currentInteractionId, eventData);
        
        // 상호작용 종료
        HideGuide();
        isInteractionActive = false;
        currentInteractionId = null;
        currentStepIndex = 0;
    }
    
    /// <summary>
    /// 오류를 표시합니다.
    /// </summary>
    protected virtual void ShowError(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
            return;
            
        // 오류 효과음
        PlaySound(errorSound);
        
        // 오류 피드백
        ShowErrorFeedback();
        
        // 이벤트 발생
        var eventData = new InteractionEventData
        {
            interactionId = currentInteractionId,
            completedSteps = currentStepIndex
        };
        OnInteractionError?.Invoke(currentInteractionId, eventData, errorMessage);
        
        // 오류 팝업 표시
        if (popupContainer != null && UIManager.Instance != null)
        {
            // UIManager에 ShowSmallPopup이 없으므로 UpdateGuideText로 대체
            UIManager.Instance.UpdateGuideText("오류: " + errorMessage);
            
            // 팝업을 표시하려면 DialogueManager를 사용하는 방법도 있음
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowSmallDialogue("오류: " + errorMessage);
            }
        }
    }
    
    /// <summary>
    /// 성공 피드백을 표시합니다.
    /// </summary>
    protected virtual void ShowSuccessFeedback()
    {
        if (feedbackPanel == null)
            return;
            
        // 기존 애니메이션 제거
        DOTween.Kill(feedbackPanel);
        
        // 성공 피드백 색상
        feedbackPanel.GetComponent<Image>().color = new Color(0f, 1f, 0f, 0.3f);
        
        // 깜빡임 애니메이션
        Sequence sequence = DOTween.Sequence();
        sequence.Append(feedbackPanel.DOFade(0.3f, feedbackDuration * 0.3f));
        sequence.Append(feedbackPanel.DOFade(0f, feedbackDuration * 0.7f));
    }
    
    /// <summary>
    /// 오류 피드백을 표시합니다.
    /// </summary>
    protected virtual void ShowErrorFeedback()
    {
        if (feedbackPanel == null)
            return;
            
        // 기존 애니메이션 제거
        DOTween.Kill(feedbackPanel);
        
        // 오류 피드백 색상
        feedbackPanel.GetComponent<Image>().color = new Color(1f, 0f, 0f, 0.3f);
        
        // 깜빡임 애니메이션
        Sequence sequence = DOTween.Sequence();
        sequence.Append(feedbackPanel.DOFade(0.3f, feedbackDuration * 0.2f));
        sequence.Append(feedbackPanel.DOFade(0f, feedbackDuration * 0.2f));
        sequence.Append(feedbackPanel.DOFade(0.3f, feedbackDuration * 0.2f));
        sequence.Append(feedbackPanel.DOFade(0f, feedbackDuration * 0.4f));
    }
    
    /// <summary>
    /// 효과음을 재생합니다.
    /// </summary>
    protected virtual void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    protected virtual void OnDestroy()
    {
        // 애니메이션 정리
        if (tutorialArrow != null)
        {
            CanvasGroup canvasGroup = tutorialArrow.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                DOTween.Kill(canvasGroup);
            }
        }
        
        if (feedbackPanel != null)
        {
            DOTween.Kill(feedbackPanel);
        }
    }
}

/// <summary>
/// 상호작용 데이터를 포함하는 클래스
/// 여러 상호작용 단계를 정의합니다.
/// </summary>
[System.Serializable]
public class InteractionData
{
    public string id;
    public string name;
    public string description;
    public List<InteractionStep> steps = new List<InteractionStep>();
}

/// <summary>
/// 상호작용 단계를 정의하는 클래스
/// </summary>
[System.Serializable]
public class InteractionStep
{
    public InteractionType interactionType;
    public string guideText;
    
    // 드래그 관련
    public float requiredDragAngle;
    public float dragAngleTolerance = 30f;
    
    // 클릭 관련
    public Rect validClickArea;
    
    // 퀴즈 관련
    public string quizQuestion;
    public string[] quizOptions;
    public int correctOptionIndex;
    
    // 튜토리얼 화살표 관련
    public Sprite tutorialArrowSprite;
    public Vector2 tutorialArrowPosition;
    public float tutorialArrowRotation;
    
    // 피드백 관련
    public string successMessage;
    public string errorMessage;
    
    // ItemInteractionHandler와의 호환성을 위한 필드
    public int penaltyPoints = 5;
}

/// <summary>
/// 상호작용 이벤트 데이터를 포함하는 클래스
/// </summary>
public class InteractionEventData
{
    public string interactionId;
    public int completedSteps;
    public object customData;
}
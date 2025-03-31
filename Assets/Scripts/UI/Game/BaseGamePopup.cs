using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using DG.Tweening;

public abstract class BaseGamePopup : MonoBehaviour
{
    [Header("Base Components")]
    [SerializeField] protected TextMeshProUGUI timerText;
    [SerializeField] protected Image successImage;
    [SerializeField] protected Image answerImage;
    [SerializeField] protected CanvasGroup canvasGroup;

    [Header("Timer Settings")]
    [SerializeField] protected float timeLimit = 10f;

    protected float currentTime;
    protected bool isTimerRunning = false;
    protected Action onSuccess;
    protected Action onFail;
    protected int penaltyScore;

    protected virtual void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // 페이드 인으로 팝업 표시
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 0.3f);

        ResetTimer();
    }

    protected virtual void Update()
    {
        if (isTimerRunning)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentTime <= 0)
            {
                OnTimeExpired();
            }
        }
    }

    protected virtual void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = $"{Mathf.Ceil(currentTime)}초";
        }
    }

    protected virtual void ResetTimer()
    {
        currentTime = timeLimit;
        UpdateTimerDisplay();
    }

    public virtual void Initialize(Action onSuccess, Action onFail, int penaltyScore)
    {
        this.onSuccess = onSuccess;
        this.onFail = onFail;
        this.penaltyScore = penaltyScore;
    }

    protected virtual void OnTimeExpired()
    {
        isTimerRunning = false;
        HandleFailure();
    }

    protected virtual void HandleSuccess()
    {
        isTimerRunning = false;
        ShowSuccessImage(() => {
            onSuccess?.Invoke();
            ClosePopup();
        });
    }

    protected virtual void HandleFailure()
    {
        isTimerRunning = false;
        ShowAnswerImage(() => {
            onFail?.Invoke();
            // 작은 대화창 표시
            DialogueManager.Instance.ShowSmallDialogue("실패했습니다. 다시 시도해보세요.", false, () => {
                ClosePopup();
            });
        });
    }

    protected virtual void ShowSuccessImage(Action onComplete)
    {
        if (successImage != null)
        {
            successImage.gameObject.SetActive(true);
            successImage.DOFade(1f, 0.5f).OnComplete(() => {
                DOVirtual.DelayedCall(1f, () => {
                    onComplete?.Invoke();
                });
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    protected virtual void ShowAnswerImage(Action onComplete)
    {
        if (answerImage != null)
        {
            answerImage.gameObject.SetActive(true);
            answerImage.DOFade(1f, 0.5f).OnComplete(() => {
                DOVirtual.DelayedCall(1f, () => {
                    onComplete?.Invoke();
                });
            });
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    protected virtual void ClosePopup()
    {
        canvasGroup.DOFade(0f, 0.3f).OnComplete(() => {
            Destroy(gameObject);
        });
    }

    public virtual void StartGame()
    {
        isTimerRunning = true;
    }

    // MiniGamePopup.cs에 추가
    public void OnGameSuccess()
    {
        HandleSuccess();
    }

    public void OnGameFailure()
    {
        HandleFailure();
    }
}
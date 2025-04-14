// Assets/Scripts/UI/PopUp/QuizPopup.cs 수정

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Collections;
using UnityEngine.EventSystems;

namespace Nursing.UI
{
    public class QuizPopup : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TextMeshProUGUI[] optionTexts;
        [SerializeField] private Image[] optionImages;
        [SerializeField] private TextMeshProUGUI timertext;
        [SerializeField] private Image timerGauge;

        [Header("결과 UI")]
        [SerializeField] private GameObject rightAnswerImage;
        [SerializeField] private GameObject wrongAnswerImage;
        [SerializeField] private Button closeButton;

        private bool quizCompleted = false;
 

        [Header("애니메이션 설정")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private Ease fadeEase = Ease.OutQuad;

        private float timeLimit;
        private float remainingTime;
        private int correctAnswerIndex;
        private bool isAnswered;

        private CanvasGroup canvasGroup;

        public event Action<bool> OnQuizComplete;
        public event Action OnDestroyed;

        private void Awake()
        {
            // 결과 이미지 초기화
            if (rightAnswerImage != null) rightAnswerImage.SetActive(false);
            if (wrongAnswerImage != null) wrongAnswerImage.SetActive(false);

            remainingTime = timeLimit;
            isAnswered = false;
            quizCompleted = false;

            // 닫기 버튼 비활성화 처리
            if (closeButton != null)
            {
                closeButton.interactable = false;
            }

            // 닫기 버튼 이벤트 설정
            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (FindObjectOfType<cakeslice.OutlineEffect>() == null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null && mainCamera.gameObject.GetComponent<cakeslice.OutlineEffect>() == null)
                {
                    mainCamera.gameObject.AddComponent<cakeslice.OutlineEffect>();
                }
            }
        }

        private void OnEnable()
        {
            FadeIn();
        }

        private void Start()
        {
            isAnswered = false;
            remainingTime = timeLimit;
        }

        private void Update()
        {
            if (quizCompleted) return;

            if (timeLimit > 0 && !isAnswered)
            {
                remainingTime -= Time.deltaTime;

                // 시간 표시 업데이트
                if (timertext != null)
                {
                    timertext.text = Mathf.Ceil(remainingTime).ToString();
                }

                // 타이머 게이지 업데이트
                if (timerGauge != null)
                {
                    float timeRatio = remainingTime / timeLimit;
                    timerGauge.fillAmount = Mathf.Clamp01(timeRatio);

                    // 시간에 따라 색상 변경 (초록색 -> 노란색 -> 빨간색)
                    Color gaugeColor = Color.Lerp(Color.red, Color.green, timeRatio);
                    timerGauge.color = gaugeColor;
                }

                    if (remainingTime <= 0)
                {
                    OnTimeUp();
                }
            }
        }

        /// <summary>
        /// 기존 퀴즈 설정 메서드 (하위 호환성 유지)
        /// </summary>
        public void SetupQuiz(string question, List<string> options, int correctIndex, Sprite[] images = null, float time = 0)
        {
            if (images != null && images.Length > 0)
            {
                SetupImageQuiz(question, images, correctIndex, time);
            }
            else
            {
                SetupTextQuiz(question, options, correctIndex, time);
            }
        }

        /// <summary>
        /// 텍스트 퀴즈 설정 메서드
        /// </summary>
        public void SetupTextQuiz(string question, List<string> options, int correctIndex, float time = 0)
        {
           

            // 질문 설정
            if (questionText != null)
                questionText.text = question;

            correctAnswerIndex = correctIndex;
            timeLimit = time;
            remainingTime = timeLimit;

            // 옵션 버튼 설정
            int optionCount = Mathf.Min(options.Count, optionButtons.Length);

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < optionCount)
                {
                    optionButtons[i].gameObject.SetActive(true);

                    int optionIndex = i; // 클로저에서 사용하기 위해 로컬 변수로 복사

                    // 텍스트 설정
                    if (optionTexts.Length > i && optionTexts[i] != null)
                    {
                        optionTexts[i].text = options[i];
                        optionTexts[i].gameObject.SetActive(true);
                    }

                    // 이미지 비활성화
                    if (optionImages.Length > i && optionImages[i] != null)
                    {
                        optionImages[i].gameObject.SetActive(false);
                    }

                    // 버튼 클릭 이벤트 설정
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => {
                        OnOptionClicked(optionIndex);
                    });
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }

            isAnswered = false;
        }

        /// <summary>
        /// 이미지 퀴즈 설정 메서드
        /// </summary>
        public void SetupImageQuiz(string question, Sprite[] images, int correctIndex, float time = 0)
        {
           

            // 질문 설정
            if (questionText != null)
                questionText.text = question;

            correctAnswerIndex = correctIndex;
            timeLimit = time;
            remainingTime = timeLimit;

            // 옵션 버튼 설정
            int optionCount = Mathf.Min(images.Length, optionButtons.Length);

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i < optionCount && images[i] != null)
                {
                    optionButtons[i].gameObject.SetActive(true);

                    int optionIndex = i; // 클로저에서 사용하기 위해 로컬 변수로 복사

                    // 텍스트 비우기/숨기기
                    if (optionTexts.Length > i && optionTexts[i] != null)
                    {
                        optionTexts[i].text = "";
                        optionTexts[i].gameObject.SetActive(false);
                    }

                    // 이미지 설정
                    if (optionImages.Length > i && optionImages[i] != null)
                    {
                        optionImages[i].sprite = images[i];
                        // Native Size 설정 추가
                        optionImages[i].SetNativeSize();
                        optionImages[i].gameObject.SetActive(true);
                    }

                    // 버튼 클릭 이벤트 설정
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => {
                        OnOptionClicked(optionIndex);
                    });
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }

            isAnswered = false;
        }

        // 옵션 버튼 클릭 처리 (기존 코드 유지)
        private void OnOptionClicked(int optionIndex)
        {
            if (isAnswered || quizCompleted) return;

            if (optionIndex > -50) isAnswered = true;

            bool isCorrect = optionIndex == correctAnswerIndex;

            // 모든 버튼 비활성화 및 정답/오답 표시
            for (int i = 0; i < optionButtons.Length; i++)
            {
                // 버튼 비활성화
                optionButtons[i].interactable = false;

                // 정답인 버튼에 파란색 테두리 표시
                if (i == correctAnswerIndex)
                {
                    SetButtonOutline(optionButtons[i], Color.blue);
                }
                // 사용자가 선택한 오답 버튼에 빨간색 테두리 표시
                else if (i == optionIndex && i != correctAnswerIndex)
                {
                    SetButtonOutline(optionButtons[i], Color.red);
                }
            }

            // 결과 표시
            ShowQuizResult(isCorrect);
        }

        // 시간 초과 처리 (기존 코드 유지)
        private void OnTimeUp()
        {
            if (isAnswered || quizCompleted) return;
            OnOptionClicked(-100);
        }

        // 결과 표시 (기존 코드 유지)
        private void ShowQuizResult(bool isCorrect)
        {
            quizCompleted = true;
            Destroy(timertext);
            Destroy(timerGauge);

            // 결과 이미지 표시
            if (isCorrect && rightAnswerImage != null)
            {
                rightAnswerImage.SetActive(true);
            }
            else if (!isCorrect && wrongAnswerImage != null)
            {
                wrongAnswerImage.SetActive(true);
            }

            // 3초 후 자동으로 결과 이미지 숨기고 완료 처리
            StartCoroutine(AutoCompleteQuiz(isCorrect));
        }

        // 버튼에 아웃라인 설정 (기존 코드 유지)
        private void SetButtonOutline(Button button, Color color)
        {
            // 방법 1: 버튼에 Outline 컴포넌트가 있는 경우
            Outline outline = button.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = color;
                outline.enabled = true;
                return;
            }
        }

        // 자동 완료 코루틴 (기존 코드 유지)
        private IEnumerator AutoCompleteQuiz(bool isCorrect)
        {
            // 3초 대기
            yield return new WaitForSeconds(0.5f);

            // 결과 이미지 숨기기
            if (rightAnswerImage != null) rightAnswerImage.SetActive(false);
            if (wrongAnswerImage != null) wrongAnswerImage.SetActive(false);

            // 오답인 경우 패널티 시스템 작동
            if (!isCorrect)
            {
                OnQuizComplete?.Invoke(false);
            }
            else
            {
                // 정답인 경우에는 닫기 버튼 클릭 시 호출되도록 함
                // 이벤트는 호출하지 않음
            }

            // 닫기 버튼 활성화
            if (closeButton != null)
            {
                closeButton.interactable = true;
            }
        }

        // 닫기 버튼 클릭 처리 (기존 코드 유지)
        private void OnCloseButtonClicked()
        {
            // 퀴즈 완료 후에만 닫기 버튼 동작
            if (quizCompleted)
            {
                bool isCorrect = isAnswered && correctAnswerIndex >= 0;

                // 정답이었던 경우에만 여기서 이벤트 호출 (오답인 경우는 이미 호출됨)
                if (isCorrect)
                {
                    OnQuizComplete?.Invoke(true);
                }

                FadeOut(() => {
                    OnDestroyed?.Invoke();
                    Destroy(gameObject);
                });
            }
        }

        // 페이드 인 애니메이션 (기존 코드 유지)
        private void FadeIn()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeEase);
            }
        }

        // 페이드 아웃 애니메이션 (기존 코드 유지)
        private void FadeOut(Action onComplete = null)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, fadeOutDuration)
                    .SetEase(fadeEase)
                    .OnComplete(() => {
                        onComplete?.Invoke();
                    });
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        private void OnDestroy()
        {
            DOTween.Kill(canvasGroup);
        }
    }
}
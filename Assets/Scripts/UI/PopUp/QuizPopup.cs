using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using System;

namespace Nursing.UI
{
    public class QuizPopup : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private TextMeshProUGUI questionText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private TextMeshProUGUI[] optionTexts;
        [SerializeField] private Image[] optionImages;
        [SerializeField] private Image timerImage;
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button closeButton;
        
        [Header("애니메이션 설정")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private Ease fadeEase = Ease.OutQuad;
        
        private float timeLimit;
        private float remainingTime;
        private int correctAnswerIndex;
        private bool isAnswered;
        private bool isTimerRunning;
        private CanvasGroup canvasGroup;
        
        public event Action<bool> OnQuizComplete;
        
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
                
            // 닫기 버튼 이벤트 설정
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(() => {
                    OnCloseButtonClicked();
                });
            }
            
            // 결과 패널 초기화
            if (resultPanel != null)
                resultPanel.SetActive(false);
        }
        
        private void OnEnable()
        {
            FadeIn();
        }
        
        private void Start()
        {
            isAnswered = false;
            isTimerRunning = timeLimit > 0;
            remainingTime = timeLimit;
            
            // 타이머 초기화
            if (timerImage != null)
            {
                timerImage.fillAmount = 1.0f;
                timerImage.gameObject.SetActive(isTimerRunning);
            }
        }
        
        private void Update()
        {
            if (isTimerRunning && !isAnswered)
            {
                remainingTime -= Time.deltaTime;
                
                if (timerImage != null)
                {
                    timerImage.fillAmount = Mathf.Clamp01(remainingTime / timeLimit);
                }
                
                if (remainingTime <= 0)
                {
                    isTimerRunning = false;
                    OnTimeUp();
                }
            }
        }
        
        /// <summary>
        /// 퀴즈를 설정합니다.
        /// </summary>
        public void SetupQuiz(string question, List<string> options, int correctIndex, Sprite[] images = null, float time = 0)
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
                    }
                    
                    // 이미지 설정 (있는 경우)
                    if (images != null && images.Length > i && optionImages.Length > i && optionImages[i] != null)
                    {
                        optionImages[i].sprite = images[i];
                        optionImages[i].gameObject.SetActive(images[i] != null);
                    }
                    else if (optionImages.Length > i && optionImages[i] != null)
                    {
                        optionImages[i].gameObject.SetActive(false);
                    }
                    
                    // 버튼 클릭 이벤트 설정
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => OnOptionClicked(optionIndex));
                }
                else
                {
                    optionButtons[i].gameObject.SetActive(false);
                }
            }
            
            // 타이머 설정
            isTimerRunning = timeLimit > 0;
            if (timerImage != null)
            {
                timerImage.gameObject.SetActive(isTimerRunning);
                timerImage.fillAmount = 1.0f;
            }
            
            // 결과 패널 초기화
            if (resultPanel != null)
                resultPanel.SetActive(false);
                
            isAnswered = false;
        }
        
        /// <summary>
        /// 옵션 버튼 클릭 처리
        /// </summary>
        private void OnOptionClicked(int optionIndex)
        {
            if (isAnswered)
                return;
                
            isAnswered = true;
            isTimerRunning = false;
            
            bool isCorrect = optionIndex == correctAnswerIndex;
            
            // 결과 표시 (선택적)
            if (resultPanel != null && resultText != null)
            {
                resultText.text = isCorrect ? "정답입니다!" : "오답입니다!";
                resultText.color = isCorrect ? Color.green : Color.red;
                resultPanel.SetActive(true);
                
                // 약간의 지연 후 닫기
                Invoke("CompleteQuiz", 1.5f);
            }
            else
            {
                // 바로 완료 처리
                CompleteQuiz();
            }
            
            // 버튼 상태 시각적으로 표시
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i == correctAnswerIndex)
                {
                    // 정답 버튼 강조
                    ColorBlock colors = optionButtons[i].colors;
                    colors.normalColor = Color.green;
                    optionButtons[i].colors = colors;
                }
                else if (i == optionIndex && !isCorrect)
                {
                    // 선택한 오답 버튼 강조
                    ColorBlock colors = optionButtons[i].colors;
                    colors.normalColor = Color.red;
                    optionButtons[i].colors = colors;
                }
                
                // 버튼 비활성화
                optionButtons[i].interactable = false;
            }
        }
        
        /// <summary>
        /// 시간 초과 처리
        /// </summary>
        private void OnTimeUp()
        {
            isAnswered = true;
            
            // 시간 초과 시 틀린 것으로 처리
            if (resultPanel != null && resultText != null)
            {
                resultText.text = "시간 초과!";
                resultText.color = Color.red;
                resultPanel.SetActive(true);
                
                // 약간의 지연 후 닫기
                Invoke("CompleteQuiz", 1.5f);
            }
            else
            {
                // 바로 완료 처리
                CompleteQuiz();
            }
            
            // 정답 버튼 표시
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i == correctAnswerIndex)
                {
                    // 정답 버튼 강조
                    ColorBlock colors = optionButtons[i].colors;
                    colors.normalColor = Color.green;
                    optionButtons[i].colors = colors;
                }
                
                // 버튼 비활성화
                optionButtons[i].interactable = false;
            }
        }
        
        /// <summary>
        /// 퀴즈 완료 처리
        /// </summary>
        private void CompleteQuiz()
        {
            bool isCorrect = isAnswered && correctAnswerIndex >= 0;
            
            // 페이드 아웃 후 완료 이벤트 발생
            FadeOut(() => {
                OnQuizComplete?.Invoke(isCorrect);
            });
        }
        
        /// <summary>
        /// 닫기 버튼 클릭 처리
        /// </summary>
        private void OnCloseButtonClicked()
        {
            // 답변 전에 닫으면 오답으로 처리
            if (!isAnswered)
            {
                isAnswered = true;
                CompleteQuiz();
            }
        }
        
        /// <summary>
        /// 페이드 인 애니메이션
        /// </summary>
        private void FadeIn()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeEase);
            }
        }
        
        /// <summary>
        /// 페이드 아웃 애니메이션
        /// </summary>
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
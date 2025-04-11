using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;
using System;
using System.Collections;

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
        [SerializeField] private Image timerGauge;  // 남은 시간 게이지

        // 추가 필요한 UI 요소들 선언
        [Header("결과 UI")]
        [SerializeField] private GameObject rightAnswerImage;  // 정답 이미지
        [SerializeField] private GameObject wrongAnswerImage;  // 오답 이미지


        [SerializeField] private Button closeButton;


        // 버튼 하이라이트용 컬러
        private Color correctHighlightColor = new Color(0.2f, 0.6f, 1.0f);  // 파란색
        private Color wrongHighlightColor = new Color(1.0f, 0.2f, 0.2f);    // 빨간색
        private Color originalButtonColor;
        private bool quizCompleted = false;

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
            // 결과 이미지 초기화
            if (rightAnswerImage != null) rightAnswerImage.SetActive(false);
            if (wrongAnswerImage != null) wrongAnswerImage.SetActive(false);

            // 원래 버튼 색상 저장
            if (optionButtons.Length > 0 && optionButtons[0] != null)
            {
                originalButtonColor = optionButtons[0].colors.normalColor;
            }

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
            
            
        }
        
        private void Update()
        {
            if (quizCompleted) return;

            if (timeLimit > 0 && !isAnswered)
            {
                remainingTime -= Time.deltaTime;

                // Update 메서드 내에서 시간 표시 부분
                if (timertext != null)
                {
                    
                    timertext.text = Mathf.Ceil(remainingTime).ToString(); // 예: "11"
                                                                          
                    
                }

                // 타이머 게이지 업데이트
                if (timerGauge != null)
                {
                    timerGauge.fillAmount = Mathf.Clamp01(remainingTime / timeLimit);
                }

                if (remainingTime <= 0)
                {
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
            
           
            
           
                
            isAnswered = false;
        }
        
        /// <summary>
        /// 옵션 버튼 클릭 처리
        /// </summary>
        private void OnOptionClicked(int optionIndex)
        {
            if (isAnswered || quizCompleted) return;

            isAnswered = true;
            bool isCorrect = optionIndex == correctAnswerIndex;

            // 모든 버튼 비활성화
            foreach (Button button in optionButtons)
            {
                button.interactable = false;
            }

            // 결과 표시
            ShowQuizResult(isCorrect, optionIndex);
        }
        
        /// <summary>
        /// 시간 초과 처리
        /// </summary>
        private void OnTimeUp()
        {
            if (isAnswered || quizCompleted) return;

            isAnswered = true;

            // 모든 버튼 비활성화
            foreach (Button button in optionButtons)
            {
                button.interactable = false;
            }

            // 오답으로 처리
            ShowQuizResult(false, -1);
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

        private void ShowQuizResult(bool isCorrect, int selectedIndex)
        {
            quizCompleted = true;

            // 정답 버튼 하이라이트
            HighlightCorrectAnswer();

            // 오답 선택 시 해당 버튼 빨간색 하이라이트
            if (!isCorrect && selectedIndex >= 0 && selectedIndex < optionButtons.Length)
            {
                ColorBlock colorBlock = optionButtons[selectedIndex].colors;
                colorBlock.normalColor = wrongHighlightColor;
                optionButtons[selectedIndex].colors = colorBlock;
            }

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

        // 정답 버튼 하이라이트
        private void HighlightCorrectAnswer()
        {
            if (correctAnswerIndex >= 0 && correctAnswerIndex < optionButtons.Length)
            {
                ColorBlock colorBlock = optionButtons[correctAnswerIndex].colors;
                colorBlock.normalColor = correctHighlightColor;
                optionButtons[correctAnswerIndex].colors = colorBlock;
            }
        }

        // 자동 완료 코루틴
        private IEnumerator AutoCompleteQuiz(bool isCorrect)
        {
            // 3초 대기
            yield return new WaitForSeconds(3.0f);

            // 결과 이미지 숨기기
            if (rightAnswerImage != null) rightAnswerImage.SetActive(false);
            if (wrongAnswerImage != null) wrongAnswerImage.SetActive(false);

            // 닫기 버튼 활성화
            if (closeButton != null)
            {
                closeButton.interactable = true;
            }

            // 오답인 경우 페널티 시스템 작동
            if (!isCorrect)
            {
                OnQuizComplete?.Invoke(false);
            }
            else
            {
                // 정답인 경우에는 닫기 버튼 클릭 시 호출되도록 함
                // 이벤트는 호출하지 않음
            }
        }

        // 닫기 버튼 클릭 처리 수정
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
                    Destroy(gameObject);
                });
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
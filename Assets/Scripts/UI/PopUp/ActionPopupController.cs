using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;
using DG.Tweening;

namespace Nursing.UI
{
    public class ActionPopupController : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Button[] actionButtons;
        [SerializeField] private TextMeshProUGUI[] buttonTexts;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Header("애니메이션 설정")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private Ease fadeEase = Ease.OutQuad;
        
        private List<string> correctButtonIds;
        private bool requireAllButtons;
        private List<string> selectedButtonIds = new List<string>();
        private CanvasGroup canvasGroup;
        
        public event Action<bool> OnActionComplete;
        
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
        }
        
        private void OnEnable()
        {
            FadeIn();
        }
        
        /// <summary>
        /// 액션 팝업을 설정합니다.
        /// </summary>
        public void SetupActionPopup(List<string> correctIds, bool requireAll)
        {
            correctButtonIds = correctIds;
            requireAllButtons = requireAll;
            selectedButtonIds.Clear();
            
            // 타이틀 텍스트 설정
            if (titleText != null)
            {
                titleText.text = requireAll ? "모든 필요한 항목을 선택하세요." : "올바른 항목을 선택하세요.";
            }
            
            // 배열 초기화 및 유효성 검사
            if (actionButtons == null || actionButtons.Length == 0)
            {
                Debug.LogError("액션 버튼이 설정되지 않았습니다.");
                return;
            }
            
            if (buttonTexts != null && buttonTexts.Length < actionButtons.Length)
            {
                Debug.LogWarning("버튼 텍스트가 버튼 개수보다 적습니다.");
            }
            
            // 랜덤 버튼 ID 목록 생성
            List<string> allButtonIds = new List<string>(correctButtonIds);
            
            // 버튼 수가 충분하지 않은 경우 더미 버튼 ID 추가
            while (allButtonIds.Count < actionButtons.Length)
            {
                allButtonIds.Add("dummy_" + allButtonIds.Count);
            }
            
            // 버튼 ID 섞기
            for (int i = 0; i < allButtonIds.Count; i++)
            {
                int randomIndex = UnityEngine.Random.Range(i, allButtonIds.Count);
                string temp = allButtonIds[i];
                allButtonIds[i] = allButtonIds[randomIndex];
                allButtonIds[randomIndex] = temp;
            }
            
            // 버튼 설정
            for (int i = 0; i < actionButtons.Length; i++)
            {
                actionButtons[i].gameObject.SetActive(i < allButtonIds.Count);
                
                if (i < allButtonIds.Count)
                {
                    int buttonIndex = i; // 클로저에서 사용하기 위해 로컬 변수로 복사
                    string buttonId = allButtonIds[i];
                    
                    // 버튼 텍스트 설정
                    if (buttonTexts != null && i < buttonTexts.Length && buttonTexts[i] != null)
                    {
                        buttonTexts[i].text = buttonId;
                    }
                    
                    // 버튼 클릭 이벤트 설정
                    actionButtons[i].onClick.RemoveAllListeners();
                    actionButtons[i].onClick.AddListener(() => OnActionButtonClicked(buttonIndex, buttonId));
                }
            }
        }
        
        /// <summary>
        /// 액션 버튼 클릭 처리
        /// </summary>
        private void OnActionButtonClicked(int buttonIndex, string buttonId)
        {
            // 이미 선택된 버튼인지 확인
            if (selectedButtonIds.Contains(buttonId))
                return;
                
            // 버튼 선택 시각적 표시
            ColorBlock colors = actionButtons[buttonIndex].colors;
            colors.normalColor = new Color(0.8f, 0.8f, 1f);
            actionButtons[buttonIndex].colors = colors;
            
            // 선택한 버튼 ID 추가
            selectedButtonIds.Add(buttonId);
            
            // 액션 성공 여부 확인
            bool isSuccess = CheckActionSuccess();
            
            // 모든 버튼이 필요한 경우, 모든 버튼이 선택되었는지 확인
            if (requireAllButtons && selectedButtonIds.Count < correctButtonIds.Count)
            {
                // 아직 모든 버튼이 선택되지 않음
                return;
            }
            
            // 성공 또는 실패 처리
            CompleteAction(isSuccess);
        }
        
        /// <summary>
        /// 액션이 성공했는지 확인합니다.
        /// </summary>
        private bool CheckActionSuccess()
        {
            if (requireAllButtons)
            {
                // 모든 올바른 버튼이 선택되어야 함
                foreach (string correctId in correctButtonIds)
                {
                    if (!selectedButtonIds.Contains(correctId))
                    {
                        return false;
                    }
                }
                
                // 추가 버튼이 선택되지 않았는지 확인
                return selectedButtonIds.Count == correctButtonIds.Count;
            }
            else
            {
                // 하나의 올바른 버튼이 선택되면 됨
                foreach (string selectedId in selectedButtonIds)
                {
                    if (correctButtonIds.Contains(selectedId))
                    {
                        return true;
                    }
                }
                
                return false;
            }
        }
        
        /// <summary>
        /// 액션 완료 처리
        /// </summary>
        private void CompleteAction(bool isSuccess)
        {
            // 잘못된 선택 시각적 표시
            if (!isSuccess)
            {
                for (int i = 0; i < actionButtons.Length; i++)
                {
                    string buttonId = i < buttonTexts.Length ? buttonTexts[i].text : actionButtons[i].name;
                    
                    if (selectedButtonIds.Contains(buttonId) && !correctButtonIds.Contains(buttonId))
                    {
                        // 잘못 선택한 버튼 빨간색으로 표시
                        ColorBlock colors = actionButtons[i].colors;
                        colors.normalColor = Color.red;
                        actionButtons[i].colors = colors;
                    }
                    else if (correctButtonIds.Contains(buttonId))
                    {
                        // 올바른 버튼 초록색으로 표시
                        ColorBlock colors = actionButtons[i].colors;
                        colors.normalColor = Color.green;
                        actionButtons[i].colors = colors;
                    }
                    
                    // 모든 버튼 비활성화
                    actionButtons[i].interactable = false;
                }
                
                // 약간의 지연 후 닫기
                Invoke("CloseWithResult", 1.5f);
            }
            else
            {
                // 성공 시 바로 닫기
                CloseWithResult();
            }
        }
        
        /// <summary>
        /// 결과와 함께 팝업을 닫습니다.
        /// </summary>
        private void CloseWithResult()
        {
            bool isSuccess = CheckActionSuccess();
            
            // 페이드 아웃 후 완료 이벤트 발생
            FadeOut(() => {
                OnActionComplete?.Invoke(isSuccess);
            });
        }
        
        /// <summary>
        /// 닫기 버튼 클릭 처리
        /// </summary>
        private void OnCloseButtonClicked()
        {
            // 액션 실패로 처리
            FadeOut(() => {
                OnActionComplete?.Invoke(false);
            });
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
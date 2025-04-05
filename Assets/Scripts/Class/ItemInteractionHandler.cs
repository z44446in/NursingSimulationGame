using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

// BaseInteractionSystem에 정의된 InteractionStep과 InteractionData를 사용하기 위한 참조

/// <summary>
/// 아이템 상호작용을 처리하는 기본 클래스입니다.
/// 다양한 상호작용 유형을 지원하고 상태를 관리합니다.
/// </summary>
public class ItemInteractionHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Image handImage;
    [SerializeField] private GameObject tutorialContainer;
    [SerializeField] private Image tutorialArrow;
    
    [Header("Settings")]
    [SerializeField] private float highlightPulseDuration = 0.5f;
    [SerializeField] private float errorFlashDuration = 0.2f;
    [SerializeField] private Color errorColor = Color.red;
    
    // 현재 아이템
    private Item currentItem;
    
    // 상호작용 상태
    private int currentStepIndex = 0;
    private bool isInteractionActive = false;
    // 이 변수는 HideGuide와 같은 다른 메서드에서 사용될 수 있으므로 속성으로 변경
    public bool IsTutorialActive { get; private set; } = false;
    
    // 튜토리얼 애니메이션
    private Sequence tutorialAnimation;
    
    // 콜백 델리게이트
    public event Action<Item, int> OnStepCompleted;
    public event Action<Item> OnInteractionCompleted;
    public event Action<Item, string, int> OnInteractionError;
    
    // 아이템별 상호작용 단계 정의 (아이템 ID => 단계 목록)
    private Dictionary<string, List<InteractionStep>> itemInteractionSteps = new Dictionary<string, List<InteractionStep>>();
    
    private void Awake()
    {
        // 초기화
        if (tutorialContainer)
        {
            tutorialContainer.SetActive(false);
        }
    }
    
    /// <summary>
    /// 아이템 상호작용을 시작합니다.
    /// </summary>
    public void StartItemInteraction(Item item)
    {
        if (item == null)
        {
            Debug.LogError("Cannot start interaction with null item");
            return;
        }
        
        currentItem = item;
        
        // 아이템 이미지 설정
        if (itemImage != null && item.itemSprite != null)
        {
            itemImage.sprite = item.itemSprite;
            itemImage.gameObject.SetActive(true);
        }
        
        // 아이템을 들고 있는 손 이미지 설정
        if (handImage != null && item.handSprite != null)
        {
            handImage.sprite = item.handSprite;
            handImage.gameObject.SetActive(true);
        }
        
        // 상호작용 ID 확인 (interactionDataId 우선, 없으면 itemId 사용)
        string interactionId = !string.IsNullOrEmpty(item.interactionDataId) ? item.interactionDataId : item.itemId;
        
        // 상호작용 단계 가져오기
        if (!itemInteractionSteps.ContainsKey(interactionId))
        {
            Debug.LogWarning($"상호작용 ID '{interactionId}'가 등록되어 있지 않습니다. 아이템: {item.itemName}");
            return;
        }
        
        currentStepIndex = 0;
        isInteractionActive = true;
        
        // 첫 단계의 튜토리얼 표시
        ShowCurrentStepTutorial();
    }
    
    /// <summary>
    /// 현재 단계의 튜토리얼을 표시합니다.
    /// </summary>
    private void ShowCurrentStepTutorial()
    {
        if (!isInteractionActive || currentItem == null)
            return;
        
        // 상호작용 ID 확인 (interactionDataId 우선, 없으면 itemId 사용)
        string interactionId = !string.IsNullOrEmpty(currentItem.interactionDataId) ? currentItem.interactionDataId : currentItem.itemId;
            
        var steps = itemInteractionSteps[interactionId];
        if (currentStepIndex >= steps.Count)
            return;
            
        var currentStep = steps[currentStepIndex];
        
        // 가이드 텍스트 업데이트 (UIManager에서 처리)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateGuideText(currentStep.guideText);
        }
        
        // 튜토리얼 화살표 표시
        if (tutorialContainer && tutorialArrow && currentStep.tutorialArrowSprite != null)
        {
            tutorialArrow.sprite = currentStep.tutorialArrowSprite;
            tutorialArrow.rectTransform.anchoredPosition = currentStep.tutorialArrowPosition;
            tutorialArrow.rectTransform.eulerAngles = new Vector3(0, 0, currentStep.tutorialArrowRotation);
            
            tutorialContainer.SetActive(true);
            IsTutorialActive = true;
            
            // 화살표 깜빡임 효과 - CanvasGroup을 사용해 페이드 처리
            CanvasGroup canvasGroup = tutorialArrow.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = tutorialArrow.gameObject.AddComponent<CanvasGroup>();
            }
            
            // 기존 애니메이션 중지
            if (tutorialAnimation != null)
            {
                tutorialAnimation.Kill();
            }
            
            // 새 애니메이션 시작
            tutorialAnimation = DOTween.Sequence();
            tutorialAnimation.Append(canvasGroup.DOFade(0.3f, highlightPulseDuration))
                .Append(canvasGroup.DOFade(1f, highlightPulseDuration))
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
    
    /// <summary>
    /// 튜토리얼을 숨깁니다.
    /// </summary>
    private void HideTutorial()
    {
        if (tutorialContainer)
        {
            tutorialContainer.SetActive(false);
            IsTutorialActive = false;
            
            // 애니메이션 정지
            if (tutorialAnimation != null)
            {
                tutorialAnimation.Kill();
                tutorialAnimation = null;
            }
            
            // 알파값 초기화
            CanvasGroup canvasGroup = tutorialArrow?.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
    }
    
    /// <summary>
    /// 드래그 상호작용을 처리합니다.
    /// </summary>
    public void HandleDragInteraction(Vector2 startPosition, Vector2 endPosition, Vector2 dragVector)
    {
        if (!isInteractionActive || currentItem == null)
            return;
            
        // 상호작용 ID 확인 (interactionDataId 우선, 없으면 itemId 사용)
        string interactionId = !string.IsNullOrEmpty(currentItem.interactionDataId) ? currentItem.interactionDataId : currentItem.itemId;
            
        var steps = itemInteractionSteps[interactionId];
        if (currentStepIndex >= steps.Count)
            return;
            
        var currentStep = steps[currentStepIndex];
        
        // 현재 단계가 드래그 상호작용이 아닌 경우
        if (currentStep.interactionType != InteractionType.Drag)
        {
            ShowError("이 단계에서는 드래그를 사용할 수 없습니다.");
            return;
        }
        
        // 드래그 방향 계산
        float dragAngle = Mathf.Atan2(dragVector.y, dragVector.x) * Mathf.Rad2Deg;
        
        // 원하는 방향과의 비교 (허용 오차 범위 내)
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(dragAngle, currentStep.requiredDragAngle));
        
        if (angleDifference <= currentStep.dragAngleTolerance)
        {
            // 드래그 성공
            CompleteCurrentStep();
        }
        else
        {
            // 드래그 방향 오류
            ShowError("올바른 방향으로 드래그하세요.", 5);
        }
    }
    
    /// <summary>
    /// 클릭 상호작용을 처리합니다.
    /// </summary>
    public void HandleClickInteraction(Vector2 clickPosition)
    {
        if (!isInteractionActive || currentItem == null)
            return;
            
        // 상호작용 ID 확인 (interactionDataId 우선, 없으면 itemId 사용)
        string interactionId = !string.IsNullOrEmpty(currentItem.interactionDataId) ? currentItem.interactionDataId : currentItem.itemId;
            
        var steps = itemInteractionSteps[interactionId];
        if (currentStepIndex >= steps.Count)
            return;
            
        var currentStep = steps[currentStepIndex];
        
        // 현재 단계가 클릭 상호작용이 아닌 경우
        if (currentStep.interactionType != InteractionType.SingleClick)
        {
            ShowError("이 단계에서는 클릭을 사용할 수 없습니다.");
            return;
        }
        
        // 클릭 위치가 유효한 영역인지 확인
        if (IsPositionInValidArea(clickPosition, currentStep.validClickArea))
        {
            // 클릭 성공
            CompleteCurrentStep();
        }
        else
        {
            // 클릭 위치 오류
            ShowError("올바른 위치를 클릭하세요.", 5);
        }
    }
    
    /// <summary>
    /// 위치가 유효한 영역 내에 있는지 확인합니다.
    /// </summary>
    private bool IsPositionInValidArea(Vector2 position, Rect validArea)
    {
        return validArea.Contains(position);
    }
    
    /// <summary>
    /// 현재 단계를 완료합니다.
    /// </summary>
    private void CompleteCurrentStep()
    {
        if (!isInteractionActive || currentItem == null)
            return;
            
        // 상호작용 ID 확인 (interactionDataId 우선, 없으면 itemId 사용)
        string interactionId = !string.IsNullOrEmpty(currentItem.interactionDataId) ? currentItem.interactionDataId : currentItem.itemId;
            
        var steps = itemInteractionSteps[interactionId];
        
        // 이벤트 발생
        OnStepCompleted?.Invoke(currentItem, currentStepIndex);
        
        // 튜토리얼 숨기기
        HideTutorial();
        
        // 다음 단계로 진행
        currentStepIndex++;
        
        // 모든 단계 완료 체크
        if (currentStepIndex >= steps.Count)
        {
            CompleteInteraction();
        }
        else
        {
            // 다음 단계 튜토리얼 표시
            ShowCurrentStepTutorial();
        }
    }
    
    /// <summary>
    /// 상호작용을 완료합니다.
    /// </summary>
    private void CompleteInteraction()
    {
        isInteractionActive = false;
        
        // 튜토리얼 숨기기
        HideTutorial();
        
        // 이벤트 발생
        OnInteractionCompleted?.Invoke(currentItem);
        
        // 리소스 정리
        currentItem = null;
        currentStepIndex = 0;
        
        // UI 정리
        if (itemImage != null)
        {
            itemImage.gameObject.SetActive(false);
        }
        if (handImage != null)
        {
            handImage.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 오류를 표시합니다.
    /// </summary>
    private void ShowError(string errorMessage, int penaltyPoints = 0)
    {
        // 에러 이벤트 발생
        OnInteractionError?.Invoke(currentItem, errorMessage, penaltyPoints);
        
        // 화면 테두리 빨간색 깜빡임 (Canvas Group이나 전체 화면 오버레이가 필요)
        Image errorOverlay = GetComponent<Image>();
        if (errorOverlay != null)
        {
            // 원래 색상 저장
            Color originalColor = errorOverlay.color;
            
            // 빨간색으로 2번 깜빡임
            Sequence flashSequence = DOTween.Sequence();
            
            // CanvasGroup을 사용해 깜빡임 구현
            CanvasGroup canvasGroup = errorOverlay.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = errorOverlay.gameObject.AddComponent<CanvasGroup>();
            }
            
            // 원래 상태 저장
            float originalAlpha = canvasGroup.alpha;
            
            // 에러 오버레이의 색상을 에러 색상으로 설정
            errorOverlay.color = errorColor;
            
            // 깜빡임 시퀀스 설정
            flashSequence.Append(canvasGroup.DOFade(0.5f, errorFlashDuration))
                .Append(canvasGroup.DOFade(0f, errorFlashDuration))
                .Append(canvasGroup.DOFade(0.5f, errorFlashDuration))
                .Append(canvasGroup.DOFade(0f, errorFlashDuration))
                .OnComplete(() => {
                    // 원래 상태로 복원
                    errorOverlay.color = originalColor;
                    canvasGroup.alpha = originalAlpha;
                });
        }
    }
    
    /// <summary>
    /// 아이템에 대한 상호작용 단계를 등록합니다.
    /// </summary>
    public void RegisterItemInteractionSteps(string itemId, List<InteractionStep> steps)
    {
        itemInteractionSteps[itemId] = steps;
    }
    
    private void OnDestroy()
    {
        // 애니메이션 정리
        if (tutorialAnimation != null)
        {
            tutorialAnimation.Kill();
            tutorialAnimation = null;
        }
    }
}

// InteractionStep 클래스는 BaseInteractionSystem.cs로 이동되었습니다.
// 이 파일에서는 BaseInteractionSystem에 정의된 InteractionStep 클래스를 사용합니다.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

/// <summary>
/// 모든 상호작용을 처리하는 범용 베이스 클래스
/// 
/// 주요 사용법:
/// 1. ScriptableObject로 GenericInteractionData 생성
/// 2. 아이템에 interactionDataId 설정
/// 3. BaseInteractionSystem이 자동으로 상호작용 처리
/// 
/// 참고: 이전의 ItemInteractionHandler 기능은 이 클래스로 통합되었습니다.
/// </summary>
public class BaseInteractionSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] protected CanvasGroup feedbackPanel;
    [SerializeField] protected Image tutorialArrow;
    [SerializeField] protected Transform popupContainer;
    [SerializeField] protected Image errorBorder;
    [SerializeField] protected GameObject waterEffectPrefab;
    [SerializeField] protected GameObject waterImagePrefab;
    [SerializeField] protected GameObject correctMarkPrefab;
    [SerializeField] protected GameObject wrongMarkPrefab;
    
    [Header("Feedback Settings")]
    [SerializeField] protected AudioClip successSound;
    [SerializeField] protected AudioClip errorSound;
    [SerializeField] protected AudioClip waterPouringSound;
    [SerializeField] protected float feedbackDuration = 0.5f;
    [SerializeField] protected int defaultErrorPenalty = 5;
    
    // 이벤트
    public event Action<string, InteractionEventData> OnInteractionStarted;
    public event Action<string, InteractionEventData> OnInteractionCompleted;
    public event Action<string, InteractionEventData, string> OnInteractionError;
    public event Action<string> OnMultiStageDragCompleted;
    public event Action<string, bool> OnConditionalTouchHandled;
    
    // 오디오 컴포넌트
    protected AudioSource audioSource;
    
    // 상호작용 데이터
    protected Dictionary<string, InteractionData> interactionsDatabase = new Dictionary<string, InteractionData>();
    
    // 현재 진행 중인 상호작용
    protected string currentInteractionId;
    protected int currentStepIndex = 0;
    protected bool isInteractionActive = false;
    protected int currentDragStage = 0;
    protected bool isTouchDisabled = false;
    
    // 생성된 초기 오브젝트 캐시
    protected Dictionary<string, GameObject> initialObjectsCache = new Dictionary<string, GameObject>();
    
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
    /// 등록된 상호작용 데이터를 가져옵니다.
    /// </summary>
    /// <param name="id">상호작용 ID</param>
    /// <returns>상호작용 데이터, 없으면 null</returns>
    public virtual InteractionData GetInteractionData(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
            
        if (interactionsDatabase.TryGetValue(id, out InteractionData data))
        {
            return data;
        }
        
        return null;
    }
    
    /// <summary>
    /// 초기 오브젝트 생성 처리
    /// </summary>
    /// <param name="interactionId">상호작용 ID</param>
    public virtual void CreateInitialObjects(string interactionId)
    {
        if (!interactionsDatabase.ContainsKey(interactionId))
        {
            Debug.LogWarning($"상호작용 ID '{interactionId}'가 등록되어 있지 않습니다.");
            return;
        }
            
        var data = interactionsDatabase[interactionId];
        if (data.steps.Count == 0)
            return;
        
        Debug.Log($"초기 오브젝트 생성을 시작합니다. 상호작용: {interactionId}");
        
        // popupContainer가 없으면 생성
        if (popupContainer == null)
        {
            Debug.LogWarning("popupContainer가 설정되지 않았습니다. 자동으로 생성합니다.");
            
            // 게임 오브젝트 찾기
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject container = new GameObject("InteractionPopupContainer");
                container.transform.SetParent(canvas.transform, false);
                
                RectTransform rectTransform = container.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                popupContainer = container.transform;
            }
            else
            {
                Debug.LogError("Canvas를 찾을 수 없어 팝업 컨테이너를 생성할 수 없습니다.");
                return;
            }
        }
        
        try
        {
            // 통합된 두 가지 방식을 지원
            
            // 1. 새로운 InteractionDataAsset 방식 - 직접 인터랙션 데이터베이스에서 찾기
            InteractionDataAsset interactionDataAsset = null;
            
            // 리소스에서 찾기
            try {
                interactionDataAsset = Resources.Load<InteractionDataAsset>($"Interactions/{interactionId}");
            } catch (System.Exception) {
                // 리소스에서 찾지 못하면 무시
            }
            
            // 기존 초기 오브젝트 정리
            foreach (var kvp in initialObjectsCache.ToList())
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value);
                }
            }
            initialObjectsCache.Clear();
            
            // 새로운 방식: InteractionDataAsset을 직접 사용
            if (interactionDataAsset != null)
            {
                // 먼저 메인 초기 오브젝트 처리
                if (interactionDataAsset.initialObjects != null && interactionDataAsset.initialObjects.Count > 0)
                {
                    Debug.Log($"상호작용 레벨에서 {interactionDataAsset.initialObjects.Count}개의 초기 오브젝트 생성");
                    
                    foreach (var objData in interactionDataAsset.initialObjects)
                    {
                        if (objData == null) continue;
                        
                        GameObject newObj = CreateInitialObject(objData);
                    }
                }
                
                // 각 단계의 초기 오브젝트 처리
                foreach (var step in interactionDataAsset.steps)
                {
                    if (step.createInitialObjects && step.initialObjects != null && step.initialObjects.Count > 0)
                    {
                        Debug.Log($"단계 {step.stepName}에서 {step.initialObjects.Count}개의 초기 오브젝트 생성");
                        
                        foreach (var objData in step.initialObjects)
                        {
                            if (objData == null) continue;
                            
                            GameObject newObj = CreateInitialObject(objData);
                        }
                    }
                }
            }
            // 데이터베이스에서 직접 가져오기
            else
            {
                // 데이터가 이미 등록되어 있는지 확인
                if (interactionsDatabase.ContainsKey(interactionId) && interactionsDatabase[interactionId].steps.Count > 0)
                {
                    var registeredData = interactionsDatabase[interactionId];
                    
                    // 각 단계의 초기 오브젝트 확인 (기본 구현 호환성)
                    foreach (var step in registeredData.steps)
                    {
                        if (step.createInitialObjects)
                        {
                            Debug.Log($"데이터베이스에서 단계의 초기 오브젝트 생성 시도");
                            // 이 부분은 InteractionStep 클래스에 초기 오브젝트 목록이 없어서 처리 불가
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"상호작용 데이터를 찾을 수 없습니다: {interactionId}");
                    return;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"초기 오브젝트 생성 중 오류 발생: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 단일 초기 오브젝트를 생성합니다.
    /// </summary>
    protected virtual GameObject CreateInitialObject(InitialObjectData objData)
    {
        if (objData == null || popupContainer == null)
            return null;
            
        // 이미 같은 ID로 오브젝트가 존재하면 제거
        if (!string.IsNullOrEmpty(objData.objectId) && 
            initialObjectsCache.TryGetValue(objData.objectId, out GameObject existingObj) && 
            existingObj != null)
        {
            Destroy(existingObj);
        }
        
        GameObject newObj = null;
        
        // 커스텀 프리팹 사용 여부에 따라 오브젝트 생성
        if (objData.useCustomPrefab && objData.customPrefab != null)
        {
            // 커스텀 프리팹 인스턴스화
            newObj = Instantiate(objData.customPrefab, popupContainer);
        }
        else
        {
            // 기본 게임 오브젝트 생성
            newObj = new GameObject(objData.objectName);
            newObj.transform.SetParent(popupContainer, false);
            
            // 이미지 컴포넌트 추가
            Image image = newObj.AddComponent<Image>();
            if (objData.objectSprite != null)
            {
                image.sprite = objData.objectSprite;
            }
        }
        
        // 기본 속성 설정
        newObj.name = objData.objectName;
        if (!string.IsNullOrEmpty(objData.tag))
        {
            newObj.tag = objData.tag;
        }
        
        // 위치, 회전, 크기 설정
        RectTransform rectTransform = newObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = objData.position;
            rectTransform.eulerAngles = objData.rotation;
            rectTransform.localScale = objData.scale;
            
            // 스프라이트가 있는 경우 이미지 크기 설정
            if (!objData.useCustomPrefab && objData.objectSprite != null)
            {
                try
                {
                    rectTransform.sizeDelta = new Vector2(
                        objData.objectSprite.rect.width,
                        objData.objectSprite.rect.height
                    );
                }
                catch
                {
                    // 기본 크기 설정
                    rectTransform.sizeDelta = new Vector2(100, 100);
                }
            }
        }
        
        // 캐시에 저장
        if (!string.IsNullOrEmpty(objData.objectId))
        {
            initialObjectsCache[objData.objectId] = newObj;
        }
        
        Debug.Log($"초기 오브젝트 '{objData.objectName}' 생성 완료");
        return newObj;
    }
    
    /// <summary>
    /// 상호작용 시작
    /// </summary>
    /// <param name="interactionId">시작할 상호작용의 ID</param>
    /// <param name="eventData">상호작용 이벤트 데이터</param>
    public virtual bool StartInteraction(string interactionId, InteractionEventData eventData = null)
    {
        // 이미 등록된 상호작용인지 확인
        if (!interactionsDatabase.ContainsKey(interactionId))
        {
            // 등록되지 않은 경우, InteractionDataAsset으로 로드 시도
            InteractionDataAsset interactionDataAsset = Resources.Load<InteractionDataAsset>($"Interactions/{interactionId}");
            
            if (interactionDataAsset != null)
            {
                // InteractionDataAsset을 InteractionData로 변환하여 등록
                InteractionData data = new InteractionData
                {
                    id = interactionDataAsset.id,
                    name = interactionDataAsset.displayName,
                    description = interactionDataAsset.description,
                    steps = new List<InteractionStep>()
                };
                
                // 각 단계를 변환
                foreach (var stepData in interactionDataAsset.steps)
                {
                    InteractionStep step = new InteractionStep
                    {
                        actionId = stepData.stepId,
                        interactionType = stepData.interactionType,
                        guideText = stepData.guideText,
                        // 기타 필요한 속성 복사
                        createInitialObjects = stepData.createInitialObjects,
                        useMultiStageDrag = stepData.useMultiStageDrag,
                        requiredDragAngle = stepData.requiredDragAngle,
                        dragAngleTolerance = stepData.dragAngleTolerance,
                        dragDistance = stepData.dragDistance,
                        successMessage = stepData.successMessage,
                        errorMessage = stepData.errorMessage,
                        showErrorBorderFlash = stepData.showErrorBorderFlash,
                        createWaterEffect = stepData.createWaterEffect,
                        waterEffectPosition = stepData.waterEffectPosition
                    };
                    
                    data.steps.Add(step);
                }
                
                // 등록
                RegisterInteraction(interactionId, data);
                Debug.Log($"상호작용 '{interactionId}'를 InteractionDataAsset에서 로드하여 등록했습니다.");
            }
            else
            {
                Debug.LogWarning($"상호작용 ID '{interactionId}'가 등록되어 있지 않고, 리소스에서도 찾을 수 없습니다.");
                return false;
            }
        }
        
        // 이미 진행 중인 상호작용이 있으면 중지
        if (isInteractionActive)
        {
            StopInteraction();
        }
        
        currentInteractionId = interactionId;
        currentStepIndex = 0;
        currentDragStage = 0;
        isInteractionActive = true;
        isTouchDisabled = false;
        
        // 초기 오브젝트 생성
        CreateInitialObjects(interactionId);
        
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
    
    /// <summary>
    /// 다중 단계 드래그 처리
    /// </summary>
    public virtual void HandleMultiStageDrag(Vector2 start, Vector2 end, Vector2 direction)
    {
        if (!isInteractionActive || !interactionsDatabase.ContainsKey(currentInteractionId))
            return;
            
        if (isTouchDisabled)
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
        
        // GenericInteractionStep의 정보를 갖고 있지 않으므로 실제 구현에서는 다르게 처리해야 합니다.
        // 멀티 드래그 스테이지 정보 확인
        
        // 드래그 각도 계산
        float dragAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (dragAngle < 0) dragAngle += 360f;
        
        // 허용 오차 범위 확인
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(dragAngle, step.requiredDragAngle));
        
        if (angleDifference <= step.dragAngleTolerance)
        {
            // 각 단계별 처리
            currentDragStage++;
            
            // 완료 이벤트 발생
            OnMultiStageDragCompleted?.Invoke(step.guideText);
            
            // 모든 단계가 완료되었는지 확인
            if (currentDragStage >= 2) // 예시로 2단계
            {
                // 성공
                CompleteCurrentStep();
                currentDragStage = 0;
            }
            else
            {
                // 다음 드래그 단계 가이드 표시
                UpdateDragStageGuide();
            }
        }
        else
        {
            // 오류: 잘못된 드래그 방향
            ShowError("올바른 방향으로 드래그하세요.");
        }
    }
    
    /// <summary>
    /// 드래그 단계 가이드 업데이트
    /// </summary>
    protected virtual void UpdateDragStageGuide()
    {
        // 실제 구현에서는 GenericInteractionStep의 정보를 이용합니다.
        if (tutorialArrow != null)
        {
            // 화살표 위치와 회전 업데이트
            if (currentDragStage == 1)
            {
                // 두 번째 드래그에 대한 정보 설정
                tutorialArrow.rectTransform.eulerAngles = new Vector3(0, 0, 180); // 왼쪽 방향
                tutorialArrow.gameObject.SetActive(true);
                
                // 깜빡임 효과
                CanvasGroup canvasGroup = tutorialArrow.gameObject.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    DOTween.Kill(canvasGroup);
                    canvasGroup.DOFade(0.4f, 0.5f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            }
        }
    }
    
    /// <summary>
    /// 조건부 터치 처리
    /// </summary>
    public virtual void HandleConditionalTouch(Vector2 position)
    {
        if (!isInteractionActive || !interactionsDatabase.ContainsKey(currentInteractionId))
            return;
            
        if (isTouchDisabled)
            return;
            
        var data = interactionsDatabase[currentInteractionId];
        if (currentStepIndex >= data.steps.Count)
            return;
            
        var step = data.steps[currentStepIndex];
        
        // 현재 단계가 조건부 터치인지 확인
        if (step.interactionType != InteractionType.SingleClick)
        {
            ShowError("이 단계에서는 터치를 사용할 수 없습니다.");
            return;
        }
        
        // GenericInteractionStep의 정보를 갖고 있지 않으므로 실제 구현에서는 다르게 처리해야 합니다.
        // 여기서는 예시 코드만 작성
        
        // 터치된 오브젝트 찾기
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(position), Vector2.zero);
        if (hit.collider != null)
        {
            string tag = hit.collider.tag;
            bool isCorrect = false;
            
            // 태그가 일치하는지 확인 (예: "TrashBin"은 올바른 선택)
            if (tag == "TrashBin" || tag == "MedicineContainer")
            {
                isCorrect = true;
                
                // 성공 시 효과
                ShowSuccessFeedback();
                PlaySound(successSound);
                
                // 물 효과 애니메이션
                ShowWaterEffect(position);
                
                // 다음 단계로 진행
                CompleteCurrentStep();
            }
            else
            {
                // 실패 시 효과
                ShowErrorFeedback();
                PlaySound(errorSound);
                
                // 오류 메시지
                ShowError("올바른 위치를 터치하세요.");
                
                // 오류 테두리 효과
                ShowErrorBorderFlash();
            }
            
            // 이벤트 발생
            OnConditionalTouchHandled?.Invoke(tag, isCorrect);
        }
    }
    
    /// <summary>
    /// 오류 테두리 깜빡임 효과
    /// </summary>
    protected virtual void ShowErrorBorderFlash()
    {
        if (errorBorder == null)
            return;
            
        // 초기 설정
        errorBorder.color = new Color(1f, 0f, 0f, 0f);
        errorBorder.gameObject.SetActive(true);
        
        // 깜빡임 시퀀스
        Sequence sequence = DOTween.Sequence();
        sequence.Append(errorBorder.DOFade(0.3f, 0.2f));
        sequence.Append(errorBorder.DOFade(0f, 0.2f));
        sequence.Append(errorBorder.DOFade(0.3f, 0.2f));
        sequence.Append(errorBorder.DOFade(0f, 0.2f));
        sequence.OnComplete(() => {
            errorBorder.gameObject.SetActive(false);
        });
    }
    
    /// <summary>
    /// 물 효과 표시
    /// </summary>
    protected virtual void ShowWaterEffect(Vector2 position)
    {
        if (waterEffectPrefab == null)
            return;
            
        // 물 효과 생성
        GameObject waterEffect = Instantiate(waterEffectPrefab, popupContainer);
        waterEffect.transform.position = position;
        
        // 물 효과음 재생
        if (waterPouringSound != null)
        {
            PlaySound(waterPouringSound);
        }
        
        // 일정 시간 후 제거
        Destroy(waterEffect, 1.5f);
    }
    
    /// <summary>
    /// 오브젝트에 물 이미지 생성
    /// </summary>
    protected virtual void CreateWaterImageOnObject(GameObject targetObject)
    {
        if (waterImagePrefab == null || targetObject == null)
            return;
            
        // 물 이미지 생성
        GameObject waterImage = Instantiate(waterImagePrefab, targetObject.transform);
        waterImage.transform.localPosition = Vector3.zero;
    }
    
    /// <summary>
    /// 터치 일시적 비활성화
    /// </summary>
    protected virtual void DisableTouch(float duration)
    {
        isTouchDisabled = true;
        
        // 지정된 시간 후 다시 활성화
        StartCoroutine(EnableTouchAfterDelay(duration));
    }
    
    /// <summary>
    /// 지정된 시간 후 터치 다시 활성화
    /// </summary>
    protected IEnumerator EnableTouchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTouchDisabled = false;
    }
    
    /// <summary>
    /// 정답 표시 이미지
    /// </summary>
    protected virtual void ShowCorrectAnswerFeedback()
    {
        if (correctMarkPrefab == null || popupContainer == null)
            return;
            
        GameObject correctMark = Instantiate(correctMarkPrefab, popupContainer);
        correctMark.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        
        // 일정 시간 후 제거
        Destroy(correctMark, 1.5f);
    }
    
    /// <summary>
    /// 오답 표시 이미지
    /// </summary>
    protected virtual void ShowWrongAnswerFeedback()
    {
        if (wrongMarkPrefab == null || popupContainer == null)
            return;
            
        GameObject wrongMark = Instantiate(wrongMarkPrefab, popupContainer);
        wrongMark.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        
        // 일정 시간 후 제거
        Destroy(wrongMark, 1.5f);
    }
    
    /// <summary>
    /// 오류 기록
    /// </summary>
    protected virtual void RecordError(string errorMessage, int penaltyPoints = 0)
    {
        if (penaltyPoints == 0)
        {
            penaltyPoints = defaultErrorPenalty;
        }
        
        // TODO: 오류 데이터베이스에 기록
        Debug.LogWarning($"오류 기록: {errorMessage}, 감점: {penaltyPoints}");
        
        // GameManager에 점수 감점 요청
        if (GameManager.Instance != null)
        {
            // GameManager에 점수 관련 메서드가 있다고 가정
            // GameManager.Instance.DeductScore(penaltyPoints);
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
        
        if (errorBorder != null)
        {
            DOTween.Kill(errorBorder);
        }
        
        // 생성된 초기 오브젝트 정리
        foreach (var obj in initialObjectsCache.Values)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        initialObjectsCache.Clear();
    }
    
    /// <summary>
    /// 팝업 컨테이너를 설정합니다.
    /// </summary>
    /// <param name="container">팝업이 생성될 부모 Transform</param>
    public virtual void SetPopupContainer(Transform container)
    {
        if (container == null)
        {
            Debug.LogWarning("Null container was provided to SetPopupContainer");
            return;
        }
        
        popupContainer = container;
        Debug.Log($"팝업 컨테이너가 설정되었습니다: {container.name}");
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
    public string actionId; // 행동 ID
    public InteractionType interactionType;
    public string guideText;
    
    // 순서 관련
    public bool isOrderImportant = true; // 순서가 중요한지 여부
    
    // 초기 오브젝트 생성 관련
    public bool createInitialObjects = false;
    
    // 다중 단계 드래그 관련
    public bool useMultiStageDrag = false;
    public int totalDragStages = 2;
    
    // 조건부 터치 관련
    public bool useConditionalTouch = false;
    public string[] validTouchTags;
    public string[] correctTouchTags;
    
    // 드래그 관련
    public float requiredDragAngle;
    public float dragAngleTolerance = 30f;
    public float dragDistance = 100f;
    
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
    public bool showErrorBorderFlash = false;
    public float disableTouchDuration = 0f;
    public string errorEntryText = "";
    
    // 시각 효과 관련
    public bool createWaterEffect = false;
    public Vector2 waterEffectPosition;
    public bool createWaterImageOnObject = false;
    
    // 필수 아이템 관련
    public List<ItemRequirement> requiredItems = new List<ItemRequirement>();
    
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
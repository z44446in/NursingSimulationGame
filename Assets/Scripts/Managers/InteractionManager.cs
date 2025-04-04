using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using DG.Tweening;

/// <summary>
/// InteractionManager: 상호작용과 아이템 카트를 관리하는 클래스.
/// </summary>
public class InteractionManager : MonoBehaviour
{
    private static InteractionManager instance;

    /// <summary>
    /// InteractionManager 싱글톤 인스턴스에 접근.
    /// </summary>
    public static InteractionManager Instance => instance;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.white; // 하이라이트 색상

    [Header("UI References")]
    [SerializeField] private Transform popupContainer;
    [SerializeField] private GameObject smallPopupPrefab;
    [SerializeField] private GameObject quizPopupPrefab;
    [SerializeField] private Image errorOverlay;

    [Header("Tutorial Settings")]
    [SerializeField] private TutorialArrowSystem tutorialArrowSystem;

    [Header("Feedback Settings")]
    [SerializeField] private Color errorColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float errorFlashDuration = 0.2f;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private AudioClip successSound;

    [Header("Item Interaction")]
    [SerializeField] private List<ItemInteractionData> itemInteractionDatabase = new List<ItemInteractionData>();
    
    // 컴포넌트 참조
    private AudioSource audioSource;
    private DragGestureDetector dragDetector;
    private BaseInteractionSystem interactionSystem;

    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>(); // 원본 재질 저장

    // 카트 관련
    private List<Item> cartItems = new List<Item>();
    public event Action<Item> OnItemAddedToCart;
    public event Action<Item> OnItemRemovedFromCart;

    // 현재 들고 있는 아이템
    private Item currentHeldItem;
    public Item CurrentHeldItem => currentHeldItem;

    // 상호작용 관련
    private Dictionary<string, List<InteractionStep>> itemInteractionStepsDatabase = new Dictionary<string, List<InteractionStep>>();
    
    // 오류 로깅 및 점수 관리
    private List<string> errorLog = new List<string>();
    
    // 오류 이벤트
    public event Action<string, int> OnInteractionError;
    public event Action<Item, int> OnInteractionStepCompleted;
    public event Action<Item> OnInteractionCompleted;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 컴포넌트 초기화
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            dragDetector = GetComponent<DragGestureDetector>();
            if (dragDetector == null)
            {
                dragDetector = gameObject.AddComponent<DragGestureDetector>();
            }
            
            interactionSystem = GetComponent<BaseInteractionSystem>();
            if (interactionSystem == null)
            {
                interactionSystem = gameObject.AddComponent<BaseInteractionSystem>();
                
                // 팝업 컨테이너 설정
                if (popupContainer != null)
                {
                    interactionSystem.SetPopupContainer(popupContainer);
                }
            }
            
            // 아이템 상호작용 데이터베이스 초기화
            InitializeInteractionDatabase();
            
            // 이벤트 구독
            if (dragDetector != null)
            {
                dragDetector.OnDragCompleted += HandleDragGesture;
                dragDetector.OnDragCancelled += HandleDragCancelled;
            }
            
            if (interactionSystem != null)
            {
                interactionSystem.OnInteractionStarted += HandleInteractionStarted;
                interactionSystem.OnInteractionCompleted += HandleInteractionCompleted;
                interactionSystem.OnInteractionError += HandleInteractionError;
            }
            
            // 에러 오버레이 초기화
            if (errorOverlay != null)
            {
                Color color = errorOverlay.color;
                color.a = 0f;
                errorOverlay.color = color;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 아이템 상호작용 데이터베이스를 초기화합니다.
    /// </summary>
    private void InitializeInteractionDatabase()
    {
        // 인스펙터에서 설정된 아이템 상호작용 데이터를 등록
        foreach (var interactionData in itemInteractionDatabase)
        {
            RegisterItemInteraction(interactionData.itemId, interactionData.interactionSteps);
        }
    }

    /// <summary>
    /// 아이템 상호작용을 데이터베이스에 등록합니다.
    /// </summary>
    public void RegisterItemInteraction(string itemId, List<InteractionStep> steps)
    {
        itemInteractionStepsDatabase[itemId] = steps;
        
        // BaseInteractionSystem에 등록
        if (interactionSystem != null)
        {
            InteractionData data = new InteractionData
            {
                id = itemId,
                name = itemId,
                description = $"Interaction for {itemId}",
                steps = steps
            };
            interactionSystem.RegisterInteraction(itemId, data);
        }
    }

    /// <summary>
    /// interactionId로 상호작용을 시작합니다. (ProcedureManager에서 사용)
    /// </summary>
    public void StartInteraction(string interactionId)
    {
        // interactionId를 사용하여 베이스 인터랙션 시스템을 통해 상호작용 시작
        if (interactionSystem != null && !string.IsNullOrEmpty(interactionId))
        {
            // 상호작용 ID로 직접 인터랙션 시스템에 명령 전달
            interactionSystem.StartInteraction(interactionId);
            
            // 가이드 텍스트 업데이트 시도
            if (UIManager.Instance != null && itemInteractionStepsDatabase.ContainsKey(interactionId) && 
                itemInteractionStepsDatabase[interactionId].Count > 0)
            {
                var firstStep = itemInteractionStepsDatabase[interactionId][0];
                UIManager.Instance.UpdateGuideText(firstStep.guideText);
            }
        }
        else
        {
            Debug.LogWarning($"StartInteraction failed: interactionSystem is null or interactionId is empty");
        }
    }
    
    /// <summary>
    /// 아이템 상호작용을 시작합니다.
    /// </summary>
    public void StartItemInteraction(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("Cannot start interaction with null item");
            return;
        }
        
        string interactionId = !string.IsNullOrEmpty(item.interactionDataId) ? item.interactionDataId : item.itemId;
        
        // 첫 번째 시도: 등록된 상호작용 데이터베이스에서 검색
        if (itemInteractionStepsDatabase.ContainsKey(interactionId))
        {
            // 상호작용 시작
            interactionSystem.StartInteraction(interactionId);
            
            // 가이드 텍스트 업데이트
            if (UIManager.Instance != null)
            {
                var firstStep = itemInteractionStepsDatabase[interactionId][0];
                UIManager.Instance.UpdateGuideText(firstStep.guideText);
            }
            
            return;
        }
        
        // 두 번째 시도: InteractionDataAsset을 리소스에서 로드
        InteractionDataAsset interactionAsset = Resources.Load<InteractionDataAsset>($"Interactions/{interactionId}");
        if (interactionAsset != null && interactionAsset.steps.Count > 0)
        {
            // InteractionData로 변환하여 등록
            List<InteractionStep> convertedSteps = interactionAsset.steps
                .Select(step => new InteractionStep
                {
                    actionId = step.stepId,
                    interactionType = step.interactionType,
                    guideText = step.guideText,
                    // 기타 필요한 속성 복사
                })
                .ToList();
                
            // 상호작용 시스템에 등록
            RegisterItemInteraction(interactionId, convertedSteps);
            
            // 상호작용 시작
            interactionSystem.StartInteraction(interactionId);
            
            // 가이드 텍스트 업데이트
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateGuideText(convertedSteps[0].guideText);
            }
            
            return;
        }
        
        // 세 번째 시도: 빈 InteractionData 생성 (상호작용 없음)
        if (!string.IsNullOrEmpty(interactionId))
        {
            Debug.LogWarning($"No interaction defined for {item.itemName}, creating empty interaction");
            
            List<InteractionStep> emptySteps = new List<InteractionStep>
            {
                new InteractionStep
                {
                    actionId = "default",
                    interactionType = InteractionType.SingleClick,
                    guideText = $"{item.itemName} 사용하기"
                }
            };
            
            RegisterItemInteraction(interactionId, emptySteps);
            interactionSystem.StartInteraction(interactionId);
            
            // 가이드 텍스트 업데이트
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateGuideText(emptySteps[0].guideText);
            }
        }
        else
        {
            Debug.LogWarning($"Cannot start interaction for {item.itemName}, no valid interaction ID");
        }
    }

    /// <summary>
    /// 드래그 제스처를 처리합니다.
    /// </summary>
    private void HandleDragGesture(Vector2 start, Vector2 end, Vector2 direction)
    {
        // 드래그 상호작용 처리
        interactionSystem.HandleDragInteraction(start, end, direction);
    }

    /// <summary>
    /// 드래그 취소를 처리합니다.
    /// </summary>
    private void HandleDragCancelled()
    {
        // 구현할 내용 있으면 추가
    }

    /// <summary>
    /// 상호작용 시작을 처리합니다.
    /// </summary>
    private void HandleInteractionStarted(string interactionId, InteractionEventData eventData)
    {
        // 현재 들고 있는, 또는 현재 처리 중인 아이템과 연관된 상호작용인지 확인
        if (currentHeldItem != null && 
            (currentHeldItem.interactionDataId == interactionId || 
             currentHeldItem.itemId == interactionId))
        {
            // 이벤트 발생 (단계별 처리를 위해 0 전달)
            OnInteractionStepCompleted?.Invoke(currentHeldItem, 0);
        }
    }

    /// <summary>
    /// 상호작용 완료를 처리합니다.
    /// </summary>
    private void HandleInteractionCompleted(string interactionId, InteractionEventData eventData)
    {
        // 효과음 재생
        PlaySound(successSound);
        
        // 현재 들고 있는 아이템과 연관된 상호작용인지 확인
        if (currentHeldItem != null && 
            (currentHeldItem.interactionDataId == interactionId || 
             currentHeldItem.itemId == interactionId))
        {
            // 이벤트 발생
            OnInteractionCompleted?.Invoke(currentHeldItem);
        }
    }

    /// <summary>
    /// 상호작용 오류를 처리합니다.
    /// </summary>
    private void HandleInteractionError(string interactionId, InteractionEventData eventData, string errorMessage)
    {
        // 오류 시각 효과
        ShowErrorFlash();
        
        // 효과음 재생
        PlaySound(errorSound);
        
        Item errorItem = currentHeldItem;
        int penaltyPoints = 5; // 기본 패널티

        // 오류 로그에 추가
        string fullErrorMessage = errorItem != null 
            ? $"{errorItem.itemName}: {errorMessage}" 
            : $"상호작용 오류: {errorMessage}";
        errorLog.Add(fullErrorMessage);
        
        // 작은 팝업 표시
        ShowSmallPopup("간호사", errorMessage);
        
        // 이벤트 발생
        OnInteractionError?.Invoke(errorMessage, penaltyPoints);
    }

    /// <summary>
    /// 오류 플래시 효과를 표시합니다.
    /// </summary>
    private void ShowErrorFlash()
    {
        if (errorOverlay == null)
            return;
            
        // CanvasGroup을 통한 페이드 구현
        CanvasGroup canvasGroup = errorOverlay.gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = errorOverlay.gameObject.AddComponent<CanvasGroup>();
        }
        
        // 에러 색상 설정
        errorOverlay.color = errorColor;
            
        // 깜빡임 시퀀스
        Sequence flashSequence = DOTween.Sequence();
        flashSequence.Append(canvasGroup.DOFade(errorColor.a, errorFlashDuration));
        flashSequence.Append(canvasGroup.DOFade(0f, errorFlashDuration));
        flashSequence.Append(canvasGroup.DOFade(errorColor.a, errorFlashDuration));
        flashSequence.Append(canvasGroup.DOFade(0f, errorFlashDuration));
    }

    /// <summary>
    /// 효과음을 재생합니다.
    /// </summary>
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// 작은 팝업을 표시합니다.
    /// </summary>
    private void ShowSmallPopup(string character, string message)
    {
        if (popupContainer == null || smallPopupPrefab == null)
            return;
            
        var popup = Instantiate(smallPopupPrefab, popupContainer).GetComponent<SmallPopup>();
        if (popup != null)
        {
            // character는 현재 무시하고 메시지만 전달 (자동 닫기 활성화)
            popup.Initialize(message, true);
        }
    }

    /// <summary>
    /// 퀴즈 팝업을 표시합니다.
    /// </summary>
    public void ShowQuizPopup(string question, List<string> options, int correctIndex, System.Action<bool> onComplete)
    {
        if (popupContainer == null || quizPopupPrefab == null)
            return;
            
        var popup = Instantiate(quizPopupPrefab, popupContainer).GetComponent<QuizPopup>();
        if (popup != null)
        {
            // 기본 타임 리밋 10초로 설정
            float timeLimit = 10f;
            popup.Initialize(question, options.ToArray(), correctIndex, timeLimit, onComplete);
        }
    }

    /// <summary>
    /// 오브젝트 하이라이트를 처리.
    /// </summary>
    public void HighlightObject(GameObject obj, bool highlight)
    {
        if (obj == null)
        {
            Debug.LogWarning("Attempted to highlight a null object.");
            return;
        }

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (highlight)
            {
                ApplyHighlight(renderer);
            }
            else
            {
                RestoreOriginalMaterials(renderer);
            }
        }
    }

    /// <summary>
    /// 하이라이트 효과를 적용.
    /// </summary>
    private void ApplyHighlight(Renderer renderer)
    {
        if (!originalMaterials.ContainsKey(renderer))
        {
            originalMaterials[renderer] = renderer.materials;

            // 모든 재질에 하이라이트 효과 적용
            foreach (var material in renderer.materials)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", highlightColor);
            }
        }
    }

    /// <summary>
    /// 오브젝트의 원래 재질로 복원.
    /// </summary>
    private void RestoreOriginalMaterials(Renderer renderer)
    {
        if (originalMaterials.ContainsKey(renderer))
        {
            renderer.materials = originalMaterials[renderer];
            foreach (var material in renderer.materials)
            {
                material.DisableKeyword("_EMISSION");
            }
            originalMaterials.Remove(renderer);
        }
    }

    /// <summary>
    /// 아이템을 카트에 추가.
    /// </summary>
    public bool AddItemToCart(Item item)
    {
        if (item == null)
        {
            Debug.LogWarning("Attempted to add a null item to the cart.");
            return false;
        }

        cartItems.Add(item);
        OnItemAddedToCart?.Invoke(item);
        
        return true;
    }

    /// <summary>
    /// 아이템을 카트에서 제거.
    /// </summary>
    public bool RemoveItemFromCart(Item item)
    {
        if (item == null || !cartItems.Contains(item))
        {
            Debug.LogWarning($"Attempted to remove a non-existent item: {item?.itemName}");
            return false;
        }

        cartItems.Remove(item);
        OnItemRemovedFromCart?.Invoke(item);
        
        return true;
    }

    /// <summary>
    /// 현재 카트에 있는 아이템 리스트를 반환.
    /// </summary>
    public List<Item> GetCartItems()
    {
        return new List<Item>(cartItems);
    }

    /// <summary>
    /// 카트를 초기화.
    /// </summary>
    public void ClearCart()
    {
        cartItems.Clear();
        Debug.Log("Cart cleared.");
    }

    /// <summary>
    /// 아이템을 들기.
    /// </summary>
    public void PickupItem(Item item)
    {
        if (currentHeldItem != null)
        {
            Debug.LogWarning("Cannot pick up item while holding another.");
            return;
        }

        currentHeldItem = item;
        UpdateCursorWithItem(item);
        
        // 상호작용 시작
        StartItemInteraction(item);
    }

    /// <summary>
    /// 아이템을 내려놓기.
    /// </summary>
    public void DropItem()
    {
        currentHeldItem = null;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Debug.Log("Dropped current held item.");
    }

    /// <summary>
    /// 아이템의 커서를 업데이트.
    /// </summary>
    private void UpdateCursorWithItem(Item item)
    {
        if (item?.itemSprite != null)
        {
            Cursor.SetCursor(TextureFromSprite(item.itemSprite), Vector2.zero, CursorMode.Auto);
        }
    }

    /// <summary>
    /// Sprite를 Texture로 변환.
    /// </summary>
    private Texture2D TextureFromSprite(Sprite sprite)
    {
        if (sprite.texture.isReadable)
        {
            return sprite.texture;
        }

        Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
        texture.SetPixels(sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height));
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// 오류 로그를 가져옵니다.
    /// </summary>
    public List<string> GetErrorLog()
    {
        return new List<string>(errorLog);
    }

    /// <summary>
    /// 오류 로그를 지웁니다.
    /// </summary>
    public void ClearErrorLog()
    {
        errorLog.Clear();
    }

    /// <summary>
    /// 파괴 시 자원 해제.
    /// </summary>
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (dragDetector != null)
        {
            dragDetector.OnDragCompleted -= HandleDragGesture;
            dragDetector.OnDragCancelled -= HandleDragCancelled;
        }
        
        if (interactionSystem != null)
        {
            interactionSystem.OnInteractionStarted -= HandleInteractionStarted;
            interactionSystem.OnInteractionCompleted -= HandleInteractionCompleted; 
            interactionSystem.OnInteractionError -= HandleInteractionError;
        }
        
        // 하이라이트 복원
        foreach (var renderer in originalMaterials.Keys)
        {
            if (renderer != null)
            {
                RestoreOriginalMaterials(renderer);
            }
        }
        originalMaterials.Clear();

        // 커서 복원
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}

/// <summary>
/// 아이템 상호작용 데이터
/// </summary>
[System.Serializable]
public class ItemInteractionData
{
    public string itemId;
    public List<InteractionStep> interactionSteps = new List<InteractionStep>();
}
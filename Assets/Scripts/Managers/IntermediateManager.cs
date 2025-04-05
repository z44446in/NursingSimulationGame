using UnityEngine;
using System.Linq; 
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;  // List 사용을 위한 using 추가
using DG.Tweening;

public class IntermediateManager : MonoBehaviour
{
   

    [Header("Screen References")]
    [SerializeField] private GameObject intermediateScreen;
    [SerializeField] private GameObject gameScreen;
    

    [Header("Game Screen Panels")]
    [SerializeField] private GameObject fullViewPanel;
    [SerializeField] private GameObject headViewPanel;
    [SerializeField] private GameObject rightViewPanel;
    [SerializeField] private GameObject leftViewPanel;

    [Header("UI References")]
    [SerializeField] private GameObject confirmationPopupPrefab;  // Prefab 참조 추가
    [SerializeField] private Transform popupParent;  // Popup의 부모 Transform
    [SerializeField] private GameObject smallDialoguePrefab;  // SmallDialogue prefab
    [SerializeField] private Transform dialogueParent;  // Dialogue의 부모 Transform
    [SerializeField] private Image handImage;
    [SerializeField] private List<Sprite> handSprites;

    [Header("Required Items")]
[SerializeField] public IntermediateRequiredItems requiredItems; // 기존 코드

[Header("Debug about Required Items")]
[SerializeField] private ProcedureRequiredItems procedureRequiredItems; // 추가: 프로시저 필수 아이템

    private Item currentHeldItem;
    private GameObject currentSmallDialogue;

    // 싱글톤 인스턴스 추가
    private static IntermediateManager instance;
    public static IntermediateManager Instance => instance;

    [Header("Initial Cart Items")]

    public List<Item> requiredPickedItems = new List<Item>();  // 이미 꺼낸 필수 아이s템들

    [Header("Cart Item Filter")]
    [SerializeField] private List<Item> itemsToExclude; // 인스펙터에서 제외할 아이템 설정


    private void Awake()
    {
       
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple IntermediateManager instances found!");
            Destroy(gameObject);
            return;
        }

       


    }

    private void Start()
    {
        if (handImage != null)
    {
        handImage.gameObject.SetActive(false); // 초기에는 손 이미지 숨김
    }
    }

    [SerializeField] private CartUI cartUI; // CartUI 참조 추가

    // IntermediateManager.cs
    // IntermediateManager.cs
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameScreenChanged += OnGameScreenChanged;
        }
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameScreenChanged -= OnGameScreenChanged;
        }
    }

    // OnGameScreenChanged 메서드 수정 (또는 추가)
    private void OnGameScreenChanged(GameManager.GameScreen newState)
    {
        // 인터미디에이트 화면으로 전환될 때
        if (newState == GameManager.GameScreen.INTERMEDIATE)
        {
            // 현재 카트의 아이템들을 가져옴
            List<Item> currentCartItems = InteractionManager.Instance.GetCartItems();

            // 카트 초기화
            InteractionManager.Instance.ClearCart();

            // 제외할 아이템을 제외하고 다시 카트에 추가
            foreach (var item in currentCartItems)
            {
                if (!itemsToExclude.Contains(item))
                {
                    InteractionManager.Instance.AddItemToCart(item);
                }
            }

            // 카트 열기
            cartUI.OpenCart();
        }
    }

    // IntermediateManager.cs에 다음 메서드 추가
    public void RefreshCartItems() // 토글 두번째로 열었을 때 카트 아이템 설정하는 거. 
    {
        // 기존 카트 초기화
        InteractionManager.Instance.ClearCart();

        // requiredPickedItems에 있는 항목만 카트에 추가
        foreach (var item in requiredPickedItems)
        {
            InteractionManager.Instance.AddItemToCart(item);
        }
    }

    // TryPickItemFromCart 메서드 (CartUI.cs에서 호출됨)
    public void AddPickedItem(Item item)
    {
        if (!requiredPickedItems.Contains(item))
        {
            requiredPickedItems.Add(item);
            Debug.Log($"Added picked item: {item.itemName}");
        }

        List<Item> cartItems = InteractionManager.Instance.GetCartItems();
    }

    // 이 메서드를 수정하여 모든 필수 아이템이 선택되었는지 확인
    public bool AreAllRequiredItemsPicked()
    {
        return requiredItems.requiredItems.Where(ri => !ri.isOptional)
            .All(ri => requiredPickedItems.Any(picked => picked.itemId == ri.item.itemId));
    }

    // 인터미디어트 매니저에 아이템이 필요한지 확인하는 메서드 추가
    public bool IsRequiredItem(Item item)
    {
        return requiredItems.requiredItems.Any(ri => ri.item == item);
    }

    





    public void OnGoToPatientClick()
    {
        StartCoroutine(TransitionToGameScreen());
       
    }

    private IEnumerator TransitionToGameScreen()
    {
        // 페이드 아웃
        CanvasGroup intermediateCanvasGroup = intermediateScreen.GetComponent<CanvasGroup>();
        if (intermediateCanvasGroup == null)
        {
            intermediateCanvasGroup = intermediateScreen.AddComponent<CanvasGroup>();
        }

        intermediateCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(0.5f);

        // 화면 전환
        
        gameScreen.SetActive(true);


        // GameScreenManager 호출
        GameManager.Instance.GoToGameScreen();
        GameScreenManager.Instance.StartGameScreen();


        intermediateScreen.SetActive(false);

    }

  
    

// AutoCloseDialogue 코루틴은 삭제

    private IEnumerator AutoCloseDialogue()
    {
        yield return new WaitForSeconds(3f);
        if (currentSmallDialogue != null)
        {
            Destroy(currentSmallDialogue);
        }
    }

   // IntermediateManager.cs에 추가할 메서드들

// 아이템을 픽업할 때 호출되는 메서드
public void PickupItem(Item item)
{
    if (item == null) return;
    
    currentHeldItem = item;
    
    // 손 이미지 업데이트
    UpdateHandImage(item);
    
    // 고급 기능
    // 특정 아이템에 대한 interactionDataId 설정은 에디터에서 직접 처리합니다
    
    // 상호작용 성공 여부 추적 변수
    bool success = false;
    
    // interactionDataId가 설정된 경우 범용 상호작용 처리
    if (!string.IsNullOrEmpty(item.interactionDataId))
    {
        // 인터랙션 데이터 레지스트리에서 데이터 확인
        InteractionData interactionData = null;
        
        if (InteractionDataRegistrar.Instance != null)
        {
            // 먼저 레지스트리에서 상호작용 데이터 가져오기 시도
            interactionData = InteractionDataRegistrar.Instance.GetInteractionData(item.interactionDataId);
            
            if (interactionData == null)
            {
                Debug.LogWarning($"상호작용 ID '{item.interactionDataId}'가 등록되어 있지 않습니다. Resources에서 로드를 시도합니다.");
                
                // 자동으로 Resources에서 로드 시도
                try
                {
                    interactionData = Resources.Load<InteractionData>($"Interactions/{item.interactionDataId}");
                    
                    if (interactionData != null)
                    {
                        // 로드 성공하면 등록
                        InteractionDataRegistrar.Instance.AddInteractionData(interactionData);
                        Debug.Log($"상호작용 데이터 '{item.interactionDataId}'가 Resources에서 자동 로드되었습니다.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Resources에서 상호작용 데이터 로드 중 오류: {ex.Message}");
                }
            }
        }
        
        // 베이스 인터랙션 시스템 찾기
        BaseInteractionSystem interactionSystem = FindObjectOfType<BaseInteractionSystem>();
        
        // 없으면 생성
        if (interactionSystem == null)
        {
            // InteractionManager가 이미 BaseInteractionSystem 컴포넌트를 가지고 있는지 확인
            if (InteractionManager.Instance != null)
            {
                interactionSystem = InteractionManager.Instance.GetComponent<BaseInteractionSystem>();
                
                // 그래도 없으면 새로 생성
                if (interactionSystem == null)
                {
                    GameObject interactionObj = new GameObject("BaseInteractionSystem");
                    interactionSystem = interactionObj.AddComponent<BaseInteractionSystem>();
                }
            }
            else
            {
                GameObject interactionObj = new GameObject("BaseInteractionSystem");
                interactionSystem = interactionObj.AddComponent<BaseInteractionSystem>();
            }
            
            // 팝업 컨테이너 설정
            if (popupParent != null)
            {
                RectTransform rectTransform = popupParent.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // 팝업 컨테이너 설정
                    interactionSystem.SetPopupContainer(popupParent);
                }
                else
                {
                    Debug.LogWarning("popupParent에 RectTransform 컴포넌트가 없습니다.");
                }
            }
            
            Debug.Log("BaseInteractionSystem이 자동으로 생성되었습니다.");
        }
        
        try
        {
            // interactionData가 있으면 직접 등록
            if (interactionData != null)
            {
                // InteractionStep으로 변환하여 등록
                List<InteractionStep> steps = InteractionDataRegistrar.Instance.ConvertToInteractionSteps(interactionData);
                RuntimeInteractionData convertedData = new RuntimeInteractionData
                {
                    id = interactionData.id,
                    name = interactionData.displayName,
                    description = interactionData.description,
                    steps = steps
                };
                
                // BaseInteractionSystem에 직접 등록
                interactionSystem.RegisterInteraction(item.interactionDataId, convertedData);
                Debug.Log($"상호작용 데이터 '{item.interactionDataId}'가 BaseInteractionSystem에 직접 등록되었습니다.");
            }
            
            // 초기 오브젝트 생성
            interactionSystem.CreateInitialObjects(item.interactionDataId);
            
            // 상호작용 시작
            bool interactionSuccess = interactionSystem.StartInteraction(item.interactionDataId);
            
            if (interactionSuccess)
            {
                Debug.Log($"[아이템] 상호작용 절차가 시작되었습니다.(ID:{item.interactionDataId})");
                return; // 성공했으므로 여기서 종료
            }
            else
            {
                Debug.LogWarning($"상호작용을 시작할 수 없습니다. ID: {item.interactionDataId}");
                // 실패 메시지 표시
                if (DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.ShowSmallDialogue($"아이템 상호작용을 시작할 수 없습니다: {item.itemName}");
                }
                
                // 실패 변수 설정 (ProcessItemInteraction 호출 위해)
                success = false;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"상호작용 시작 중 오류 발생: {ex.Message}");
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowSmallDialogue($"상호작용 시작 중 오류가 발생했습니다: {ex.Message}");
            }
        }
    }
    
    // interactionDataId가 없거나 상호작용을 시작하지 못한 경우에만 
    // 기존 아이템 타입에 따른 처리 실행
    if (string.IsNullOrEmpty(item.interactionDataId) || !success)
    {
        ProcessItemInteraction(item);
    }
}

// 손 이미지 업데이트 메서드
private void UpdateHandImage(Item item)
{
    if (handImage != null)
    {
        // 아이템에 핸드 스프라이트가 설정되어 있으면 사용
        if (item.handSprite != null)
        {
            handImage.sprite = item.handSprite;
            handImage.gameObject.SetActive(true);
        }
        // 아니면 기본 손 스프라이트 중에서 선택
        else if (handSprites != null && handSprites.Count > 0)
        {
            handImage.sprite = handSprites[0]; // 기본 손 이미지 사용
            handImage.gameObject.SetActive(true);
        }
    }
}

// 아이템 상호작용 처리 메서드
private void ProcessItemInteraction(Item item)
{
    switch (item.interactionType)
    {
        case InteractionType.None:
            // 상호작용 없음
            break;
            
        case InteractionType.SingleClick:
            // 단순 클릭 상호작용 - 메시지 표시 등
            if (!string.IsNullOrEmpty(item.guideText))
            {
                DialogueManager.Instance?.ShowSmallDialogue(item.guideText);
            }
            break;
            
        case InteractionType.Drag:
        case InteractionType.TwoFingerDrag:
        case InteractionType.Draw:
        case InteractionType.RotateDrag:
            // 복잡한 상호작용 - 미니게임 실행
            StartItemMiniGame(item);
            break;
    }
}

// 미니게임 시작 메서드
private void StartItemMiniGame(Item item)
{
    if (item.miniGamePrefab == null)
    {
        DialogueManager.Instance?.ShowSmallDialogue("이 아이템에 대한 미니게임이 설정되지 않았습니다.");
        return;
    }
    
    GameObject miniGameObj = Instantiate(item.miniGamePrefab, popupParent);
    MiniGameBase miniGame = miniGameObj.GetComponent<MiniGameBase>();
    
    if (miniGame != null)
    {
        miniGame.Initialize(
            item.timeLimit,
            item.successThreshold,
            (success) => {
                // 미니게임 완료 후 처리
                if (success)
                {
                    DialogueManager.Instance?.ShowSmallDialogue("성공적으로 완료했습니다!");
                    // 성공 시 추가 처리
                }
                else
                {
                    DialogueManager.Instance?.ShowSmallDialogue("실패했습니다. 다시 시도하세요.");
                    // 실패 시 추가 처리
                }
            }
        );
    }
    else
    {
        Debug.LogError($"미니게임 컴포넌트를 찾을 수 없습니다: {item.itemName}");
        Destroy(miniGameObj);
    }
}

   // 디버깅용 메서드 - 준비 단계에서 필요한 아이템을 자동으로 카트에 채우기
public void DEBUG_FillCartWithRequiredItems()
{
    // 먼저 PreparationManager에서 ProcedureRequiredItems 가져오기
    ProcedureRequiredItems items = null;
    
    if (procedureRequiredItems != null)
    {
        // IntermediateManager에 직접 설정된 ProcedureRequiredItems 사용
        items = procedureRequiredItems;
    }
    else if (PreparationManager.Instance != null)
    {
        // PreparationManager에서 현재 설정된 ProcedureRequiredItems 가져오기
        items = PreparationManager.Instance.GetCurrentProcedureItems();
    }
    
    if (items == null || items.requiredItems.Count == 0)
    {
        Debug.LogWarning("No procedure required items are defined or accessible.");
        DialogueManager.Instance?.ShowSmallDialogue("필수 아이템이 설정되지 않았습니다.");
        return;
    }
    
    // 현재 카트 초기화
    InteractionManager.Instance.ClearCart();
    requiredPickedItems.Clear();
    
    // 필요한 아이템들을 카트에 추가
    foreach (var requiredItem in items.requiredItems)
    {
        if (!requiredItem.isOptional)  // 필수 아이템만 추가
        {
            InteractionManager.Instance.AddItemToCart(requiredItem.item);
            
            // IntermediateManager의 필수 항목과 비교하여 필요한 것만 선택된 목록에 추가
            if (requiredItems != null && 
                requiredItems.requiredItems.Any(ri => ri.item.itemId == requiredItem.item.itemId))
            {
                requiredPickedItems.Add(requiredItem.item);
            }
        }
    }
    
    // CartUI가 있으면 업데이트
    if (cartUI != null)
    {
        cartUI.UpdateCartDisplay();
    }
    
    Debug.Log("[DEBUG] All required items have been added to cart");
    
    // 팝업 메시지로 알림
    DialogueManager.Instance?.ShowSmallDialogue("디버그: 모든 필수 아이템이 카트에 추가되었습니다.");
}
    


}
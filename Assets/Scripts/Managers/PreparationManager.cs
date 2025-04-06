using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Nursing.Procedure;
using Nursing.Managers;



public class PreparationManager : MonoBehaviour
{
    private static PreparationManager instance;
    public static PreparationManager Instance => instance;

    [Header("Popup Parents")]
    [SerializeField] private Transform popupParent; // 기존 팝업용
    [SerializeField] private Transform dialogueParent; // 대화창 전용

    [Header("UI References")]
    [SerializeField] private Button prepareCompleteButton;
    [SerializeField] private GameObject itemSelectionPopupPrefab;
    [SerializeField] private GameObject confirmationPopupPrefab;
    [SerializeField] private CartUI cartUI;
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Screen References")]
    [SerializeField] private GameObject prepareScreen;
    [SerializeField] private GameObject intermediateScreen;
    [SerializeField] private GameObject gameScreen;

    [System.Serializable]
    public class AreaItems
    {
        public PreparationAreaType area;
        public List<Item> items;
    }
    [Header("Area Setup")]
    [SerializeField] private List<AreaItems> areaItems;

    [Header("Required Items")]
    [SerializeField] private ProcedureRequiredItems currentProcedureItems;
    [SerializeField] private List<ProcedureRequiredItems> ItemListForEachProcedure;
    private Queue<Item> optionalItemsToExplain = new Queue<Item>();
    // 이 변수는 추후 옵셔널 아이템 설명 기능에서 사용될 예정이므로 속성으로 변경
    public bool IsShowingOptionalItems { get; private set; } = false;

    // 현재 선택된 아이템 관리
    private List<Item> selectedItems = new List<Item>();
    
   
    // ItemSelectionPopup 참조
    private ItemSelectionPopup currentPopup;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        GameManager.Instance.DebugforPrepare();

    }

    private void Start()
    {

        InitializeUI();

        // 카트 초기화
        
            cartUI?.ClearCart();
            selectedItems.Clear();
            cartUI?.UpdateCartDisplay();
        
    }

    private void InitializeUI()
    {
        if (prepareCompleteButton != null)
        {
            prepareCompleteButton.onClick.AddListener(CheckPrepareComplete);
        }

        if (popupParent == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                popupParent = canvas.transform;
            }
        }
    }

    public void OnAreaClicked(PreparationAreaType area)
    {
        ShowItemSelectionPopup(area);
    }

    private void ShowItemSelectionPopup(PreparationAreaType area)
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup.gameObject);
        }

        var items = GetItemsForArea(area);
        if (items == null || items.Count == 0)
        {
            DialogueManager.Instance.ShowSmallDialogue("이 구역에는 사용 가능한 아이템이 없습니다.");
            return;
        }

        var popup = InstantiatePopup<ItemSelectionPopup>(itemSelectionPopupPrefab, popupParent);
        if (popup != null)
        {
            currentPopup = popup;
            popup.Initialize(area, items, OnItemSelected);
        }
    }

    private void OnItemSelected(Item item)
    {
        if (item == null) return;

        // 이미 카트에 있는 아이템인지 확인
        if (selectedItems.Any(selectedItem => selectedItem.itemId == item.itemId))
        {
            DialogueManager.Instance.ShowSmallDialogue("이건 이미 있어", false);
            return;
        }
        var popup = InstantiatePopup<ConfirmationPopup>(confirmationPopupPrefab, popupParent);
        if (popup != null)
        {
            popup.Initialize(
                item,
                () =>
                {
                    AddItemToCart(item);
                },
                () => Destroy(popup.gameObject)
            );
        }
    }

    private void AddItemToCart(Item item)
    {
        if (item == null ) return;

 
            selectedItems.Add(item);
            cartUI?.AddItemToCart(item);
            cartUI?.UpdateCartDisplay();
            

    }

    public void RemoveItemFromCart(Item item)
    {
        if (item == null) return;

        
            selectedItems.Remove(item);
            if (item == null)
            {
                Debug.LogWarning("Attempted to remove a null item to the cart.");
            return;
            }
            cartUI?.cartItems.Remove(item);
            
            
             cartUI?.UpdateCartDisplay();
        
    }


    private void ProcessOptionalItems(List<RequiredItem> optionalItems)
    {

        if (optionalItems.Count > 0)
        {
            optionalItemsToExplain = new Queue<Item>();
            foreach (var item in optionalItems)
            {
                optionalItemsToExplain.Enqueue(item.item);
            }
            IsShowingOptionalItems = true;
            ShowNextOptionalItemDialogue();
        }
        else
        {
            DialogueManager.Instance.ShowSmallDialogue("준비 잘했어!", false, () =>
            {
                GoToIntermediateScreen();
            });
        }
    }
    private void ShowNextOptionalItemDialogue()
    {
        if (optionalItemsToExplain.Count > 0)
        {
            var item = optionalItemsToExplain.Dequeue();

            // 중요: 여기서 클릭 이벤트 핸들러를 직접 등록하여 다음 대화창을 표시하도록 함
            DialogueManager.Instance.ShowSmallDialogue($"{item.itemName}: {item.description}", true, () => {
                // 이 콜백은 대화창을 클릭할 때 호출됨
                // 잠시 지연 후 다음 대화상자 표시
                StartCoroutine(DelayNextDialogue());
            });
        }
        else
        {
            // 모든 아이템을 다 보여줬을 경우
            DialogueManager.Instance.ShowSmallDialogue("어쨌든! 준비 잘했어!", false, () => {
                GoToIntermediateScreen();
            });
        }
    }

    // 다음 대화창 표시를 위한 지연 코루틴
    private IEnumerator DelayNextDialogue()
    {
        // 짧은 지연 시간을 줘서 대화창이 완전히 닫히고 다음 대화창이 표시되도록 함
        yield return new WaitForSeconds(0.3f);

        if (optionalItemsToExplain.Count > 0)
        {
            ShowNextOptionalItemDialogue();
        }
        else
        {
            // 모든 아이템을 다 보여줬을 경우
            DialogueManager.Instance.ShowSmallDialogue("어쨌든! 준비 잘했어!", false, () => {
                GoToIntermediateScreen();
            });
        }
    }

    private void ShowMissingItemsDialogue(List<RequiredItem> missingItems)
    {
        
        string message = "지금 부족한 물건은 다음과 같아.\n\n";
        for (int i = 0; i < missingItems.Count; i++)
        {
            message += $"{i + 1}) ? : {missingItems[i].item.description}\n";
        }
        DialogueManager.Instance.ShowLargeDialogue(message);
    }

    private void ShowExtraItemsDialogue(List<Item> extraItems)
    {
        string message = "다음 물건은 필요없어.\n\n";
        for (int i = 0; i < extraItems.Count; i++)
        {
            message += $"{i + 1}) {extraItems[i].itemName} : {extraItems[i].description}\n";
        }
        DialogueManager.Instance.ShowLargeDialogue(message);
    }

    private bool isProcessingDialogue = false;

    public void CheckPrepareComplete()
    {
        if (isProcessingDialogue) return;  // 이미 대화창 처리 중이면 리턴
        
        List<RequiredItem> missingItems = GetMissingItems();
        List<Item> extraItems = GetExtraItems();
        List<RequiredItem> optionalItems = GetOptionalItems();

        isProcessingDialogue = true;  // 대화창 처리 시작

        if (missingItems.Count > 0)
        {
            ShowMissingItemsDialogue(missingItems);
            isProcessingDialogue = false;
            return;
        }

        if (extraItems.Count > 0)
        {
            ShowExtraItemsDialogue(extraItems);
            isProcessingDialogue = false;

            return;
        }

        ProcessOptionalItems(optionalItems);
    }
    private List<RequiredItem> GetMissingItems()
    {
        if (currentProcedureItems == null) return new List<RequiredItem>();

        return currentProcedureItems.requiredItems
            .Where(required => !required.isOptional &&
                   !selectedItems.Any(selected => selected.itemId == required.item.itemId))
            .ToList();
    }

    private List<Item> GetExtraItems()
    {
        if (currentProcedureItems == null) return new List<Item>();

        return selectedItems
            .Where(selected => !currentProcedureItems.requiredItems
                .Any(required => required.item.itemId == selected.itemId))
            .ToList();
    }

    private List<RequiredItem> GetOptionalItems()
    {
        if (currentProcedureItems == null) return new List<RequiredItem>();

        return currentProcedureItems.requiredItems
            .Where(required => required.isOptional &&
                   selectedItems.Any(selected => selected.itemId == required.item.itemId))
            .ToList();
    }

    public void SetCurrentProcedure(Nursing.Procedure.ProcedureTypeEnum procedureType)
    {
        currentProcedureItems = ItemListForEachProcedure.Find(x => x.procedureType == procedureType);
        if (currentProcedureItems == null)
        {
            Debug.LogError($"No procedure items found for {procedureType}");
        }
    }

    private List<Item> GetItemsForArea(PreparationAreaType area)
    {
        var areaItem = areaItems?.Find(x => x.area == area);
        return areaItem?.items ?? new List<Item>();
    }

    public void GoToIntermediateScreen()
    {
        if (this.gameObject.activeInHierarchy)
        {
            StartCoroutine(SwitchToIntermediateScreen());
        }

    }

// 현재 설정된 ProcedureRequiredItems를 반환하는 메서드
public ProcedureRequiredItems GetCurrentProcedureItems()
{
    return currentProcedureItems;
}


    private IEnumerator SwitchToIntermediateScreen()
    {
        CanvasGroup prepareCanvasGroup = prepareScreen.GetComponent<CanvasGroup>();
        if (prepareCanvasGroup == null)
        {
            prepareCanvasGroup = prepareScreen.AddComponent<CanvasGroup>();
        }

        prepareCanvasGroup.DOFade(0f, 0.5f).SetEase(Ease.InOutQuad);
        yield return new WaitForSeconds(0.5f);

        // Intermediate Screen 활성화
        intermediateScreen.SetActive(true);



        // 상태 변경
        GameManager.Instance.GoToIntermediate();
        prepareScreen.SetActive(false);

    }





    private T InstantiatePopup<T>(GameObject prefab, Transform parent) where T : Component
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null.");
            return null;
        }

        var popupObject = Instantiate(prefab, parent);
        var popupComponent = popupObject.GetComponent<T>();

        if (popupComponent == null)
        {
            Debug.LogError($"The prefab does not contain a {typeof(T).Name} component.");
            Destroy(popupObject);
        }

        return popupComponent;
    }



    private void OnDestroy()
    {
        
        if (currentPopup != null)
            Destroy(currentPopup.gameObject);
    }


    // 디버그용 코드 


    // 디버그용 - 모든 필수 아이템을 한 번에 추가하는 기능
    public void DEBUG_AddAllRequiredItems()
    {
        if (currentProcedureItems == null)
        {
            Debug.LogWarning("No procedure items set!");
            return;
        }

        // 현재 카트 초기화
        cartUI?.ClearCart();
        selectedItems.Clear();
        

        // 모든 필수 아이템 추가
        foreach (var requiredItem in currentProcedureItems.requiredItems)
        {
            if (!requiredItem.isOptional)  // 필수 아이템만 추가
            {
               
                    selectedItems.Add(requiredItem.item);
                    cartUI?.AddItemToCart(requiredItem.item);


            }
        }

        // 카트 UI 업데이트
        cartUI?.UpdateCartDisplay();

        Debug.Log("[DEBUG] All required items have been added to cart");
    }

}
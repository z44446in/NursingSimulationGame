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
    [SerializeField] private CentralOutlineManager outlineManager;

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
    [SerializeField] private Nursing.Managers.ProcedureManager procedureManager; // ProcedureManager 참조 추가
    private Queue<Item> optionalItemsToExplain = new Queue<Item>();
    // 이 변수는 추후 옵셔널 아이템 설명 기능에서 사용될 예정이므로 속성으로 변경
    public bool IsShowingOptionalItems { get; private set; } = false;

    // 현재 선택된 아이템 관리
    private List<Item> selectedItems = new List<Item>();
    
   
    // ItemSelectionPopup 참조
    private ItemSelectionPopup currentPopup;

    public bool InPopup { get; set; } = false;

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

     void Update()
        {
            /*
          if (outlineManager != null)  // null 체크 추가
            {
                if (!InPopup)
                {
                    outlineManager.StartOutline();
                }
                else
                {
                    outlineManager.StopOutline();
                }

                Debug.Log(InPopup);
            }
           
           */
            
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
        if (item == null) return;
    
    // 중복 기능 아이템 체크
        Item duplicateFunctionalItem = null;
    
        // functionalGroupId가 있고 비어있지 않은 경우에만 체크
        if (!string.IsNullOrEmpty(item.functionalGroupId))
        {
            Debug.Log("중복이 인지는 됐어욤");
            duplicateFunctionalItem = selectedItems.Find(i => 
                !string.IsNullOrEmpty(i.functionalGroupId) && 
                i.functionalGroupId == item.functionalGroupId && 
                i.itemId != item.itemId); // 같은 아이템이 아니면서 같은 기능 그룹
        }

        if (duplicateFunctionalItem != null)
        {
            Debug.Log("중복 아이템 찾는 것도 함");
            // 중복 기능 아이템이 있는 경우 경고 메시지
            string message = $"중복되는 기능을 하는 {duplicateFunctionalItem.itemName}이 카트에 있어! " + 
                            $"{item.itemName}를 담고싶다면 카트에 있는 {duplicateFunctionalItem.itemName}을 제거한 뒤에 다시 담아줘!";
            
            DialogueManager.Instance.ShowSmallDialogue(message, false);
            return; // 아이템 추가하지 않고 리턴
                

        }
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
        InPopup = true;
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

        InPopup = false;
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
        // 필요한 이유 메시지 사용
        string reason = !string.IsNullOrEmpty(missingItems[i].needReason) 
            ? missingItems[i].needReason 
            : missingItems[i].item.description;
            
        message += $"{i + 1}) {"[  ?   ]"} : {reason}\n\n";
    }
        DialogueManager.Instance.ShowLargeDialogue(message);
       
    }

    private void ShowExtraItemsDialogue(List<Item> extraItems)
    {
        ProcedureData procedureData = procedureManager?.GetCurrentProcedureData();
    string message = "다음 물건은 필요없어.\n\n";
    
    for (int i = 0; i < extraItems.Count; i++)
    {
        // 기본 이유 메시지
        string reason = extraItems[i].description;
        
        // 불필요 아이템 목록에서 이유 찾기
        if (procedureData != null && procedureData.unnecessaryItems != null)
        {
            var unnecessaryItem = procedureData.unnecessaryItems
                .FirstOrDefault(u => u.item != null && u.item.itemId == extraItems[i].itemId);
                
            if (unnecessaryItem != null && !string.IsNullOrEmpty(unnecessaryItem.unnecessaryReason))
            {
                reason = unnecessaryItem.unnecessaryReason;
            }
        }
        
        message += $"{i + 1}) {extraItems[i].itemName} : {reason}\n";
    }
     DialogueManager.Instance.ShowLargeDialogue(message);
    }

    private bool isProcessingDialogue = false;

    public void CheckPrepareComplete()
    {
        
        if (isProcessingDialogue) return;  // 이미 대화창 처리 중이면 리턴
        InPopup = true;

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
        InPopup = false;
    }
    private List<RequiredItem> GetMissingItems()
{
    if (procedureManager == null) return new List<RequiredItem>();

    List<RequiredItem> requiredItems = procedureManager.GetRequiredItems();
    List<RequiredItem> missingItems = new List<RequiredItem>();
    
    // 1. 먼저 기본 필수 아이템 체크 (isOptional이 false인 것들)
    foreach (var required in requiredItems)
    {
        if (!required.isOptional && !selectedItems.Any(selected => selected.itemId == required.item.itemId))
        {
            missingItems.Add(required);
        }
    }
    
    // 2. functionalGroup 체크 로직 추가
    // 모든 functionalGroup ID 수집
    HashSet<string> functionalGroups = new HashSet<string>();
    foreach (var required in requiredItems)
    {
        
        if (!string.IsNullOrEmpty(required.item.functionalGroupId))
            functionalGroups.Add(required.item.functionalGroupId);
    }
    
    foreach (var groupId in functionalGroups)
{
    var groupRequiredItems = requiredItems.Where(ri => ri.item.functionalGroupId == groupId).ToList();
    bool hasGroupItem = selectedItems.Any(selected => 
        !string.IsNullOrEmpty(selected.functionalGroupId) && 
        selected.functionalGroupId == groupId);

    if (!hasGroupItem)
    {
        var firstRequiredInGroup = groupRequiredItems.FirstOrDefault();

        if (firstRequiredInGroup != null && !missingItems.Contains(firstRequiredInGroup))
        {
            // 필수 아이템 포함 여부 판단
            if (groupRequiredItems.Any(ri => !ri.isOptional))
            {
                Debug.Log("❗필수 아이템 누락 경고");
            }
            else
            {
                Debug.Log("ℹ️ 선택 아이템도 선택 안 됨 (필수는 아님)");
            }

            missingItems.Add(firstRequiredInGroup);
        }
    }
}

    
    return missingItems;
}

    private List<Item> GetExtraItems()
    {
        if (procedureManager == null) return new List<Item>();

        List<RequiredItem> requiredItems = procedureManager.GetRequiredItems();
        
        return selectedItems
            .Where(selected => !requiredItems
                .Any(required => required.item.itemId == selected.itemId))
            .ToList();
    }

    private List<RequiredItem> GetOptionalItems()
    {
        if (procedureManager == null) return new List<RequiredItem>();

        List<RequiredItem> requiredItems = procedureManager.GetRequiredItems();
        
        return requiredItems
            .Where(required => required.isOptional &&
                   selectedItems.Any(selected => selected.itemId == required.item.itemId))
            .ToList();
    }

    // ProcedureManager에서 현재 프로시저를 가져오므로 이 메서드는 필요 없음
    // SetCurrentProcedure는 ProcedureManager에서 처리함

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

// ProcedureData를 통해 필요한 아이템을 반환하는 메서드
public List<RequiredItem> GetCurrentRequiredItems()
{
    if (procedureManager != null)
    {
        return procedureManager.GetRequiredItems();
    }
    return new List<RequiredItem>();
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
        if (procedureManager == null)
        {
            Debug.LogWarning("No procedure manager set!");
            return;
        }

        List<RequiredItem> requiredItems = procedureManager.GetRequiredItems();
        
        if (requiredItems.Count == 0)
        {
            Debug.LogWarning("No required items found in procedure data!");
            return;
        }

        // 현재 카트 초기화
        cartUI?.ClearCart();
        selectedItems.Clear();
        
        // 모든 필수 아이템 추가
        foreach (var requiredItem in requiredItems)
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
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Nursing.Managers;
using System;


public class CartUI : MonoBehaviour
{
    [Header("Cart UI References")]
    [SerializeField] private GameObject cartPanel;
    [SerializeField] private Button cartToggleButton;
    [SerializeField] private TextMeshProUGUI cartToggleButtonText;
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private TextMeshProUGUI cartInstructionText;

    [Header("Cart Button Text")]
    [SerializeField] private string openText = "카트보기";
    [SerializeField] private string closeText = "카트닫기 ";

    [Header("Popup References")]
    [SerializeField] private GameObject confirmationPopupPrefab;
    [SerializeField] private GameObject smallPopupPrefab;
    [SerializeField] private GameObject quizPopupPrefab;
    [SerializeField] private GameObject actionPopupPrefab;
    [SerializeField] private Transform popupParent;

    // 화면별 상수 메시지 정의
    private const string PREPARE_SCREEN_MESSAGE = "유치도뇨에 필요한 준비물품을 고르시오.";
    private const string INTERMEDIATE_SCREEN_MESSAGE = "환자한테 가기 전, 카트에서 꺼내서 준비가 필요한 물품을 전부 고르세요. 다 고르면, 카트닫기를 눌러 주세요.";
    private const string GAME_SCREEN_MESSAGE = "손에 들 물건을 고르시오.";

    private const string PREPARE_CONFIRMATION_MESSAGE = "카트에서 제거하시겠습니까?";
    private const string INTERMEDIATE_CONFIRMATION_MESSAGE = "카트에서 꺼내겠습니까?";
    private const string GAME_CONFIRMATION_MESSAGE = "손에 들겠습니까?";

    private GameManager.GameScreen currentScreen;
    private GameManager.GameScreen lastScreen;
    bool hasCartBeenClosedOnce = false;

    // 카트 관련
    public List<Item> cartItems = new List<Item>();



    private void Start()
    {
        currentScreen = GameManager.Instance.GetCurrentScreen(); // 현재 화면 가져오기
        InitializeUI();
        UpdateCartInstruction();

    }



    private void Update()
    {
        var currentScreenFromManager = GameManager.Instance.GetCurrentScreen();

        if (lastScreen != currentScreenFromManager)
        {
            lastScreen = currentScreenFromManager;
            currentScreen = currentScreenFromManager;

            UpdateCartInstruction();
        }
    }

    private void InitializeUI()
    {
        if (cartPanel != null)
        {
            cartPanel.SetActive(false);
        }

        if (cartToggleButton != null)
        {
            cartToggleButton.onClick.AddListener(ToggleCart);
            UpdateToggleButtonText(false);
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.OnGameScreenChanged += HandleScreenChange;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameScreenChanged -= HandleScreenChange;
        }
        else
        {
            Debug.LogWarning("GameManager instance is null during OnDisable. Event unsubscription skipped.");
        }
    }

    private void HandleScreenChange(GameManager.GameScreen newScreen)
    {
        currentScreen = newScreen;
        UpdateCartInstruction();
    }

    private void UpdateCartInstruction()
    {
        if (cartInstructionText == null) return;

        switch (currentScreen)
        {
            case GameManager.GameScreen.GAMESCREEN:
                cartInstructionText.text = GAME_SCREEN_MESSAGE;
                break;
            case GameManager.GameScreen.INTERMEDIATE:
                cartInstructionText.text = INTERMEDIATE_SCREEN_MESSAGE;
                break;
            case GameManager.GameScreen.PREPARING:
                cartInstructionText.text = PREPARE_SCREEN_MESSAGE;
                break;
        }
    }

    // CartUI.cs의 OpenCart 메서드 수정
    public void OpenCart()
    {
        if (cartPanel == null)
        {
            Debug.LogError("Cart Panel is null!");
            return;
        }

        cartPanel.SetActive(true);
        UpdateToggleButtonText(true);

        UpdateCartDisplay();
        UpdateCartInstruction();
    }

    // ItemButton에서 Item을 가져오는 임시 메서드
    private Item GetItemFromButton(ItemButton itemButton)
    {
        return itemButton.GetCurrentItem();
    }

    // CartUI.cs의 ToggleCart 메서드
    // CartUI.cs의 ToggleCart 메서드 수정
    private void ToggleCart()
    {
        if (cartPanel == null) return;

        if (cartPanel.activeSelf && currentScreen == GameManager.GameScreen.INTERMEDIATE)
        {



            if (hasCartBeenClosedOnce == false)
            {
                bool itemsAllPicked = IntermediateManager.Instance.AreAllRequiredItemsPicked();

                if (!itemsAllPicked)
                {
                    DialogueManager.Instance.ShowSmallDialogue("아직 덜골랐어.. 좀 더 생각해봐. 환자한테 가면 완전 멸균이어야 한다구?");
                    return;
                }

                hasCartBeenClosedOnce = true;

                cartPanel.SetActive(false);
                UpdateToggleButtonText(false);
                IntermediateManager.Instance.RefreshCartItems();
                cartInstructionText.text = "어떤 물품을 꺼내시겠습니까?";
                UpdateCartDisplay();


                return;

            }



        }



        bool newState = !cartPanel.activeSelf;
        cartPanel.SetActive(newState);
        UpdateToggleButtonText(newState);


    }
    private void UpdateToggleButtonText(bool isCartOpen)
    {
        if (cartToggleButtonText != null)
        {
            cartToggleButtonText.text = isCartOpen ? closeText : openText;
        }
    }

    public void UpdateCartDisplay()
    {
        if (itemContainer == null || itemButtonPrefab == null) return;

        ClearItemContainer();

        List<Item> cartItems = GetCartItems();

        if (cartItems == null || cartItems.Count == 0)
        {
            Debug.Log("No items in the cart to display.");
            return;
        }

        foreach (var item in cartItems)
        {
            CreateItemButton(item);
        }
    }

    private void ClearItemContainer()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateItemButton(Item item)
    {
        if (item == null) return;

        GameObject itemUI = Instantiate(itemButtonPrefab, itemContainer, false);
        ItemButton itemButton = itemUI.GetComponent<ItemButton>();

        if (itemButton == null)
        {
            Debug.LogError("아이템 버튼이 업습니다.");
            return;
        }

        switch (currentScreen)
        {

            case GameManager.GameScreen.INTERMEDIATE:
                itemButton.Initialize(item, HandleIntermediateItemClick);
                break;
            case GameManager.GameScreen.PREPARING:
                itemButton.Initialize(item, HandlePrepareScreenItemClick);
                break;
        }
    }


    // CartUI.cs 수정사항

    // HandleIntermediateItemClick 메서드 수정
    private void HandleIntermediateItemClick(Item item)
    {
        ShowConfirmationPopup(item, INTERMEDIATE_CONFIRMATION_MESSAGE, TryPickItemFromCart);


    }

    // TryPickItemFromCart 메서드 수정
    // CartUI.cs의 TryPickItemFromCart 메서드 수정
    private void TryPickItemFromCart(Item item)
    {
        if (IntermediateManager.Instance.IsRequiredItem(item))
        {
            // 선택한 아이템은 카트에서 제거
            RemoveItemFromCart(item);

            if (hasCartBeenClosedOnce == true)
            {
                IntermediateManager.Instance.PickupItem(item);
                UpdateToggleButtonText(true);
                cartPanel.SetActive(false);
                UpdateCartDisplay();
                return;
            }
            // 선택한 아이템 목록에 추가
            IntermediateManager.Instance.AddPickedItem(item);


            // 카트 UI 업데이트
            UpdateCartDisplay();


        }
        else
        {
            DialogueManager.Instance?.ShowSmallDialogue("이건 지금 필요한 게 아니야");
        }


    }

    // OpenCart 메서드는 그대로 두고, 카트 UI 업데이트를 위한 메서드를 적절히 호출
    private void HandlePrepareScreenItemClick(Item item)
    {
        ShowConfirmationPopup(item, PREPARE_CONFIRMATION_MESSAGE, RemoveItemFromCart);
    }

    private void ShowConfirmationPopup(Item item, string message, System.Action<Item> onConfirm)
    {
        if (confirmationPopupPrefab == null || popupParent == null)
        {
            Debug.LogError("Popup prefab or parent is missing!");
            return;
        }

        var popup = InstantiatePopup<ConfirmationPopup>(confirmationPopupPrefab, popupParent);
        if (popup != null)
        {
            popup.Initialize(
                item,
                () => onConfirm(item),
                null
            );
            popup.SetCustomMessage($"{item.itemName}을(를) {message}");
        }
    }



    private void RemoveItemFromCart(Item item)
    {
        if (PreparationManager.Instance != null)
        {
            PreparationManager.Instance.RemoveItemFromCart(item);
           
        }
        cartItems.Remove(item);
    }

    private T InstantiatePopup<T>(GameObject prefab, Transform parent) where T : Component
    {
        if (prefab == null) return null;

        var popupObject = Instantiate(prefab, parent);
        return popupObject.GetComponent<T>();
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

}
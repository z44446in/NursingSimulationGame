using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // Make sure DOTween is imported
using UnityEngine.EventSystems;
using System.Linq;

public class GameScreenManager : MonoBehaviour
{
    private static GameScreenManager instance;
    public static GameScreenManager Instance => instance;

    [Header("Screen Panels")]
    [SerializeField] private GameObject fullViewPanel;
    [SerializeField] private GameObject headViewPanel;
    [SerializeField] private GameObject rightViewPanel;
    [SerializeField] private GameObject leftViewPanel;

    [Header("View Buttons")]
    [SerializeField] private Button headViewButton;    // Add in Inspector
    [SerializeField] private Button rightViewButton;   // Add in Inspector
    [SerializeField] private Button leftViewButton;    // Add in Inspector
    [SerializeField] private Button backToFullViewButton; // Add in Inspector

    [Header("Initial Cart Setup (destroy)")]
    [SerializeField] private List<Item> initialCartItems;
    [SerializeField] private CartUI cartUI;

   

    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private Ease transitionEase = Ease.InOutQuad;

    [Header("Popup References")]
    [SerializeField] private GameObject actPopup; // 씬에 있는 ActPopup 참조
    [SerializeField] private Button actButton;    // 행동하기 버튼 참조

    [Header("Popup References")]
    [SerializeField] private GameObject talkPopup; // 씬에 있는 TalkPopup 참조
    [SerializeField] private Button talkButton;    // 행동하기 버튼 참조

    [Header("UI Elements")]
    [SerializeField] private Button addAllRequiredItemsButton; // "필요한 물건 다 담기" 버튼
    [SerializeField] private List<Item> requiredItemsToAdd; // 인스펙터에서 설정할 필수 아이템 리스트
    [SerializeField] private Button debugModeButton; // 디버그 모드용 버튼


    [Header("Hand Sprites")]
    [SerializeField] private Image handImage;  // 손 이미지 UI 컴포넌트
    [SerializeField] private Sprite defaultHandSprite;  // 기본 손 이미지
    [SerializeField] private Sprite tarpHandSprite;     // 방수포 든 손 이미지

    private CanvasGroup canvasGroup;
    private GameObject currentActivePanel;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePanels();
        SetupButtonListeners();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void InitializePanels()
    {
        fullViewPanel.SetActive(true);
        headViewPanel.SetActive(false);
        rightViewPanel.SetActive(false);
        leftViewPanel.SetActive(false);
        currentActivePanel = fullViewPanel;
    }


    private void Start()
    {
        InitializeGameScreen();
        GameManager.Instance.ChangeGameScreen(GameManager.GameScreen.GAMESCREEN);

        if (actPopup != null)
        {
      
            actPopup.SetActive(false); // 시작시 비활성화
   

        }
        if (talkPopup != null)
        {
            talkPopup.SetActive(false); // 시작시 비활성화
        }

    }

    

    private void ShowActPopup()
    {
        actPopup.SetActive(true);
    }

    private void ShowTalkPopup()
    {
      
     talkPopup.SetActive(true);
    }

    private void AddAllRequiredItemsToCart()
    {
        if (requiredItemsToAdd == null || requiredItemsToAdd.Count == 0)
        {
            Debug.LogWarning("No required items are set in the inspector!");
            return;
        }

        if (InteractionManager.Instance == null)
        {
            Debug.LogError("InteractionManager instance is missing!");
            return;
        }

        // 필요한 아이템을 카트에 추가
        foreach (var item in requiredItemsToAdd)
        {
            if (item != null)
            {
                bool success = InteractionManager.Instance.AddItemToCart(item);
                if (success)
                {
                   
                }
                else
                {
                    Debug.LogWarning($"Failed to add item: {item.itemName} to cart.");
                }
            }
        }

        // UI 업데이트
        cartUI?.UpdateCartDisplay();
    }


    private void InitializeGameScreen()
    {
        // 패널 초기화
        SetActiveAllPanels(false);
        fullViewPanel.SetActive(true);


        // 카트 초기화
        InitializeCart();
    }

    private void SetupButtonListeners()
    {
        if (headViewButton != null)
            headViewButton.onClick.AddListener(() => SwitchToPanel(headViewPanel));

        if (rightViewButton != null)
            rightViewButton.onClick.AddListener(() => SwitchToPanel(rightViewPanel));

        if (leftViewButton != null)
            leftViewButton.onClick.AddListener(() => SwitchToPanel(leftViewPanel));

        if (backToFullViewButton != null)
            backToFullViewButton.onClick.AddListener(() => SwitchToPanel(fullViewPanel));
    }

    private void SwitchToPanel(GameObject targetPanel)
    {
        if (targetPanel == null || targetPanel == currentActivePanel)
            return;

        currentActivePanel.SetActive(false);
        targetPanel.SetActive(true);
        currentActivePanel = targetPanel;

        // Back 버튼은 fullViewPanel이 아닐 때만 보이게
        if (backToFullViewButton != null)
            backToFullViewButton.gameObject.SetActive(targetPanel != fullViewPanel);
    }

    private void OnDestroy()
    {
        if (headViewButton != null)
            headViewButton.onClick.RemoveAllListeners();
        if (rightViewButton != null)
            rightViewButton.onClick.RemoveAllListeners();
        if (leftViewButton != null)
            leftViewButton.onClick.RemoveAllListeners();
        if (backToFullViewButton != null)
            backToFullViewButton.onClick.RemoveAllListeners();
       
       if (actButton != null)
           actButton.onClick.RemoveAllListeners();
       if (talkButton != null)
           talkButton.onClick.RemoveAllListeners();

        if (addAllRequiredItemsButton != null)
        {
            addAllRequiredItemsButton.onClick.RemoveListener(AddAllRequiredItemsToCart);
        }
      

    }

    private void SetActiveAllPanels(bool active)
    {
        fullViewPanel.SetActive(active);
        headViewPanel.SetActive(active);
        rightViewPanel.SetActive(active);
        leftViewPanel.SetActive(active);
    }

    private void InitializeCart()
    {
        if (InteractionManager.Instance != null)
        {
            // 현재 카트의 아이템들을 임시 저장
            List<Item> intermediateCartItems = InteractionManager.Instance.GetCartItems();

            // 카트 초기화
            InteractionManager.Instance.ClearCart();

            // 중간화면에서 가져온 아이템들 다시 추가
            foreach (var item in intermediateCartItems)
            {
                InteractionManager.Instance.AddItemToCart(item);
            }

            // 추가로 필요한 게임화면 전용 아이템들 추가
            foreach (var item in initialCartItems)
            {
                // 이미 카트에 있는 아이템이 아닐 경우에만 추가
                if (!intermediateCartItems.Any(x => x.itemId == item.itemId))
                {
                    InteractionManager.Instance.AddItemToCart(item);
                }
            }

            cartUI?.UpdateCartDisplay();
        }
    }

    public void SwitchToPanel(string panelType)
    {
        SetActiveAllPanels(false);

        switch (panelType.ToLower())
        {
            case "full":
                fullViewPanel.SetActive(true);
                break;
            case "head":
                headViewPanel.SetActive(true);
                break;
            case "right":
                rightViewPanel.SetActive(true);
                break;
            case "left":
                leftViewPanel.SetActive(true);
                break;
        }
    }

    public void StartGameScreen()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(true);
        canvasGroup.DOFade(1f, transitionDuration).SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                InitializeGameScreen();
                
            });
    }

    public void HandleItemPickup(Item item)
    {
        if (item == null) return;

        // 유치도뇨 술기 관련 아이템은 UrinaryCatheterManager로 위임
        UrinaryCatheterManager.Instance.HandleItemInteraction(item);
    }

    public void HandleTarpPickup()
    {
        // 손 이미지 변경
        // 방수포를 들고 있는 손 이미지로 변경
        UpdateHandSprite("tarpHand");

        // 가이드 텍스트 표시
        GuidePanel.Instance.ShowGuide("방수포를 둘 곳을 터치하세요.");

        // Collider나 버튼이 있는 영역만 터치 가능하도록 설정
        EnableTarpPlacementAreas(true);
    }

    public void EnableTarpPlacementAreas(bool enable)
    {
        // 방수포를 둘 수 있는 영역의 Collider나 Button 활성화/비활성화
        TarpPlacementArea[] areas = FindObjectsOfType<TarpPlacementArea>();
        foreach (var area in areas)
        {
            area.gameObject.SetActive(enable);
        }
    }

    private void UpdateHandSprite(string handType)
    {
        if (handImage == null) return;

        switch (handType)
        {
            case "tarpHand":
                handImage.sprite = tarpHandSprite;
                break;
            default:
                handImage.sprite = defaultHandSprite;
                break;
        }
    }
}
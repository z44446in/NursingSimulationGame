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
    [SerializeField] private GameObject actPopup; // ���� �ִ� ActPopup ����
    [SerializeField] private Button actButton;    // �ൿ�ϱ� ��ư ����

    [Header("Popup References")]
    [SerializeField] private GameObject talkPopup; // ���� �ִ� TalkPopup ����
    [SerializeField] private Button talkButton;    // �ൿ�ϱ� ��ư ����

    [Header("UI Elements")]
    [SerializeField] private Button addAllRequiredItemsButton; // "�ʿ��� ���� �� ���" ��ư
    [SerializeField] private List<Item> requiredItemsToAdd; // �ν����Ϳ��� ������ �ʼ� ������ ����Ʈ
    [SerializeField] private Button debugModeButton; // ����� ���� ��ư


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
      
            actPopup.SetActive(false); // ���۽� ��Ȱ��ȭ
   

        }
        if (talkPopup != null)
        {
            talkPopup.SetActive(false); // ���۽� ��Ȱ��ȭ
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

        // �ʿ��� �������� īƮ�� �߰�
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

        // UI ������Ʈ
        cartUI?.UpdateCartDisplay();
    }


    private void InitializeGameScreen()
    {
        // �г� �ʱ�ȭ
        SetActiveAllPanels(false);
        fullViewPanel.SetActive(true);


        // īƮ �ʱ�ȭ
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

        // Back ��ư�� fullViewPanel�� �ƴ� ���� ���̰�
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
            // ���� īƮ�� �����۵��� �ӽ� ����
            List<Item> intermediateCartItems = InteractionManager.Instance.GetCartItems();

            // īƮ �ʱ�ȭ
            InteractionManager.Instance.ClearCart();

            // �߰�ȭ�鿡�� ������ �����۵� �ٽ� �߰�
            foreach (var item in intermediateCartItems)
            {
                InteractionManager.Instance.AddItemToCart(item);
            }

            // �߰��� �ʿ��� ����ȭ�� ���� �����۵� �߰�
            foreach (var item in initialCartItems)
            {
                // �̹� īƮ�� �ִ� �������� �ƴ� ��쿡�� �߰�
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

}
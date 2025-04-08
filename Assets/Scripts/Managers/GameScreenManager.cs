using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // Make sure DOTween is imported
using UnityEngine.EventSystems;
using System.Linq;
using Nursing.Managers;
using Nursing.Interaction;

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
    [SerializeField, Tooltip("화면 전환 애니메이션 효과 타입")] 
    private Ease transitionEase = Ease.InOutQuad;
    
    // 트랜지션 이펙트를 위한 캔버스 그룹
    private CanvasGroup panelCanvasGroup;

    [Header("Popup References")]
    [SerializeField] private GameObject actPopup; // ���� �ִ� ActPopup ����
    [SerializeField] private Button actButton;    // �ൿ�ϱ� ��ư ����



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
     

    }

    

    private void ShowActPopup()
    {
        actPopup.SetActive(true);
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
        
        // 트랜지션 효과 적용하기
        CanvasGroup currentCanvasGroup = currentActivePanel.GetComponent<CanvasGroup>();
        if (currentCanvasGroup == null)
        {
            currentCanvasGroup = currentActivePanel.AddComponent<CanvasGroup>();
        }
        
        CanvasGroup targetCanvasGroup = targetPanel.GetComponent<CanvasGroup>();
        if (targetCanvasGroup == null)
        {
            targetCanvasGroup = targetPanel.AddComponent<CanvasGroup>();
        }
        
        // 트랜지션 시작
        targetPanel.SetActive(true);
        targetCanvasGroup.alpha = 0;
        
        // 현재 패널 페이드 아웃
        currentCanvasGroup.DOFade(0, transitionDuration)
            .SetEase(transitionEase)
            .OnComplete(() => {
                currentActivePanel.SetActive(false);
            });
            
        // 타겟 패널 페이드 인
        targetCanvasGroup.DOFade(1, transitionDuration)
            .SetEase(transitionEase);
        
        currentActivePanel = targetPanel;

        // Back 버튼은 fullViewPanel이 아닐 때만 표시
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
        //작성필요
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
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
    [SerializeField] public IntermediateRequiredItems requiredItems;

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
    public void RefreshCartItems()
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

   

   
    


}
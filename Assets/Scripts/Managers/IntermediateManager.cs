using UnityEngine;
using System.Linq; 
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;  // List 사용을 위한 using 추가
using DG.Tweening;
using Nursing.Managers;
using Nursing.Interaction;

public class IntermediateManager : MonoBehaviour
{
   

    [Header("Screen References")]
    [SerializeField] private GameObject intermediateScreen;
    [SerializeField] private GameObject gameScreen;
    


    [Header("UI References")]
    [SerializeField] private GameObject confirmationPopupPrefab;  // Prefab 참조 추가
    [SerializeField] private Transform popupParent;  // Popup의 부모 Transform
    [SerializeField] private GameObject smallDialoguePrefab;  // SmallDialogue prefab
    [SerializeField] private Transform dialogueParent;  // Dialogue의 부모 Transform
   


    [Header("Procedure Manager")]

    private Item currentHeldItem;
    private GameObject currentSmallDialogue;

    // 싱글톤 인스턴스 추가
    private static IntermediateManager instance;
    public static IntermediateManager Instance => instance;

    [Header("Initial Cart Items")]

    public List<Item> requiredPickedItems = new List<Item>();  // 이미 꺼낸 필수 아이s템들

    // 제외할 아이템은 이제 ProcedureData에서 관리됨


    [SerializeField] private ProcedureManager procedureManager;
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

    // OnGameScreenChanged 메서드 수정
    private void OnGameScreenChanged(GameManager.GameScreen newState)
    {
        // 인터미디에이트 화면으로 전환될 때
        if (newState == GameManager.GameScreen.INTERMEDIATE)
        {
            // 현재 카트의 아이템들을 가져옴
            List<Item> currentCartItems = cartUI?.GetCartItems();

            // 카트 초기화
            cartUI?.ClearCart();
            
        
            // 중간화면에서 표시하지 않을 아이템 목록 가져오기
        List<Item> hiddenItems = procedureManager.GetHiddenInIntermediateItems();
        
        //prepareromm에 있던 거 그대로 가져옴 (숨길 아이템 제외)
        foreach (var item in currentCartItems)
        {
            // 숨겨야 할 아이템이 아닌 경우만 표시
            if (!hiddenItems.Any(hiddenItem => hiddenItem.itemId == item.itemId))
            {
                cartUI?.AddItemToCart(item);
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
        cartUI ? .ClearCart();

        // requiredPickedItems에 있는 항목만 카트에 추가
        foreach (var item in requiredPickedItems)
        {
            cartUI?.AddItemToCart(item);
        }
    }

    // TryPickItemFromCart 메서드 (CartUI.cs에서 호출됨)
    public void AddPickedItem(Item item)
    {
        if (!requiredPickedItems.Contains(item))
        {
            requiredPickedItems.Add(item);
           
        }

        List<Item> cartItems = cartUI?.GetCartItems();
    }

    // 이 메서드를 수정하여 모든 필수 아이템이 선택되었는지 확인
    public bool AreAllRequiredItemsPicked()
    {
        if (procedureManager == null) return false;
        
        var intermediateRequiredItems = procedureManager.GetIntermediateRequiredItems();
        
        return intermediateRequiredItems.Where(ri => !ri.isOptional)
            .All(ri => requiredPickedItems.Any(picked => picked.itemId == ri.item.itemId));
    }

    // 인터미디어트 매니저에 아이템이 필요한지 확인하는 메서드 추가
    public bool IsRequiredItem(Item item)
    {
        if (procedureManager == null) return false;
        
        var intermediateRequiredItems = procedureManager.GetIntermediateRequiredItems();
        
        return intermediateRequiredItems.Any(ri => ri.item.itemId == item.itemId);
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

  
// 아이템을 픽업할 때 호출되는 메서드
public void PickupItem(Item item)
{
    if (item == null) return;

         // currentHeldItem 대신 파라미터로 받은 item 사용
    currentHeldItem = item; // 이 라인 추가
    
    if (procedureManager != null) {
        procedureManager.HandleItemClick(item.itemId);
    }

    }


    


}
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
    private void OnGameScreenChanged(GameManager.GameScreen newState)
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

        if (GameManager.Instance.CurrentGameScreen == GameManager.GameScreen.INTERMEDIATE)
        { cartUI.OpenCart(); }




    }

        

    public bool AreAllRequiredItemsPicked()
    {
        return requiredItems.requiredItems.Where(ri => !ri.isOptional)
            .All(ri => requiredPickedItems.Any(picked => picked.itemId == ri.item.itemId));
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

   

    public bool IsRequiredItem(Item item)
    {
        return requiredItems.requiredItems.Any(ri => ri.item == item);
    }

    public void AddPickedItem(Item item)
    {
        if (!requiredPickedItems.Contains(item))
        {
            requiredPickedItems.Add(item);
            Debug.Log($"Added picked item: {item.itemName}");
        }

        List<Item> cartItems = InteractionManager.Instance.GetCartItems();

       

    }




}
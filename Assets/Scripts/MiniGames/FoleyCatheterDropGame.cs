using UnityEngine;
using UnityEngine.UI;
using Nursing.Interaction;
using Nursing.Managers;
using System.Collections.Generic; 

public class FoleyCatheterDropGame : MonoBehaviour, Nursing.Managers.MiniGameController
{
    [Header("Game Objects")]
    [SerializeField] private Image catheterImage;
    [SerializeField] private RectTransform dropZone;
    
    [Header("Arrow Settings")]
    [SerializeField] private GameObject arrowPrefab; 
    [SerializeField] private bool showDirectionArrows = true;
    [SerializeField] private Vector2 arrowStartPosition;
    
    [Header("Game Settings")]
    [SerializeField] private Vector2 dragDirection = new Vector2(-1f, -1f); // 남서쪽 방향
    [SerializeField] private float directionTolerance = 30f;
    
    // MiniGameController 인터페이스 구현
    public event System.Action<bool> OnGameComplete;
    
    private bool isDragging = false;
    private Vector2 startDragPosition;
    private bool gameCompleted = false;
    private List<GameObject> createdArrows = new List<GameObject>();
    private InteractionManager interactionManager;
    private Canvas mainCanvas;

    private void Awake()
    {
        interactionManager = FindObjectOfType<InteractionManager>();
        mainCanvas = FindObjectOfType<Canvas>();
    }

    private void Start()
    {
        // 이미지에 태그 설정 (기존 인터랙션 시스템과 호환)
        catheterImage.gameObject.tag = "FoleyCatheter";
        dropZone.gameObject.tag = "CatheterDropZone";
        
        // 이미지 초기화 및 위치 설정
        ResetCatheterPosition();
        
        // 화살표 생성 (InteractionManager 방식으로)
        if (showDirectionArrows)
        {
            CreateDirectionArrows();
        }
    }

    private void Update()
    {
        if (gameCompleted) return;
        
        HandleDrag();
    }
    
    private void CreateDirectionArrows()
    {
        // 기존 화살표 제거
        ClearArrows();
        
        if (arrowPrefab == null)
        {
            Debug.LogError("화살표 프리팹이 없습니다.");
            return;
        }

        // 화살표 시작 위치가 설정되지 않은 경우 카테터 이미지 위치 사용
        Vector2 arrowPos = arrowStartPosition;
        if (arrowStartPosition == Vector2.zero)
        {
            arrowPos = catheterImage.transform.position;
        }

        // 화살표 생성
        var arrow = Instantiate(arrowPrefab, mainCanvas.transform);
        arrow.transform.position = arrowPos;

        // 화살표 방향 설정
        float angle = Mathf.Atan2(dragDirection.y, dragDirection.x) * Mathf.Rad2Deg;
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 생성된 화살표 저장
        createdArrows.Add(arrow);
    }
    
    private void ClearArrows()
    {
        foreach (var arrow in createdArrows)
        {
            if (arrow != null)
                Destroy(arrow);
        }
        createdArrows.Clear();
    }
    
    private void HandleDrag()
    {
        // 터치 또는 마우스 입력 처리
        bool isPressed = Input.GetMouseButton(0) || (Input.touchCount > 0);
        bool pressStarted = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        bool pressEnded = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
        
        Vector2 inputPosition = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : Input.mousePosition;
        
        // 드래그 시작
        if (!isDragging && pressStarted)
        {
            // 카테터 이미지 클릭 확인
            if (RectTransformUtility.RectangleContainsScreenPoint(catheterImage.rectTransform, inputPosition))
            {
                isDragging = true;
                startDragPosition = inputPosition;
                
                // 드래그 시작 시 화살표 숨기기
                ClearArrows();
            }
        }
        
        // 드래그 중
        if (isDragging && isPressed)
        {
            // 이미지 위치 업데이트
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform as RectTransform, inputPosition, null, out localPoint))
            {
                catheterImage.rectTransform.localPosition = localPoint;
            }
        }
        
        // 드래그 완료
        if (isDragging && pressEnded)
        {
            Vector2 dragVector = inputPosition - startDragPosition;
            
            // 방향 확인
            Vector2 normalizedDrag = dragVector.normalized;
            float angleBetween = Vector2.Angle(normalizedDrag, dragDirection);
            
            // 드롭존에 있는지 확인
            bool isInDropZone = RectTransformUtility.RectangleContainsScreenPoint(
                dropZone, inputPosition);
            
            // 조건 충족 시 게임 완료 (방향이 맞고 드롭존에 있는 경우)
            if (angleBetween <= directionTolerance && isInDropZone)
            {
                CompleteGame(true);
            }
            else
            {
                // 조건 불충족 시 카테터 원위치
                ResetCatheterPosition();
                // 화살표 다시 표시
                if (showDirectionArrows)
                {
                    CreateDirectionArrows();
                }
            }
            
            isDragging = false;
        }
    }
    
    private void ResetCatheterPosition()
    {
        catheterImage.rectTransform.anchoredPosition = Vector2.zero;
    }
    
    private void CompleteGame(bool success)
    {
        gameCompleted = true;
        ClearArrows();
        OnGameComplete?.Invoke(success);
    }
    
    private void OnDestroy()
    {
        ClearArrows();
    }
}
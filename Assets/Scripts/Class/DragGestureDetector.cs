using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 드래그 제스처를 감지하는 클래스입니다.
/// 다양한 드래그 방식과 패턴을 지원합니다.
/// </summary>
public class DragGestureDetector : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Detection Settings")]
    [SerializeField] private float minDragDistance = 20f;
    [SerializeField] private float dragTimeout = 1.5f;
    
    // 드래그 상태
    private Vector2 dragStartPosition;
    private Vector2 currentDragPosition;
    private bool isDragging = false;
    private Coroutine dragTimeoutCoroutine;
    
    // 드래그 경로 기록
    private Vector2[] dragPathPoints = new Vector2[10];
    private int dragPathIndex = 0;
    
    // 이벤트
    public event Action<Vector2, Vector2, Vector2> OnDragCompleted;
    public event Action<Vector2> OnDragPathUpdated;
    public event Action OnDragCancelled;
    
    // 커스텀 터치 대상 필터링
    [SerializeField] private LayerMask interactableLayers;
    
    private void Awake()
    {
        ClearDragPath();
    }
    
    /// <summary>
    /// 드래그 시작 처리
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드래그 시작 위치 저장
        dragStartPosition = eventData.position;
        currentDragPosition = dragStartPosition;
        
        // 드래그 시작 상태 설정
        isDragging = true;
        
        // 드래그 경로 초기화
        ClearDragPath();
        RecordDragPoint(dragStartPosition);
        
        // 드래그 시간 제한 시작
        if (dragTimeoutCoroutine != null)
            StopCoroutine(dragTimeoutCoroutine);
        dragTimeoutCoroutine = StartCoroutine(DragTimeoutRoutine());
    }
    
    /// <summary>
    /// 드래그 중 처리
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
            
        // 현재 드래그 위치 업데이트
        currentDragPosition = eventData.position;
        
        // 드래그 경로 기록
        RecordDragPoint(currentDragPosition);
        
        // 경로 업데이트 이벤트 발생
        OnDragPathUpdated?.Invoke(currentDragPosition);
    }
    
    /// <summary>
    /// 드래그 종료 처리
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
            
        // 드래그 상태 종료
        isDragging = false;
        
        // 타임아웃 코루틴 중지
        if (dragTimeoutCoroutine != null)
        {
            StopCoroutine(dragTimeoutCoroutine);
            dragTimeoutCoroutine = null;
        }
        
        // 최종 드래그 위치
        Vector2 dragEndPosition = eventData.position;
        
        // 드래그 벡터 계산
        Vector2 dragVector = dragEndPosition - dragStartPosition;
        float dragDistance = dragVector.magnitude;
        
        // 최소 드래그 거리 확인
        if (dragDistance >= minDragDistance)
        {
            // 유효한 드래그인 경우 이벤트 발생
            OnDragCompleted?.Invoke(dragStartPosition, dragEndPosition, dragVector.normalized);
        }
        else
        {
            // 드래그 거리가 충분하지 않은 경우
            OnDragCancelled?.Invoke();
        }
        
        // 드래그 경로 초기화
        ClearDragPath();
    }
    
    /// <summary>
    /// 드래그 경로를 기록합니다.
    /// </summary>
    private void RecordDragPoint(Vector2 point)
    {
        // 기존 점들과 최소 거리 확인
        for (int i = 0; i < dragPathIndex; i++)
        {
            if (Vector2.Distance(dragPathPoints[i], point) < minDragDistance * 0.5f)
                return; // 너무 가까운 점은 무시
        }
        
        // 배열 크기 확인
        if (dragPathIndex >= dragPathPoints.Length)
        {
            // 배열 크기 확장
            Vector2[] newArray = new Vector2[dragPathPoints.Length * 2];
            Array.Copy(dragPathPoints, newArray, dragPathPoints.Length);
            dragPathPoints = newArray;
        }
        
        // 점 기록
        dragPathPoints[dragPathIndex] = point;
        dragPathIndex++;
    }
    
    /// <summary>
    /// 드래그 경로를 초기화합니다.
    /// </summary>
    private void ClearDragPath()
    {
        dragPathIndex = 0;
    }
    
    /// <summary>
    /// 드래그 시간제한 코루틴
    /// </summary>
    private IEnumerator DragTimeoutRoutine()
    {
        yield return new WaitForSeconds(dragTimeout);
        
        // 시간 초과 시 드래그 취소
        if (isDragging)
        {
            isDragging = false;
            OnDragCancelled?.Invoke();
            ClearDragPath();
        }
        
        dragTimeoutCoroutine = null;
    }
    
    /// <summary>
    /// 특정 방향으로의 드래그가 유효한지 확인합니다.
    /// </summary>
    public bool IsValidDirectionalDrag(Vector2 start, Vector2 end, float targetAngle, float tolerance)
    {
        Vector2 dragVector = end - start;
        float dragAngle = Mathf.Atan2(dragVector.y, dragVector.x) * Mathf.Rad2Deg;
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(dragAngle, targetAngle));
        
        return angleDifference <= tolerance;
    }
    
    /// <summary>
    /// 드래그 경로가 특정 형태(예: 원)와 일치하는지 확인합니다.
    /// </summary>
    public bool IsValidShapeDrag(DragShape shape, float matchThreshold)
    {
        // 실제 구현에서는 경로 점들을 분석하여 특정 형태와 일치하는지 확인
        // 현재는 단순히 true 반환
        return true;
    }
}

/// <summary>
/// 드래그 형태 열거형
/// </summary>
public enum DragShape
{
    Line,
    Circle,
    Zigzag,
    Square,
    Triangle
}
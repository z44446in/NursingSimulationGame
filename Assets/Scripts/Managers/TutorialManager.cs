using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 튜토리얼을 관리하는 매니저 클래스
/// 화살표, 하이라이트, 힌트, 안내 등을 제어합니다.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    private static TutorialManager instance;
    public static TutorialManager Instance => instance;

    [Header("Tutorial Elements")]
    [SerializeField] private TutorialArrowSystem arrowSystem;
    [SerializeField] private Transform highlightContainer;
    [SerializeField] private Image highlightImage;
    
    [Header("Tutorial Settings")]
    [SerializeField] private float highlightPulseDuration = 0.5f;
    [SerializeField] private float highlightMinAlpha = 0.3f;
    [SerializeField] private float highlightMaxAlpha = 0.7f;
    [SerializeField] private Color highlightColor = new Color(1f, 1f, 1f, 0.5f);
    
    // 튜토리얼 상태
    // 이 변수는 실제로 다른 메서드에서 참조될 수 있으므로 public으로 변경합니다
    public bool IsTutorialActive { get; private set; } = false;
    private Coroutine currentTutorialCoroutine;
    
    // 하이라이트 애니메이션
    private Sequence highlightSequence;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 하이라이트 초기 설정
            if (highlightImage != null)
            {
                highlightImage.gameObject.SetActive(false);
                highlightImage.color = highlightColor;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 화살표 튜토리얼을 표시합니다.
    /// </summary>
    public void ShowArrowTutorial(Vector2 position, TutorialArrowSystem.ArrowDirection direction, bool animate = true)
    {
        if (arrowSystem == null)
            return;
            
        arrowSystem.ShowArrow(position, direction, animate);
    }
    
    /// <summary>
    /// 화살표 튜토리얼을 숨깁니다.
    /// </summary>
    public void HideArrowTutorial()
    {
        if (arrowSystem == null)
            return;
            
        arrowSystem.HideArrow();
    }
    
    /// <summary>
    /// 영역 하이라이트를 표시합니다.
    /// </summary>
    public void ShowAreaHighlight(Vector2 position, Vector2 size, bool pulse = true)
    {
        if (highlightImage == null || highlightContainer == null)
            return;
            
        // 이미지 위치와 크기 설정
        RectTransform rectTransform = highlightImage.rectTransform;
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;
        
        // 표시
        highlightImage.gameObject.SetActive(true);
        
        // 기존 애니메이션 중지
        if (highlightSequence != null)
        {
            highlightSequence.Kill();
            highlightSequence = null;
        }
        
        // 펄스 애니메이션 시작 (선택적)
        if (pulse)
        {
            // DOTween 시퀀스 생성
            highlightSequence = DOTween.Sequence();
            
            // CanvasGroup을 사용하는 방식으로 페이드 처리
            CanvasGroup canvasGroup = highlightImage.gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = highlightImage.gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = highlightMaxAlpha;
            
            // 시퀀스에 알파값 애니메이션 추가
            highlightSequence.Append(canvasGroup.DOFade(highlightMinAlpha, highlightPulseDuration))
                .Append(canvasGroup.DOFade(highlightMaxAlpha, highlightPulseDuration))
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
    }
    
    /// <summary>
    /// 영역 하이라이트를 숨깁니다.
    /// </summary>
    public void HideAreaHighlight()
    {
        if (highlightImage == null)
            return;
            
        // 애니메이션 중지
        if (highlightSequence != null)
        {
            highlightSequence.Kill();
            highlightSequence = null;
        }
        
        // 숨김
        highlightImage.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 드래그 경로 튜토리얼을 표시합니다.
    /// </summary>
    public void ShowDragPathTutorial(Vector2 start, Vector2 end, float duration = 2f)
    {
        if (currentTutorialCoroutine != null)
        {
            StopCoroutine(currentTutorialCoroutine);
        }
        
        currentTutorialCoroutine = StartCoroutine(AnimateDragPathRoutine(start, end, duration));
    }
    
    /// <summary>
    /// 드래그 경로 애니메이션 코루틴
    /// </summary>
    private IEnumerator AnimateDragPathRoutine(Vector2 start, Vector2 end, float duration)
    {
        // 시작 지점에 화살표 표시
        ShowArrowTutorial(start, GetDirectionForDrag(start, end));
        
        yield return new WaitForSeconds(1f);
        
        // 경로 따라 화살표 이동
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector2 position = Vector2.Lerp(start, end, t);
            
            // 화살표 업데이트
            ShowArrowTutorial(position, GetDirectionForDrag(start, end));
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 종료 지점에 화살표 표시
        ShowArrowTutorial(end, GetDirectionForDrag(start, end));
        
        yield return new WaitForSeconds(1f);
        
        // 다시 처음으로 돌아가기
        currentTutorialCoroutine = StartCoroutine(AnimateDragPathRoutine(start, end, duration));
    }
    
    /// <summary>
    /// 드래그 방향에 따른 화살표 방향 계산
    /// </summary>
    private TutorialArrowSystem.ArrowDirection GetDirectionForDrag(Vector2 start, Vector2 end)
    {
        Vector2 direction = (end - start).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        if (angle < 0)
        {
            angle += 360f;
        }
        
        if (angle >= 337.5f || angle < 22.5f)
        {
            return TutorialArrowSystem.ArrowDirection.Right;
        }
        else if (angle >= 22.5f && angle < 67.5f)
        {
            return TutorialArrowSystem.ArrowDirection.UpRight;
        }
        else if (angle >= 67.5f && angle < 112.5f)
        {
            return TutorialArrowSystem.ArrowDirection.Up;
        }
        else if (angle >= 112.5f && angle < 157.5f)
        {
            return TutorialArrowSystem.ArrowDirection.UpLeft;
        }
        else if (angle >= 157.5f && angle < 202.5f)
        {
            return TutorialArrowSystem.ArrowDirection.Left;
        }
        else if (angle >= 202.5f && angle < 247.5f)
        {
            return TutorialArrowSystem.ArrowDirection.DownLeft;
        }
        else if (angle >= 247.5f && angle < 292.5f)
        {
            return TutorialArrowSystem.ArrowDirection.Down;
        }
        else
        {
            return TutorialArrowSystem.ArrowDirection.DownRight;
        }
    }
    
    /// <summary>
    /// 현재 튜토리얼을 중지합니다.
    /// </summary>
    public void StopCurrentTutorial()
    {
        if (currentTutorialCoroutine != null)
        {
            StopCoroutine(currentTutorialCoroutine);
            currentTutorialCoroutine = null;
        }
        
        HideArrowTutorial();
        HideAreaHighlight();
        
        IsTutorialActive = false;
    }
    
    /// <summary>
    /// 특정 오브젝트를 클릭하도록 안내하는 튜토리얼
    /// </summary>
    public void ShowClickObjectTutorial(Vector2 position, float radius = 50f)
    {
        // 오브젝트 위치에 화살표 표시
        ShowArrowTutorial(position, TutorialArrowSystem.ArrowDirection.Tap);
        
        // 영역 하이라이트
        ShowAreaHighlight(position, new Vector2(radius * 2, radius * 2), true);
    }
    
    /// <summary>
    /// 여러 오브젝트를 순차적으로 클릭하도록 안내하는 튜토리얼
    /// </summary>
    public void ShowSequentialClickTutorial(Vector2[] positions, float duration = 2f)
    {
        if (currentTutorialCoroutine != null)
        {
            StopCoroutine(currentTutorialCoroutine);
        }
        
        currentTutorialCoroutine = StartCoroutine(SequentialClickRoutine(positions, duration));
    }
    
    /// <summary>
    /// 순차적 클릭 코루틴
    /// </summary>
    private IEnumerator SequentialClickRoutine(Vector2[] positions, float duration)
    {
        if (positions == null || positions.Length == 0)
            yield break;
            
        int index = 0;
        
        while (true)
        {
            // 현재 위치에 화살표 표시
            ShowArrowTutorial(positions[index], TutorialArrowSystem.ArrowDirection.Tap);
            ShowAreaHighlight(positions[index], new Vector2(100, 100), true);
            
            yield return new WaitForSeconds(duration);
            
            // 다음 위치로
            index = (index + 1) % positions.Length;
            
            HideAreaHighlight();
        }
    }
    
    private void OnDestroy()
    {
        // 리소스 정리
        if (highlightSequence != null)
        {
            highlightSequence.Kill();
        }
        
        if (currentTutorialCoroutine != null)
        {
            StopCoroutine(currentTutorialCoroutine);
        }
    }
}
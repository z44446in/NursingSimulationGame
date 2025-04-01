using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 튜토리얼 화살표 시스템을 관리하는 클래스
/// 방향, 위치, 애니메이션을 설정할 수 있습니다.
/// </summary>
public class TutorialArrowSystem : MonoBehaviour
{
    [Header("Arrow Settings")]
    [SerializeField] private Image arrowImage;
    [SerializeField] private RectTransform arrowRectTransform;
    [SerializeField] private List<Sprite> arrowSprites = new List<Sprite>();
    
    [Header("Animation Settings")]
    [SerializeField] private float pulseDuration = 0.5f;
    [SerializeField] private float minAlpha = 0.3f;
    [SerializeField] private float maxAlpha = 1.0f;
    [SerializeField] private float moveDistance = 20f;
    
    // 애니메이션 시퀀스
    private Sequence currentAnimation;
    private CanvasGroup canvasGroup;
    
    // 화살표 방향 열거형
    public enum ArrowDirection
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
        Rotate,
        Tap
    }
    
    private void Awake()
    {
        if (arrowImage == null)
        {
            arrowImage = GetComponent<Image>();
        }
        
        if (arrowRectTransform == null && arrowImage != null)
        {
            arrowRectTransform = arrowImage.rectTransform;
        }
        
        // CanvasGroup 가져오기 또는 추가하기
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        HideArrow();
    }
    
    /// <summary>
    /// 화살표를 표시합니다.
    /// </summary>
    public void ShowArrow(Vector2 position, ArrowDirection direction, bool animate = true)
    {
        if (arrowImage == null || arrowRectTransform == null)
            return;
            
        // 위치 설정
        arrowRectTransform.anchoredPosition = position;
        
        // 방향에 따른 회전 설정
        float rotation = GetRotationForDirection(direction);
        arrowRectTransform.eulerAngles = new Vector3(0, 0, rotation);
        
        // 스프라이트 설정
        int spriteIndex = (int)direction;
        if (spriteIndex < arrowSprites.Count)
        {
            arrowImage.sprite = arrowSprites[spriteIndex];
        }
        
        // 화살표 표시
        arrowImage.gameObject.SetActive(true);
        
        // 애니메이션 설정
        if (animate)
        {
            StartArrowAnimation(direction);
        }
    }
    
    /// <summary>
    /// 화살표를 숨깁니다.
    /// </summary>
    public void HideArrow()
    {
        if (arrowImage == null)
            return;
            
        arrowImage.gameObject.SetActive(false);
        
        // 애니메이션 정지
        StopArrowAnimation();
    }
    
    /// <summary>
    /// 방향에 따른 회전 각도를 반환합니다.
    /// </summary>
    private float GetRotationForDirection(ArrowDirection direction)
    {
        switch (direction)
        {
            case ArrowDirection.Up:
                return 0f;
            case ArrowDirection.Right:
                return 90f;
            case ArrowDirection.Down:
                return 180f;
            case ArrowDirection.Left:
                return 270f;
            case ArrowDirection.UpRight:
                return 45f;
            case ArrowDirection.DownRight:
                return 135f;
            case ArrowDirection.DownLeft:
                return 225f;
            case ArrowDirection.UpLeft:
                return 315f;
            case ArrowDirection.Rotate:
                return 0f; // 특수 회전 애니메이션
            case ArrowDirection.Tap:
                return 0f; // 특수 탭 애니메이션
            default:
                return 0f;
        }
    }
    
    /// <summary>
    /// 화살표 애니메이션을 시작합니다.
    /// </summary>
    private void StartArrowAnimation(ArrowDirection direction)
    {
        // 기존 애니메이션 정지
        StopArrowAnimation();
        
        // 초기 설정
        canvasGroup.alpha = maxAlpha;
        
        // 새 애니메이션 시퀀스 생성
        currentAnimation = DOTween.Sequence();
        
        // 방향에 따른 애니메이션 설정
        switch (direction)
        {
            case ArrowDirection.Rotate:
                // 회전 애니메이션
                currentAnimation.Append(arrowRectTransform.DORotate(new Vector3(0, 0, 360), pulseDuration * 2, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart));
                break;
                
            case ArrowDirection.Tap:
                // 탭 애니메이션 (확대/축소)
                currentAnimation.Append(arrowRectTransform.DOScale(1.2f, pulseDuration / 2))
                    .Append(arrowRectTransform.DOScale(1f, pulseDuration / 2))
                    .SetLoops(-1, LoopType.Restart);
                break;
                
            default:
                // 기본 방향 애니메이션 (투명도 + 이동)
                Vector2 moveOffset = GetMoveOffsetForDirection(direction);
                Vector2 originalPos = arrowRectTransform.anchoredPosition;
                Vector2 targetPos = originalPos + moveOffset;
                
                currentAnimation.Append(canvasGroup.DOFade(minAlpha, pulseDuration))
                    .Join(arrowRectTransform.DOAnchorPos(targetPos, pulseDuration))
                    .Append(canvasGroup.DOFade(maxAlpha, pulseDuration))
                    .Join(arrowRectTransform.DOAnchorPos(originalPos, pulseDuration))
                    .SetLoops(-1, LoopType.Restart);
                break;
        }
    }
    
    /// <summary>
    /// 화살표 애니메이션을 정지합니다.
    /// </summary>
    private void StopArrowAnimation()
    {
        if (currentAnimation != null)
        {
            currentAnimation.Kill();
            currentAnimation = null;
        }
        
        // 원래 상태로 복원
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1.0f;
        }
        
        if (arrowRectTransform != null)
        {
            arrowRectTransform.localScale = Vector3.one;
        }
    }
    
    /// <summary>
    /// 방향에 따른 이동 오프셋을 반환합니다.
    /// </summary>
    private Vector2 GetMoveOffsetForDirection(ArrowDirection direction)
    {
        switch (direction)
        {
            case ArrowDirection.Up:
                return new Vector2(0, moveDistance);
            case ArrowDirection.Right:
                return new Vector2(moveDistance, 0);
            case ArrowDirection.Down:
                return new Vector2(0, -moveDistance);
            case ArrowDirection.Left:
                return new Vector2(-moveDistance, 0);
            case ArrowDirection.UpRight:
                return new Vector2(moveDistance * 0.7f, moveDistance * 0.7f);
            case ArrowDirection.DownRight:
                return new Vector2(moveDistance * 0.7f, -moveDistance * 0.7f);
            case ArrowDirection.DownLeft:
                return new Vector2(-moveDistance * 0.7f, -moveDistance * 0.7f);
            case ArrowDirection.UpLeft:
                return new Vector2(-moveDistance * 0.7f, moveDistance * 0.7f);
            default:
                return Vector2.zero;
        }
    }
    
    private void OnDestroy()
    {
        StopArrowAnimation();
    }
}
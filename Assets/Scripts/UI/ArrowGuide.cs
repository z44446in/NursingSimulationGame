using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Nursing.UI
{
    public class ArrowGuide : MonoBehaviour
    {
        [Header("화살표 설정")]
        [SerializeField] private Image arrowImage;
        [SerializeField] private float blinkDuration = 0.5f;
        //[SerializeField] private float blinkInterval = 0.3f;
        
        [Header("애니메이션 설정")]
        [SerializeField] private float moveDistance = 30f;
        [SerializeField] private float moveDuration = 1f;
        [SerializeField] private Ease moveEase = Ease.InOutQuad;
        
        private RectTransform rectTransform;
        private CanvasGroup canvasGroup;
        private Sequence blinkSequence;
        private Sequence moveSequence;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (arrowImage == null)
                arrowImage = GetComponent<Image>();
                
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        private void OnEnable()
        {
            StartAnimations();
        }
        
        private void OnDisable()
        {
            StopAnimations();
        }
        
        /// <summary>
        /// 화살표 방향을 설정합니다.
        /// </summary>
        public void SetDirection(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        
        /// <summary>
        /// 애니메이션을 시작합니다.
        /// </summary>
        private void StartAnimations()
        {
            StopAnimations();
            
            // 깜빡임 애니메이션
            blinkSequence = DOTween.Sequence();
            blinkSequence.Append(canvasGroup.DOFade(1f, blinkDuration / 2))
                .Append(canvasGroup.DOFade(0.5f, blinkDuration / 2))
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            
            // 이동 애니메이션
            Vector2 originalPosition = rectTransform.anchoredPosition;
            Vector2 direction = new Vector2(
                Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad), 
                Mathf.Sin(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)
            );
            Vector2 targetPosition = originalPosition + direction.normalized * moveDistance;
            
            moveSequence = DOTween.Sequence();
            moveSequence.Append(rectTransform.DOAnchorPos(targetPosition, moveDuration).SetEase(moveEase))
                .Append(rectTransform.DOAnchorPos(originalPosition, moveDuration).SetEase(moveEase))
                .SetLoops(-1, LoopType.Restart);
        }
        
        /// <summary>
        /// 애니메이션을 중지합니다.
        /// </summary>
        private void StopAnimations()
        {
            if (blinkSequence != null && blinkSequence.IsActive())
            {
                blinkSequence.Kill();
                blinkSequence = null;
            }
            
            if (moveSequence != null && moveSequence.IsActive())
            {
                moveSequence.Kill();
                moveSequence = null;
            }
        }
        
        /// <summary>
        /// 화살표 색상을 설정합니다.
        /// </summary>
        public void SetColor(Color color)
        {
            if (arrowImage != null)
            {
                arrowImage.color = color;
            }
        }
        
        private void OnDestroy()
        {
            StopAnimations();
        }
    }
}
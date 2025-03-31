using UnityEngine;
using UnityEngine.UI;

public abstract class BasePopup : MonoBehaviour
{
    [SerializeField] protected Image backgroundPanel;

    protected virtual void Awake()
    {
        if (backgroundPanel == null)
        {
            CreateBackgroundPanel();
        }
        SetBackgroundToFullScreen();
    }

    private void CreateBackgroundPanel()
    {
        GameObject bgObject = new GameObject("BackgroundPanel");
        bgObject.transform.SetParent(transform, false);
        bgObject.transform.SetAsFirstSibling();

        // Canvas Group 추가
        CanvasGroup canvasGroup = bgObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        RectTransform rectTransform = bgObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        backgroundPanel = bgObject.AddComponent<Image>();
        backgroundPanel.color = new Color(0, 0, 0, 0.5f);
        backgroundPanel.raycastTarget = true;
    }

    private void SetBackgroundToFullScreen()
    {
        if (backgroundPanel != null)
        {
            RectTransform bgRect = backgroundPanel.GetComponent<RectTransform>();
            if (bgRect != null)
            {
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
            }
        }
    }

    // ClosePopup 메서드 추가
    public virtual void ClosePopup()
    {
        // 애니메이션이나 다른 정리 작업이 필요한 경우 자식 클래스에서 오버라이드
        Destroy(gameObject);
        
    }

    public virtual void DisablePopup()
    {
        gameObject.SetActive(false);
    }


}
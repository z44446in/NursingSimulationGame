using UnityEngine;
using TMPro;
using DG.Tweening;

public class GuidePanel : MonoBehaviour
{

    private static GuidePanel instance;
    public static GuidePanel Instance => instance;


    [SerializeField] private TextMeshProUGUI guideText;
    [SerializeField] private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowGuide(string text)
    {
        gameObject.SetActive(true);
        guideText.text = text;

        // 페이드 인 효과
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(0.8f, 0.3f);
    }

    public void HideGuide()
    {
        canvasGroup.DOFade(0f, 0.3f).OnComplete(() => {
            gameObject.SetActive(false);
        });
    }

    public void UpdateGuideText(string newText)
    {
        guideText.text = newText;
    }
}
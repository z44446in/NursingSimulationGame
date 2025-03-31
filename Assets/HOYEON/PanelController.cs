using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PanelController : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject panel1;
    [SerializeField] private GameObject panel2;
    
    [Header("Button References")]
    [SerializeField] private Button button1; // Panel1에서 Panel2로 전환하는 버튼
    [SerializeField] private Button button2; // Panel2에서 Panel1로 전환하는 버튼
    
    [Header("Transition Settings")]
    [SerializeField] private float fadeTime = 0.5f; // 페이드 효과 시간

    // 각 패널의 CanvasGroup 컴포넌트
    private CanvasGroup panel1CanvasGroup;
    private CanvasGroup panel2CanvasGroup;

    private void Awake()
    {
        // 각 패널에 CanvasGroup 컴포넌트가 없다면 추가
        panel1CanvasGroup = panel1.GetComponent<CanvasGroup>();
        if (panel1CanvasGroup == null)
            panel1CanvasGroup = panel1.AddComponent<CanvasGroup>();

        panel2CanvasGroup = panel2.GetComponent<CanvasGroup>();
        if (panel2CanvasGroup == null)
            panel2CanvasGroup = panel2.AddComponent<CanvasGroup>();

        // 버튼 이벤트 리스너 추가
        button1.onClick.AddListener(SwitchToPanel2);
        button2.onClick.AddListener(SwitchToPanel1);

        // 초기 상태 설정: Panel1 활성화, Panel2 비활성화
        InitializePanels();
    }

    private void InitializePanels()
    {
        panel1.SetActive(true);
        panel2.SetActive(false);
        panel1CanvasGroup.alpha = 1f;
        panel2CanvasGroup.alpha = 0f;
    }

    private void SwitchToPanel2()
    {
        // Panel2를 먼저 활성화하고 알파값 0으로 설정
        panel2.SetActive(true);
        panel2CanvasGroup.alpha = 0f;

        // Panel1 페이드 아웃
        panel1CanvasGroup.DOFade(0f, fadeTime).OnComplete(() => {
            panel1.SetActive(false);
        });

        // Panel2 페이드 인
        panel2CanvasGroup.DOFade(1f, fadeTime);
    }

    private void SwitchToPanel1()
    {
        // Panel1를 먼저 활성화하고 알파값 0으로 설정
        panel1.SetActive(true);
        panel1CanvasGroup.alpha = 0f;

        // Panel2 페이드 아웃
        panel2CanvasGroup.DOFade(0f, fadeTime).OnComplete(() => {
            panel2.SetActive(false);
        });

        // Panel1 페이드 인
        panel1CanvasGroup.DOFade(1f, fadeTime);
    }

    private void OnDestroy()
    {
        // 버튼 이벤트 리스너 제거
        button1.onClick.RemoveListener(SwitchToPanel2);
        button2.onClick.RemoveListener(SwitchToPanel1);

        // 실행 중인 모든 DOTween 애니메이션 종료
        panel1CanvasGroup.DOKill();
        panel2CanvasGroup.DOKill();
    }
}
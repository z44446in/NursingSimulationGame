using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UIManager: 게임 UI를 관리하고 상태에 따라 패널을 전환하는 클래스.
/// </summary>
public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    /// <summary>
    /// 전역 UIManager 인스턴스에 접근.
    /// </summary>
    public static UIManager Instance => instance;

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;    // 메인 게임 UI
    [SerializeField] private GameObject pausePanel;   // 일시정지 패널
    [SerializeField] private GameObject resultPanel;  // 결과 패널

    [Header("Game Info")]
    [SerializeField] private TextMeshProUGUI timeText; // 시간 표시 텍스트
    [SerializeField] private TextMeshProUGUI guideText; // 가이드 텍스트 
    [SerializeField] private TextMeshProUGUI scoreText; // 점수 표시 텍스트

    private float elapsedTime; // 경과 시간 추적

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeUI();
        }
        else
        {
            Destroy(gameObject); // 중복된 UIManager 제거
        }
    }

    private void Start()
    {
        // GameManager의 상태 변경 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }

    /// <summary>
    /// UI 상태를 초기화.
    /// </summary>
    private void InitializeUI()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    private void Update()
    {
        // 게임이 진행 중일 때만 시간 업데이트
        if (GameManager.Instance != null && GameManager.Instance.CurrentGameState == GameManager.GameState.PLAYING)
        {
            UpdateTimeDisplay();
        }
    }

    /// <summary>
    /// 경과 시간을 업데이트하여 UI에 표시.
    /// </summary>
    private void UpdateTimeDisplay()
    {
        elapsedTime = Time.time; // Time.time은 전체 경과 시간을 반환
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        if (timeText != null)
        {
            timeText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    /// <summary>
    /// GameState 변경에 따른 UI 상태 전환.
    /// </summary>
    private void OnGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.PLAYING)
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
        }
        else if (newState == GameManager.GameState.PAUSE)
        {
            if (pausePanel != null) pausePanel.SetActive(true);
        }
        else if (newState == GameManager.GameState.END)
        {
            if (resultPanel != null) resultPanel.SetActive(true);
        }
    }

    
    /// <summary>
    /// 가이드 텍스트를 업데이트합니다.
    /// </summary>
    public void UpdateGuideText(string text)
    {
        if (guideText != null)
        {
            guideText.text = text;
        }
    }

    /// <summary>
    /// 점수를 업데이트합니다.
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    /// <summary>
    /// 파괴 시 이벤트 구독 해제.
    /// </summary>
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}

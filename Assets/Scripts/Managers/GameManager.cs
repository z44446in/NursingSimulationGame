using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;
using Nursing.Procedure;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static GameManager instance = null;

    /// <summary>
    /// GameManager의 전역 인스턴스에 접근하는 프로퍼티.
    /// </summary>
    public static GameManager Instance => instance;

    public ProcedureTypeEnum currentProcedureType;
    public ProcedureVersionType currentVersionType;
    public ProcedurePlayType currentPlayType;

    public enum GameState // 게임 상태 정의
    {
        INIT, // 메인홈
        READY, // 준비화면 상태
        PLAYING, // 술기시작 = 중간화면 + 게임화면
        PAUSE, // 게임 정지
        END // 게임 종료 
    }

    public enum GameScreen  // 게임 화면 정의
    {
        PREPARING,    // 준비실 상태 추가
        INTERMEDIATE, // 중간 상태 추가
        GAMESCREEN    // 게임 상태 추가
    }

    // 현재 게임 상태 저장
    private GameState currentGameState;

    // 현재 게임 화면 저장
    private GameScreen currentGameScreen;

    /// 현재 게임 상태를 외부에서 읽을 수 있으며, 변경 시 이벤트 발생.
    public GameState CurrentGameState
    {
        get => currentGameState;
        private set
        {
            if (currentGameState != value) // 상태가 변경된 경우만 처리
            {
                currentGameState = value;
                OnGameStateChanged?.Invoke(currentGameState); // 상태 변경 이벤트 호출
            }
        }
    }

    /// 현재 게임 화면을 외부에서 읽을 수 있으며, 변경 시 이벤트 발생.
    public GameScreen CurrentGameScreen
    {
        get => currentGameScreen;
        private set
        {
            if (currentGameScreen != value) // 상태가 변경된 경우만 처리
            {
                currentGameScreen = value;
                OnGameScreenChanged?.Invoke(currentGameScreen); // 상태 변경 이벤트 호출
            }
        }
    }

    /// 게임 상태 변경 시 호출되는 이벤트.
    public event Action<GameState> OnGameStateChanged;
    public event Action<GameScreen> OnGameScreenChanged;

    public void ChangeScreen(GameScreen newScreen)
    {
        if (CurrentGameScreen != newScreen)
        {
            CurrentGameScreen = newScreen;
            OnGameScreenChanged?.Invoke(newScreen); // 화면 변경 이벤트 호출
        }
    }

    // 씬 이름 상수
    public const string MAIN_HOME_SCENE = "MainHome";
    public const string MAIN_GAME_SCENE = "MainGame";

    // 씬 전환 여부를 체크하는 플래그
    private bool isTransitioning = false;

    private void Awake() // 초기화: 싱글톤 인스턴스 설정 및 이벤트 구독.
    {
        DOTween.SetTweensCapacity(500, 50);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정
            SceneManager.sceneLoaded += OnSceneLoaded; // 씬 로드 이벤트 구독
        }
        else
        {
            Destroy(gameObject); // 중복 GameManager 파괴
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
        /// GameManager가 파괴될 때 이벤트 구독 해제.
    }

    public void ChangeGameState(GameState newState)  // 겜상태 변경
    {
        if (!isTransitioning && currentGameState != newState)
        {
            CurrentGameState = newState; // 새로운 상태로 변경
        }
    }

    public void ChangeGameScreen(GameScreen newScreen) // 겜화면 변경
    {
        if (!isTransitioning && currentGameScreen != newScreen)
        {
            CurrentGameScreen = newScreen; // 새로운 화면으로 변경
        }
    }

    public void LoadScene(string sceneName) // 지정씬 로드
    {
        if (isTransitioning) return; // 이미 전환 중인 경우 무시

        isTransitioning = true; // 전환 중 플래그 설정
        SceneManager.LoadScene(sceneName); // 씬 로드
    }

    /// <summary>
    /// 게임을 시작하는 메서드.
    /// </summary>
    public void StartProcedure() // 술기 시작 
    {
        if (CurrentGameState == GameState.READY) // 레디에서 플레이 상태로!  
        {
            CurrentGameState = GameState.PLAYING;
        }
    }

    public void PauseGame() // 게임 일시정지
    {
        if (CurrentGameState == GameState.PLAYING || CurrentGameState == GameState.READY)
        {
            CurrentGameState = GameState.PAUSE;
            Time.timeScale = 0f; // 게임 루프 정지
        }
    }

    public void ResumeGame() // 일시정지에서 게임 다시 시작
    {
        if (CurrentGameState == GameState.PAUSE)
        {
            if (CurrentGameScreen == GameScreen.PREPARING)
            {
                CurrentGameState = GameState.READY;
            }
            else
            {
                CurrentGameState = GameState.PLAYING;
                Time.timeScale = 1f; // 게임 루프 재개 
            }
        }
    }

    public void EndGame() // 겜 종료
    {
        if (CurrentGameState != GameState.END)
        {
            CurrentGameState = GameState.END;
        }
    }

    public void StartGameScene() // 게임씬 시작!! 레디 상태, 준비화면
    {
        if (CurrentGameState == GameState.INIT)
        {
            CurrentGameState = GameState.READY;
            CurrentGameScreen = GameScreen.PREPARING;
            LoadScene(MAIN_GAME_SCENE);
        }
    }

    public void DebugforPrepare() // 디버그용 준비화면! 나중에 비활성화 
    {
       
            CurrentGameState = GameState.READY;
            CurrentGameScreen = GameScreen.PREPARING;
            Debug.Log($"현재 씬은?: {GameManager.Instance.CurrentGameScreen}");
            Debug.Log($"현재 게임상태는?: {GameManager.Instance.CurrentGameState}");

        
    }
    

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isTransitioning = false;

        if (scene.name == MAIN_HOME_SCENE)
        {
            CurrentGameState = GameState.INIT;
        }
        else if (scene.name == MAIN_GAME_SCENE)
        {
            CurrentGameState = GameState.READY;
        }
    }

    public void GoToGameScreen()
    {
        
            
            CurrentGameScreen = GameScreen.GAMESCREEN;
            Debug.Log($"현재 게임화면: {CurrentGameScreen}");
            Debug.Log($"현재 게임상태: {CurrentGameState}");
      
    }

    public void GoToIntermediate()
    {
       
        CurrentGameScreen = GameScreen.INTERMEDIATE;
        StartProcedure();
        Debug.Log($"현재 게임화면: {CurrentGameScreen}");
        Debug.Log($"현재 게임상태: {CurrentGameState}");
    }

    public GameScreen GetCurrentScreen()
    {
         if (IntermediateManager.Instance != null && IntermediateManager.Instance.gameObject.activeSelf)
        {
            return GameScreen.INTERMEDIATE;
        }
        else if (GameScreenManager.Instance != null && GameScreenManager.Instance.gameObject.activeSelf)
        {
            return GameScreen.GAMESCREEN;
        }

        return GameScreen.PREPARING; // Default 값
    }

    // proceduretype설정 
    public void SetCurrentProcedureType(ProcedureTypeEnum procedureType)
    {
        // 프로시저 타입 설정 로직
        currentProcedureType = procedureType;
        Debug.Log($"GameManager: Set current procedure type to {procedureType}");
    }

    public void SetProcedureVersionType(ProcedureVersionType versionType)
    {
        // 버전 타입 설정 로직
        currentVersionType = versionType;
        Debug.Log($"GameManager: Set version type to {versionType}");
    }

    public void SetProcedurePlayType(ProcedurePlayType playType)
    {
        // 플레이 타입 설정 로직
        currentPlayType = playType;
        Debug.Log($"GameManager: Set play type to {playType}");
    }




}

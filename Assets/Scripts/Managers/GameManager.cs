using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static GameManager instance = null;

    /// <summary>
    /// GameManager의 전역 인스턴스에 접근하는 프로퍼티.
    /// </summary>
    public static GameManager Instance => instance;

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

    private void Start()
    {
        // GameManager의 이벤트 구독 코드...

        // CommonUI Canvas의 Alpha 값을 항상 1로 유지
        GameObject commonUICanvas = GameObject.Find("CommonUI Canvas");
        if (commonUICanvas != null)
        {
            CanvasGroup canvasGroup = commonUICanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;

                // 게임 상태 변경 시 Alpha 값을 확인하고 복원하는 코드 추가
                OnGameStateChanged += (GameState state) => {
                    if (canvasGroup.alpha < 1f)
                    {
                        Debug.Log("Restoring CommonUI Canvas alpha to 1");
                        canvasGroup.alpha = 1f;
                    }
                };
            }
        }
        
        // 상호작용 데이터 등록 처리
        RegisterInteractionData();
    }
    
    /// <summary>
    /// 상호작용 데이터를 찾아서 등록합니다
    /// </summary>
    private void RegisterInteractionData()
    {
        // 1. 먼저 InteractionDataRegistrar가 씬에 있는지 확인
        InteractionDataRegistrar registrar = FindObjectOfType<InteractionDataRegistrar>();
        
        // 2. 없으면 새로 생성
        if (registrar == null)
        {
            GameObject registrarObj = new GameObject("InteractionDataRegistrar");
            registrar = registrarObj.AddComponent<InteractionDataRegistrar>();
            Debug.Log("Created InteractionDataRegistrar gameobject");
        }
        
        // 3. Resources 폴더에서 모든 GenericInteractionData 에셋 로드
        // Resources 폴더에 GenericInteractionData를 저장해야 합니다
        // 예: Resources/Interactions/aa.asset
        GenericInteractionData[] interactions = Resources.LoadAll<GenericInteractionData>("");
        
        if (interactions != null && interactions.Length > 0)
        {
            foreach (var interaction in interactions)
            {
                registrar.AddGenericInteraction(interaction);
                Debug.Log($"Added interaction data: {interaction.interactionName} (ID: {interaction.interactionId})");
            }
        }
        else
        {
            Debug.LogWarning("No GenericInteractionData assets found in Resources folder. " +
                "Make sure your interaction data is in a Resources folder.");
            
            // 직접 경로를 지정하여 특정 상호작용 데이터 로드 (런타임에는 작동하지 않음)
            #if UNITY_EDITOR
            // 4. Resources 폴더에 없으면 직접 에셋 경로를 지정하여 로드 시도
            string[] possiblePaths = new string[] 
            { 
                "Assets/ScriptableObjects",
                "Assets/ScriptableObjects/Interactions"
            };
            
            foreach (string path in possiblePaths)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GenericInteractionData", new[] { path });
                
                if (guids != null && guids.Length > 0)
                {
                    foreach (string guid in guids)
                    {
                        string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        GenericInteractionData data = UnityEditor.AssetDatabase.LoadAssetAtPath<GenericInteractionData>(assetPath);
                        
                        if (data != null)
                        {
                            registrar.AddGenericInteraction(data);
                            Debug.Log($"Added interaction data from path: {data.interactionName} (ID: {data.interactionId})");
                        }
                    }
                    break; // 에셋을 찾았으면 루프 종료
                }
            }
            #endif
        }
    }
    // GameManager.cs의 Awake 메서드 수정
    private void Awake()
    {
        DOTween.SetTweensCapacity(500, 50);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 파괴되지 않도록 설정

            // CommonUI Canvas 찾아서 DontDestroyOnLoad 설정
            GameObject commonUICanvas = GameObject.Find("CommonUI Canvas");
            if (commonUICanvas != null)
            {
                DontDestroyOnLoad(commonUICanvas);
                Debug.Log("CommonUI Canvas set to DontDestroyOnLoad");
            }
            else
            {
                Debug.LogWarning("CommonUI Canvas not found in the scene!");
            }

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





}

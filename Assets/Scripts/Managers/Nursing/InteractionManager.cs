using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Nursing.Interaction;
using Nursing.Penalty;
using Nursing.UI;
using System.Linq;

namespace Nursing.Managers
{
    public class InteractionManager : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private PenaltyManager penaltyManager;
        [SerializeField] private DialogueManager dialogueManager;
        [SerializeField] private GameObject quizPopupPrefab;
        [SerializeField] private Canvas mainCanvas; // UI 생성을 위한 캔버스 참조
        
        [Header("화살표 가이드")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private float arrowBlinkDuration = 0.5f;
        [SerializeField] private float arrowBlinkInterval = 0.3f;
        
        private InteractionData currentInteraction;
        private int currentStageIndex = -1;
        private InteractionStage currentStage;
        private Dictionary<string, GameObject> taggedObjectsCache = new Dictionary<string, GameObject>();
        private List<GameObject> createdArrows = new List<GameObject>();
        private bool interactionInProgress = false;
        private bool isDragging = false;
        private Vector2 dragStartPosition;
        private GameObject draggedObject;
        private Coroutine arrowBlinkCoroutine;
        private List<Coroutine> activeCoroutines = new List<Coroutine>();

       

        // 인터랙션 완료 이벤트
        public event System.Action<bool> OnInteractionComplete;

        private Dictionary<int, List<GameObject>> fingerArrows = new Dictionary<int, List<GameObject>>();
        // 필요한 준비
        Dictionary<int, FingerDragStatus> fingerDragStatus = new();   // fingerId → status
        Dictionary<int, int> fingerToSetting = new();                 // fingerId → assigned setting index
        HashSet<int> usedSettingIndices = new();                      // 중복 방지

        bool waitingForSimultaneousStart = true;                      // 상태 flag

        private class FingerDragStatus
        {
            public bool isDragging = false;
            public bool isComplete = false;
            public Vector2 startPosition;
            public GameObject draggedObject;
            public int matchedSettingIndex = -1;
        }

       


        



        private void Awake()
        {
            if (penaltyManager == null)
                penaltyManager = FindObjectOfType<PenaltyManager>();
                
            if (dialogueManager == null)
                dialogueManager = FindObjectOfType<DialogueManager>();
                
            if (mainCanvas == null)
                mainCanvas = FindObjectOfType<Canvas>();
        }
        
        /// <summary>
        /// 인터랙션을 시작합니다.
        /// </summary>
        public void StartInteraction(InteractionData interactionData)
        {
            if (interactionData == null)
            {
                Debug.LogError("인터랙션 데이터가 없습니다.");
                return;
            }
            
            // 진행 중인 인터랙션이 있으면 정리
            CleanupCurrentInteraction();
            
            currentInteraction = interactionData;
            currentStageIndex = -1;
            
            // 가이드 메시지 표시
            
            
            // 첫 스테이지로 진행
            AdvanceToNextStage();
        }
        
        /// <summary>
        /// 다음 인터랙션 스테이지로 진행합니다.
        /// </summary>
        public void AdvanceToNextStage()
        {
            // 모든 화살표 제거
            ClearArrows();
            
            // 활성 코루틴 정리
            foreach (var coroutine in activeCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
            activeCoroutines.Clear();
            
            currentStageIndex++;
            
            // 모든 스테이지를 완료했는지 확인
            if (currentStageIndex >= currentInteraction.stages.Count)
            {
                CompleteInteraction();
                return;
            }
            
            currentStage = currentInteraction.stages[currentStageIndex];
            
            // 가이드 메시지 업데이트
            if (!string.IsNullOrEmpty(currentStage.guideMessage) && dialogueManager != null)
            {
                dialogueManager.ShowGuideMessage(currentStage.guideMessage);
            }
            
            // 순서 확인 - 이전 스테이지가 완료되어야 하는 경우
            if (currentStage.requireSpecificOrder && currentStage.requiredPreviousStageIds.Count > 0)
            {
                bool validOrder = true;
                // 필요한 이전 스테이지가 모두 완료되었는지 확인 로직
                // 실제 구현에서는 완료된 스테이지 ID를 추적하는 방식으로 구현
                
                if (!validOrder && currentStage.incorrectOrderPenalty != null)
                {
                    ApplyPenalty(currentStage.incorrectOrderPenalty);
                    return; // 순서가 잘못된 경우 진행하지 않음
                }
            }
            
            // 스테이지 타입에 따른 처리
            SetupStageBasedOnType();
        }
        
        /// <summary>
        /// 현재 스테이지 타입에 따라 인터랙션을 설정합니다.
        /// </summary>
        private void SetupStageBasedOnType()
        {
            switch (currentStage.interactionType)
            {
                case InteractionType.SingleDragInteraction:
                    SetupSingleDragInteraction();
                    break;

                case InteractionType.MultiDragInteraction:
                    SetupMultiDragInteraction();
                    break;

                case InteractionType.ObjectCreation:
                    SetupObjectCreation();
                    break;
                    
                case InteractionType.ConditionalClick:
                    SetupConditionalClick();
                    break;
                    
                case InteractionType.SustainedClick:
                    SetupSustainedClick();
                    break;
                    
                case InteractionType.ObjectDeletion:
                    SetupObjectDeletion();
                    break;
                    
                case InteractionType.ObjectMovement:
                    SetupObjectMovement();
                    break;
                    
                case InteractionType.QuizPopup:
                    SetupQuizPopup();
                    break;
                    
                case InteractionType.MiniGame:
                    SetupMiniGame();
                    break;
                    
                default:
                    Debug.LogWarning("지원하지 않는 인터랙션 타입입니다: " + currentStage.interactionType);
                    AdvanceToNextStage(); // 지원하지 않는 타입은 건너뜁니다.
                    break;
            }
        }

        #region 인터랙션 타입별 설정 메서드
        /// <summary>
        /// 드래그 인터랙션을 설정합니다.
        /// </summary>

        private void SetupSingleDragInteraction()
        {
            // 기존 SetupDragInteraction() 메서드의 내용을 옮김
            var settings = currentStage.settings;

            if (settings == null)
            {
                Debug.LogError("드래그 인터랙션 설정이 없습니다.");
                AdvanceToNextStage();
                return;
            }

            // 방향 화살표 설정
            if (settings.haveDirection)
            {
                if (settings.requiredDragDirection && settings.showDirectionArrows)
                {
                    // 화살표 시작 위치 설정
                    Vector2 arrowPos = settings.arrowStartPosition;

                    // 화살표 시작 위치가 없으면 대상 오브젝트 대상으로 생성
                    if (settings.arrowStartPosition == Vector2.zero )
                    {
                        GameObject targetObj = GameObject.FindWithTag(settings.targetObjectTag);
                        if (targetObj != null)
                        {
                            arrowPos = targetObj.transform.position;
                            settings.arrowStartPosition = arrowPos; // 위치 업데이트
                        }
                    }

                    // 화살표 생성
                    CreateDirectionArrows(arrowPos, settings.arrowDirection);
                }
            }

            interactionInProgress = true; // 인터랙션 활성화
        }

        private void SetupMultiDragInteraction()
        {
            var settings = currentStage.settings;

            if (settings == null || settings.fingerSettings.Count == 0)
            {
                Debug.LogError("다중 드래그 인터랙션 설정이 없거나 손가락 설정이 없습니다.");
                AdvanceToNextStage();
                return;
            }

            // 각 손가락 설정에 대한 화살표 생성
            for (int i = 0; i < settings.fingerSettings.Count; i++)
            {
                var fingerSetting = settings.fingerSettings[i];

                if (fingerSetting.haveDirection && fingerSetting.requiredDragDirection && fingerSetting.showDirectionArrows)
                {
                    Vector2 arrowPos = fingerSetting.arrowStartPosition;

                    

                    // 화살표 생성 (각 손가락에 고유한 식별자 추가)
                    CreateDirectionArrows(arrowPos, fingerSetting.arrowDirection, i);
                }
            }

            // 손가락 상태 정보 초기화
            fingerDragStatus.Clear();
            for (int i = 0; i < settings.fingerSettings.Count; i++)
            {
                fingerDragStatus.Add(i, new FingerDragStatus());
            }

            interactionInProgress = true;
        }

        /// <summary>
        /// 오브젝트 생성 인터랙션을 설정합니다.
        /// </summary>
        private void SetupObjectCreation()
        {
            var settings = currentStage.settings;

            if (settings == null || settings.objectToCreate == null || settings.objectToCreate.Length == 0)
            {
                Debug.LogError("오브젝트 생성 설정이 없습니다.");
                AdvanceToNextStage();
                return;
            }

            // 캔버스가 설정되지 않았다면 현재 씬의 캔버스 찾기
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas == null)
                {
                    Debug.LogError("캔버스를 찾을 수 없습니다. 오브젝트 생성이 원하는 위치에 되지 않을 수 있습니다.");
                }
            }

            foreach (var prefab in settings.objectToCreate)
            {
                if (prefab != null)
                {
                    // 캔버스가 있는 경우 캔버스의 자식으로 생성
                    GameObject instance;
                    if (mainCanvas != null)
                    {
                        // UI 요소인지 확인
                        if (prefab.GetComponent<RectTransform>() != null)
                        {
                            // UI 요소라면 캔버스 아래에 생성
                            instance = Instantiate(prefab, mainCanvas.transform);
                            // 원래 프리팹의 RectTransform 설정 유지
                            RectTransform rectTransform = instance.GetComponent<RectTransform>();
                            if (rectTransform != null)
                            {
                                // UI 요소의 위치를 캔버스 기준으로 조정
                                rectTransform.anchoredPosition = prefab.GetComponent<RectTransform>().anchoredPosition;
                            }
                        }
                        else
                        {
                            // UI 요소가 아니라면 월드 공간에 생성
                            instance = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);
                        }
                    }
                    else
                    {
                        // 캔버스가 없으면 원래 위치에 생성
                        instance = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation);
                    }
                    
                    Debug.Log($"오브젝트 생성: {instance.name}, 부모: {(instance.transform.parent ? instance.transform.parent.name : "없음")}");
                }
            }

            AdvanceToNextStage();
        }
        
        /// <summary>
        /// 조건부 클릭 인터랙션을 설정합니다.
        /// </summary>
        private void SetupConditionalClick()
        {
            // 조건부 클릭은 Update 메서드에서 클릭 감지 후 처리합니다.
            interactionInProgress = true;
        }
        
        /// <summary>
        /// 지속 클릭 인터랙션을 설정합니다.
        /// </summary>
        private void SetupSustainedClick()
        {
            // 지속 클릭은 Update 메서드에서 클릭 감지 및 유지 시간 측정 후 처리합니다.
            interactionInProgress = true;
        }
        
        /// <summary>
        /// 오브젝트 삭제 인터랙션을 설정합니다.
        /// </summary>
        private void SetupObjectDeletion()
        {
            var settings = currentStage.settings;
            
            if (settings == null || settings.objectToDeleteTag == null || settings.objectToDeleteTag.Length == 0)
            {
                Debug.LogError("오브젝트 삭제 설정이 없거나 삭제할 오브젝트 태그가 없습니다.");
                AdvanceToNextStage();
                return;
            }
            
            // 태그 지정된 모든 오브젝트 비활성화
            foreach (var tag in settings.objectToDeleteTag)
            {
                var objects = GameObject.FindGameObjectsWithTag(tag);
                
                foreach (var obj in objects)
                {
                    if (obj.activeSelf)
                    {
                        Destroy(obj);
                        
                       
                    }
                }
            }
            
            // 오브젝트 삭제 후 다음 단계로 진행
            AdvanceToNextStage();
        }
        
        /// <summary>
        /// 오브젝트 이동 인터랙션을 설정합니다.
        /// </summary>
        private void SetupObjectMovement()
        {
            var settings = currentStage.settings;
            
            if (settings == null || string.IsNullOrEmpty(settings.objectToMoveTag))
            {
                Debug.LogError("오브젝트 이동 설정이 없거나 이동할 오브젝트 태그가 없습니다.");
                AdvanceToNextStage();
                return;
            }
            
            // 태그 지정된 모든 오브젝트를 이동시킵니다.
            var objects = GameObject.FindGameObjectsWithTag(settings.objectToMoveTag);
            
            if (objects.Length == 0)
            {
                Debug.LogWarning($"이동할 오브젝트를 찾을 수 없습니다. 태그: {settings.objectToMoveTag}");
                AdvanceToNextStage();
                return;
            }
            
            foreach (var obj in objects)
            {
                // 경로가 지정되어 있으면 경로를 따라 이동, 아니면 지정된 방향으로 이동
                if (settings.movePath != null && settings.movePath.Length > 0)
                {
                    var movePathCoroutine = StartCoroutine(MoveObjectAlongPath(obj, settings.movePath, settings.moveSpeed));
                    activeCoroutines.Add(movePathCoroutine);
                }
                else
                {
                    var moveDirectionCoroutine = StartCoroutine(MoveObjectInDirection(obj, settings.moveDirection, settings.moveSpeed));
                    activeCoroutines.Add(moveDirectionCoroutine);
                }
            }
            
            // 이동이 시작되면 다음 단계로 진행하지 않고 이동이 완료될 때까지 기다립니다.
            // 이동 코루틴이 완료되면 AdvanceToNextStage를 호출합니다.
        }

        /// <summary>
        /// 퀴즈 팝업 인터랙션을 설정합니다.
        /// </summary>
        private void SetupQuizPopup()
        {
            var settings = currentStage.settings;

            if (settings == null || quizPopupPrefab == null)
            {
                Debug.LogError("퀴즈 팝업 설정이 없거나 퀴즈 팝업 프리팹이 없습니다.");
                AdvanceToNextStage();
                return;
            }

            // 퀴즈 팝업 생성
            var quizPopup = Instantiate(quizPopupPrefab, mainCanvas.transform);
            var quizController = quizPopup.GetComponent<Nursing.UI.QuizPopup>();

            if (quizController == null)
            {
                Debug.LogError("퀴즈 팝업 프리팹에 QuizPopup 컴포넌트가 없습니다.");
                Destroy(quizPopup);
                AdvanceToNextStage();
                return;
            }

            // 퀴즈 설정
            quizController.SetupQuiz(
                settings.questionText,
                settings.quizOptions,
                settings.correctAnswerIndex,
                settings.optionImages,
                settings.timeLimit
            );

            // 퀴즈 결과 이벤트 구독
            quizController.OnQuizComplete += (bool isCorrect) => {
                if (!isCorrect && settings.WrongAnswer != null)
                {
                    ApplyPenalty(settings.WrongAnswer);
                }
                else
                {
                    // 퀴즈 완료 후 다음 단계로 진행
                    AdvanceToNextStage();
                }
            };

            // 퀴즈가 표시되면 인터랙션 완료를 기다립니다.
            interactionInProgress = true;
        }

        /// <summary>
        /// 미니게임 인터랙션을 설정합니다.
        /// </summary>
        private void SetupMiniGame()
        {
            var settings = currentStage.settings;
            
            if (settings == null || settings.miniGamePrefab == null)
            {
                Debug.LogError("미니게임 설정이 없거나 미니게임 프리팹이 없습니다.");
                AdvanceToNextStage();
                return;
            }
            
            // 미니게임 생성
            var miniGame = Instantiate(settings.miniGamePrefab, transform);
            var miniGameController = miniGame.GetComponent<MiniGameController>();
            
            if (miniGameController == null)
            {
                Debug.LogError("미니게임 프리팹에 MiniGameController 컴포넌트가 없습니다.");
                Destroy(miniGame);
                AdvanceToNextStage();
                return;
            }
            
            // 미니게임 결과 이벤트 구독
            miniGameController.OnGameComplete += (bool success) => {
                if (!success && settings.WrongAnswer != null)
                {
                    ApplyPenalty(settings.WrongAnswer);
                }
                else
                {
                    // 미니게임 완료 후 다음 단계로 진행
                    AdvanceToNextStage();
                }
                
                Destroy(miniGame);
            };
            
            // 미니게임이 시작되면 인터랙션 완료를 기다립니다.
            interactionInProgress = true;
        }

        #endregion

        #region 유틸리티 메서드

        /// <summary>
        /// 방향 화살표를 생성합니다.
        /// </summary>
        private void CreateDirectionArrows(Vector2 startPosition, Vector2 direction, int fingerIndex = -1)
        {
            if (arrowPrefab == null)
            {
                Debug.LogError("화살표 프리팹이 없습니다.");
                return;
            }

            // 화살표 생성
            var arrow = Instantiate(arrowPrefab, mainCanvas.transform);
            arrow.transform.position = startPosition;

            // 화살표 방향 설정
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0, 0, angle);

            // 손가락별 색상 설정 (선택적)
            if (fingerIndex >= 0 && arrow.GetComponent<Image>() != null)
            {
                Image arrowImage = arrow.GetComponent<Image>();
                // 첫 번째 손가락: 빨간색, 두 번째 손가락: 파란색, 세 번째 이상: 초록색
                switch (fingerIndex % 3)
                {
                    case 0: arrowImage.color = new Color(1f, 0.3f, 0.3f); break; // 빨강
                    case 1: arrowImage.color = new Color(0.3f, 0.3f, 1f); break; // 파랑
                    case 2: arrowImage.color = new Color(0.3f, 1f, 0.3f); break; // 초록
                }
            }

            // 생성된 화살표 저장 (손가락 인덱스 정보 포함)
            if (fingerIndex >= 0)
            {
                // 손가락별 화살표 관리를 위한 딕셔너리에 저장
                if (!fingerArrows.ContainsKey(fingerIndex))
                {
                    fingerArrows[fingerIndex] = new List<GameObject>();
                }
                fingerArrows[fingerIndex].Add(arrow);
            }
            else
            {
                createdArrows.Add(arrow);
            }

            // 화살표 깜빡임 애니메이션 시작
            if (arrowBlinkCoroutine == null)
            {
                arrowBlinkCoroutine = StartCoroutine(BlinkArrows());
            }
        }

        /// <summary>
        /// 화살표를 깜빡이게 합니다.
        /// </summary>
        private IEnumerator BlinkArrows()
        {
            while (true)
            {
                // 일반 화살표 표시
                foreach (var arrow in createdArrows)
                {
                    if (arrow != null)
                        arrow.SetActive(true);
                }

                // 손가락별 화살표 표시
                foreach (var entry in fingerArrows)
                {
                    foreach (var arrow in entry.Value)
                    {
                        if (arrow != null)
                            arrow.SetActive(true);
                    }
                }

                yield return new WaitForSeconds(arrowBlinkDuration);

                // 일반 화살표 숨김
                foreach (var arrow in createdArrows)
                {
                    if (arrow != null)
                        arrow.SetActive(false);
                }

                // 손가락별 화살표 숨김
                foreach (var entry in fingerArrows)
                {
                    foreach (var arrow in entry.Value)
                    {
                        if (arrow != null)
                            arrow.SetActive(false);
                    }
                }

                yield return new WaitForSeconds(arrowBlinkInterval);
            }
        }

        /// <summary>
        /// 모든 화살표를 제거합니다.
        /// </summary>
        private void ClearArrows()
        {
            if (arrowBlinkCoroutine != null)
            {
                StopCoroutine(arrowBlinkCoroutine);
                arrowBlinkCoroutine = null;
            }

            // 일반 화살표 제거
            foreach (var arrow in createdArrows)
            {
                if (arrow != null)
                    Destroy(arrow);
            }
            createdArrows.Clear();

            // 손가락별 화살표 제거
            foreach (var entry in fingerArrows)
            {
                foreach (var arrow in entry.Value)
                {
                    if (arrow != null)
                        Destroy(arrow);
                }
            }
            fingerArrows.Clear();
        }

        /// <summary>
        /// 오브젝트를 특정 방향으로 이동시킵니다.
        /// </summary>
        private IEnumerator MoveObjectInDirection(GameObject obj, Vector2 direction, float speed)
        {
            float distance = 10f; // 기본 이동 거리
            Vector2 normalizedDir = direction.normalized;
            Vector3 startPos = obj.transform.position;
            Vector3 targetPos = startPos + new Vector3(normalizedDir.x, normalizedDir.y, 0) * distance;
            float journeyLength = Vector3.Distance(startPos, targetPos);
            float startTime = Time.time;
            
            while (obj != null) // 오브젝트가 존재하는 동안
            {
                float distCovered = (Time.time - startTime) * speed;
                float fractionOfJourney = distCovered / journeyLength;
                
                if (fractionOfJourney >= 1f)
                {
                    if (obj != null)
                        obj.transform.position = targetPos;
                    break;
                }
                
                if (obj != null)
                    obj.transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
                
                yield return null;
            }
            
            // 모든 오브젝트 이동이 완료되면 다음 단계로 진행
            AdvanceToNextStage();
        }
        
        /// <summary>
        /// 오브젝트를 경로를 따라 이동시킵니다.
        /// </summary>
        private IEnumerator MoveObjectAlongPath(GameObject obj, Vector2[] path, float speed)
        {
            if (path.Length < 2)
            {
                Debug.LogWarning("경로 포인트가 2개 미만입니다.");
                yield break;
            }
            
            Vector3 startPos = obj.transform.position;
            List<Vector3> fullPath = new List<Vector3>();
            fullPath.Add(startPos);
            
            foreach (var point in path)
            {
                fullPath.Add(new Vector3(point.x, point.y, startPos.z));
            }
            
            for (int i = 0; i < fullPath.Count - 1; i++)
            {
                Vector3 currentTarget = fullPath[i + 1];
                float journeyLength = Vector3.Distance(fullPath[i], currentTarget);
                float startTime = Time.time;
                
                while (obj != null) // 오브젝트가 존재하는 동안
                {
                    float distCovered = (Time.time - startTime) * speed;
                    float fractionOfJourney = distCovered / journeyLength;
                    
                    if (fractionOfJourney >= 1f)
                    {
                        if (obj != null)
                            obj.transform.position = currentTarget;
                        break;
                    }
                    
                    if (obj != null)
                        obj.transform.position = Vector3.Lerp(fullPath[i], currentTarget, fractionOfJourney);
                    
                    yield return null;
                }
            }
            
            // 경로 이동이 완료되면 다음 단계로 진행
            AdvanceToNextStage();
        }
        
        /// <summary>
        /// 패널티를 적용합니다.
        /// </summary>
        private void ApplyPenalty(PenaltyData penaltyData)
        {
            if (penaltyData == null || penaltyManager == null)
                return;
            
            penaltyManager.ApplyPenalty(penaltyData);
        }
        
        /// <summary>
        /// 오브젝트를 태그로 찾아 캐싱합니다.
        /// </summary>
        private GameObject FindObjectByTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return null;
            
            if (taggedObjectsCache.ContainsKey(tag))
                return taggedObjectsCache[tag];
            
            GameObject obj = GameObject.FindWithTag(tag);
            
            if (obj != null)
                taggedObjectsCache[tag] = obj;
            
            return obj;
        }
        
        /// <summary>
        /// 인터랙션을 완료합니다.
        /// </summary>
        private void CompleteInteraction()
        {
            Debug.Log("인터랙션 완료: " + currentInteraction.displayName);
            
            // 인터랙션 정리
            CleanupCurrentInteraction();

            // 인터랙션 완료 이벤트 발생
            OnInteractionComplete?.Invoke(true);
        }
        
        /// <summary>
        /// 현재 인터랙션을 정리합니다.
        /// </summary>
        private void CleanupCurrentInteraction()
        {
            // 화살표 제거
            ClearArrows();
            
            // 활성 코루틴 정리
            foreach (var coroutine in activeCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }
            activeCoroutines.Clear();
            
            // 캐시 초기화
            taggedObjectsCache.Clear();
            
            interactionInProgress = false;
            isDragging = false;
            draggedObject = null;
            
            currentInteraction = null;
            currentStage = null;
            currentStageIndex = -1;
        }
        
        #endregion
        
        #region 입력 처리
        
        private void Update()
        {
            if (!interactionInProgress || currentStage == null || currentStage.settings == null)
                return;

            switch (currentStage.interactionType)
            {
                case InteractionType.SingleDragInteraction:
                    HandleSingleDragInteraction();
                    break;

                case InteractionType.MultiDragInteraction:
                    HandleMultiDragInteraction();
                    break;

                case InteractionType.ConditionalClick:
                    HandleConditionalClick();
                    break;
                    
                case InteractionType.SustainedClick:
                    HandleSustainedClick();
                    break;
            }
            var settings = currentStage.settings;
            if (waitingForSimultaneousStart && Input.touchCount == settings.fingerSettings.Count)
            {
                bool allBegan = Input.touches.All(t => t.phase == TouchPhase.Began);
                if (allBegan)
                {
                    // 동시에 터치가 시작되었으므로 매핑 시도
                    foreach (Touch touch in Input.touches)
                    {
                        PointerEventData eventData = new(EventSystem.current) { position = touch.position };
                        List<RaycastResult> results = new();
                        EventSystem.current.RaycastAll(eventData, results);

                        foreach (RaycastResult result in results)
                        {
                            for (int i = 0; i < settings.fingerSettings.Count; i++)
                            {
                                var setting = settings.fingerSettings[i];

                                if (!usedSettingIndices.Contains(i) && result.gameObject.CompareTag(setting.targetObjectTag))
                                {
                                    fingerToSetting[touch.fingerId] = i;
                                    fingerDragStatus[touch.fingerId] = new FingerDragStatus
                                    {
                                        isDragging = true,
                                        startPosition = touch.position,
                                        draggedObject = result.gameObject
                                    };
                                    usedSettingIndices.Add(i);
                                    break;
                                }
                            }
                        }
                    }

                    if (fingerToSetting.Count == settings.fingerSettings.Count)
                    {
                        Debug.Log("🎯 Simultaneous multi-drag 시작!");
                        waitingForSimultaneousStart = false;
                    }
                    else
                    {
                        // 실패한 경우 초기화
                        fingerToSetting.Clear();
                        fingerDragStatus.Clear();
                        usedSettingIndices.Clear();
                    }
                }
            }

        }

        /// <summary>
        /// 드래그 인터랙션을 처리합니다.
        /// </summary>


        // 단일 드래그 처리
        private void HandleSingleDragInteraction()
        {
            var settings = currentStage.settings;
            HandleSingleFingerDrag(settings);

            if (settings == null)
                return;

            
        }

        // 다중 드래그 처리 (각 손가락 독립적으로)
        private void HandleMultiDragInteraction()
        {
            var settings = currentStage.settings;
            if (settings == null || settings.fingerSettings.Count == 0)
                return;


            int requiredCount = settings.fingerSettings.Count;

            // [1] 아직 초기화 안된 경우: 동시에 터치 시작한 상태인지 체크
            if (fingerDragStatus.Count == 0)
            {
                if (Input.touchCount != requiredCount)
                    return;

                // 동시에 Began 상태인지 확인
                bool allBegan = true;
                for (int i = 0; i < requiredCount; i++)
                {
                    if (Input.GetTouch(i).phase != TouchPhase.Began)
                    {
                        allBegan = false;
                        break;
                    }
                }

                if (!allBegan)
                    return;

                // [2] 터치 시작 시, 설정과 매핑해서 상태 초기화
                for (int i = 0; i < requiredCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    var setting = settings.fingerSettings[i];

                    PointerEventData eventData = new PointerEventData(EventSystem.current)
                    {
                        position = touch.position
                    };
                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(eventData, results);

                    foreach (var result in results)
                    {
                        if (result.gameObject.CompareTag(setting.targetObjectTag))
                        {
                            fingerDragStatus[touch.fingerId] = new FingerDragStatus
                            {
                                isDragging = true,
                                draggedObject = result.gameObject,
                                startPosition = touch.position,
                                isComplete = false
                            };
                            break;
                        }
                    }
                }
            }




            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (!fingerDragStatus.ContainsKey(touch.fingerId))
                    continue;

                var status = fingerDragStatus[touch.fingerId];
                var setting = settings.fingerSettings[i]; // 순서 대응

                switch (touch.phase)
                {
                   

                    case TouchPhase.Moved:
                        if (status.isDragging && status.draggedObject != null && setting.followDragMovement)
                        {
                            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 10f));
                            status.draggedObject.transform.position = new Vector3(worldPos.x, worldPos.y, status.draggedObject.transform.position.z);
                        }
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (status.isDragging && status.draggedObject != null && status.matchedSettingIndex >= 0)
                        {
                            Vector2 endPos = touch.position;
                            Vector2 dragDir = (endPos - status.startPosition).normalized;
                            float dragDist = Vector2.Distance(endPos, status.startPosition);

                            bool valid = true;
                            if (setting.requiredDragDirection)
                            {
                                Vector2 required = setting.arrowDirection.normalized;
                                float dot = Vector2.Dot(dragDir, required);
                                float minDot = Mathf.Cos(setting.dragDirectionTolerance * Mathf.Deg2Rad);

                                Debug.Log($"드래그 방향: {dragDir}, 요구 방향: { setting.arrowDirection.normalized}, 각도 코사인: {dot}, 허용 오차: {setting.dragDirectionTolerance}도");

                                if (dot < minDot)
                                {
                                    valid = false;
                                    // 시각적 피드백 - 잘못된 방향 표시
                                    if (dialogueManager != null)
                                    {
                                        // 방향 힌트 계산
                                        float angle = Vector2.SignedAngle(dragDir, setting.arrowDirection.normalized);
                                        string directionHint = "";

                                        if (angle > 15f) directionHint = "더 왼쪽으로";
                                        else if (angle < -15f) directionHint = "더 오른쪽으로";
                                        else if (dot < 0) directionHint = "반대 방향으로";

                                        dialogueManager.ShowGuideMessage($"올바른 방향으로 드래그해주세요. (허용 오차: {setting.dragDirectionTolerance}°) {directionHint}");

                                    }

                                   

                                }

                                if (setting.dragDistanceLimit > 0 && dragDist > setting.dragDistanceLimit)
                                    valid = false;


                                if (valid)
                                {
                                    status.isComplete = true;
                                    Debug.Log($"[MultiDrag] 손가락 {touch.fingerId} 완료됨");
                                }
                                else
                                {
                                    Debug.LogWarning($"[MultiDrag] 손가락 {touch.fingerId} 실패");
                                }

                                status.isDragging = false;
                                // 화살표 방향을 다시 표시 (힌트)
                                if (settings.showDirectionArrows && arrowPrefab != null)
                                {
                                    CreateDirectionArrows(settings.arrowStartPosition, settings.arrowDirection);
                                }
                            }
                           
                        }
                        break;
                }

            }

            // [4] 완료 체크
            if (fingerDragStatus.Values.All(s => s.isComplete))
            {
                AdvanceToNextStage();
            }

        }

        private bool AllMultiDragCompleted(int requiredCount)
        {
            for (int i = 0; i < requiredCount; i++)
            {
                if (!fingerDragStatus.ContainsKey(i) || !fingerDragStatus[i].isComplete)
                    return false;
            }
            return true;
        }



        /// <summary>
        /// 단일 손가락 드래그를 처리합니다.
        /// </summary>
        private void HandleSingleFingerDrag(InteractionSettings settings)
        {
            // 마우스 또는 터치 입력 처리
            bool isTouching = Input.touchCount > 0;
            bool isMouseDown = Input.GetMouseButton(0);
            
            if (!isDragging && (isTouching || isMouseDown))
            {
                // 드래그 시작 포인트 얻기
                Vector2 touchPos = isTouching ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                
                // UI 요소 체크를 위한 레이캐스트
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = touchPos;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);
                
                foreach (RaycastResult result in results)
                {
                    // 태그가 일치하는 오브젝트인지 확인
                    if (result.gameObject.CompareTag(settings.targetObjectTag))
                    {
                        isDragging = true;
                        dragStartPosition = touchPos;
                        draggedObject = result.gameObject;
                        
                        // 화살표 숨김
                        ClearArrows();
                        
                        break;
                    }
                }
            }
            else if (isDragging)
            {
                // 드래그 중, 릴리스 확인
                bool touchEnded = isTouching && Input.GetTouch(0).phase == TouchPhase.Ended;
                bool mouseUp = !isMouseDown && !isTouching;
                
                if (touchEnded || mouseUp)
                {
                    // 드래그 완료, 방향 확인
                    Vector2 dragEndPosition = isTouching ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                    Vector2 dragDirection = (dragEndPosition - dragStartPosition).normalized;
                    
                    // 드래그 방향 요구사항이 있는 경우 확인
                    if (settings.requiredDragDirection)
                    {
                        Vector2 requiredDirection = settings.arrowDirection.normalized;
                        float dot = Vector2.Dot(dragDirection, requiredDirection);
                        float angleTolerance = settings.dragDirectionTolerance; // 설정에서 오차 범위 가져오기
                        float minDotValue = Mathf.Cos(angleTolerance * Mathf.Deg2Rad);
                        
                        // 디버그 정보 출력
                        Debug.Log($"드래그 방향: {dragDirection}, 요구 방향: {requiredDirection}, 각도 코사인: {dot}, 허용 오차: {angleTolerance}도");
                        
                        if (dot < minDotValue)
                        {
                            // 시각적 피드백 - 잘못된 방향 표시
                            if (dialogueManager != null)
                            {
                                // 방향 힌트 계산
                                float angle = Vector2.SignedAngle(dragDirection, requiredDirection);
                                string directionHint = "";
                                
                                if (angle > 15f) directionHint = "더 왼쪽으로";
                                else if (angle < -15f) directionHint = "더 오른쪽으로";
                                else if (dot < 0) directionHint = "반대 방향으로";
                                
                                dialogueManager.ShowGuideMessage($"올바른 방향으로 드래그해주세요. (허용 오차: {angleTolerance}°) {directionHint}");
                            }
                            
                            // 잘못된 방향으로 드래그
                            if (settings.OverDrag != null)
                            {
                                ApplyPenalty(settings.OverDrag);
                            }
                            
                            // 드래그 상태 리셋
                            isDragging = false;
                            draggedObject = null;
                            
                            // 화살표 방향을 다시 표시 (힌트)
                            if (settings.showDirectionArrows && arrowPrefab != null)
                            {
                                CreateDirectionArrows(settings.arrowStartPosition, settings.arrowDirection);
                            }
                            
                            return;
                        }
                    }
                    
                    // 드래그 거리 제한 확인
                    float dragDistance = Vector2.Distance(dragEndPosition, dragStartPosition);
                    if (settings.dragDistanceLimit > 0 && dragDistance > settings.dragDistanceLimit)
                    {
                        // 드래그 거리 초과
                        if (settings.OverDrag != null)
                        {
                            ApplyPenalty(settings.OverDrag);
                        }
                        
                        // 드래그 상태 리셋
                        isDragging = false;
                        draggedObject = null;
                        return;
                    }
                    
                    // 드래그에 따른 오브젝트 이동 (설정된 경우)
                    if (draggedObject != null)
                    {
                        if (settings.followDragMovement)
                        {
                            // 드래그 포지션을 월드 위치로 변환
                            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(dragEndPosition.x, dragEndPosition.y, 10f));
                            draggedObject.transform.position = new Vector3(worldPos.x, worldPos.y, draggedObject.transform.position.z);
                            
                            // 경계 확인 (설정된 경우)
                            if (!string.IsNullOrEmpty(settings.boundaryObjectTag))
                            {
                                GameObject boundary = FindObjectByTag(settings.boundaryObjectTag);
                                if (boundary != null)
                                {
                                    // 경계를 벗어났는지 확인하는 로직
                                    // 예시: 경계 콜라이더와 오브젝트 콜라이더 간의 충돌 확인
                                    Collider2D boundaryCollider = boundary.GetComponent<Collider2D>();
                                    Collider2D objCollider = draggedObject.GetComponent<Collider2D>();
                                    
                                    if (boundaryCollider != null && objCollider != null)
                                    {
                                        bool isWithinBoundary = boundaryCollider.bounds.Contains(draggedObject.transform.position);
                                        
                                        if (!isWithinBoundary && settings.OverDrag != null)
                                        {
                                            ApplyPenalty(settings.OverDrag);
                                            
                                            // 드래그 상태 리셋
                                            isDragging = false;
                                            draggedObject = null;
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 고정 이동: 지정된 방향으로 고정 거리만큼 이동
                            Vector3 moveDirection = new Vector3(settings.arrowDirection.x, settings.arrowDirection.y, 0).normalized;
                            float moveDistance = 1.0f; // 기본 이동 거리, 필요에 따라 조정
                            draggedObject.transform.position += moveDirection * moveDistance;
                        }

                        
                    }

                    // 드래그 후 오브젝트 비활성화 (설정된 경우)
                    if (draggedObject != null && settings.deactivateObjectAfterDrag)
                    {
                        Destroy(draggedObject);
                    }

                    // 드래그 완료, 다음 단계로 진행
                    isDragging = false;
                    draggedObject = null;
                    AdvanceToNextStage();
                }
                else if (isDragging && settings.followDragMovement)
                {
                    // 실시간 드래그 이동 (설정된 경우)
                    Vector2 currentTouchPos = isTouching ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                    
                    if (draggedObject != null)
                    {
                        // 드래그 포지션을 월드 위치로 변환
                        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(currentTouchPos.x, currentTouchPos.y, 10f));
                        draggedObject.transform.position = new Vector3(worldPos.x, worldPos.y, draggedObject.transform.position.z);
                        
                        // 충돌 영역 확인 (설정된 경우)
                        if (!string.IsNullOrEmpty(settings.collisionZoneTag))
                        {
                            GameObject[] collisionZones = GameObject.FindGameObjectsWithTag(settings.collisionZoneTag);
                            
                            foreach (GameObject zone in collisionZones)
                            {
                                Collider2D zoneCollider = zone.GetComponent<Collider2D>();
                                Collider2D objCollider = draggedObject.GetComponent<Collider2D>();
                                
                                if (zoneCollider != null && objCollider != null)
                                {
                                    bool isColliding = zoneCollider.bounds.Intersects(objCollider.bounds);
                                    
                                    if (isColliding && settings.OverDrag != null)
                                    {
                                        ApplyPenalty(settings.CollideDrag);
                                        
                                        // 드래그 상태 리셋
                                        isDragging = false;
                                        draggedObject = null;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        


        /// <summary>
        /// 조건부 클릭 인터랙션을 처리합니다.
        /// </summary>
        private void HandleConditionalClick()
        {
            var settings = currentStage.settings;

            if (settings == null)
                return;

            // 클릭 또는 터치 확인
            bool isClicking = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

            if (isClicking)
            {
                Vector2 clickPos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

                // UI 요소 체크를 위한 레이캐스트
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = clickPos;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                // Default 레이어 인덱스 가져오기
               
                string clickedTag = null;
                
                
                foreach (RaycastResult result in results)
                {
                    GameObject hitObject = result.gameObject;

                    if(hitObject.layer == LayerMask.NameToLayer("UI"))
                    {
                        
                        break;
                    }

                    
                        clickedTag = hitObject.tag;

                    

                    // 유효한 클릭 태그인지 확인
                    if (settings.validClickTags.Contains(clickedTag))
                        {
                            // 유효한 클릭, 다음 단계로 진행
                            AdvanceToNextStage();
                            return;
                        }

                        // 잘못된 클릭 태그인지 확인
                        int invalidIndex = settings.invalidClickTags.IndexOf(clickedTag);
                        if (invalidIndex >= 0 && invalidIndex < settings.conditionalClickPenalties.Count)
                        {
                            // 잘못된 클릭, 해당 태그에 맞는 패널티 적용
                            ApplyPenalty(settings.conditionalClickPenalties[invalidIndex]);
                       
                            return;
                        }
                    
                }
                }
        }
        
        /// <summary>
        /// 지속 클릭 인터랙션을 처리합니다.
        /// </summary>
        private float clickHoldTime = 0f;
        private bool isHolding = false;
        
        private void HandleSustainedClick()
        {
            var settings = currentStage.settings;
            
            if (settings == null)
                return;
            
            bool isPressed = Input.GetMouseButton(0) || (Input.touchCount > 0);
            bool isReleased = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
            
            // 클릭 시작
            if (!isHolding && isPressed)
            {
                Vector2 clickPos = Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;
                
                // UI 요소 체크를 위한 레이캐스트
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = clickPos;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);
                
                foreach (RaycastResult result in results)
                {
                    // 타겟 오브젝트인지 확인
                    if (result.gameObject.CompareTag(settings.sustainedClickTargetTag))
                    {
                        isHolding = true;
                        clickHoldTime = 0f;
                        break;
                    }
                }
            }
            // 클릭 유지 중
            else if (isHolding && isPressed)
            {
                clickHoldTime += Time.deltaTime;
                
                // 너무 오래 누르고 있는 경우 (선택적)
                if (clickHoldTime > settings.sustainedClickDuration * 1.5f && settings.lateReleasePenalty != null)
                {
                    ApplyPenalty(settings.lateReleasePenalty);
                    isHolding = false;
                    clickHoldTime = 0f;
                }
            }
            // 클릭 릴리스
            else if (isHolding && isReleased)
            {
                // 충분한 시간 동안 눌렀는지 확인
                if (clickHoldTime >= settings.sustainedClickDuration)
                {
                    // 성공적인 지속 클릭
                    isHolding = false;
                    clickHoldTime = 0f;
                    AdvanceToNextStage();
                }
                else
                {
                    // 너무 일찍 릴리스한 경우
                    if (settings.earlyReleasePenalty != null)
                    {
                        ApplyPenalty(settings.earlyReleasePenalty);
                    }
                    
                    isHolding = false;
                    clickHoldTime = 0f;
                }
            }
        }
        
        #endregion
    }
    
    
    
    /// <summary>
    /// 미니게임 컨트롤러 인터페이스
    /// </summary>
    public interface MiniGameController
    {
        event System.Action<bool> OnGameComplete;
    }
}
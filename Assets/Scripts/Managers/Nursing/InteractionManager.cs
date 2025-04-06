using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nursing.Interaction;
using Nursing.Penalty;
using Nursing.Scoring;

namespace Nursing.Managers
{
    public class InteractionManager : MonoBehaviour
    {
        [Header("인터랙션 참조")]
        [SerializeField] private InteractionData currentInteraction;
        [SerializeField] private int currentStageIndex = -1;
        
        [Header("UI 요소")]
        [SerializeField] private Text guideMessageText;
        [SerializeField] private GameObject directionArrowPrefab;
        [SerializeField] private GameObject quizPopupPrefab;
        
        [Header("인터랙션 상태")]
        [SerializeField] private bool isInteractionActive = false;
        [SerializeField] private bool isInteractionBlocked = false;
        
        private NewScoringSystem scoringSystem;
        private PenaltyManager penaltyManager;
        private DialogueManager dialogueManager;
        
        private List<GameObject> activeArrows = new List<GameObject>();
        private List<GameObject> activeCreatedObjects = new List<GameObject>();
        
        private void Awake()
        {
            scoringSystem = FindObjectOfType<NewScoringSystem>();
            penaltyManager = FindObjectOfType<PenaltyManager>();
            dialogueManager = FindObjectOfType<DialogueManager>();
        }
        
        /// <summary>
        /// 인터랙션을 시작합니다.
        /// </summary>
        /// <param name="interactionData">시작할 인터랙션 데이터</param>
        public void StartInteraction(InteractionData interactionData)
        {
            if (interactionData == null)
            {
                Debug.LogWarning("Cannot start interaction: InteractionData is null");
                return;
            }
            
            currentInteraction = interactionData;
            currentStageIndex = -1;
            isInteractionActive = true;
            isInteractionBlocked = false;
            
            // 가이드 메시지 표시
            if (guideMessageText != null && !string.IsNullOrEmpty(interactionData.guideMessage))
            {
                guideMessageText.text = interactionData.guideMessage;
            }
            
            // 첫 번째 스테이지 시작
            AdvanceToNextStage();
            
            Debug.Log($"Interaction started: {interactionData.displayName}");
        }
        
        /// <summary>
        /// 다음 스테이지로 진행합니다.
        /// </summary>
        public void AdvanceToNextStage()
        {
            if (!isInteractionActive || currentInteraction == null)
                return;
            
            if (isInteractionBlocked)
            {
                Debug.Log("Cannot advance: Interaction is blocked");
                return;
            }
            
            // 이전 스테이지 정리
            CleanupCurrentStage();
            
            // 다음 스테이지 인덱스 계산
            currentStageIndex++;
            
            // 인터랙션 완료 확인
            if (currentStageIndex >= currentInteraction.stages.Count)
            {
                CompleteInteraction();
                return;
            }
            
            // 현재 스테이지 설정 및 시작
            InteractionStage currentStage = currentInteraction.stages[currentStageIndex];
            SetupStage(currentStage);
            
            Debug.Log($"Advanced to stage {currentStageIndex + 1}: {currentStage.name}");
        }
        
        /// <summary>
        /// 현재 스테이지를 설정합니다.
        /// </summary>
        private void SetupStage(InteractionStage stage)
        {
            if (stage == null)
                return;
            
            // 가이드 메시지 업데이트
            if (guideMessageText != null && !string.IsNullOrEmpty(stage.guideMessage))
            {
                guideMessageText.text = stage.guideMessage;
            }
            
            // 인터랙션 타입에 따른 설정
            switch (stage.interactionType)
            {
                case InteractionType.Drag:
                    SetupDragInteraction(stage);
                    break;
                case InteractionType.ObjectCreation:
                    SetupObjectCreation(stage);
                    break;
                case InteractionType.QuizPopup:
                    SetupQuizPopup(stage);
                    break;
                case InteractionType.MiniGame:
                    SetupMiniGame(stage);
                    break;
                case InteractionType.ObjectMovement:
                    SetupObjectMovement(stage);
                    break;
                case InteractionType.ObjectDeletion:
                    SetupObjectDeletion(stage);
                    break;
            }
        }
        
        /// <summary>
        /// 드래그 인터랙션을 설정합니다.
        /// </summary>
        private void SetupDragInteraction(InteractionStage stage)
        {
            InteractionSettings settings = stage.settings;
            
            if (settings.showDirectionArrows && directionArrowPrefab != null)
            {
                GameObject arrow = Instantiate(directionArrowPrefab, transform);
                arrow.transform.position = settings.arrowStartPosition;
                
                // 화살표 방향 설정
                arrow.transform.right = settings.arrowDirection;
                
                // 깜빡임 효과 설정
                StartCoroutine(BlinkArrow(arrow));
                
                activeArrows.Add(arrow);
            }
        }
        
        /// <summary>
        /// 오브젝트 생성 인터랙션을 설정합니다.
        /// </summary>
        private void SetupObjectCreation(InteractionStage stage)
        {
            InteractionSettings settings = stage.settings;
            
            if (settings.createObject && !string.IsNullOrEmpty(settings.objectToCreateTag))
            {
                // 태그로 비활성화된 오브젝트 찾기
                GameObject[] objectsToActivate = GameObject.FindGameObjectsWithTag(settings.objectToCreateTag);
                
                foreach (GameObject obj in objectsToActivate)
                {
                    if (!obj.activeInHierarchy)
                    {
                        obj.SetActive(true);
                        activeCreatedObjects.Add(obj);
                    }
                }
            }
        }
        
        /// <summary>
        /// 퀴즈 팝업 인터랙션을 설정합니다.
        /// </summary>
        private void SetupQuizPopup(InteractionStage stage)
        {
            InteractionSettings settings = stage.settings;
            
            if (settings.showQuizPopup && quizPopupPrefab != null)
            {
                GameObject quizPopup = Instantiate(quizPopupPrefab, transform);
                QuizPopup quizPopupComponent = quizPopup.GetComponent<QuizPopup>();
                
                if (quizPopupComponent != null)
                {
                    quizPopupComponent.Initialize(
                        settings.questionText,
                        settings.quizOptions,
                        settings.correctAnswerIndex,
                        settings.optionImages,
                        settings.timeLimit,
                        OnQuizCompleted
                    );
                }
                
                activeCreatedObjects.Add(quizPopup);
            }
        }
        
        /// <summary>
        /// 미니게임 인터랙션을 설정합니다.
        /// </summary>
        private void SetupMiniGame(InteractionStage stage)
        {
            InteractionSettings settings = stage.settings;
            
            if (settings.startMiniGame && settings.miniGamePrefab != null)
            {
                GameObject miniGame = Instantiate(settings.miniGamePrefab, transform);
                MiniGameBase miniGameComponent = miniGame.GetComponent<MiniGameBase>();
                
                if (miniGameComponent != null)
                {
                    miniGameComponent.Initialize(OnMiniGameCompleted);
                }
                
                activeCreatedObjects.Add(miniGame);
            }
        }
        
        /// <summary>
        /// 오브젝트 이동 인터랙션을 설정합니다.
        /// </summary>
        private void SetupObjectMovement(InteractionStage stage)
        {
            InteractionSettings settings = stage.settings;
            
            if (settings.moveObject && !string.IsNullOrEmpty(settings.objectToMoveTag))
            {
                GameObject[] objectsToMove = GameObject.FindGameObjectsWithTag(settings.objectToMoveTag);
                
                foreach (GameObject obj in objectsToMove)
                {
                    StartCoroutine(MoveObject(obj, settings.moveDirection, settings.moveSpeed, settings.movePath));
                }
            }
        }
        
        /// <summary>
        /// 오브젝트 삭제 인터랙션을 설정합니다.
        /// </summary>
        private void SetupObjectDeletion(InteractionStage stage)
        {
            InteractionSettings settings = stage.settings;
            
            if (settings.deleteObject && !string.IsNullOrEmpty(settings.objectToDeleteTag))
            {
                GameObject[] objectsToDelete = GameObject.FindGameObjectsWithTag(settings.objectToDeleteTag);
                
                foreach (GameObject obj in objectsToDelete)
                {
                    obj.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// 현재 스테이지를 정리합니다.
        /// </summary>
        private void CleanupCurrentStage()
        {
            // 화살표 제거
            foreach (GameObject arrow in activeArrows)
            {
                if (arrow != null)
                {
                    Destroy(arrow);
                }
            }
            activeArrows.Clear();
            
            // 생성된 오브젝트 정리 (미니게임, 퀴즈 등)
            foreach (GameObject obj in activeCreatedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            activeCreatedObjects.Clear();
            
            // 실행 중인 모든 코루틴 중지
            StopAllCoroutines();
        }
        
        /// <summary>
        /// 화살표를 깜빡이는 효과를 적용합니다.
        /// </summary>
        private IEnumerator BlinkArrow(GameObject arrow)
        {
            if (arrow == null)
                yield break;
            
            Image arrowImage = arrow.GetComponent<Image>();
            if (arrowImage == null)
                yield break;
            
            float blinkSpeed = 1.0f;
            
            while (true)
            {
                // 페이드 아웃
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * blinkSpeed;
                    Color color = arrowImage.color;
                    color.a = Mathf.Lerp(1f, 0.3f, t);
                    arrowImage.color = color;
                    yield return null;
                }
                
                // 페이드 인
                t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * blinkSpeed;
                    Color color = arrowImage.color;
                    color.a = Mathf.Lerp(0.3f, 1f, t);
                    arrowImage.color = color;
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// 오브젝트를 이동시킵니다.
        /// </summary>
        private IEnumerator MoveObject(GameObject obj, Vector2 direction, float speed, Vector2[] path = null)
        {
            if (obj == null)
                yield break;
            
            if (path != null && path.Length > 0)
            {
                // 경로를 따라 이동
                for (int i = 0; i < path.Length; i++)
                {
                    Vector3 targetPosition = new Vector3(path[i].x, path[i].y, obj.transform.position.z);
                    
                    while (Vector3.Distance(obj.transform.position, targetPosition) > 0.01f)
                    {
                        obj.transform.position = Vector3.MoveTowards(
                            obj.transform.position,
                            targetPosition,
                            speed * Time.deltaTime
                        );
                        yield return null;
                    }
                }
            }
            else
            {
                // 방향으로 이동 (5초 동안)
                float duration = 5f;
                float elapsedTime = 0f;
                
                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    obj.transform.Translate(direction.normalized * speed * Time.deltaTime);
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// 퀴즈가 완료되었을 때 호출됩니다.
        /// </summary>
        private void OnQuizCompleted(bool isCorrect)
        {
            if (!isCorrect && currentStageIndex < currentInteraction.stages.Count)
            {
                // 오답에 대한 패널티 적용
                InteractionStage currentStage = currentInteraction.stages[currentStageIndex];
                ApplyPenalty(currentStage.incorrectInteractionPenalty);
            }
            
            // 다음 스테이지로 진행
            AdvanceToNextStage();
        }
        
        /// <summary>
        /// 미니게임이 완료되었을 때 호출됩니다.
        /// </summary>
        private void OnMiniGameCompleted(bool success, int score)
        {
            if (!success && currentStageIndex < currentInteraction.stages.Count)
            {
                // 실패에 대한 패널티 적용
                InteractionStage currentStage = currentInteraction.stages[currentStageIndex];
                ApplyPenalty(currentStage.incorrectInteractionPenalty);
            }
            
            // 다음 스테이지로 진행
            AdvanceToNextStage();
        }
        
        /// <summary>
        /// 아이템 클릭 이벤트를 처리합니다.
        /// </summary>
        public void OnItemClicked(string itemId)
        {
            if (!isInteractionActive || isInteractionBlocked || currentStageIndex < 0 || currentStageIndex >= currentInteraction.stages.Count)
                return;
            
            InteractionStage currentStage = currentInteraction.stages[currentStageIndex];
            
            // 클릭 인터랙션 처리
            if (currentStage.interactionType == InteractionType.SingleClick ||
                currentStage.interactionType == InteractionType.ConditionalClick)
            {
                HandleClickInteraction(itemId, currentStage);
            }
        }
        
        /// <summary>
        /// 드래그 이벤트를 처리합니다.
        /// </summary>
        public void OnDragPerformed(string objectTag, Vector2 dragDirection, bool isTwoFingerDrag)
        {
            if (!isInteractionActive || isInteractionBlocked || currentStageIndex < 0 || currentStageIndex >= currentInteraction.stages.Count)
                return;
            
            InteractionStage currentStage = currentInteraction.stages[currentStageIndex];
            
            // 드래그 인터랙션 처리
            if (currentStage.interactionType == InteractionType.Drag)
            {
                HandleDragInteraction(objectTag, dragDirection, isTwoFingerDrag, currentStage);
            }
        }
        
        /// <summary>
        /// 클릭 인터랙션을 처리합니다.
        /// </summary>
        private void HandleClickInteraction(string itemId, InteractionStage stage)
        {
            InteractionSettings settings = stage.settings;
            
            if (stage.interactionType == InteractionType.ConditionalClick)
            {
                // 조건부 클릭 처리
                bool isValidClick = settings.validClickTags.Contains(itemId);
                bool isInvalidClick = settings.invalidClickTags.Contains(itemId);
                
                if (isValidClick)
                {
                    // 유효한 클릭, 다음 스테이지로 진행
                    AdvanceToNextStage();
                }
                else if (isInvalidClick)
                {
                    // 잘못된 클릭, 패널티 적용
                    int penaltyIndex = settings.invalidClickTags.IndexOf(itemId);
                    
                    if (penaltyIndex >= 0 && penaltyIndex < settings.conditionalClickPenalties.Count)
                    {
                        ApplyPenalty(settings.conditionalClickPenalties[penaltyIndex]);
                    }
                    else
                    {
                        ApplyPenalty(stage.incorrectInteractionPenalty);
                    }
                }
            }
            else
            {
                // 일반 클릭 처리
                if (settings.targetObjectTag == itemId)
                {
                    // 유효한 클릭, 다음 스테이지로 진행
                    AdvanceToNextStage();
                }
                else
                {
                    // 잘못된 클릭, 패널티 적용
                    ApplyPenalty(stage.incorrectInteractionPenalty);
                }
            }
        }
        
        /// <summary>
        /// 드래그 인터랙션을 처리합니다.
        /// </summary>
        private void HandleDragInteraction(string objectTag, Vector2 dragDirection, bool isTwoFingerDrag, InteractionStage stage)
        {
            InteractionSettings settings = stage.settings;
            
            // 모든 화살표 숨기기
            foreach (GameObject arrow in activeArrows)
            {
                if (arrow != null)
                {
                    arrow.SetActive(false);
                }
            }
            
            // 올바른 대상 확인
            if (settings.targetObjectTag != objectTag)
            {
                ApplyPenalty(stage.incorrectInteractionPenalty);
                return;
            }
            
            // 투 핑거 드래그 확인
            if (settings.requireTwoFingerDrag && !isTwoFingerDrag)
            {
                ApplyPenalty(stage.incorrectInteractionPenalty);
                return;
            }
            
            // 드래그 방향 확인
            if (settings.requiredDragDirection)
            {
                float angle = Vector2.Angle(dragDirection, settings.arrowDirection);
                
                if (angle > 30f) // 30도 이상 차이나면 잘못된 방향
                {
                    ApplyPenalty(stage.incorrectInteractionPenalty);
                    return;
                }
            }
            
            // 모든 조건 통과, 다음 스테이지로 진행
            AdvanceToNextStage();
        }
        
        /// <summary>
        /// 패널티를 적용합니다.
        /// </summary>
        private void ApplyPenalty(PenaltyData penaltyData)
        {
            if (penaltyData == null)
                return;
            
            // 패널티 적용
            if (scoringSystem != null)
            {
                scoringSystem.ApplyPenalty(penaltyData, "Interaction");
            }
            else if (penaltyManager != null)
            {
                penaltyManager.ApplyPenalty(penaltyData);
            }
            
            // 인터랙션 일시 중지
            isInteractionBlocked = true;
            
            // 패널티 후 인터랙션 다시 활성화
            StartCoroutine(UnblockInteractionAfterDelay(2f));
        }
        
        /// <summary>
        /// 지정된 시간 후에 인터랙션 차단을 해제합니다.
        /// </summary>
        private IEnumerator UnblockInteractionAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            isInteractionBlocked = false;
        }
        
        /// <summary>
        /// 인터랙션을 완료합니다.
        /// </summary>
        private void CompleteInteraction()
        {
            isInteractionActive = false;
            currentStageIndex = -1;
            
            // 가이드 메시지 초기화
            if (guideMessageText != null)
            {
                guideMessageText.text = "";
            }
            
            Debug.Log($"Interaction completed: {currentInteraction.displayName}");
            
            // 완료 이벤트 발생 (필요시 추가)
        }
    }
}
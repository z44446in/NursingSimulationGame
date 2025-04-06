using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nursing.Procedure;
using Nursing.Interaction;
using Nursing.Penalty;
using Nursing.Scoring;

namespace Nursing.Managers
{
    public class ProcedureManager : MonoBehaviour
    {
        [Header("프로시저 참조")]
        [SerializeField] private ProcedureType currentProcedureType;
        [SerializeField] private int currentStepIndex = -1;
        
        [Header("UI 요소")]
        [SerializeField] private Text guideMessageText;
        [SerializeField] private GameObject actionButtonPrefab;
        [SerializeField] private Transform actionButtonContainer;
        
        [Header("프로시저 상태")]
        [SerializeField] private bool isProcedureActive = false;
        [SerializeField] private bool isProcedureBlocked = false;
        
        private NewScoringSystem scoringSystem;
        private PenaltyManager penaltyManager;
        private InteractionManager interactionManager;
        private DialogueManager dialogueManager;
        
        private List<GameObject> activeActionButtons = new List<GameObject>();
        
        private void Awake()
        {
            scoringSystem = FindObjectOfType<NewScoringSystem>();
            penaltyManager = FindObjectOfType<PenaltyManager>();
            interactionManager = FindObjectOfType<InteractionManager>();
            dialogueManager = FindObjectOfType<DialogueManager>();
        }
        
        /// <summary>
        /// 프로시저를 시작합니다.
        /// </summary>
        /// <param name="procedureType">시작할 프로시저 타입</param>
        public void StartProcedure(ProcedureType procedureType)
        {
            if (procedureType == null || procedureType.procedureData == null)
            {
                Debug.LogWarning("Cannot start procedure: ProcedureType or ProcedureData is null");
                return;
            }
            
            currentProcedureType = procedureType;
            currentStepIndex = -1;
            isProcedureActive = true;
            isProcedureBlocked = false;
            
            ProcedureData procedureData = procedureType.procedureData;
            
            // 가이드 메시지 표시
            if (guideMessageText != null && !string.IsNullOrEmpty(procedureData.guideMessage))
            {
                guideMessageText.text = procedureData.guideMessage;
            }
            
            // 첫 번째 스텝 시작
            AdvanceToNextStep();
            
            Debug.Log($"Procedure started: {procedureType.displayName} ({procedureType.versionType})");
        }
        
        /// <summary>
        /// 다음 스텝으로 진행합니다.
        /// </summary>
        public void AdvanceToNextStep()
        {
            if (!isProcedureActive || currentProcedureType == null || currentProcedureType.procedureData == null)
                return;
            
            if (isProcedureBlocked)
            {
                Debug.Log("Cannot advance: Procedure is blocked");
                return;
            }
            
            // 이전 스텝 정리
            CleanupCurrentStep();
            
            ProcedureData procedureData = currentProcedureType.procedureData;
            
            // 다음 스텝 인덱스 계산
            currentStepIndex++;
            
            // 프로시저 완료 확인
            if (currentStepIndex >= procedureData.steps.Count)
            {
                CompleteProcedure();
                return;
            }
            
            // 현재 스텝 설정 및 시작
            ProcedureStep currentStep = procedureData.steps[currentStepIndex];
            SetupStep(currentStep);
            
            Debug.Log($"Advanced to step {currentStepIndex + 1}: {currentStep.name}");
        }
        
        /// <summary>
        /// 현재 스텝을 설정합니다.
        /// </summary>
        private void SetupStep(ProcedureStep step)
        {
            if (step == null)
                return;
            
            // 가이드 메시지 업데이트
            if (guideMessageText != null && !string.IsNullOrEmpty(step.guideMessage))
            {
                guideMessageText.text = step.guideMessage;
            }
            
            // 스텝 타입에 따른 설정
            switch (step.stepType)
            {
                case ProcedureStepType.ItemClick:
                    SetupItemClickStep(step);
                    break;
                case ProcedureStepType.ActionButtonClick:
                    SetupActionButtonStep(step);
                    break;
                case ProcedureStepType.PlayerInteraction:
                    SetupPlayerInteractionStep(step);
                    break;
            }
        }
        
        /// <summary>
        /// 아이템 클릭 스텝을 설정합니다.
        /// </summary>
        private void SetupItemClickStep(ProcedureStep step)
        {
            // 아이템 클릭 스텝은 특별한 설정이 필요 없음
            // 아이템 클릭을 기다림
        }
        
        /// <summary>
        /// 액션 버튼 스텝을 설정합니다.
        /// </summary>
        private void SetupActionButtonStep(ProcedureStep step)
        {
            ProcedureStepSettings settings = step.settings;
            
            if (settings.isActionButtonClick && actionButtonPrefab != null && actionButtonContainer != null)
            {
                // 액션 버튼 패널 표시
                foreach (string buttonId in settings.correctButtonIds)
                {
                    GameObject buttonObj = Instantiate(actionButtonPrefab, actionButtonContainer);
                    Button button = buttonObj.GetComponent<Button>();
                    Text buttonText = buttonObj.GetComponentInChildren<Text>();
                    
                    if (buttonText != null)
                    {
                        buttonText.text = buttonId;
                    }
                    
                    if (button != null)
                    {
                        string capturedId = buttonId; // 클로저 변수 캡처
                        button.onClick.AddListener(() => OnActionButtonClicked(capturedId));
                    }
                    
                    activeActionButtons.Add(buttonObj);
                }
                
                // 잘못된 버튼 몇 개 추가 (선택사항)
                // ...
            }
        }
        
        /// <summary>
        /// 플레이어 인터랙션 스텝을 설정합니다.
        /// </summary>
        private void SetupPlayerInteractionStep(ProcedureStep step)
        {
            // 플레이어 인터랙션 스텝은 특별한 설정이 필요 없음
            // 플레이어 인터랙션을 기다림
        }
        
        /// <summary>
        /// 현재 스텝을 정리합니다.
        /// </summary>
        private void CleanupCurrentStep()
        {
            // 액션 버튼 제거
            foreach (GameObject button in activeActionButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            activeActionButtons.Clear();
            
            // 실행 중인 코루틴 중지
            StopAllCoroutines();
        }
        
        /// <summary>
        /// 아이템 클릭 이벤트를 처리합니다.
        /// </summary>
        public void OnItemClicked(string itemId)
        {
            if (!isProcedureActive || isProcedureBlocked || currentStepIndex < 0 || 
                currentProcedureType == null || currentProcedureType.procedureData == null)
                return;
            
            ProcedureData procedureData = currentProcedureType.procedureData;
            
            if (currentStepIndex >= procedureData.steps.Count)
                return;
            
            ProcedureStep currentStep = procedureData.steps[currentStepIndex];
            
            // 순서 요구사항 확인
            if (ShouldCheckOrder(currentStep))
            {
                if (!IsCorrectOrder(currentStep))
                {
                    ApplyPenalty(currentStep.incorrectOrderPenalty);
                    return;
                }
            }
            
            if (currentStep.stepType == ProcedureStepType.ItemClick)
            {
                HandleItemClickStep(itemId, currentStep);
            }
        }
        
        /// <summary>
        /// 액션 버튼 클릭 이벤트를 처리합니다.
        /// </summary>
        public void OnActionButtonClicked(string buttonId)
        {
            if (!isProcedureActive || isProcedureBlocked || currentStepIndex < 0 || 
                currentProcedureType == null || currentProcedureType.procedureData == null)
                return;
            
            ProcedureData procedureData = currentProcedureType.procedureData;
            
            if (currentStepIndex >= procedureData.steps.Count)
                return;
            
            ProcedureStep currentStep = procedureData.steps[currentStepIndex];
            
            if (currentStep.stepType == ProcedureStepType.ActionButtonClick)
            {
                HandleActionButtonStep(buttonId, currentStep);
            }
        }
        
        /// <summary>
        /// 아이템 클릭 스텝을 처리합니다.
        /// </summary>
        private void HandleItemClickStep(string itemId, ProcedureStep step)
        {
            ProcedureStepSettings settings = step.settings;
            
            if (settings.isItemClick && settings.itemId == itemId)
            {
                // 올바른 아이템 클릭
                
                // 연결된 인터랙션 시작
                if (!string.IsNullOrEmpty(settings.interactionDataId) && interactionManager != null)
                {
                    // 인터랙션 ID로 인터랙션 데이터 찾기 (구현 필요)
                    InteractionData interactionData = FindInteractionData(settings.interactionDataId);
                    
                    if (interactionData != null)
                    {
                        // 인터랙션 관리자에 인터랙션 시작 위임
                        interactionManager.StartInteraction(interactionData);
                        
                        // 인터랙션 완료 후 다음 스텝으로 진행하는 로직은
                        // 인터랙션 관리자에서 이벤트 또는 콜백을 통해 처리해야 함
                    }
                    else
                    {
                        // 인터랙션 데이터를 찾을 수 없음, 그냥 다음 스텝으로 진행
                        AdvanceToNextStep();
                    }
                }
                else
                {
                    // 연결된 인터랙션 없음, 그냥 다음 스텝으로 진행
                    AdvanceToNextStep();
                }
            }
            else
            {
                // 잘못된 아이템 클릭, 패널티 적용
                ApplyPenalty(step.incorrectActionPenalty);
            }
        }
        
        /// <summary>
        /// 액션 버튼 스텝을 처리합니다.
        /// </summary>
        private void HandleActionButtonStep(string buttonId, ProcedureStep step)
        {
            ProcedureStepSettings settings = step.settings;
            
            if (settings.isActionButtonClick)
            {
                if (settings.correctButtonIds.Contains(buttonId))
                {
                    // 올바른 버튼 클릭
                    
                    if (settings.requireAllButtons)
                    {
                        // 버튼 체크 로직 필요 (모든 버튼을 클릭했는지 확인)
                        // 간단한 구현을 위해 생략
                        
                        // 일단 모든 버튼이 클릭되었다고 가정
                        AdvanceToNextStep();
                    }
                    else
                    {
                        // 하나의 버튼만 필요한 경우
                        AdvanceToNextStep();
                    }
                }
                else
                {
                    // 잘못된 버튼 클릭, 패널티 적용
                    ApplyPenalty(step.incorrectActionPenalty);
                }
            }
        }
        
        /// <summary>
        /// 플레이어 인터랙션 이벤트를 처리합니다.
        /// </summary>
        public void OnPlayerInteraction(string interactionTag)
        {
            if (!isProcedureActive || isProcedureBlocked || currentStepIndex < 0 || 
                currentProcedureType == null || currentProcedureType.procedureData == null)
                return;
            
            ProcedureData procedureData = currentProcedureType.procedureData;
            
            if (currentStepIndex >= procedureData.steps.Count)
                return;
            
            ProcedureStep currentStep = procedureData.steps[currentStepIndex];
            
            if (currentStep.stepType == ProcedureStepType.PlayerInteraction)
            {
                HandlePlayerInteractionStep(interactionTag, currentStep);
            }
        }
        
        /// <summary>
        /// 플레이어 인터랙션 스텝을 처리합니다.
        /// </summary>
        private void HandlePlayerInteractionStep(string interactionTag, ProcedureStep step)
        {
            ProcedureStepSettings settings = step.settings;
            
            if (settings.isPlayerInteraction && settings.validInteractionTags.Contains(interactionTag))
            {
                // 올바른 인터랙션, 다음 스텝으로 진행
                AdvanceToNextStep();
            }
            else
            {
                // 잘못된 인터랙션, 패널티 적용
                ApplyPenalty(step.incorrectActionPenalty);
            }
        }
        
        /// <summary>
        /// 순서 확인이 필요한지 여부를 반환합니다.
        /// </summary>
        private bool ShouldCheckOrder(ProcedureStep step)
        {
            return step.requireSpecificOrder && step.requiredPreviousStepIds.Count > 0;
        }
        
        /// <summary>
        /// 올바른 순서인지 확인합니다.
        /// </summary>
        private bool IsCorrectOrder(ProcedureStep step)
        {
            // 이전에 완료된 스텝 목록을 가져와야 함
            // (구현이 필요함 - 완료된 스텝 ID 추적)
            List<string> completedStepIds = GetCompletedStepIds();
            
            // 필요한 모든 이전 스텝이 완료되었는지 확인
            foreach (string requiredStepId in step.requiredPreviousStepIds)
            {
                if (!completedStepIds.Contains(requiredStepId))
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 완료된 스텝 ID 목록을 가져옵니다.
        /// </summary>
        private List<string> GetCompletedStepIds()
        {
            List<string> completedStepIds = new List<string>();
            
            if (currentProcedureType == null || currentProcedureType.procedureData == null)
                return completedStepIds;
            
            ProcedureData procedureData = currentProcedureType.procedureData;
            
            // 현재 스텝 이전의 모든 스텝을 완료로 간주
            for (int i = 0; i < currentStepIndex; i++)
            {
                if (i < procedureData.steps.Count)
                {
                    completedStepIds.Add(procedureData.steps[i].id);
                }
            }
            
            return completedStepIds;
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
                scoringSystem.ApplyPenalty(penaltyData, "Procedure");
            }
            else if (penaltyManager != null)
            {
                penaltyManager.ApplyPenalty(penaltyData);
            }
            
            // 프로시저 일시 중지
            isProcedureBlocked = true;
            
            // 패널티 후 프로시저 다시 활성화
            StartCoroutine(UnblockProcedureAfterDelay(2f));
        }
        
        /// <summary>
        /// 지정된 시간 후에 프로시저 차단을 해제합니다.
        /// </summary>
        private IEnumerator UnblockProcedureAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            isProcedureBlocked = false;
        }
        
        /// <summary>
        /// 인터랙션 ID로 인터랙션 데이터를 찾습니다.
        /// </summary>
        private InteractionData FindInteractionData(string interactionId)
        {
            // 인터랙션 데이터 찾기 구현 (인터랙션 데이터 레지스트리 또는 리소스에서 가져오기)
            // 예시 구현 (간소화):
            InteractionData[] allInteractions = Resources.LoadAll<InteractionData>("InteractionData");
            
            foreach (InteractionData interaction in allInteractions)
            {
                if (interaction.id == interactionId)
                {
                    return interaction;
                }
            }
            
            Debug.LogWarning($"InteractionData with ID '{interactionId}' not found");
            return null;
        }
        
        /// <summary>
        /// 프로시저를 완료합니다.
        /// </summary>
        private void CompleteProcedure()
        {
            isProcedureActive = false;
            currentStepIndex = -1;
            
            // 가이드 메시지 초기화
            if (guideMessageText != null)
            {
                guideMessageText.text = "";
            }
            
            Debug.Log($"Procedure completed: {currentProcedureType.displayName}");
            
            // 완료 이벤트 발생 (필요시 추가)
        }
    }
}
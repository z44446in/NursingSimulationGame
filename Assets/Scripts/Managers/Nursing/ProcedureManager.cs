using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nursing.Procedure;
using Nursing.Interaction;
using Nursing.Penalty;

namespace Nursing.Managers
{
    public class ProcedureManager : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private InteractionManager interactionManager;
        [SerializeField] private PenaltyManager penaltyManager;
        [SerializeField] private DialogueManager dialogueManager;
        
        [Header("액션 버튼 설정")]
        [SerializeField] private GameObject actionPopupPrefab;

        [Header("프로시저 타입 참조")]
        [SerializeField] private ProcedureType[] availableProcedureTypes;

        private ProcedureType currentProcedureType;
        private ProcedureData currentProcedure;
        private int currentStepIndex = -1;
        private ProcedureStep currentStep;
        private List<string> completedStepIds = new List<string>();
        private bool procedureInProgress = false;
        
        private void Awake()
        {
            if (interactionManager == null)
                interactionManager = FindObjectOfType<InteractionManager>();
                
            if (penaltyManager == null)
                penaltyManager = FindObjectOfType<PenaltyManager>();
                
            if (dialogueManager == null)
                dialogueManager = FindObjectOfType<DialogueManager>();
        }
        private void Start()
        {
            // GameManager로부터 선택된 정보 가져오기
            ProcedureTypeEnum selectedType = GameManager.Instance.currentProcedureType;
            ProcedureVersionType selectedVersion = GameManager.Instance.currentVersionType;
            ProcedurePlayType selectedPlayType = GameManager.Instance.currentPlayType;

            // 선택된 정보에 맞는 ProcedureType 찾기
            ProcedureType procedureToLoad = FindMatchingProcedureType(selectedType, selectedVersion, selectedPlayType);

            if (procedureToLoad != null)
            {
                // 해당 프로시저 시작
                StartProcedure(procedureToLoad);
            }
            else
            {
                Debug.LogError("선택한 조건에 맞는 프로시저를 찾을 수 없습니다!");
            }
        }

        private ProcedureType FindMatchingProcedureType(ProcedureTypeEnum type, ProcedureVersionType version, ProcedurePlayType playType)
        {
            foreach (var procedure in availableProcedureTypes)
            {
                if (procedure.ProcdureTypeName == type &&
                    procedure.versionType == version &&
                    procedure.procedurePlayType == playType)
                {
                    return procedure;
                }
            }
            return null;
        }
        /// <summary>
        /// 프로시저를 시작합니다.
        /// </summary>
        public void StartProcedure(ProcedureType procedureType)
        {
            if (procedureType == null)
            {
                Debug.LogError("프로시저 타입이 없습니다.");
                return;
            }
            
            // 진행 중인 프로시저가 있으면 정리
            if (procedureInProgress)
            {
                CleanupCurrentProcedure();
            }
            
            currentProcedureType = procedureType;
            currentProcedure = procedureType.procedureData;
            
            if (currentProcedure == null)
            {
                Debug.LogError("프로시저 데이터가 없습니다.");
                return;
            }
            
            currentStepIndex = -1;
            completedStepIds.Clear();
            procedureInProgress = true;
            
            // 가이드 메시지 표시
            if (!string.IsNullOrEmpty(currentProcedure.guideMessage) && dialogueManager != null)
            {
                dialogueManager.ShowGuideMessage(currentProcedure.guideMessage);
            }
            
            // 첫 스텝으로 진행
            AdvanceToNextStep();
        }
        
        /// <summary>
        /// 다음 프로시저 스텝으로 진행합니다.
        /// </summary>
        public void AdvanceToNextStep()
        {
            if (currentProcedure == null || !procedureInProgress)
                return;
            
            currentStepIndex++;
            
            // 모든 스텝을 완료했는지 확인
            if (currentStepIndex >= currentProcedure.steps.Count)
            {
                CompleteProcedure();
                return;
            }
            
            currentStep = currentProcedure.steps[currentStepIndex];
            
            // 가이드 메시지 업데이트
            if (!string.IsNullOrEmpty(currentStep.guideMessage) && dialogueManager != null)
            {
                dialogueManager.ShowGuideMessage(currentStep.guideMessage);
            }
            
            // 순서 확인 - 이전 스텝이 완료되어야 하는 경우
            if (currentStep.requireSpecificOrder && currentStep.requiredPreviousStepIds.Count > 0)
            {
                bool validOrder = true;
                
                foreach (string requiredStepId in currentStep.requiredPreviousStepIds)
                {
                    if (!completedStepIds.Contains(requiredStepId))
                    {
                        validOrder = false;
                        break;
                    }
                }
                
                if (!validOrder && currentStep.incorrectOrderPenalty != null)
                {
                    ApplyPenalty(currentStep.incorrectOrderPenalty);
                    return; // 순서가 잘못된 경우 진행하지 않음
                }
            }
            
            // 스텝 타입에 따른 처리
            SetupStepBasedOnType();
        }
        
        /// <summary>
        /// 현재 스텝 타입에 따라 프로시저를 설정합니다.
        /// </summary>
        private void SetupStepBasedOnType()
        {
            switch (currentStep.stepType)
            {
                case ProcedureStepType.ItemClick:
                    // 아이템 클릭 스텝은 외부에서 HandleItemClick 메서드를 호출하여 처리합니다.
                    break;
                    
                case ProcedureStepType.ActionButtonClick:
                    SetupActionButtonClick();
                    break;
                    
                case ProcedureStepType.PlayerInteraction:
                    // 플레이어 직접 상호작용 스텝은 외부에서 HandlePlayerInteraction 메서드를 호출하여 처리합니다.
                    break;
                    
                default:
                    Debug.LogWarning("지원하지 않는 스텝 타입입니다: " + currentStep.stepType);
                    AdvanceToNextStep(); // 지원하지 않는 타입은 건너뜁니다.
                    break;
            }
        }
        
        /// <summary>
        /// 액션 버튼 클릭 스텝을 설정합니다.
        /// </summary>
        private void SetupActionButtonClick()
        {
            if (actionPopupPrefab == null)
            {
                Debug.LogError("액션 팝업 프리팹이 없습니다.");
                return;
            }
            
            // 액션 팝업 생성
            GameObject actionPopup = Instantiate(actionPopupPrefab, transform);
            ActionPopupController actionController = actionPopup.GetComponent<ActionPopupController>();
            
            if (actionController == null)
            {
                Debug.LogError("액션 팝업 프리팹에 ActionPopupController 컴포넌트가 없습니다.");
                Destroy(actionPopup);
                return;
            }
            
            // 액션 버튼 설정
            var settings = currentStep.settings;
            
            if (settings == null || settings.correctButtonIds == null || settings.correctButtonIds.Count == 0)
            {
                Debug.LogError("액션 버튼 설정이 없거나 올바른 버튼 ID가 없습니다.");
                Destroy(actionPopup);
                return;
            }
            
            // 액션 팝업 설정
            actionController.SetupActionPopup(
                settings.correctButtonIds,
                settings.requireAllButtons
            );
            
            // 액션 결과 이벤트 구독
            actionController.OnActionComplete += (bool success) => {
                if (success)
                {
                    // 성공적인 액션 버튼 클릭
                    CompleteStep();
                }
                else if (currentStep.incorrectActionPenalty != null)
                {
                    // 잘못된 액션 버튼 클릭
                    ApplyPenalty(currentStep.incorrectActionPenalty);
                }
                
                Destroy(actionPopup);
            };
        }
        
        /// <summary>
        /// 아이템 클릭을 처리합니다.
        /// </summary>
        public bool HandleItemClick(string itemId)
        {
            if (!procedureInProgress || currentStep == null || currentStep.stepType != ProcedureStepType.ItemClick)
                return false;
            
            var settings = currentStep.settings;
            
            if (settings == null || !settings.isItemClick || string.IsNullOrEmpty(settings.itemId))
                return false;
            
            // 클릭한 아이템이 현재 스텝의 아이템과 일치하는지 확인
            if (settings.itemId == itemId)
            {
                // 인터랙션 데이터가 있으면 인터랙션 시작
                if (!string.IsNullOrEmpty(settings.interactionDataId) && interactionManager != null)
                {
                    InteractionData interactionData = Resources.Load<InteractionData>("Interactions/" + settings.interactionDataId);
                    
                    if (interactionData != null)
                    {
                        interactionManager.StartInteraction(interactionData);
                        
                        // 인터랙션이 완료되면 스텝 완료 처리 (인터랙션 매니저에서 이벤트를 통해 알림)
                        // TODO: InteractionManager에 OnInteractionComplete 이벤트 추가
                    }
                    else
                    {
                        Debug.LogWarning("인터랙션 데이터를 찾을 수 없습니다: " + settings.interactionDataId);
                        CompleteStep(); // 인터랙션 데이터가 없어도 스텝 완료 처리
                    }
                }
                else
                {
                    CompleteStep(); // 인터랙션 없이 스텝 완료 처리
                }
                
                return true;
            }
            
            // 잘못된 아이템 클릭
            if (currentStep.incorrectActionPenalty != null)
            {
                ApplyPenalty(currentStep.incorrectActionPenalty);
            }
            
            return false;
        }
        
        /// <summary>
        /// 플레이어 상호작용을 처리합니다.
        /// </summary>
        public bool HandlePlayerInteraction(string interactionTag)
        {
            if (!procedureInProgress || currentStep == null || currentStep.stepType != ProcedureStepType.PlayerInteraction)
                return false;
            
            var settings = currentStep.settings;
            
            if (settings == null || !settings.isPlayerInteraction || settings.validInteractionTags == null)
                return false;
            
            // 상호작용 태그가 유효한지 확인
            if (settings.validInteractionTags.Contains(interactionTag))
            {
                CompleteStep();
                return true;
            }
            
            // 잘못된 상호작용
            if (currentStep.incorrectActionPenalty != null)
            {
                ApplyPenalty(currentStep.incorrectActionPenalty);
            }
            
            return false;
        }
        
        /// <summary>
        /// 현재 스텝을 완료합니다.
        /// </summary>
        private void CompleteStep()
        {
            if (currentStep != null && !string.IsNullOrEmpty(currentStep.id))
            {
                completedStepIds.Add(currentStep.id);
            }
            
            AdvanceToNextStep();
        }
        
        /// <summary>
        /// 프로시저를 완료합니다.
        /// </summary>
        private void CompleteProcedure()
        {
            Debug.Log("프로시저 완료: " + currentProcedure.displayName);
            
            // 프로시저 정리
            CleanupCurrentProcedure();
            
            // 프로시저 완료 이벤트를 발생시킬 수 있음
            // OnProcedureComplete?.Invoke(currentProcedureType);
        }
        
        /// <summary>
        /// 현재 프로시저를 정리합니다.
        /// </summary>
        private void CleanupCurrentProcedure()
        {
            currentProcedureType = null;
            currentProcedure = null;
            currentStep = null;
            currentStepIndex = -1;
            completedStepIds.Clear();
            procedureInProgress = false;
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
    }
    
    /// <summary>
    /// 액션 팝업 컨트롤러 클래스
    /// </summary>
    public class ActionPopupController : MonoBehaviour
    {
        [SerializeField] private Button[] actionButtons;
        [SerializeField] private Text[] buttonTexts;
        
        private List<string> correctButtonIds;
        private bool requireAllButtons;
        private List<string> selectedButtonIds = new List<string>();
        
        public event System.Action<bool> OnActionComplete;
        
        /// <summary>
        /// 액션 팝업을 설정합니다.
        /// </summary>
        public void SetupActionPopup(List<string> correctIds, bool requireAll)
        {
            correctButtonIds = correctIds;
            requireAllButtons = requireAll;
            selectedButtonIds.Clear();
            
            // 버튼 설정
            for (int i = 0; i < actionButtons.Length; i++)
            {
                int buttonIndex = i; // 클로저에서 사용하기 위해 로컬 변수로 복사
                
                // 버튼 텍스트 설정 (선택적)
                if (buttonTexts.Length > i && buttonTexts[i] != null && i < correctButtonIds.Count)
                {
                    buttonTexts[i].text = correctButtonIds[i];
                }
                
                // 버튼 클릭 이벤트 설정
                actionButtons[i].onClick.RemoveAllListeners();
                actionButtons[i].onClick.AddListener(() => OnActionButtonClicked(buttonIndex));
            }
        }
        
        /// <summary>
        /// 액션 버튼 클릭 처리
        /// </summary>
        private void OnActionButtonClicked(int buttonIndex)
        {
            if (buttonIndex >= actionButtons.Length)
                return;
            
            // 버튼 ID 가져오기 (텍스트 또는 이름으로 설정할 수 있음)
            string buttonId = buttonTexts.Length > buttonIndex && buttonTexts[buttonIndex] != null 
                ? buttonTexts[buttonIndex].text 
                : actionButtons[buttonIndex].name;
            
            // 이미 선택된 버튼인지 확인
            if (selectedButtonIds.Contains(buttonId))
                return;
            
            selectedButtonIds.Add(buttonId);
            
            // 모든 필요한 버튼이 선택되었는지 확인
            bool isSuccess = CheckActionSuccess();
            
            if (isSuccess || !requireAllButtons)
            {
                // 성공 또는 실패 이벤트 발생
                OnActionComplete?.Invoke(isSuccess);
            }
        }
        
        /// <summary>
        /// 액션이 성공했는지 확인합니다.
        /// </summary>
        private bool CheckActionSuccess()
        {
            if (requireAllButtons)
            {
                // 모든 올바른 버튼이 선택되어야 함
                foreach (string correctId in correctButtonIds)
                {
                    if (!selectedButtonIds.Contains(correctId))
                    {
                        return false;
                    }
                }
                
                // 추가 버튼이 선택되지 않았는지 확인
                return selectedButtonIds.Count == correctButtonIds.Count;
            }
            else
            {
                // 하나의 올바른 버튼이 선택되면 됨
                foreach (string selectedId in selectedButtonIds)
                {
                    if (correctButtonIds.Contains(selectedId))
                    {
                        return true;
                    }
                }
                
                return false;
            }
        }
    }
}
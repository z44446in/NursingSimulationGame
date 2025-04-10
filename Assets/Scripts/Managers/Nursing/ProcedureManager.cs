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
       
       
        private List<string> completedStepIds = new List<string>();
        private List<ProcedureStep> availableSteps = new List<ProcedureStep>();
        private bool procedureInProgress = false;

        [SerializeField] private List<InteractionData> availableInteractions; //클로드가 scriptableobject 폴더에서 찾고싶으면 추가하라고 한 코드 

        [SerializeField] private CartUI cartUI; // CartUI 참조 추가

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
                Debug.Log("프로시저 시작합니다!:"+ selectedType + selectedVersion + selectedPlayType );
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

            completedStepIds.Clear();
            procedureInProgress = true;

            // 초기 가용 스텝 설정
            UpdateAvailableSteps();

            // 가이드 메시지 표시


        }

        // 가용 스텝 업데이트 - 패널티 적용 없이 단순 업데이트만 수행
        private void UpdateAvailableSteps()
        {
            availableSteps.Clear();

            foreach (var step in currentProcedure.steps)
            {
                // 이미 완료된 스텝은 제외
                if (completedStepIds.Contains(step.id))
                    continue;

                // 순서 제약이 있는 스텝의 경우 선행 스텝 확인
                if (step.requireSpecificOrder)
                {
                    bool validOrder = true;

                    foreach (string requiredStepId in step.requiredPreviousStepIds)
                    {
                        if (!completedStepIds.Contains(requiredStepId))
                        {
                            validOrder = false;
                            break;
                        }
                    }

                    if (!validOrder)
                        continue; // 순서가 맞지 않으면 가용 스텝에 추가하지 않음 (패널티 적용 없음)
                }

                // 모든 조건을 만족하면 가용 스텝에 추가
                availableSteps.Add(step);
            }
        }

        // 스텝 처리 (외부에서 호출)
        public bool HandleStepInteraction(string interactionType, string targetId)
        {
            if (!procedureInProgress || availableSteps.Count == 0)
                return false;

            // 현재 가용 스텝 중에서 일치하는 스텝 찾기
            foreach (var step in availableSteps)
            {
                bool isMatch = false;

                // 인터랙션 타입에 따라 처리
                switch (step.stepType)
                {
                    case ProcedureStepType.ItemClick:
                        isMatch = (interactionType == "ItemClick" && step.settings.itemId == targetId);
                        break;

                    case ProcedureStepType.ActionButtonClick:
                        isMatch = (interactionType == "ActionButtonClick" && step.settings.correctButtonIds.Contains(targetId));
                        break;

                    case ProcedureStepType.PlayerInteraction:
                        isMatch = (interactionType == "PlayerInteraction" && step.settings.validInteractionTags.Contains(targetId));
                        break;
                }

                if (isMatch)
                {
                    // 스텝 처리 및 완료
                    ProcessStep(step);
                    return true;
                }
            }

            return false;
        }

        // 스텝 처리 및 완료
        // 스텝 처리 및 완료
        private void ProcessStep(ProcedureStep step)
        {
            // 가이드 메시지 표시
            if (!string.IsNullOrEmpty(step.guideMessage) && dialogueManager != null)
            {
                dialogueManager.ShowGuideMessage(step.guideMessage);
            }

            // 스텝 설정 - SetupStepBasedOnType 호출
            SetupStepBasedOnType(step);
        }

        // 현재 스텝 타입에 따라 프로시저를 설정합니다.
        private void SetupStepBasedOnType(ProcedureStep step)
        {
            switch (step.stepType)
            {
                case ProcedureStepType.ItemClick:
                    // 아이템 클릭 스텝의 인터랙션 데이터 처리
                    if (!string.IsNullOrEmpty(step.settings.interactionDataId) && interactionManager != null)
                    {
                        InteractionData interactionData = FindInteractionDataById(step.settings.interactionDataId);
                        if (interactionData != null)
                        {
                            // 완료 처리를 위한 로컬 함수 정의
                            void OnComplete(bool success)
                            {
                                // 이벤트 구독 해제
                                interactionManager.OnInteractionComplete -= OnComplete;

                                if (success)
                                {
                                    CompleteStep(step);
                                }
                            }

                            // 인터랙션 완료 이벤트 구독
                            interactionManager.OnInteractionComplete += OnComplete;

                            // 인터랙션 시작
                            interactionManager.StartInteraction(interactionData);
                        }
                        else
                        {
                            Debug.LogWarning("인터랙션 데이터를 찾을 수 없습니다: " + step.settings.interactionDataId);
                            CompleteStep(step);
                        }
                    }
                    else
                    {
                        // 인터랙션 데이터 없이 바로 완료
                        CompleteStep(step);
                    }
                    break;

                case ProcedureStepType.ActionButtonClick:
                    SetupActionButtonClick(step);
                    break;

                case ProcedureStepType.PlayerInteraction:
                    // 플레이어 상호작용은 HandlePlayerInteraction에서 처리
                    // 특별한 설정이 필요하면 여기에 추가
                    break;

                default:
                    Debug.LogWarning("지원하지 않는 스텝 타입입니다: " + step.stepType);
                    // 지원하지 않는 타입은 건너뛰고 완료 처리
                    CompleteStep(step);
                    break;
            }
        }

        // 스텝 완료
        // CompleteStep() -> 파라미터로 step을 받음
        private void CompleteStep(ProcedureStep step)
        {
            if (step != null && !string.IsNullOrEmpty(step.id))
            {
                completedStepIds.Add(step.id);
            }

            // 가용 스텝 업데이트
            UpdateAvailableSteps();

            // 모든 스텝 완료 체크
            CheckProcedureCompletion();
        }

        // 프로시저 완료 체크
        private void CheckProcedureCompletion()
        {
            if (completedStepIds.Count >= currentProcedure.steps.Count)
            {
                CompleteProcedure();
            }
        }

       
        
        
        /// <summary>
        /// 액션 버튼 클릭 스텝을 설정합니다.
        /// </summary>
        private void SetupActionButtonClick(ProcedureStep step)
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
            var settings = step.settings;
            
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
                    CompleteStep(step);
                }
                else if (step.incorrectActionPenalty != null)
                {
                    // 잘못된 액션 버튼 클릭
                    ApplyPenalty(step.incorrectActionPenalty);
                }
                
                Destroy(actionPopup);
            };
        }

        /// <summary>
        /// 아이템 클릭을 처리합니다.
        /// </summary>
        // 아이템 클릭 처리 메서드 - 여기서 순서 검사 후 패널티 적용
        public bool HandleItemClick(string itemId)
        {
            if (!procedureInProgress)
                return false;

            // 모든 스텝을 확인 (가용 스텝에 없는 스텝도 확인)
            foreach (var step in currentProcedure.steps)
            {
                if (step.stepType == ProcedureStepType.ItemClick &&
                    step.settings != null &&
                    step.settings.isItemClick &&
                    step.settings.itemId == itemId)
                {
                    // 이미 완료된 스텝인지 확인
                    if (completedStepIds.Contains(step.id))
                        return false; // 이미 완료된 스텝에 대한 상호작용은 무시

                    // 순서 제약 확인 및 패널티 적용
                    if (step.requireSpecificOrder)
                    {
                        List<string> missingStepIds = new List<string>();
                        bool validOrder = true;

                        foreach (string requiredStepId in step.requiredPreviousStepIds)
                        {
                            if (!completedStepIds.Contains(requiredStepId))
                            {
                                validOrder = false;
                                missingStepIds.Add(requiredStepId);
                            }
                        }

                        if (!validOrder)
                        {
                            // 중요: 동작 실행 전에 undoAction 설정
                            if (step.incorrectOrderPenalty != null)
                            {
                                // 현재 아이템 상태 미리 저장
                                Item itemReference = FindItemById(itemId);

                                step.incorrectOrderPenalty.undoAction = () => {
                                    Debug.Log("순서 오류 동작 취소: " + itemId);
                                    // 아이템 상태 복원 로직
                                    if (itemReference != null && cartUI != null)
                                    {
                                        // 이미 사라진 경우 다시 추가
                                        cartUI.AddItemToCart(itemReference);
                                    }
                                };

                                // 패널티 적용
                                ApplyPenalty(step.incorrectOrderPenalty);
                            }
                            return true;
                        }
                    }

                    // 가용 스텝 목록에 있는지 확인
                    if (availableSteps.Contains(step))
                    {
                        // 인터랙션 데이터 처리
                        if (!string.IsNullOrEmpty(step.settings.interactionDataId) && interactionManager != null)
                        {
                            InteractionData interactionData = FindInteractionDataById(step.settings.interactionDataId);

                            if (interactionData != null)
                            {
                                // 인터랙션 완료 이벤트 구독
                                void OnComplete(bool success)
                                {
                                    // 이벤트 구독 해제
                                    interactionManager.OnInteractionComplete -= OnComplete;

                                    if (success)
                                    {
                                        CompleteStep(step);
                                    }
                                    else if (step.incorrectActionPenalty != null)
                                    {
                                        ApplyPenalty(step.incorrectActionPenalty);
                                    }
                                }

                                interactionManager.OnInteractionComplete += OnComplete;
                                interactionManager.StartInteraction(interactionData);
                            }
                            else
                            {
                                Debug.LogWarning("인터랙션 데이터를 찾을 수 없습니다: " + step.settings.interactionDataId);
                                CompleteStep(step);
                            }
                        }
                        else
                        {
                            CompleteStep(step);
                        }
                        return true;
                    }

                    // 스텝을 찾았지만 가용 스텝 목록에 없는 경우 (다른 이유로 비활성화된 경우)
                    if (step.incorrectActionPenalty != null)
                    {
                        Debug.Log($"스텝을 찾았지만 현재 실행할 수 없음: {step.id}");
                        ApplyPenalty(step.incorrectActionPenalty);
                    }
                    return true;
                }
            }

            // 매칭되는 스텝을 찾지 못한 경우
            Debug.Log($"일치하는 아이템 스텝 없음: {itemId}");
            return false;
        }

        // 아이템 ID로 Item 찾는 유틸리티 메서드 추가
        // 런타임에서 Item ScriptableObject들을 찾는 유틸리티 메서드
        private Item FindItemById(string itemId)
        {
            // Resources 폴더 내 모든 Item 가져오기
            Item[] allItems = Resources.LoadAll<Item>("");

            foreach (Item item in allItems)
            {
                if (item.itemId == itemId)
                    return item;
            }

            // 못 찾은 경우 특정 경로 직접 확인
            Item specificItem = Resources.Load<Item>("PrepareRoomItems" + itemId);
            if (specificItem != null)
                return specificItem;

            return null;
        }

        private InteractionData FindInteractionDataById(string id)
        {
            foreach (var interaction in availableInteractions)
            {
                if (interaction != null && interaction.id == id)
                    return interaction;
            }
            return null;
        }
        /// <summary>
        /// 플레이어 상호작용을 처리합니다.
        /// </summary>
        public bool HandlePlayerInteraction(string interactionTag)
        {
            if (!procedureInProgress)
                return false;

            // 모든 스텝을 확인 (가용 스텝에 없는 스텝도 확인)
            foreach (var step in currentProcedure.steps)
            {
                if (step.stepType == ProcedureStepType.PlayerInteraction &&
                    step.settings != null &&
                    step.settings.isPlayerInteraction &&
                    step.settings.validInteractionTags != null &&
                    step.settings.validInteractionTags.Contains(interactionTag))
                {
                    // 이미 완료된 스텝인지 확인
                    if (completedStepIds.Contains(step.id))
                        return false; // 이미 완료된 스텝에 대한 상호작용은 무시

                    // 순서 제약 확인 및 패널티 적용
                    if (step.requireSpecificOrder)
                    {
                        List<string> missingStepIds = new List<string>();
                        bool validOrder = true;

                        foreach (string requiredStepId in step.requiredPreviousStepIds)
                        {
                            if (!completedStepIds.Contains(requiredStepId))
                            {
                                validOrder = false;
                                missingStepIds.Add(requiredStepId);
                            }
                        }

                        if (!validOrder)
                        {
                            // 여기서 패널티 적용
                            if (step.incorrectOrderPenalty != null)
                            {
                                string missingStepsText = string.Join(", ", missingStepIds);
                                Debug.Log($"잘못된 순서 패널티 적용: {step.id}에 대한 선행 스텝 누락 - {missingStepsText}");
                                ApplyPenalty(step.incorrectOrderPenalty);
                            }
                            return true; // 패널티는 적용했지만 스텝은 완료하지 않음
                        }
                    }

                    // 가용 스텝 목록에 있는지 확인
                    if (availableSteps.Contains(step))
                    {
                        CompleteStep(step);
                        return true;
                    }

                    // 스텝을 찾았지만 가용 스텝 목록에 없는 경우
                    if (step.incorrectActionPenalty != null)
                    {
                        Debug.Log($"스텝을 찾았지만 현재 실행할 수 없음: {step.id}");
                        ApplyPenalty(step.incorrectActionPenalty);
                    }
                    return true;
                }
            }

            // 유효하지 않은 상호작용에 대한 패널티 처리
            Debug.Log($"일치하는 플레이어 상호작용 스텝 없음: {interactionTag}");
            return false;
        }

        /// <summary>
        /// 현재 스텝을 완료합니다.
        /// </summary>
        // CompleteStep() -> 파라미터로 step을 받음


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
        /// <summary>
        /// 현재 프로시저를 정리합니다.
        /// </summary>
        private void CleanupCurrentProcedure()
        {
            currentProcedureType = null;
            currentProcedure = null;
            // currentStep 변수 제거
            // currentStepIndex 변수 제거
            completedStepIds.Clear();
            availableSteps.Clear(); // 가용 스텝 목록 초기화
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
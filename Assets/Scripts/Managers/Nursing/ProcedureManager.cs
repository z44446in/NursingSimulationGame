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
        [SerializeField] private CentralOutlineManager outlineManager;

        public PenaltyData[] previousStepPenalties; // 배열로 변경

        [Header("액션 버튼 설정")]
        [SerializeField] private GameObject actionPopupPrefab;

        

        [Header("다음 스텝 제한")]
        public bool restrictNextSteps; // 다음 스텝 제한 여부
        public List<string> allowedNextStepIds; // 허용된 다음 스텝 ID 목록
        public PenaltyData invalidNextStepPenalty; // 허용되지 않은 다음 스텝 시도 시 패널티

        [Header("생략 설정")]
        public bool canBeSkipped; // 이 스텝이 생략 가능한지 여부

        private ProcedureType currentProcedureType;
        private ProcedureData currentProcedure;
       
       
        private List<string> completedStepIds = new List<string>();
        private List<ProcedureStep> availableSteps = new List<ProcedureStep>();
        public bool procedureInProgress = false;
        public bool Instep = false;
      


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

        void Update()
        {
          
                if (!Instep)
                {
                    outlineManager.StartBlinking();
                }
                else
                {
                    outlineManager.StopBlinking();
                }

           
            
        }

     
       

        private ProcedureType FindMatchingProcedureType(ProcedureTypeEnum type, ProcedureVersionType version, ProcedurePlayType playType)
        {
            ProcedureType[] allType = Resources.LoadAll<ProcedureType>("");

            foreach (ProcedureType Type in allType)
            {
                if (Type.ProcdureTypeName == type &&
                    Type.versionType == version &&
                    Type.procedurePlayType == playType)
                {
                    return Type;
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
            if (step.restrictNextSteps)
            {
                // 다음 허용된 스텝에 대한 가이드나 힌트를 제공하는 코드 추가 가능
                string allowedStepsMessage = "다음 수행 가능한 단계: ";
                for (int i = 0; i < step.allowedNextStepIds.Count; i++)
                {
                    string nextStepId = step.allowedNextStepIds[i];
                    ProcedureStep nextStep = currentProcedure.steps.Find(s => s.id == nextStepId);
                    if (nextStep != null)
                    {
                        allowedStepsMessage += nextStep.name;
                        if (i < step.allowedNextStepIds.Count - 1)
                            allowedStepsMessage += ", ";
                    }
                }

                // 가이드 메시지 표시
                if (dialogueManager != null)
                {
                    dialogueManager.ShowGuideMessage(allowedStepsMessage);
                }
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

            Instep = false;
        }

        // 프로시저 완료 체크
        private void CheckProcedureCompletion()
        {
            bool allRequiredStepsCompleted = true;

            foreach (var step in currentProcedure.steps)
            {
                // 생략 불가능한 스텝이 완료되지 않았는지 확인
                if (!step.canBeSkipped && !completedStepIds.Contains(step.id))
                {
                    allRequiredStepsCompleted = false;
                    break;
                }
            }

            if (allRequiredStepsCompleted)
            {
                CompleteProcedure();
            }
        }

       
        
        
        /// <summary>
        /// 액션 버튼 클릭 스텝을 설정합니다.
        /// </summary>
        private void SetupActionButtonClick(ProcedureStep step)
        {
            Instep = true;
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
            Instep = true;
            // ❗❗ 현재 시도하려는 step 먼저 찾기
            ProcedureStep stepCandidate = currentProcedure.steps.Find(s =>
                s.stepType == ProcedureStepType.ItemClick &&
                s.settings != null &&
                s.settings.isItemClick &&
                s.settings.itemId == itemId);

            // ❗❗ 없는 경우 early return
            if (stepCandidate == null)
            {
                Debug.Log($"일치하는 아이템 스텝 없음: {itemId}");
                return false;
            }

            // ✅ ✅ 새로운 검사 추가: 이전 완료된 스텝 중 restrictNextSteps == true인 애들 기준으로 검사
            foreach (var completedId in completedStepIds)
            {
                ProcedureStep completedStep = currentProcedure.steps.Find(s => s.id == completedId);
                if (completedStep != null && completedStep.restrictNextSteps)
                {
                    if (!completedStep.allowedNextStepIds.Contains(stepCandidate.id))
                    {
                        if (completedStep.invalidNextStepPenalty != null)
                        {
                            ApplyPenalty(completedStep.invalidNextStepPenalty);
                            Debug.LogWarning($"[제한 위반] 스텝 '{stepCandidate.id}'는 스텝 '{completedStep.id}' 이후 허용되지 않은 스텝입니다.");
                            return false;
                        }
                    }
                }
            }

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
                            if (!step.requireSpecificOrder || step.requiredPreviousStepIds.Count == 0)
                                return true;

                            for (int i = 0; i < step.requiredPreviousStepIds.Count; i++)
                            {
                                string requiredStepId = step.requiredPreviousStepIds[i];
                                if (!completedStepIds.Contains(requiredStepId))
                                {
                                    // 해당 인덱스에 패널티가 설정되어 있으면 적용
                                    if (i < step.previousStepPenalties.Length && step.previousStepPenalties[i] != null)
                                    {
                                        ApplyPenalty(step.previousStepPenalties[i]);
                                    }
                                    else if (step.previousStepPenalties[0] != null)
                                    {
                                        // 기본 패널티 적용
                                        ApplyPenalty(step.previousStepPenalties[0]);
                                    }
                                    return false;
                                }
                            }

                            return true;
                        }
                    }

                    // 이 스텝이 완료되었고 다음 스텝을 제한하는 경우
                    if (completedStepIds.Contains(step.id) && step.restrictNextSteps)
                    {
                       

                        // 모든 스텝을 확인하여 시도하려는 스텝이 허용되는지 확인
                        foreach (var nextStep in currentProcedure.steps)
                        {
                            if (nextStep.stepType == ProcedureStepType.ItemClick &&
                                nextStep.settings.isItemClick &&
                                nextStep.settings.itemId == itemId &&
                                !completedStepIds.Contains(nextStep.id))
                            {
                                // 이 스텝이 시도하려는 스텝인 경우
                                if (!step.allowedNextStepIds.Contains(nextStep.id))
                                {
                                    // 허용되지 않은 다음 스텝을 시도하는 경우
                                    if (step.invalidNextStepPenalty != null)
                                    {
                                        ApplyPenalty(step.invalidNextStepPenalty);
                                    }
                                    return false; // 스텝 진행 불가
                                }
                            }
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
            // Resources 폴더 내 모든 interactionData 가져오기
            InteractionData[] alldata = Resources.LoadAll<InteractionData>("");

            foreach (InteractionData data in alldata)
            {
                if (data.id == id)
                    return data;
            }

            // 못 찾은 경우 특정 경로 직접 확인
            InteractionData specificInteraction = Resources.Load<InteractionData>("ScriptableObjects" + id);
            if (specificInteraction != null)
                return specificInteraction;

            return null;
        }
        /// <summary>
        /// 플레이어 상호작용을 처리합니다.
        /// </summary>
        public bool HandlePlayerInteraction(string interactionTag)
        {
            if (!procedureInProgress)
                return false;
            Instep = true;
            // ❗❗ 현재 시도하려는 step 먼저 찾기
            ProcedureStep stepCandidate = currentProcedure.steps.Find(s =>
                s.stepType == ProcedureStepType.PlayerInteraction &&
                s.settings != null &&
                s.settings.isPlayerInteraction &&
                s.settings.validInteractionTags != null &&
                s.settings.validInteractionTags.Contains(interactionTag));

            if (stepCandidate == null)
            {
                Debug.Log($"일치하는 플레이어 상호작용 스텝 없음: {interactionTag}");
                return false;
            }

            // ✅ 새로운 검사: 이전 완료된 스텝 기준으로 허용되지 않은 스텝일 경우
            foreach (var completedId in completedStepIds)
            {
                ProcedureStep completedStep = currentProcedure.steps.Find(s => s.id == completedId);
                if (completedStep != null && completedStep.restrictNextSteps)
                {
                    if (!completedStep.allowedNextStepIds.Contains(stepCandidate.id))
                    {
                        if (completedStep.invalidNextStepPenalty != null)
                        {
                            ApplyPenalty(completedStep.invalidNextStepPenalty);
                            Debug.LogWarning($"[제한 위반] 스텝 '{stepCandidate.id}'는 스텝 '{completedStep.id}' 이후 허용되지 않은 스텝입니다.");
                            return false;
                        }
                    }
                }
            }
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
                            if (!step.requireSpecificOrder || step.requiredPreviousStepIds.Count == 0)
                                return true;

                            for (int i = 0; i < step.requiredPreviousStepIds.Count; i++)
                            {
                                string requiredStepId = step.requiredPreviousStepIds[i];
                                if (!completedStepIds.Contains(requiredStepId))
                                {
                                    // 해당 인덱스에 패널티가 설정되어 있으면 적용
                                    if (i < step.previousStepPenalties.Length && step.previousStepPenalties[i] != null)
                                    {
                                        ApplyPenalty(step.previousStepPenalties[i]);
                                    }
                                    else if (step.previousStepPenalties[0] != null)
                                    {
                                        // 기본 패널티 적용
                                        ApplyPenalty(step.previousStepPenalties[0]);
                                    }
                                    return false;
                                }
                            }
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

                if (completedStepIds.Contains(step.id) && step.restrictNextSteps)
                {
                    foreach (var nextStep in currentProcedure.steps)
                    {
                        if (nextStep.stepType == ProcedureStepType.PlayerInteraction &&
                            nextStep.settings.isPlayerInteraction &&
                            nextStep.settings.validInteractionTags.Contains(interactionTag) &&
                            !completedStepIds.Contains(nextStep.id))
                        {
                            if (!step.allowedNextStepIds.Contains(nextStep.id))
                            {
                                if (step.invalidNextStepPenalty != null)
                                {
                                    ApplyPenalty(step.invalidNextStepPenalty);
                                }
                                return false;
                            }
                        }
                    }
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
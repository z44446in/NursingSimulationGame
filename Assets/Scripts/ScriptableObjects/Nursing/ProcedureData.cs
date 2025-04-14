using System;
using System.Collections.Generic;
using UnityEngine;
using Nursing.Penalty;

namespace Nursing.Procedure
{
    [CreateAssetMenu(fileName = "New Procedure", menuName = "Nursing/Procedure Data", order = 2)]
    public class ProcedureData : ScriptableObject
    {
        [Header("기본 정보")]
        public string id;
        public string displayName;
        [TextArea(3, 5)] public string description;
        
        [Header("프로시저 스텝")]
        public List<ProcedureStep> steps = new List<ProcedureStep>();
        
      
    }

    [Serializable]
    public class ProcedureStep
    {
        [Header("스텝 정보")]
        public string id;
        public string name;
        [TextArea(2, 3)] public string guideMessage;
        
        [Header("스텝 타입")]
        public ProcedureStepType stepType;

        [Header("인터랙션 설정")]
        public ProcedureStepSettings settings;

        [Header("순서 요구사항")]
        public bool requireSpecificOrder;
        public List<string> requiredPreviousStepIds;
        public PenaltyData[] previousStepPenalties; // 새로 추가: 이전 스텝별 패널티

        [Header("다음 스텝 제한")]
        public bool restrictNextSteps; // 새로 추가: 다음 스텝 제한 여부
        public List<string> allowedNextStepIds; // 새로 추가: 허용된 다음 스텝 ID 목록
        public PenaltyData invalidNextStepPenalty; // 새로 추가: 허용되지 않은 다음 스텝 시도 시 패널티

        [Header("생략 설정")]
        public bool canBeSkipped; // 새로 추가: 이 스텝이 생략 가능한지 여부

        [Header("패널티 설정")]
        
        public PenaltyData incorrectActionPenalty;
        // 생성자 추가
        public ProcedureStep()
        {
            id = System.Guid.NewGuid().ToString().Substring(0, 8);
            name = "New Step";
            requiredPreviousStepIds = new List<string>();
            previousStepPenalties = new PenaltyData[0];
            restrictNextSteps = false;
            allowedNextStepIds = new List<string>();
            canBeSkipped = false;
        }

    }

    [Serializable]
    public class ProcedureStepSettings
    {
        [Header("아이템 클릭")]
        public bool isItemClick;
        public string itemId;
        public string interactionDataId; // Link to InteractionData
        
        [Header("액션 버튼 클릭")]
        public bool isActionButtonClick;
        public List<string> correctButtonIds;
        public bool requireAllButtons;
        
        [Header("플레이어 상호작용")]
        public bool isPlayerInteraction;
        public List<string> validInteractionTags;
    }

    public enum ProcedureStepType
    {
        [Tooltip("아이템 클릭 상호작용")]
        ItemClick,
        [Tooltip("액션 버튼 클릭 상호작용")]
        ActionButtonClick,
        [Tooltip("플레이어 직접 상호작용")]
        PlayerInteraction
    }
}
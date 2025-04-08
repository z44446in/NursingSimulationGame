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
        
        [Header("패널티 설정")]
        public PenaltyData incorrectOrderPenalty;
        public PenaltyData incorrectActionPenalty;
        
       
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
using System;
using System.Collections.Generic;
using UnityEngine;
using Nursing.Penalty;

namespace Nursing.Procedure
{
    // UnnecessaryItem 클래스 추가
[Serializable]
public class UnnecessaryItem
{
    public Item item;
    [TextArea(2, 4)] public string unnecessaryReason; // 이 아이템이 불필요한 이유
}

    
    
    [System.Serializable]
    public class AreaExcludedItems
    {
        public PreparationAreaType area;
        public List<Item> excludedItems = new List<Item>();
    }
    [CreateAssetMenu(fileName = "New Procedure", menuName = "Nursing/Procedure Data", order = 2)]
    public class ProcedureData : ScriptableObject
    {
        [Header("기본 정보")]
        public string id;
        public string displayName;
        [TextArea(3, 5)] public string description;
        
        [Header("프로시저 스텝")]
        public List<ProcedureStep> steps = new List<ProcedureStep>();
        
       // [Header("준비실 필수 아이템")]
        public List<RequiredItem> requiredItems = new List<RequiredItem>();
        
       // [Header("준비실 불필요 아이템")]
public List<UnnecessaryItem> unnecessaryItems = new List<UnnecessaryItem>();

       // [Header("중간 단계 필수 아이템")]
        public List<RequiredItem> intermediateRequiredItems = new List<RequiredItem>();
        
        [Header("중간화면 미표시 아이템")]
public List<Item> hiddenInIntermediateItems = new List<Item>();
      //  [Header("준비실 제외 아이템")]
        public List<AreaExcludedItems> excludedAreaItems = new List<AreaExcludedItems>();
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

        [Header("반복 설정")]
public bool isRepeatable; // 이 스텝이 반복 가능한지 여부

        [Header("자동 다음 스텝 설정")]                // 신규
 public bool isAutoNext;                      // 자동으로 다음 스텝으로 넘어갈지
public string autoNextStepId;                // 넘어갈 스텝의 id


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

        [Header("Interaction 바로 실행 ")]
        public bool isInteractionOnly;
        public string  OnlyinteractionDataId;
    }

    public enum ProcedureStepType
    {
        [Tooltip("아이템 클릭 상호작용")]
        ItemClick,
        [Tooltip("액션 버튼 클릭 상호작용")]
        ActionButtonClick,
        [Tooltip("플레이어 직접 상호작용")]
        PlayerInteraction,
        [Tooltip("Interaction 바로 실행")]
        InteractionOnly,
                
    }

    
}
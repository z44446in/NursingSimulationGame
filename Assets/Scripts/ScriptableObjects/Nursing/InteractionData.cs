using System;
using System.Collections.Generic;
using UnityEngine;
using Nursing.Penalty;

namespace Nursing.Interaction
{
    [CreateAssetMenu(fileName = "New Interaction", menuName = "Nursing/Interaction Data", order = 1)]
    public class InteractionData : ScriptableObject
    {
        [Header("기본 정보")]
        public string id;
        public string displayName;
        [TextArea(3, 5)] public string description;

        [Header("인터랙션 스테이지")]
        public List<InteractionStage> stages = new List<InteractionStage>();

        [Header("가이드 메시지")]
        public string guideMessage;
    }

    [Serializable]
    public class InteractionStage
    {
        [Header("스테이지 정보")]
        public string id;
        public string name;
        [TextArea(2, 3)] public string guideMessage;
        
        [Header("인터랙션 타입")]
        public InteractionType interactionType;
        
        [Header("순서 요구사항")]
        public bool requireSpecificOrder;
        public List<string> requiredPreviousStageIds;
        
        [Header("패널티 설정")]
        public PenaltyData incorrectOrderPenalty;
        public PenaltyData incorrectInteractionPenalty;
        
        [Header("인터랙션 설정")]
        public InteractionSettings settings;
    }

    [Serializable]
    public class InteractionSettings
    {
        [Header("드래그 설정")]
        public bool isDragInteraction;
        public bool showDirectionArrows;
        public Vector2 arrowStartPosition;
        public Vector2 arrowDirection;
        public bool requireTwoFingerDrag;
        public bool requiredDragDirection;
        public string targetObjectTag;
        public bool followDragMovement;
        public float dragDistanceLimit;
        public string boundaryObjectTag;
        public string collisionZoneTag;
        
        [Header("오브젝트 생성")]
        public bool createObject;
        public string objectToCreateTag;
        
        [Header("조건부 클릭")]
        public bool isConditionalClick;
        public List<string> validClickTags;
        public List<string> invalidClickTags;
        public List<PenaltyData> conditionalClickPenalties;
        
        [Header("지속 클릭")]
        public bool isSustainedClick;
        public float sustainedClickDuration;
        public PenaltyData earlyReleasePenalty;
        public string sustainedClickTargetTag;
        
        [Header("오브젝트 삭제")]
        public bool deleteObject;
        public string objectToDeleteTag;
        
        [Header("오브젝트 이동")]
        public bool moveObject;
        public string objectToMoveTag;
        public Vector2 moveDirection;
        public float moveSpeed;
        public Vector2[] movePath;
        
        [Header("퀴즈 팝업")]
        public bool showQuizPopup;
        public string questionText;
        public List<string> quizOptions;
        public int correctAnswerIndex;
        public Sprite[] optionImages;
        public float timeLimit;
        
        [Header("미니게임")]
        public bool startMiniGame;
        public GameObject miniGamePrefab;
    }
}
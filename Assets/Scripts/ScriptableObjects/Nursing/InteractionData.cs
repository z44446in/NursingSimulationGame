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

       
    }

    [Serializable]
    public class InteractionStage
    {
        [Header("스테이지 정보")]
        public string id;
        public string name;
        public int StageNum;
        [TextArea(2, 3)] public string guideMessage;
        
        
        [Header("인터랙션 타입")]
        public InteractionType interactionType;

        [Header("인터랙션 설정")]
        public InteractionSettings settings;

        [Header("순서 요구사항")]
        public bool requireSpecificOrder;
        public List<string> requiredPreviousStageIds;
        
        [Header("패널티 설정")]
        public PenaltyData incorrectOrderPenalty;
        public PenaltyData incorrectInteractionPenalty;
        
        
    }

    [Serializable]
  
    public class InteractionSettings
    {
        // SingleDragInteraction 설정
        [Header("드래그 설정")]
        public bool isDragInteraction;
        public bool showDirectionArrows;
        public bool haveDirection;
        public Vector2 arrowStartPosition;
        public Vector2 arrowDirection;
        public bool requireTwoFingerDrag; // 이 필드는 유지하되 SingleDragInteraction에서만 사용
        public bool requiredDragDirection;
        [Range(0, 90)] public float dragDirectionTolerance = 45f;
        public string targetObjectTag;
        public bool deactivateObjectAfterDrag = false;
        public bool followDragMovement;
        public float dragDistanceLimit;
        public string boundaryObjectTag;
        public string collisionZoneTag;
        public PenaltyData OverDrag;
        public PenaltyData CollideDrag;

        // MultiDragInteraction 설정
        [Header("다중 드래그 설정")]
        public List<FingerDragSettings> fingerSettings = new List<FingerDragSettings>();

        // 추가: 다중 손가락 드래그를 위한 내부 클래스
        [Serializable]
        public class FingerDragSettings
        {
            public string name = "손가락";
            public bool showDirectionArrows;
            public bool haveDirection;
            public Vector2 arrowStartPosition;
            public Vector2 arrowDirection;
            public bool requiredDragDirection;
            [Range(0, 90)] public float dragDirectionTolerance = 45f;
            public string targetObjectTag;
            public bool deactivateObjectAfterDrag = false;
            public bool followDragMovement;
            public float dragDistanceLimit;
            public string boundaryObjectTag;
            public string collisionZoneTag;
            public PenaltyData OverDrag;
        }


        [Header("멀티 드래그 조건")]
        [Tooltip("두 손가락이 얼마나 동시에 눌러야 인정되는지 (초 단위)")]
        [Range(0f, 1f)] public float multiDragSyncThreshold = 0.2f;

        [Header("오브젝트 생성")]
        public bool createObject;
        public GameObject[] objectToCreate;
        

        [Header("조건부 클릭")]
        public bool isConditionalClick;
        public List<string> validClickTags;
        public List<string> invalidClickTags;
        public List<PenaltyData> conditionalClickPenalties;
        
        [Header("지속 클릭")]
        public bool isSustainedClick;
        public float sustainedClickDuration;
        public PenaltyData earlyReleasePenalty;
        public PenaltyData lateReleasePenalty;//특정단계 이상으로 오래 지속할 때 발생
        public string sustainedClickTargetTag;
        
        [Header("오브젝트 삭제")]
        public bool deleteObject;
        public string[] objectToDeleteTag;
        
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
        public PenaltyData WrongAnswer;//틀린 답을 골랐을 때 발생 

        [Header("미니게임")]
        public bool startMiniGame;
        public GameObject miniGamePrefab;
    }


}
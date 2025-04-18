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

        // InteractionSettings 클래스 내에 추가
        [Header("드래그 목표 영역")]
        public string targetZoneTag; // 드래그 목표 영역 태그
        public bool requireReachTargetZone = false; // 목표 영역 도달 필요 여부
        public float minOverlapPercentage = 0.5f; // 목표 영역과 겹쳐야 하는 최소 비율 (0-1)

        [Header("터치 충돌 설정")]
        public bool detectTouchCollision = false; // 터치 충돌 감지 활성화
        public string noTouchZoneTag; // 터치 불가 영역 태그
        public PenaltyData touchCollisionPenalty; // 터치 충돌 시 패널티


        [Header("멀티 드래그 조건")]
        [Tooltip("두 손가락이 얼마나 동시에 눌러야 인정되는지 (초 단위)")]
        [Range(0f, 1f)] public float multiDragSyncThreshold = 0.2f;

        [Header("오브젝트 생성")]
        public bool createObject;
        public GameObject[] objectToCreate;
        public bool    randomizeSpawnPosition = false;  // 랜덤 스폰 활성화 여부
        public string  spawnAreaTag;                  // 태그로 영역 오브젝트 지정
        

        [Header("조건부 클릭")]
        public bool isConditionalClick;
        public List<string> validClickTags;
        public List<string> invalidClickTags;
        public List<PenaltyData> conditionalClickPenalties;
        public bool destroyValidClickedObject = false;


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

        // 새로운 텍스트 퀴즈 설정 추가
        [Header("텍스트 퀴즈 팝업")]
        public bool showTextQuizPopup;
        [TextArea(3, 5)]
        public string textQuizQuestionText;
        public List<string> textQuizOptions;
        public int textQuizCorrectAnswerIndex;
        public float textQuizTimeLimit;
        public PenaltyData textQuizWrongAnswer;

        // 새로운 이미지 퀴즈 설정 추가
        [Header("이미지 퀴즈 팝업")]
        public bool showImageQuizPopup;
        [TextArea(3, 5)]
        public string imageQuizQuestionText;
        public Sprite[] imageQuizOptions;
        public int imageQuizCorrectAnswerIndex;
        public float imageQuizTimeLimit;
        public PenaltyData imageQuizWrongAnswer;



        [Header("미니게임")]
        public bool startMiniGame;
        public GameObject miniGamePrefab;

        [Header("다양한 선택")]
        public bool isVariousChoice;
        public string choiceQuestionText;
        public Sprite choicePopupImage; // 추가: 팝업에 표시될 이미지
        public InteractionData alternativeInteraction; // 'Yes' 버튼 클릭 시 실행할 대체 인터랙션
        public bool treatNoAsFailure ; // 새로 추가: '아니오' 응답을 실패로 처리할지 여부
    }


}
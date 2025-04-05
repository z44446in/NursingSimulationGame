using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 상호작용 데이터 - 아이템 클릭 시 발생하는 상호작용 단계들을 관리
/// </summary>
[CreateAssetMenu(fileName = "New Interaction", menuName = "Nursing/Interaction Data")]
public class InteractionData : ScriptableObject
{
    [Header("기본 정보")]
    public string id; // 고유 ID
    public string displayName; // 화면에 표시될 이름
    [TextArea(3, 5)] public string description; // 설명
    
    [Header("가이드 설정")]
    public string guideMessage; // 가이드 패널에 표시될 메시지

    [Header("상호작용 단계")]
    public List<InteractionStage> stages = new List<InteractionStage>(); // 상호작용 단계들

    [Header("피드백 정보")]
    public AudioClip successSound; // 성공 효과음
    public AudioClip errorSound; // 오류 효과음
    public Sprite successFeedbackSprite; // 성공 피드백 이미지
    public Sprite errorFeedbackSprite; // 오류 피드백 이미지
}

/// <summary>
/// 상호작용 단계 - 각 상호작용 단계의 세부 정보
/// </summary>
[Serializable]
public class InteractionStage
{
    [Header("기본 정보")]
    public string id; // 고유 ID
    public string stageName; // 단계 이름
    [TextArea(2, 4)] public string guideMessage; // 가이드 메시지 (필수)
    public InteractionType interactionType; // 상호작용 유형
    
    [Header("단계 순서")]
    public bool isRequired = true; // 필수 단계 여부
    public bool isOrderImportant = true; // 순서가 중요한지 여부

    [Header("페널티 설정")]
    public PenaltyData penaltyData = new PenaltyData(); // 페널티 정보
    
    #region 드래그 관련 설정
    [Header("드래그 설정")]
    public bool useDragInteraction = false; // 드래그 상호작용 사용 여부
    [Range(0, 360)]
    public float requiredDragAngle = 0; // 필요한 드래그 각도
    [Range(5, 60)]
    public float dragAngleTolerance = 30f; // 드래그 각도 허용 오차
    public Sprite dragArrowSprite; // 드래그 화살표 스프라이트
    public Vector2 dragArrowPosition; // 드래그 화살표 위치
    public float dragArrowRotation; // 드래그 화살표 회전
    
    [Header("다중 손가락 드래그")]
    public bool useTwoFingerDrag = false; // 두 손가락 드래그 사용 여부
    public bool requireSameDirection = true; // 같은 방향 드래그 필요 여부
    public TwoFingerDragSetting twoFingerDragSetting = new TwoFingerDragSetting(); // 두 손가락 드래그 설정
    
    [Header("드래그 대상 오브젝트")]
    public string[] dragTargetTags; // 드래그 인식 대상 태그들
    
    [Header("드래그 이동 설정")]
    public DragMoveType dragMoveType = DragMoveType.Fixed; // 드래그 이동 유형
    public Vector3 fixedMovementDirection; // 고정 이동 방향
    public float fixedMovementAmount = 100f; // 고정 이동량
    public string boundaryTag; // 경계 객체 태그
    public string collisionTag; // 충돌 감지 태그
    #endregion
    
    #region 오브젝트 생성 설정
    [Header("오브젝트 생성 설정")]
    public bool createObjects = false; // 오브젝트 생성 여부
    public List<ObjectCreationSetting> objectsToCreate = new List<ObjectCreationSetting>(); // 생성할 오브젝트 설정
    #endregion
    
    #region 조건부 클릭 설정
    [Header("조건부 클릭 설정")]
    public bool useConditionalClick = false; // 조건부 클릭 사용 여부
    public List<ConditionalClickSetting> conditionalClickSettings = new List<ConditionalClickSetting>(); // 조건부 클릭 설정
    #endregion
    
    #region 지속 클릭 설정
    [Header("지속 클릭 설정")]
    public bool useSustainedClick = false; // 지속 클릭 사용 여부
    public float requiredPressDuration = 3.0f; // 필요한 누르는 시간(초)
    public string sustainedClickTargetTag; // 지속 클릭 대상 태그
    public PenaltyData earlyReleaseData = new PenaltyData(); // 조기 해제 페널티
    #endregion
    
    #region 오브젝트 삭제/이동 설정
    [Header("오브젝트 삭제 설정")]
    public bool deleteObjects = false; // 오브젝트 삭제 여부
    public string[] tagsToDelete; // 삭제할 오브젝트 태그
    
    [Header("오브젝트 이동 설정")]
    public bool moveObjects = false; // 오브젝트 이동 여부
    public List<ObjectMovementSetting> objectMovements = new List<ObjectMovementSetting>(); // 이동 설정
    #endregion
    
    #region 퀴즈 설정
    [Header("퀴즈 설정")]
    public bool useQuizPopup = false; // 퀴즈 팝업 사용 여부
    public GameObject quizPrefab; // 퀴즈 프리팹
    public string quizQuestion; // 질문
    public Sprite[] optionImages; // 옵션 이미지
    public string[] optionTexts; // 옵션 텍스트
    public int correctOptionIndex; // 정답 옵션 인덱스
    public float quizTimeLimit = 30f; // 시간 제한(초)
    #endregion
    
    #region 미니게임 설정
    [Header("미니게임 설정")]
    public bool useMiniGame = false; // 미니게임 사용 여부
    public GameObject miniGamePrefab; // 미니게임 프리팹
    #endregion
}

/// <summary>
/// 두 손가락 드래그 설정
/// </summary>
[Serializable]
public class TwoFingerDragSetting
{
    [Range(0, 360)]
    public float firstFingerDragAngle = 0; // 첫 번째 손가락 드래그 각도
    [Range(5, 60)]
    public float firstFingerAngleTolerance = 30f; // 첫 번째 손가락 허용 오차
    
    [Range(0, 360)]
    public float secondFingerDragAngle = 0; // 두 번째 손가락 드래그 각도
    [Range(5, 60)]
    public float secondFingerAngleTolerance = 30f; // 두 번째 손가락 허용 오차
    
    public float minDragDistance = 50f; // 최소 드래그 거리
}

/// <summary>
/// 드래그 이동 유형
/// </summary>
public enum DragMoveType
{
    Fixed,       // 고정 방향 및 거리로 이동
    FollowDrag   // 드래그를 따라 직접 이동
}

/// <summary>
/// 오브젝트 생성 설정
/// </summary>
[Serializable]
public class ObjectCreationSetting
{
    public string tag; // 오브젝트 태그
    public string objectName; // 오브젝트 이름
    public GameObject prefab; // 생성할 프리팹
    public Vector3 position; // 생성 위치
    public Vector3 rotation; // 생성 회전
    public Vector3 scale = Vector3.one; // 생성 크기
    public bool setNativeSize = true; // 이미지의 경우 네이티브 사이즈 설정
}

/// <summary>
/// 조건부 클릭 설정
/// </summary>
[Serializable]
public class ConditionalClickSetting
{
    public string targetTag; // 대상 태그
    public bool isCorrectOption; // 올바른 옵션인지 여부
    public PenaltyData incorrectChoicePenalty = new PenaltyData(); // 잘못된 선택 시 페널티
    public string successMessage; // 성공 메시지
}

/// <summary>
/// 오브젝트 이동 설정
/// </summary>
[Serializable]
public class ObjectMovementSetting
{
    public string targetTag; // 대상 태그
    public Vector3 targetPosition; // 목표 위치
    public Vector3 targetRotation; // 목표 회전
    public float movementDuration = 1.0f; // 이동 지속 시간
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 이동 곡선
}
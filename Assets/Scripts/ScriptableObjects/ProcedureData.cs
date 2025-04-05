using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 간호 시술 전체 데이터 - 시술 과정, 단계, 요구 조건 등을 정의
/// </summary>
[CreateAssetMenu(fileName = "New Procedure", menuName = "Nursing/Procedure Data")]
public class ProcedureData : ScriptableObject
{
    [Header("기본 정보")]
    public string id; // 고유 ID
    public string displayName; // 화면에 표시될 이름
    [TextArea(3, 5)] public string description; // 설명
    
    [Header("시술 정보")]
    public NursingProcedureType procedureType; // 시술 유형
    public bool isGuidelineVersion = false; // 가이드라인 버전 여부 (false: 임상 버전)
    
    [Header("시술 단계")]
    public List<ProcedureStep> steps = new List<ProcedureStep>(); // 시술 단계들
    
    [Header("평가 설정")]
    public int maxScore = 100; // 최대 점수
    public float timeLimit = 300f; // 제한 시간(초)
    
    [Header("UI 설정")]
    public Sprite backgroundImage; // 배경 이미지
    public Color titleColor = Color.white; // 제목 색상
    
    [Header("음향 설정")]
    public AudioClip backgroundMusic; // 배경 음악
    public AudioClip completionSound; // 완료 효과음
}

/// <summary>
/// 시술 단계 - 각 시술 단계의 세부 정보
/// </summary>
[Serializable]
public class ProcedureStep
{
    [Header("기본 정보")]
    public string id; // 고유 ID
    public string stepName; // 단계 이름
    [TextArea(2, 4)] public string description; // 설명
    public string guideMessage; // 가이드 메시지
    
    [Header("단계 유형")]
    public ProcedureStepType stepType; // 단계 유형
    
    [Header("단계 순서")]
    public bool isRequired = true; // 필수 단계 여부
    public bool isOrderImportant = true; // 순서가 중요한지 여부
    
    [Header("페널티 설정")]
    public PenaltyData orderViolationPenalty = new PenaltyData(); // 순서 위반 시 페널티
    public PenaltyData skipPenalty = new PenaltyData(); // 건너뛰기 시 페널티
    
    #region 아이템 클릭 설정
    [Header("아이템 클릭 설정")]
    public bool useItemInteraction = false; // 아이템 상호작용 사용 여부
    public string itemInteractionId; // 관련 InteractionData ID
    #endregion
    
    #region 액션 버튼 설정
    [Header("액션 버튼 설정")]
    public bool useActionButton = false; // 액션 버튼 사용 여부
    public List<ActionButtonSetting> actionButtons = new List<ActionButtonSetting>(); // 액션 버튼 설정
    #endregion
}

/// <summary>
/// 시술 단계 유형
/// </summary>
public enum ProcedureStepType
{
    ItemInteraction,    // 아이템 상호작용
    ActionButton,       // 액션 버튼 클릭
    Dialogue,           // 대화
    Observation         // 관찰
}

/// <summary>
/// 액션 버튼 설정
/// </summary>
[Serializable]
public class ActionButtonSetting
{
    public string buttonId; // 버튼 ID
    public string buttonText; // 버튼 텍스트
    public Sprite buttonIcon; // 버튼 아이콘
    
    public bool isCorrectOption; // 올바른 옵션인지 여부
    public PenaltyData incorrectChoicePenalty = new PenaltyData(); // 잘못된 선택 시 페널티
    
    public bool requiresMultipleButtons = false; // 여러 버튼이 필요한지 여부
    public string secondRequiredButtonId; // 두 번째로 필요한 버튼 ID
}

/// <summary>
/// 간호 시술 유형
/// </summary>
public enum NursingProcedureType
{
    UrinaryCatheterization,  // 도뇨관 삽입
    TracheostomyCare,        // 기관지절개 관리
    OxygenTherapy,           // 산소 요법
    Medication,              // 투약
    WoundCare,               // 상처 관리
    VitalSigns,              // 활력징후 측정
    BloodTransfusion,        // 수혈
    IVInjection,             // 정맥주사
    PainManagement,          // 통증 관리
    FallPrevention,          // 낙상 예방
    InfectionControl,        // 감염 관리
    PatientEducation,        // 환자 교육
    Other                    // 기타
}
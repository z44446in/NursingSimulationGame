using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 아이템 상호작용 데이터 - 특정 아이템과의 상호작용을 정의
/// </summary>
[CreateAssetMenu(fileName = "New Interaction", menuName = "Nursing/Interaction Data")]
public class InteractionData : BaseData
{
    [Header("상호작용 단계")]
    public List<InteractionStep> steps = new List<InteractionStep>();
    
    [Header("피드백 정보")]
    public AudioClip successSound;
    public AudioClip errorSound;
    public string successMessage;
    public string errorMessage;
    
    [Header("초기 오브젝트 설정")]
    public List<InitialObjectData> initialObjects = new List<InitialObjectData>();
    
    /// <summary>
    /// 이 상호작용을 BaseInteractionSystem에 등록합니다
    /// </summary>
    [ContextMenu("Register To Interaction System")]
    public void RegisterToInteractionSystem()
    {
        var interactionSystem = GameObject.FindObjectOfType<BaseInteractionSystem>();
        if (interactionSystem != null)
        {
            InteractionData data = this;
            
            // InteractionData 형식을 BaseInteractionSystem이 사용하는 형식으로 변환
            List<InteractionStep> convertedSteps = new List<InteractionStep>();
            foreach (var step in steps)
            {
                convertedSteps.Add(step);
            }
            
            // BaseInteractionSystem에 등록
            InteractionData convertedData = ScriptableObject.CreateInstance<InteractionData>();
            convertedData.id = id;
            convertedData.displayName = displayName;
            convertedData.description = description;
            convertedData.steps = convertedSteps;
            convertedData.successSound = successSound;
            convertedData.errorSound = errorSound;
            
            // 등록
            Debug.Log($"{displayName} 상호작용이 등록되었습니다.");
        }
        else
        {
            Debug.LogWarning("Scene에 BaseInteractionSystem이 없습니다.");
        }
    }
}

/// <summary>
/// 상호작용 단계 정의
/// </summary>
[Serializable]
public class InteractionStep
{
    public string stepId;
    public string stepName;
    public InteractionType interactionType;
    public string guideText;
    
    // 단계 순서
    public int order;
    
    // 초기 오브젝트 생성 설정
    public bool createInitialObjects = false;
    public List<InitialObjectData> initialObjects = new List<InitialObjectData>();
    
    // 드래그 관련 설정
    [Header("드래그 설정")]
    public float requiredDragAngle;
    public float dragAngleTolerance = 30f;
    public float dragDistance = 100f;
    
    // 다중 단계 드래그 설정
    public bool useMultiStageDrag = false;
    public List<MultiStageDragSettings> multiStageDragSteps = new List<MultiStageDragSettings>();
    
    // 클릭 관련 설정
    [Header("클릭 설정")]
    public Rect validClickArea = new Rect(0, 0, 100, 100);
    public string[] validTags;
    
    // 퀴즈 관련 설정
    [Header("퀴즈 설정")]
    public string quizQuestion;
    public string[] quizOptions;
    public int correctOptionIndex;
    
    // 시각적 가이드
    [Header("가이드 설정")]
    public Sprite tutorialArrowSprite;
    public Vector2 tutorialArrowPosition;
    public float tutorialArrowRotation;
    
    // 피드백
    [Header("피드백 설정")]
    public string successMessage;
    public string errorMessage;
    public bool showErrorBorderFlash = true;
    
    // 조건부 터치 설정
    [Header("조건부 터치 설정")]
    public bool useConditionalTouch = false;
    public List<ConditionalTouchOption> touchOptions = new List<ConditionalTouchOption>();
    
    [Header("물 효과 설정")]
    public bool createWaterEffect = false;
    public Vector2 waterEffectPosition;
    public bool createWaterImageOnObject = false;
}

/// <summary>
/// 초기 오브젝트 데이터 정의
/// </summary>
[Serializable]
public class InitialObjectData
{
    public string objectId;
    public string objectName;
    public Sprite objectSprite;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
    public string tag = "Untagged";
    public bool useCustomPrefab = false;
    public GameObject customPrefab;
}

/// <summary>
/// 다중 단계 드래그 설정
/// </summary>
[Serializable]
public class MultiStageDragSettings
{
    public string stepId;
    public Vector2 arrowPosition;
    public float arrowRotation;
    public float requiredDragAngle;
    public float dragAngleTolerance = 30f;
    public bool requireStartOnObject = true;
    public string requiredStartTag;
    public Vector3 targetPositionOffset;
    public Vector3 targetRotationOffset;
}

/// <summary>
/// 조건부 터치 옵션
/// </summary>
[Serializable]
public class ConditionalTouchOption
{
    public string optionId;
    public string targetTag;
    public string successMessage;
    public string errorMessage;
    public bool isCorrectOption = false;
    public bool showErrorBorderFlash = false;
    public float disableTouchDuration = 0f;
    public string errorEntryText = "";
    public Vector2 waterEffectPosition;
    public bool createWaterImageOnObject = false;
}
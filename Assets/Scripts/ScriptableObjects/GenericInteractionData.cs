using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 범용 상호작용 데이터를 정의하는 ScriptableObject
/// 모든 종류의 간호 절차와 아이템에 대한 상호작용을 정의할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "InteractionData", menuName = "Nursing/Generic/Interaction Data")]
public class GenericInteractionData : ScriptableObject
{
    [Header("기본 정보")]
    public string interactionId;
    public string interactionName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    
    [Header("상호작용 단계")]
    public List<GenericInteractionStep> steps = new List<GenericInteractionStep>();
    
    [Header("시각 및 음향 효과")]
    public AudioClip successSound;
    public AudioClip errorSound;
    public Sprite successFeedbackSprite;
    public Sprite errorFeedbackSprite;
    
    [Header("설정")]
    public bool isOrderImportant = true;
    public bool allowSkipSteps = false;
    public float timeLimit = 0f; // 0이면 시간 제한 없음
    
    /// <summary>
    /// 인터랙션 매니저에 데이터 등록
    /// </summary>
    [ContextMenu("Register To Interaction System")]
    public void RegisterToInteractionSystem()
    {
        var interactionSystem = FindObjectOfType<BaseInteractionSystem>();
        if (interactionSystem != null)
        {
            // InteractionData로 변환
            InteractionData data = new InteractionData
            {
                id = interactionId,
                name = interactionName,
                description = description,
                steps = new List<InteractionStep>()
            };
            
            // 단계 변환
            foreach (var step in steps)
            {
                InteractionStep newStep = new InteractionStep
                {
                    interactionType = step.interactionType,
                    guideText = step.guideText,
                    requiredDragAngle = step.requiredDragAngle,
                    dragAngleTolerance = step.dragAngleTolerance,
                    validClickArea = step.validClickArea,
                    quizQuestion = step.quizQuestion,
                    quizOptions = step.quizOptions,
                    correctOptionIndex = step.correctOptionIndex,
                    tutorialArrowSprite = step.tutorialArrowSprite,
                    tutorialArrowPosition = step.tutorialArrowPosition,
                    tutorialArrowRotation = step.tutorialArrowRotation,
                    successMessage = step.successMessage,
                    errorMessage = step.errorMessage
                };
                
                data.steps.Add(newStep);
            }
            
            // 등록
            interactionSystem.RegisterInteraction(interactionId, data);
            Debug.Log($"{name} 상호작용이 등록되었습니다.");
        }
        else
        {
            Debug.LogWarning("Scene에 BaseInteractionSystem이 없습니다.");
        }
    }
}

/// <summary>
/// 범용 상호작용 단계 정의
/// </summary>
[System.Serializable]
public class GenericInteractionStep
{
    [Header("기본 설정")]
    public string stepId;
    public string stepName;
    public InteractionType interactionType;
    [TextArea(2, 4)]
    public string guideText;
    
    [Header("드래그 설정")]
    [Range(0, 360)]
    public float requiredDragAngle;
    [Range(5, 60)]
    public float dragAngleTolerance = 30f;
    public float dragDistance = 100f;
    
    [Header("클릭 설정")]
    public Rect validClickArea = new Rect(0, 0, 100, 100);
    public string[] validTags;
    public int requiredClicks = 1;
    
    [Header("퀴즈 설정")]
    [TextArea(2, 3)]
    public string quizQuestion;
    public string[] quizOptions;
    public int correctOptionIndex;
    
    [Header("시각적 가이드")]
    public Sprite tutorialArrowSprite;
    public Vector2 tutorialArrowPosition;
    public float tutorialArrowRotation;
    public Sprite highlightSprite;
    
    [Header("피드백")]
    public string successMessage;
    public string errorMessage;
    
    [Header("고급 설정")]
    public float stepTimeLimit;
    public string[] requiredCompletedStepIds;
    public bool isOptional;
    public System.Action<GenericInteractionStep> customAction;
}
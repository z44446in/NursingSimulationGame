using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개별 간호 행동에 대한 데이터를 정의하는 ScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "NursingActionData", menuName = "Nursing/Nursing Action Data")]
public class NursingActionData : ScriptableObject
{
    [Header("Action Info")]
    public string actionId;
    public string actionName;
    [TextArea(3, 5)]
    public string description;
    public bool isRequired = true;
    public int scoreWeight = 1;

    [Header("Required Items")]
    public List<Item> requiredItems = new List<Item>();

    [Header("Guide Info")]
    [TextArea(2, 4)]
    public string hintText;
    [TextArea(2, 4)]
    public string feedbackMessage;

    [Header("Interaction Settings")]
    public InteractionType interactionType = InteractionType.SingleClick;
    [TextArea(2, 4)]
    public string guideText;
    public GameObject tutorialPrefab;
    
    [Header("Drag Interaction Settings")]
    public bool useDragInteraction = false;
    [Range(0, 360)]
    public float requiredDragAngle = 0;
    [Range(5, 60)]
    public float dragAngleTolerance = 30f;
    public Sprite dragArrowSprite;
    public Vector2 dragArrowPosition;
    public float dragArrowRotation;
    public int dragStepsRequired = 1;
    
    [Header("Click Interaction Settings")]
    public bool useClickInteraction = false;
    public Rect validClickArea = new Rect(0, 0, 100, 100);
    public string[] validClickTargetTags;
    public Sprite clickHighlightSprite;
    
    [Header("Quiz Interaction Settings")]
    public bool useQuizInteraction = false;
    public string quizQuestion;
    public string[] quizOptions;
    public int correctOptionIndex;
    
    [Header("Error Handling")]
    public string errorMessage;
    public PenaltyType penaltyType = PenaltyType.Minor;
    
    [Header("Visual Feedback")]
    public Sprite successFeedbackSprite;
    public AudioClip successSound;
    public Sprite errorFeedbackSprite;
    public AudioClip errorSound;
    
    [Header("Next Step Conditions")]
    public bool waitForUserInput = false;
    public float autoAdvanceDelay = 0f;
    
    // 에디터 확장을 위한 도우미 속성
    #if UNITY_EDITOR
    public bool showDragSettings = false;
    public bool showClickSettings = false;
    public bool showQuizSettings = false;
    #endif
}
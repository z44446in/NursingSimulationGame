using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // LINQ 메서드(Any, Where 등)를 사용하기 위해 필요
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
                    errorMessage = step.errorMessage,
                    
                    // 확장된 기능 추가
                    createInitialObjects = step.createInitialObjects,
                    useMultiStageDrag = step.useMultiStageDrag,
                    totalDragStages = step.multiStageDragSteps != null ? step.multiStageDragSteps.Count : 0,
                    useConditionalTouch = step.useConditionalTouch,
                    
                    // 시각 효과 설정
                    showErrorBorderFlash = step.showErrorBorderFlash,
                    disableTouchDuration = step.disableTouchDuration,
                    errorEntryText = step.errorEntryText,
                    createWaterEffect = step.touchOptions != null && step.touchOptions.Any(o => o.isCorrectOption),
                    createWaterImageOnObject = step.touchOptions != null && step.touchOptions.Any(o => o.isCorrectOption && o.createWaterImageOnObject)
                };
                
                // 올바른 터치 태그 설정
                if (step.useConditionalTouch && step.touchOptions != null && step.touchOptions.Count > 0)
                {
                    // 모든 태그 수집
                    List<string> allTags = new List<string>();
                    List<string> correctTags = new List<string>();
                    
                    foreach (var option in step.touchOptions)
                    {
                        if (!string.IsNullOrEmpty(option.targetTag))
                        {
                            allTags.Add(option.targetTag);
                            
                            if (option.isCorrectOption)
                            {
                                correctTags.Add(option.targetTag);
                                
                                // 물 효과 위치 설정
                                if (option.createWaterImageOnObject)
                                {
                                    newStep.waterEffectPosition = option.waterEffectPosition;
                                }
                            }
                        }
                    }
                    
                    newStep.validTouchTags = allTags.ToArray();
                    newStep.correctTouchTags = correctTags.ToArray();
                }
                
                data.steps.Add(newStep);
            }
            
            // 등록
            interactionSystem.RegisterInteraction(interactionId, data);
            
            // 따로 초기 오브젝트 처리를 위해 초기 오브젝트 데이터도 전달
            if (steps.Any(s => s.createInitialObjects && s.initialObjects.Count > 0))
            {
                // 초기 오브젝트 생성 등록
                foreach (var step in steps.Where(s => s.createInitialObjects && s.initialObjects.Count > 0))
                {
                    Debug.Log($"단계 '{step.stepId}'에서 {step.initialObjects.Count}개의 초기 오브젝트 등록");
                    
                    // 각 단계별로 초기 오브젝트 데이터 처리
                    foreach (var objData in step.initialObjects)
                    {
                        // 여기서는 초기 오브젝트 데이터만 로그로 출력합니다.
                        // 실제 오브젝트 생성은 BaseInteractionSystem.CreateInitialObjects에서 수행됩니다.
                        Debug.Log($"   - 오브젝트: {objData.objectName}, 태그: {objData.tag}");
                    }
                }
                
                Debug.Log($"{name} 상호작용이 초기 오브젝트와 함께 등록되었습니다.");
            }
            else
            {
                Debug.Log($"{name} 상호작용이 등록되었습니다.");
            }
            
            // InteractionDataRegistrar가 있으면 여기에도 등록
            var registrar = FindObjectOfType<InteractionDataRegistrar>();
            if (registrar != null)
            {
                // 리플렉션을 사용하여 addGenericInteraction 메서드 호출
                var method = registrar.GetType().GetMethod("AddGenericInteraction");
                if (method != null)
                {
                    method.Invoke(registrar, new object[] { this });
                    Debug.Log($"{name} 상호작용이 InteractionDataRegistrar에 등록되었습니다.");
                }
            }
        }
        else
        {
            Debug.LogWarning("Scene에 BaseInteractionSystem이 없습니다.");
        }
    }
    
    #if UNITY_EDITOR
    /// <summary>
    /// 이 상호작용 데이터를 사용하는 모든 아이템을 찾아 자동으로 연결합니다.
    /// </summary>
    [ContextMenu("Auto Link To Items")]
    public void AutoLinkToItems()
    {
        if (string.IsNullOrEmpty(interactionId))
        {
            Debug.LogError("interactionId가 비어 있습니다. 먼저 ID를 설정하세요.");
            return;
        }

        // 프로젝트에서 모든 Item 에셋 찾기
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Item");
        int linkedCount = 0;
        
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Item item = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(assetPath);
            
            if (item != null)
            {
                // 아이템의 interactionDataId가 현재 상호작용 ID와 일치하는지 또는 비어있는지 확인
                if (item.interactionDataId == interactionId)
                {
                    Debug.Log($"아이템 '{item.itemName}'은(는) 이미 '{interactionId}'와 연결되어 있습니다.");
                    linkedCount++;
                }
            }
        }
        
        if (linkedCount > 0)
        {
            Debug.Log($"총 {linkedCount}개의 아이템이 '{interactionId}' 상호작용과 연결되어 있습니다.");
        }
        else
        {
            Debug.Log($"'{interactionId}' 상호작용과 연결된 아이템이 없습니다.");
        }
    }
    
    /// <summary>
    /// Resources 폴더에 이 상호작용 데이터 복사본 저장
    /// </summary>
    [ContextMenu("Save To Resources")]
    public void SaveToResources()
    {
        if (string.IsNullOrEmpty(interactionId))
        {
            Debug.LogError("interactionId가 비어 있습니다. 먼저 ID를 설정하세요.");
            return;
        }
        
        // Resources 폴더 경로 생성
        string resourcesPath = "Assets/Resources";
        string interactionsPath = resourcesPath + "/Interactions";
        
        // Resources 폴더가 없으면 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(resourcesPath))
        {
            UnityEditor.AssetDatabase.CreateFolder("Assets", "Resources");
            Debug.Log("Resources 폴더를 생성했습니다.");
        }
        
        // Interactions 폴더가 없으면 생성
        if (!UnityEditor.AssetDatabase.IsValidFolder(interactionsPath))
        {
            UnityEditor.AssetDatabase.CreateFolder(resourcesPath, "Interactions");
            Debug.Log("Interactions 폴더를 생성했습니다.");
        }
        
        // 현재 에셋 경로 가져오기
        string currentPath = UnityEditor.AssetDatabase.GetAssetPath(this);
        
        // 목적지 경로 생성
        string destinationPath = $"{interactionsPath}/{interactionId}.asset";
        
        // 이미 존재하는지 확인
        if (UnityEditor.AssetDatabase.LoadAssetAtPath<GenericInteractionData>(destinationPath) != null)
        {
            // 이미 존재하면 업데이트
            Debug.Log($"Resources/Interactions/{interactionId}.asset 파일이 이미 존재하여 업데이트합니다.");
            UnityEditor.AssetDatabase.DeleteAsset(destinationPath);
        }
        
        // 복사
        bool success = UnityEditor.AssetDatabase.CopyAsset(currentPath, destinationPath);
        
        if (success)
        {
            Debug.Log($"상호작용 데이터가 Resources/Interactions/{interactionId}.asset에 성공적으로 저장되었습니다.");
            UnityEditor.AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError($"Resources 폴더에 저장하는 데 실패했습니다. 경로: {destinationPath}");
        }
    }
    
    /// <summary>
    /// 한 번에 모든 설정 처리
    /// </summary>
    [ContextMenu("Setup Everything")]
    public void SetupEverything()
    {
        if (string.IsNullOrEmpty(interactionId))
        {
            Debug.LogError("interactionId가 비어 있습니다. 먼저 ID를 설정하세요.");
            return;
        }
        
        // 1. Resources 폴더에 저장
        SaveToResources();
        
        // 2. 인터랙션 매니저에 등록
        RegisterToInteractionSystem();
        
        // 3. 아이템과 연결
        AutoLinkToItems();
        
        Debug.Log($"'{interactionId}' 상호작용 데이터 설정이 완료되었습니다!");
    }
    #endif
}

/// <summary>
/// 초기 오브젝트 데이터 정의
/// </summary>
[System.Serializable]
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
[System.Serializable]
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
[System.Serializable]
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
    
    [Header("초기 오브젝트 생성 설정")]
    public bool createInitialObjects = false;
    public List<InitialObjectData> initialObjects = new List<InitialObjectData>();
    
    [Header("다중 단계 드래그 설정")]
    public bool useMultiStageDrag = false;
    public List<MultiStageDragSettings> multiStageDragSteps = new List<MultiStageDragSettings>();
    
    [Header("조건부 터치 설정")]
    public bool useConditionalTouch = false;
    public List<ConditionalTouchOption> touchOptions = new List<ConditionalTouchOption>();
    
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
    [System.NonSerialized]
    public System.Action<GenericInteractionStep> customAction;

    [Header("Error Handling")]
    public bool showErrorBorderFlash = true;
    public float disableTouchDuration = 1.0f;
    [TextArea(2, 3)]
    public string errorEntryText;
    
    [Header("Water Effect")]
    public bool createWaterEffect = false;
    public Vector2 waterEffectPosition;
    public bool createWaterImageOnObject = false;

}
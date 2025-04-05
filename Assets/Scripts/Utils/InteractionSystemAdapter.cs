using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 기존 상호작용 시스템과 새로운 데이터 구조 간의 호환성을 제공하는 어댑터 클래스
/// 이 클래스는 새 시스템에서 생성된 InteractionData와 ProcedureData가 
/// 기존의 InteractionManager 및 ProcedureManager와 호환되도록 합니다.
/// </summary>
public class InteractionSystemAdapter : MonoBehaviour
{
    private static InteractionSystemAdapter instance;
    public static InteractionSystemAdapter Instance => instance;

    [Header("References")]
    [SerializeField] private InteractionDataRegistrar interactionDataRegistrar;
    
    private Dictionary<string, RuntimeInteractionData> convertedInteractions = new Dictionary<string, RuntimeInteractionData>();
    private Dictionary<string, ProcedureData> procedureDataCache = new Dictionary<string, ProcedureData>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // InteractionDataRegistrar 참조 확인
        if (interactionDataRegistrar == null)
        {
            interactionDataRegistrar = FindObjectOfType<InteractionDataRegistrar>();
        }
    }

    private void Start()
    {
        // 등록된 모든 인터랙션 데이터를 캐싱
        if (interactionDataRegistrar != null)
        {
            CacheInteractionData();
        }
    }

    /// <summary>
    /// 인터랙션 데이터를 캐싱하여 변환 시간을 줄입니다.
    /// </summary>
    private void CacheInteractionData()
    {
        if (interactionDataRegistrar == null)
            return;
            
        var interactions = interactionDataRegistrar.GetAllInteractionData();
        foreach (var interaction in interactions)
        {
            // 새 상호작용 데이터를 이전 시스템에서 사용할 수 있는 형식으로 변환
            ConvertAndRegisterInteractionData(interaction);
        }
    }

    /// <summary>
    /// 새로운 InteractionData를 RuntimeInteractionData로 변환하고 등록합니다.
    /// </summary>
    public RuntimeInteractionData ConvertAndRegisterInteractionData(InteractionData interactionData)
    {
        if (interactionData == null)
            return null;
            
        string id = interactionData.id;
        
        // 이미 변환된 데이터가 있는지 확인
        if (convertedInteractions.ContainsKey(id))
        {
            return convertedInteractions[id];
        }
        
        // 새 RuntimeInteractionData 생성
        RuntimeInteractionData runtimeData = new RuntimeInteractionData
        {
            id = id,
            name = interactionData.displayName,
            description = interactionData.description
        };
        
        // 단계 변환
        List<InteractionStep> steps = new List<InteractionStep>();
        
        foreach (var stage in interactionData.stages)
        {
            foreach (var action in stage.actions)
            {
                InteractionStep step = new InteractionStep
                {
                    actionId = action.id,
                    guideText = action.guideMessage,
                    interactionType = ConvertInteractionType(action.interactionType),
                    isOrderImportant = true, // 기본값
                    
                    // 위치와 방향 속성 설정 (있는 경우)
                    targetPosition = action.targetPosition,
                    targetRotation = action.targetRotation,
                    
                    // 필요한 아이템 (있는 경우)
                    requiredItems = new List<RequiredItemInfo>()
                };
                
                // 필요한 아이템 정보 추가
                if (action.requiredItems != null && action.requiredItems.Count > 0)
                {
                    foreach (var requiredItem in action.requiredItems)
                    {
                        step.requiredItems.Add(new RequiredItemInfo
                        {
                            itemId = requiredItem.item != null ? requiredItem.item.itemId : "",
                            itemName = requiredItem.item != null ? requiredItem.item.displayName : "",
                            isOptional = requiredItem.isOptional
                        });
                    }
                }
                
                steps.Add(step);
            }
        }
        
        runtimeData.steps = steps;
        
        // 캐시에 저장
        convertedInteractions[id] = runtimeData;
        
        // InteractionManager의 BaseInteractionSystem에 등록
        if (InteractionManager.Instance != null && 
            InteractionManager.Instance.GetComponent<BaseInteractionSystem>() != null)
        {
            var baseInteractionSystem = InteractionManager.Instance.GetComponent<BaseInteractionSystem>();
            baseInteractionSystem.RegisterInteraction(id, runtimeData);
        }
        
        return runtimeData;
    }

    /// <summary>
    /// ProcedureType을 기반으로 적절한 ProcedureData를 가져와 등록합니다.
    /// </summary>
    public ProcedureData RegisterProcedureTypeWithManager(ProcedureType procedureType, bool useGuideline = true)
    {
        if (procedureType == null)
            return null;
            
        string cacheKey = procedureType.id + (useGuideline ? "_guideline" : "_clinical");
        
        // 캐시 확인
        if (procedureDataCache.ContainsKey(cacheKey))
        {
            return procedureDataCache[cacheKey];
        }
        
        // 적절한 ProcedureData 가져오기
        ProcedureData procedureData = useGuideline ? 
            procedureType.guidelineVersion : 
            procedureType.clinicalVersion;
            
        if (procedureData == null)
        {
            Debug.LogError($"ProcedureType {procedureType.displayName}에 {(useGuideline ? "가이드라인" : "임상")} 버전이 없습니다.");
            return null;
        }
        
        // 캐시에 저장
        procedureDataCache[cacheKey] = procedureData;
        
        // ProcedureManager에 등록 
        // (현재는 참조만 가지므로 실제 등록은 이벤트 발생 시나 직접 선택 시 필요)
        
        return procedureData;
    }

    /// <summary>
    /// 새 InteractionType을 기존 시스템의 InteractionType으로 변환합니다.
    /// </summary>
    private InteractionType ConvertInteractionType(Interaction.InteractionType newType)
    {
        switch (newType)
        {
            case Interaction.InteractionType.SingleClick:
                return InteractionType.SingleClick;
            case Interaction.InteractionType.DoubleClick:
                return InteractionType.DoubleClick;
            case Interaction.InteractionType.LongPress:
                return InteractionType.LongPress;
            case Interaction.InteractionType.Drag:
                return InteractionType.Drag;
            case Interaction.InteractionType.TwoFingerDrag:
                return InteractionType.TwoFingerDrag;
            case Interaction.InteractionType.ClickAndDrag:
                return InteractionType.ClickAndDrag;
            case Interaction.InteractionType.Quiz:
                return InteractionType.Quiz;
            case Interaction.InteractionType.MiniGame:
                return InteractionType.MiniGame;
            default:
                return InteractionType.SingleClick; // 기본값
        }
    }

    /// <summary>
    /// 기존 InteractionType을 새 시스템의 InteractionType으로 변환합니다.
    /// </summary>
    public Interaction.InteractionType ConvertToNewInteractionType(InteractionType oldType)
    {
        switch (oldType)
        {
            case InteractionType.SingleClick:
                return Interaction.InteractionType.SingleClick;
            case InteractionType.DoubleClick:
                return Interaction.InteractionType.DoubleClick;
            case InteractionType.LongPress:
                return Interaction.InteractionType.LongPress;
            case InteractionType.Drag:
                return Interaction.InteractionType.Drag;
            case InteractionType.TwoFingerDrag:
                return Interaction.InteractionType.TwoFingerDrag;
            case InteractionType.ClickAndDrag:
                return Interaction.InteractionType.ClickAndDrag;
            case InteractionType.Quiz:
                return Interaction.InteractionType.Quiz;
            case InteractionType.MiniGame:
                return Interaction.InteractionType.MiniGame;
            default:
                return Interaction.InteractionType.SingleClick; // 기본값
        }
    }

    /// <summary>
    /// ID로 RuntimeInteractionData를 가져옵니다.
    /// </summary>
    public RuntimeInteractionData GetRuntimeInteractionData(string id)
    {
        if (convertedInteractions.ContainsKey(id))
        {
            return convertedInteractions[id];
        }
        
        // ID로 InteractionData를 찾아 변환 시도
        if (interactionDataRegistrar != null)
        {
            var interactionData = interactionDataRegistrar.GetInteractionData(id);
            if (interactionData != null)
            {
                return ConvertAndRegisterInteractionData(interactionData);
            }
        }
        
        return null;
    }

    /// <summary>
    /// 모든 변환된 RuntimeInteractionData를 반환합니다.
    /// </summary>
    public List<RuntimeInteractionData> GetAllRuntimeInteractionData()
    {
        return convertedInteractions.Values.ToList();
    }
}

/// <summary>
/// 런타임에 사용되는 상호작용 데이터 구조
/// </summary>
[System.Serializable]
public class RuntimeInteractionData
{
    public string id;
    public string name;
    public string description;
    public List<InteractionStep> steps = new List<InteractionStep>();
}

/// <summary>
/// 상호작용 단계 데이터 구조
/// </summary>
[System.Serializable]
public class InteractionStep
{
    public string actionId;
    public string guideText;
    public InteractionType interactionType;
    public bool isOrderImportant;
    
    // 위치와 방향 (드래그 등에 사용)
    public Vector3 targetPosition;
    public Vector3 targetRotation;
    
    // 필요한 아이템
    public List<RequiredItemInfo> requiredItems = new List<RequiredItemInfo>();
}

/// <summary>
/// 필요한 아이템 정보
/// </summary>
[System.Serializable]
public class RequiredItemInfo
{
    public string itemId;
    public string itemName;
    public bool isOptional;
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Nursing.Interaction;

/// <summary>
/// ScriptableObject로 만든 상호작용 데이터를 런타임에 InteractionManager에 등록하는 유틸리티 클래스
/// </summary>
public class InteractionDataRegistrar : MonoBehaviour
{
    // 싱글톤 인스턴스
    private static InteractionDataRegistrar instance;
    public static InteractionDataRegistrar Instance => instance;
    
    [Header("Interaction Data")]
    [SerializeField] private List<Nursing.Interaction.InteractionData> interactionAssets = new List<Nursing.Interaction.InteractionData>();
    
    // 등록된 모든 상호작용 데이터 캐시
    private Dictionary<string, Nursing.Interaction.InteractionData> cachedInteractions = new Dictionary<string, Nursing.Interaction.InteractionData>();
    
    private void Awake()
    {
        // 싱글톤 설정
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // 상호작용 데이터 로드 및 등록
        LoadInteractionsFromResources();
        RegisterActions();
    }
    
    /// <summary>
    /// Resources 폴더에서 상호작용 데이터를 로드합니다.
    /// </summary>
    private void LoadInteractionsFromResources()
    {
        try
        {
            // 디렉토리에서 모든 상호작용 데이터 로드
            Nursing.Interaction.InteractionData[] interactions = Resources.LoadAll<Nursing.Interaction.InteractionData>("Interactions");
            
            if (interactions != null && interactions.Length > 0)
            {
                foreach (var data in interactions)
                {
                    if (data != null)
                    {
                        if (!string.IsNullOrEmpty(data.id))
                        {
                            // 캐시에 저장
                            cachedInteractions[data.id] = data;
                            
                            // 인스펙터에도 추가 (에디터에서 볼 수 있도록)
                            if (!interactionAssets.Contains(data))
                            {
                                interactionAssets.Add(data);
                            }
                            
                            Debug.Log($"Loaded interaction data from Resources: {data.displayName} (ID: {data.id})");
                        }
                        else
                        {
                            Debug.LogWarning($"Skipped interaction data with empty ID: {data.name}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading interactions from Resources: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 모든 액션과 상호작용을 매니저에 등록합니다.
    /// </summary>
    private void RegisterActions()
    {
        if (Nursing.Managers.InteractionManager.Instance == null)
        {
            Debug.LogError("InteractionManager.Instance is null. Make sure it's initialized before this component.");
            return;
        }
        
        // 모든 상호작용 데이터 등록
        foreach (var interaction in interactionAssets)
        {
            if (interaction == null) continue;
            
            // 상호작용 등록
            Nursing.Managers.InteractionManager.Instance.StartInteraction(interaction);
            
            // 캐시에 저장
            cachedInteractions[interaction.id] = interaction;
            
            Debug.Log($"Registered interaction: {interaction.displayName} (ID: {interaction.id})");
        }
    }
    
    /// <summary>
    /// ID로 상호작용 데이터를 가져옵니다.
    /// </summary>
    public Nursing.Interaction.InteractionData GetInteractionData(string interactionId)
    {
        if (string.IsNullOrEmpty(interactionId))
            return null;
            
        // 캐시에서 찾기
        if (cachedInteractions.TryGetValue(interactionId, out Nursing.Interaction.InteractionData data))
            return data;
            
        // 캐시에 없으면 Resources에서 다시 로드 시도
        try
        {
            data = Resources.Load<Nursing.Interaction.InteractionData>($"Interactions/{interactionId}");
            if (data != null)
            {
                cachedInteractions[interactionId] = data;
                return data;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading interaction data for ID '{interactionId}': {ex.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// 런타임에 새 상호작용 데이터를 추가합니다.
    /// </summary>
    public void AddInteractionData(Nursing.Interaction.InteractionData data)
    {
        if (data == null || string.IsNullOrEmpty(data.id))
            return;
            
        // 캐시와 리스트에 추가
        cachedInteractions[data.id] = data;
        
        // 콜렉션에 없으면 추가
        if (!interactionAssets.Contains(data))
        {
            interactionAssets.Add(data);
        }
        
        // InteractionManager에 등록
        if (Nursing.Managers.InteractionManager.Instance != null)
        {
            Nursing.Managers.InteractionManager.Instance.StartInteraction(data);
            Debug.Log($"Runtime added interaction data: {data.displayName} (ID: {data.id})");
        }
    }
    
    /// <summary>
    /// 모든 상호작용 데이터를 가져옵니다.
    /// </summary>
    public List<Nursing.Interaction.InteractionData> GetAllInteractionData()
    {
        return interactionAssets;
    }
    
    /// <summary>
    /// 에디터에서 버튼으로 등록 테스트를 위한 메소드
    /// </summary>
    [ContextMenu("Test Register Actions")]
    private void TestRegisterActions()
    {
        LoadInteractionsFromResources();
        RegisterActions();
    }
}
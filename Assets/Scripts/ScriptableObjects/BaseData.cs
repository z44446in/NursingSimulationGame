using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 모든 ScriptableObject 데이터의 기본 클래스
/// </summary>
public abstract class BaseData : ScriptableObject
{
    [Header("기본 정보")]
    public string id;
    public string displayName;
    [TextArea(3, 5)] public string description;
}

/// <summary>
/// 아이템 요구사항 인터페이스
/// </summary>
public interface IRequiresItems
{
    List<Item> GetRequiredItems(bool includeOptional = false);
}

/// <summary>
/// 아이템 요구사항 정의
/// </summary>
[Serializable]
public class ItemRequirement
{
    public Item item;
    public bool isOptional = false;
    public string usageDescription;
    
    // 추가 속성 - 아이템 이름 참조 (호환성 유지)
    public string itemName => item != null ? item.itemName : "Unknown Item";
}

/// <summary>
/// 대화 항목 정의
/// </summary>
[Serializable]
public class DialogueEntry
{
    public string speakerName;
    public Sprite speakerImage;
    public string dialogueText;
    public List<string> responseOptions = new List<string>();
    public int correctResponseIndex = -1; // -1은 정답이 없음을 의미
    public bool requiresResponse = false;
}

/// <summary>
/// 단계 유형 정의
/// </summary>
public enum StepType
{
    Dialogue,
    Interaction,
    Preparation,
    Assessment,
    Documentation
}
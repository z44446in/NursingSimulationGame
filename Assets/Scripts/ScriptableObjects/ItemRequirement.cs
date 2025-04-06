using UnityEngine;
using System;

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
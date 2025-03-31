using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Nursing/Item")]
public class Item : ScriptableObject
{
    // 기존 필드들
    public string itemId;
    public string itemName;
    public Sprite itemSprite;
    public string description;

    
}
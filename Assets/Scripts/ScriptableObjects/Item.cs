using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Nursing/Item")]
public class Item : ScriptableObject
{
    // ���� �ʵ��
    public string itemId;
    public string itemName;
    public Sprite itemSprite;
    public string description;

    
}
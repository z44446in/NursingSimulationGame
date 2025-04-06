using UnityEngine;
using Nursing.Interaction; // 또는 InteractionType이 정의된 네임스페이스]

[CreateAssetMenu(fileName = "New Item", menuName = "Nursing/Item")]
public class Item : ScriptableObject
{
  
    public string itemId;
    public string itemName;
    public Sprite itemSprite;
    public string description;
    public Sprite[] handSprite; // 손 스프라이트 (아이템을 들고 있을 때)
    

    
}
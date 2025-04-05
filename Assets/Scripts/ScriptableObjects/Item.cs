using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Nursing/Item")]
public class Item : ScriptableObject
{
    // ���� �ʵ��
    public string itemId;
    public string itemName;
    public Sprite itemSprite;
    public string description;


    // Item.cs 에 추가할 필드

[Header("Interaction Settings")]
public InteractionType interactionType = InteractionType.None;
public string interactionDataId; // 범용 상호작용 데이터 ID
public string guideText; // 가이드 텍스트
public Sprite guideImage; // 가이드 이미지
public Sprite handSprite; // 손 스프라이트 (아이템을 들고 있을 때)
public string interactionDataId; // 연결할 GenericInteractionData의 ID (예: "dilutedWaterInteraction")

[Header("MiniGame Settings")]
public GameObject miniGamePrefab; // 미니게임 프리팹
public bool requiresTwoFingers; // 두 손가락이 필요한지
public float timeLimit = 10f; // 제한 시간
public float successThreshold = 0.7f; // 성공 기준값

[Header("Tutorial Settings")]
public AnimationClip tutorialAnimation; // 튜토리얼 애니메이션
public float tutorialDuration = 5f; // 튜토리얼 지속 시간

    
}
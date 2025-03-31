using UnityEngine;
using UnityEngine.UI;

public class UrinaryCatheterManager : MonoBehaviour
{
    private static UrinaryCatheterManager instance;
    public static UrinaryCatheterManager Instance => instance;

    [Header("MiniGame Prefabs")]
    [SerializeField] private GameObject tarpMiniGamePrefab;
    [SerializeField] private GuidePanel guidePanel;

    [Header("Hand Sprites")]
    [SerializeField] private Image handImage;
    [SerializeField] private Sprite defaultHandSprite;
    [SerializeField] private Sprite tarpHandSprite;
    [SerializeField] private Sprite foleySetHandSprite;
    [SerializeField] private Sprite urineBagHandSprite;
    [SerializeField] private Sprite foleyCatheterHandSprite;
    [SerializeField] private Sprite multieFixerHandSprite;
    [SerializeField] private Sprite tapeHandSprite;

    [Header("Items")]
    [SerializeField] private Item tarpItem;
    [SerializeField] private Item foleySetItem;
    [SerializeField] private Item urineBagItem;
    [SerializeField] private Item foleyCatheterItem;
    [SerializeField] private Item multieFixerItem;
    [SerializeField] private Item tapeItem;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void HandleItemInteraction(Item item)
    {
        if (item == null) return;

        if (item == tarpItem)
        {
            HandleTarpPickup();
        }
        else if (item == foleySetItem)
        {
            HandleFoleySetPickup();
        }
        else if (item == urineBagItem)
        {
            HandleUrineBagPickup();
        }
        else if (item == foleyCatheterItem)
        {
            HandleFoleyCatheterPickup();
        }
        else if (item == multieFixerItem)
        {
            HandleMultieFixerPickup();
        }
        else if (item == tapeItem)
        {
            HandleTapePickup();
        }
        else
        {
            Debug.LogWarning($"No handler for item: {item.itemName}");
        }
    }

    private void UpdateHandSprite(string handType)
    {
        if (handImage == null) return;

        switch (handType)
        {
            case "tarp":
                handImage.sprite = tarpHandSprite;
                break;
            case "foleySet":
                handImage.sprite = foleySetHandSprite;
                break;
            case "urineBag":
                handImage.sprite = urineBagHandSprite;
                break;
            case "foleyCatheter":
                handImage.sprite = foleyCatheterHandSprite;
                break;
            case "multieFixer":
                handImage.sprite = multieFixerHandSprite;
                break;
            case "tape":
                handImage.sprite = tapeHandSprite;
                break;
            default:
                handImage.sprite = defaultHandSprite;
                break;
        }
    }

    private void HandleTarpPickup()
    {
        UpdateHandSprite("tarp");
        GuidePanel.Instance.ShowGuide("방수포를 둘 곳을 터치하세요.");
        EnableTarpPlacementAreas(true);
    }

    private void EnableTarpPlacementAreas(bool enable)
    {
        // 방수포를 둘 수 있는 영역의 Collider나 Button 활성화/비활성화
        TarpPlacementArea[] areas = FindObjectsOfType<TarpPlacementArea>();
        foreach (var area in areas)
        {
            area.gameObject.SetActive(enable);
        }
    }

    // 나머지 아이템들의 핸들러 메서드들 (나중에 구현)
    private void HandleFoleySetPickup()
    {
        UpdateHandSprite("foleySet");
        // TODO: Implement
    }

    private void HandleUrineBagPickup()
    {
        UpdateHandSprite("urineBag");
        // TODO: Implement
    }

    private void HandleFoleyCatheterPickup()
    {
        UpdateHandSprite("foleyCatheter");
        // TODO: Implement
    }

    private void HandleMultieFixerPickup()
    {
        UpdateHandSprite("multieFixer");
        // TODO: Implement
    }

    private void HandleTapePickup()
    {
        UpdateHandSprite("tape");
        // TODO: Implement
    }
}
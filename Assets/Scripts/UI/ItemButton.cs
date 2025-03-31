using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemImage;
    private Item currentItem;
    private System.Action<Item> onItemClicked;

    public void SetItem(Item item)
    {
        currentItem = item;
        if (itemImage != null && item.itemSprite != null)
        {
            itemImage.sprite = item.itemSprite;
            itemImage.preserveAspect = true;
        }
    }

    public void Initialize(Item item, System.Action<Item> onClickCallback)
    {
        SetItem(item);
        onItemClicked = onClickCallback;
    }

    // 이미지 클릭 이벤트 처리
    public void OnPointerClick(PointerEventData eventData)
    {
        
        onItemClicked?.Invoke(currentItem);
    }

    // ItemButton.cs에 다음 메서드 추가
    public Item GetCurrentItem()
    {
        return currentItem;
    }
}
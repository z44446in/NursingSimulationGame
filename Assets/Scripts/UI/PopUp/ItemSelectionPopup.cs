using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;

public class ItemSelectionPopup : BasePopup
{
    [SerializeField] private GameObject itemButtonPrefab;
   [SerializeField] private Transform itemGrid; 
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GridLayoutGroup gridLayout; // 그리드 레이아웃 추가

    private Action<Item> onItemSelected;

   public void Initialize(PreparationAreaType areaType, List<Item> items, Action<Item> onItemSelected)
{
    this.onItemSelected = onItemSelected;
    titleText.text = $"{GetAreaTitle(areaType)}에서 선택할 물품";
    
    // 기존 아이템 버튼들 제거
    foreach (Transform child in itemGrid)
    {
        Destroy(child.gameObject);
    }

    // 새 아이템 버튼들 생성
    if (items != null && items.Count > 0)
    {
       
        foreach (var item in items)
        {
            if (item != null)
            {
                CreateItemButton(item);
               
            }
        }
    }

    closeButton.onClick.AddListener(() => Destroy(gameObject));
}

private void CreateItemButton(Item item)
{
    GameObject buttonObj = Instantiate(itemButtonPrefab, itemGrid);
    ItemButton itemButton = buttonObj.GetComponent<ItemButton>();
    
    if (itemButton != null)
    {
        itemButton.Initialize(item, onItemSelected);
    }
}

    private string GetAreaTitle(PreparationAreaType areaType)
    {
        return areaType switch
        {
            PreparationAreaType.MedicationTable => "투약대",
            PreparationAreaType.UpperCabinet => "투약대 위 수납장",
            PreparationAreaType.LeftCabinet => "왼쪽 수납장",
            PreparationAreaType.RightCabinet => "오른쪽 수납장",
            _ => string.Empty
        };
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmationPopup : BasePopup
{
    [Header("UI References")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI confirmButtonText;
    [SerializeField] private TextMeshProUGUI cancelButtonText;
    [SerializeField] private TextMeshProUGUI itemNameText; // 아이템 이름을 표시할 텍스트 추가

    private Action onConfirmAction;
    private Action onCancelAction;

    public void Initialize(Item item, Action onConfirm, Action onCancel)
    {
        onConfirmAction = onConfirm;
        onCancelAction = onCancel;

        // 아이템 이미지 설정
        if (itemImage != null && item.itemSprite != null)
        {
            itemImage.sprite = item.itemSprite;
            itemImage.preserveAspect = true; // 이미지 비율 유지
            itemImage.gameObject.SetActive(true); // 이미지가 있을 때만 표시
        }
        else
        {
            // 이미지가 없으면 이미지 오브젝트를 숨김
            if (itemImage != null)
                itemImage.gameObject.SetActive(false);
        }

        // 아이템 이름 설정
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
        }

        // 기본 메시지 설정
        if (messageText != null)
        {
            messageText.text = $"{item.itemName}을(를) 카트에 넣겠습니까?";
        }

        // 버튼 텍스트 설정
        if (confirmButtonText != null) confirmButtonText.text = "네";
        if (cancelButtonText != null) cancelButtonText.text = "아니오";

        // 버튼 이벤트 설정
        confirmButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();

        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void SetCustomMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    // 이미지 크기를 동적으로 조절하고 싶은 경우 사용할 수 있는 메서드
    public void SetImageSize(Vector2 size)
    {
        if (itemImage != null)
        {
            RectTransform rectTransform = itemImage.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = size;
            }
        }
    }

    private void OnConfirmClicked()
    {
        onConfirmAction?.Invoke();
        ClosePopup();
    }

    private void OnCancelClicked()
    {
        onCancelAction?.Invoke();
        ClosePopup();
    }

    private void OnDestroy()
    {
        if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
        if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// VariousChoice 타입을 위한 커스텀 설정 메서드
    /// </summary>
    /// <summary>
/// VariousChoice 타입을 위한 커스텀 설정 메서드
/// </summary>
public void SetupForVariousChoice(string message, Sprite popupImage, Action onConfirm, Action onCancel)
{
    // 버튼 텍스트 설정
    if (confirmButtonText != null) confirmButtonText.text = "예";
    if (cancelButtonText != null) cancelButtonText.text = "아니오";

    // 메시지 설정
    if (messageText != null)
    {
        messageText.text = message;
    }

    // 이미지 설정 - 이미지가 제공된 경우에만 표시
    if (itemImage != null)
    {
        if (popupImage != null)
        {
            itemImage.sprite = popupImage;
            itemImage.preserveAspect = true;
            itemImage.gameObject.SetActive(true);
        }
        else
        {
            itemImage.gameObject.SetActive(false);
        }
    }

    // 아이템 이름 숨김 (필요 없음)
    if (itemNameText != null)
        itemNameText.gameObject.SetActive(false);

    // 버튼 이벤트 설정
    onConfirmAction = onConfirm;
    onCancelAction = onCancel;

    confirmButton.onClick.RemoveAllListeners();
    cancelButton.onClick.RemoveAllListeners();

    confirmButton.onClick.AddListener(OnConfirmClicked);
    cancelButton.onClick.AddListener(OnCancelClicked);
}
    
}
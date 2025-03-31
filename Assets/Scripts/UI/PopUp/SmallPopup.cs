using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SmallPopup : BasePopup
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button closeButton;
    [SerializeField] private bool autoClose = true;
    [SerializeField] private float autoCloseDelay = 3f;

    private Action onCloseCallback;

    public void Initialize(string message, bool useAutoClose, Action onClose = null)
    {
        if (messageText != null)
            messageText.text = message;

        onCloseCallback = onClose;
        autoClose = useAutoClose;

        if (autoClose)
            Invoke("ClosePopup", autoCloseDelay);
    }

    protected virtual void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
    }

    public override void ClosePopup()
    {
        CancelInvoke("ClosePopup");
        onCloseCallback?.Invoke();
        base.ClosePopup();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();
    }
}
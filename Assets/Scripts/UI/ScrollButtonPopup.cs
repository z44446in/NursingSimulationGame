using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ScrollButtonPopup : BasePopup
{
    [Header("UI References")]
   
    [SerializeField] private Button closeButton;

    protected virtual void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => DisablePopup());
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();
    }
}

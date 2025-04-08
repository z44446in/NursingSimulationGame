using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ActionPopup : BasePopup
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Transform actionButtonsContainer;
    [SerializeField] private Button actionButtonPrefab;
    [SerializeField] private Button closeButton;

    private List<Button> actionButtons = new List<Button>();

    [System.Serializable]
    public class ActionButtonData
    {
        public string text;
        public Action action;
    }

    public void Initialize(string title, List<ActionButtonData> actions)
    {
        if (titleText != null)
            titleText.text = title;

        CreateActionButtons(actions);
    }

    protected virtual void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePopup);
    }

    private void CreateActionButtons(List<ActionButtonData> actions)
    {
        // 기존 버튼 제거
        foreach (var button in actionButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        actionButtons.Clear();

        // 새 버튼 생성
        foreach (var actionData in actions)
        {
            Button actionButton = Instantiate(actionButtonPrefab, actionButtonsContainer);
            TextMeshProUGUI buttonText = actionButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = actionData.text;

            // 액션 바인딩
            Action action = actionData.action;
            actionButton.onClick.AddListener(() => {
                action?.Invoke();
                ClosePopup();
            });
            
            actionButtons.Add(actionButton);
        }
    }

    public override void ClosePopup()
    {
        base.ClosePopup();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveAllListeners();

        foreach (var button in actionButtons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }
}
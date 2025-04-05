using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TalkPopup : ScrollButtonPopup
{
    [Header("Dialogue References")]
    [SerializeField] private Transform buttonContainerbox;  // ��ư���� ������ �θ� Transform
    [SerializeField] private Button dialogueButtonPrefab;  // ��ȭ ��ư ������
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private List<DialogueEntry> dialogueEntries;  // 인스펙터에서 할당

    private int currentStepIndex = 0;

    protected override void Start()
    {
        base.Start(); // �θ� Ŭ������ Start() �޼��� ����
        UpdateDialogueOptions();
    }
    public void SetStep(int stepIndex)
    {
        currentStepIndex = stepIndex;
        UpdateDialogueOptions();
    }

    private void UpdateDialogueOptions()
    {
        // ���� ��ư�� ����
        foreach (Transform child in buttonContainerbox)
        {
            Destroy(child.gameObject);
        }

        // 현재 대화 엔트리 가져오기
        DialogueEntry currentEntry = null;
        if (currentStepIndex < dialogueEntries.Count)
        {
            currentEntry = dialogueEntries[currentStepIndex];
        }
        if (currentEntry == null) return;

        // 각 응답 옵션에 대한 버튼 생성
        foreach (var option in currentEntry.responseOptions)
        {
            Button newButton = Instantiate(dialogueButtonPrefab, buttonContainerbox).GetComponent<Button>();
            newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = option;

           
        }
    }
}
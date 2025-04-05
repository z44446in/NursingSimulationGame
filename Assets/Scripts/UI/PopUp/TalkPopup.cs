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
    [SerializeField] private List<ProcedureDialogueStep> dialogueSteps;  // �ν����Ϳ��� �Ҵ�

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

        // ���� �ܰ��� ��ȭ �ɼ� ã��
        var currentStep = dialogueSteps.Find(step => step.stepIndex == currentStepIndex);
        if (currentStep == null) return;

        // �� ��ư�� ����
        foreach (var option in currentStep.dialogueOptions)
        {
            Button newButton = Instantiate(dialogueButtonPrefab, buttonContainerbox).GetComponent<Button>();
            newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = option.optionText;

           
        }
    }
}
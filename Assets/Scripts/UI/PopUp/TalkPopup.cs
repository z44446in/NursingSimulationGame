using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class TalkPopup : ScrollButtonPopup
{
    [Header("Dialogue References")]
    [SerializeField] private Transform buttonContainerbox;  // 버튼들이 생성될 부모 Transform
    [SerializeField] private Button dialogueButtonPrefab;  // 대화 버튼 프리팹
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private List<ProcedureDialogueStep> dialogueSteps;  // 인스펙터에서 할당

    private int currentStepIndex = 0;

    protected override void Start()
    {
        base.Start(); // 부모 클래스의 Start() 메서드 실행
        UpdateDialogueOptions();
    }
    public void SetStep(int stepIndex)
    {
        currentStepIndex = stepIndex;
        UpdateDialogueOptions();
    }

    private void UpdateDialogueOptions()
    {
        // 기존 버튼들 제거
        foreach (Transform child in buttonContainerbox)
        {
            Destroy(child.gameObject);
        }

        // 현재 단계의 대화 옵션 찾기
        var currentStep = dialogueSteps.Find(step => step.stepIndex == currentStepIndex);
        if (currentStep == null) return;

        // 새 버튼들 생성
        foreach (var option in currentStep.dialogueOptions)
        {
            Button newButton = Instantiate(dialogueButtonPrefab, buttonContainerbox).GetComponent<Button>();
            newButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = option.optionText;

           
        }
    }
}
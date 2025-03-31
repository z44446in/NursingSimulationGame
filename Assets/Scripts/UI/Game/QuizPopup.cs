using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class QuizPopup : BaseGamePopup
{
    [Header("Quiz Components")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform optionButtonsContainer;
    [SerializeField] private Button optionButtonPrefab;

    private List<Button> optionButtons = new List<Button>();
    private int correctAnswerIndex;

    public void SetupQuiz(string question, string[] options, int correctIndex)
    {
        questionText.text = question;
        correctAnswerIndex = correctIndex;

        // 기존 버튼들 제거
        foreach (var button in optionButtons)
        {
            Destroy(button.gameObject);
        }
        optionButtons.Clear();

        // 새로운 옵션 버튼들 생성
        for (int i = 0; i < options.Length; i++)
        {
            int index = i; // 클로저를 위한 복사
            Button newButton = Instantiate(optionButtonPrefab, optionButtonsContainer);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = options[i];
            newButton.onClick.AddListener(() => OnOptionSelected(index));
            optionButtons.Add(newButton);
        }

        StartGame();
    }

    private void OnOptionSelected(int selectedIndex)
    {
        if (selectedIndex == correctAnswerIndex)
        {
            HandleSuccess();
        }
        else
        {
            HandleFailure();
        }
    }
}
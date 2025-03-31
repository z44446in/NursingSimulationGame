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

        // ���� ��ư�� ����
        foreach (var button in optionButtons)
        {
            Destroy(button.gameObject);
        }
        optionButtons.Clear();

        // ���ο� �ɼ� ��ư�� ����
        for (int i = 0; i < options.Length; i++)
        {
            int index = i; // Ŭ������ ���� ����
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
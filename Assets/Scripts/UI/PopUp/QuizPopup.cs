using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class QuizPopup : BasePopup
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Transform optionsContainer;
    [SerializeField] private Button optionButtonPrefab;
    [SerializeField] private Slider timerSlider;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Settings")]
    [SerializeField] private float timeLimit = 10f;
    
    private float currentTime;
    private bool isRunning = false;
    private List<Button> optionButtons = new List<Button>();
    private Action<bool> onQuizCompleted;
    private int correctOptionIndex;

    /// <summary>
    /// 기본 시간 제한을 사용하는 간단한 초기화
    /// </summary>
    public void Initialize(string question, string[] options, int correctIndex, Action<bool> onCompleted)
    {
        Initialize(question, options, correctIndex, this.timeLimit, onCompleted);
    }

    /// <summary>
    /// 전체 매개변수로 초기화
    /// </summary>
    public void Initialize(string question, string[] options, int correctIndex, float time, Action<bool> onCompleted)
    {
        // 퀴즈 데이터 설정
        questionText.text = question;
        timeLimit = time;
        correctOptionIndex = correctIndex;
        onQuizCompleted = onCompleted;

        // 옵션 버튼 생성
        CreateOptionButtons(options);
        
        // 타이머 초기화
        SetupTimer();
        
        // 타이머 시작
        StartCoroutine(RunTimer());
    }

    private void CreateOptionButtons(string[] options)
    {
        // 기존 버튼 제거
        foreach (var button in optionButtons)
        {
            if (button != null)
                Destroy(button.gameObject);
        }
        optionButtons.Clear();

        // 새 버튼 생성
        for (int i = 0; i < options.Length; i++)
        {
            int index = i; // 클로저를 위한 변수 복사
            Button optionButton = Instantiate(optionButtonPrefab, optionsContainer);
            TextMeshProUGUI buttonText = optionButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = options[i];

            optionButton.onClick.AddListener(() => OnOptionSelected(index));
            optionButtons.Add(optionButton);
        }
    }

    private void OnOptionSelected(int index)
    {
        isRunning = false;
        bool isCorrect = index == correctOptionIndex;

        // 정답/오답 표시
        foreach (var button in optionButtons)
        {
            button.interactable = false;
        }

        if (isCorrect)
        {
            // 정답 효과
            optionButtons[index].GetComponent<Image>().color = Color.green;
        }
        else
        {
            // 오답 효과
            optionButtons[index].GetComponent<Image>().color = Color.red;
            optionButtons[correctOptionIndex].GetComponent<Image>().color = Color.green;
        }

        // 결과 콜백 호출 및 팝업 닫기
        StartCoroutine(DelayedClose(isCorrect));
    }

    private IEnumerator DelayedClose(bool isCorrect)
    {
        yield return new WaitForSeconds(1.5f);
        onQuizCompleted?.Invoke(isCorrect);
        ClosePopup();
    }

    private void SetupTimer()
    {
        currentTime = timeLimit;
        if (timerSlider != null)
        {
            timerSlider.maxValue = timeLimit;
            timerSlider.value = timeLimit;
        }
        
        if (timerText != null)
            timerText.text = currentTime.ToString("F1");
    }

    private IEnumerator RunTimer()
    {
        isRunning = true;
        
        while (isRunning && currentTime > 0)
        {
            yield return null;
            currentTime -= Time.deltaTime;
            
            // UI 업데이트
            if (timerSlider != null)
                timerSlider.value = currentTime;
                
            if (timerText != null)
                timerText.text = currentTime.ToString("F1");
            
            // 시간 종료 확인
            if (currentTime <= 0)
            {
                OnTimeUp();
            }
        }
    }

    private void OnTimeUp()
    {
        isRunning = false;
        
        // 모든 버튼 비활성화
        foreach (var button in optionButtons)
        {
            button.interactable = false;
        }
        
        // 정답 표시
        optionButtons[correctOptionIndex].GetComponent<Image>().color = Color.green;
        
        // 실패 콜백 호출 및 팝업 닫기
        StartCoroutine(DelayedClose(false));
    }

    public override void ClosePopup()
    {
        isRunning = false;
        StopAllCoroutines();
        base.ClosePopup();
    }

    private void OnDestroy()
    {
        foreach (var button in optionButtons)
        {
            if (button != null)
                button.onClick.RemoveAllListeners();
        }
    }
}
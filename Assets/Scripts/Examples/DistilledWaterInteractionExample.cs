using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
// We need to access classes from ScoringSystem
using System;

/// <summary>
/// 멸균증류수 인터랙션 시스템 사용 예제 클래스입니다.
/// 이 스크립트를 통해 실제 게임 상황에서 어떻게 구현할지 보여줍니다.
/// </summary>
public class DistilledWaterInteractionExample : MonoBehaviour
{
    [Header("Item References")]
    [SerializeField] private Item distilledWaterItem;
    
    [Header("UI References")]
    [SerializeField] private Button startInteractionButton;
    [SerializeField] private TextMeshProUGUI guideText;
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Header("Interaction Handler")]
    [SerializeField] private ItemInteractionHandler interactionHandler;
    
    // 점수 관리
    private int score = 0;
    private int maxScore = 100;
    
    private void Start()
    {
        if (startInteractionButton != null)
        {
            startInteractionButton.onClick.AddListener(StartDistilledWaterInteraction);
        }
        
        // 인터랙션 핸들러 이벤트 등록
        if (interactionHandler != null)
        {
            interactionHandler.OnStepCompleted += HandleStepCompleted;
            interactionHandler.OnInteractionCompleted += HandleInteractionCompleted;
            interactionHandler.OnInteractionError += HandleInteractionError;
        }
        
        // 초기 UI 설정
        UpdateScoreUI();
        if (guideText != null)
        {
            guideText.text = "멸균증류수 상호작용을 시작하려면 버튼을 누르세요.";
        }
    }
    
    /// <summary>
    /// 멸균증류수 상호작용을 시작합니다.
    /// </summary>
    private void StartDistilledWaterInteraction()
    {
        if (interactionHandler == null)
        {
            Debug.LogError("Interaction handler is not assigned!");
            return;
        }
        
        if (distilledWaterItem == null)
        {
            Debug.LogError("Distilled water item is not assigned!");
            return;
        }
        
        // 상호작용 시작
        interactionHandler.StartItemInteraction(distilledWaterItem);
        
        // 버튼 비활성화
        if (startInteractionButton != null)
        {
            startInteractionButton.interactable = false;
        }
    }
    
    /// <summary>
    /// 단계가 완료되었을 때 처리하는 이벤트 핸들러입니다.
    /// </summary>
    private void HandleStepCompleted(Item item, int stepIndex)
    {
        // 점수 추가
        score += 10;
        UpdateScoreUI();
        
        Debug.Log($"Step {stepIndex} completed for item {item.itemName}!");
    }
    
    /// <summary>
    /// 상호작용이 모두 완료되었을 때 처리하는 이벤트 핸들러입니다.
    /// </summary>
    private void HandleInteractionCompleted(Item item)
    {
        // 완료 보너스 점수
        score += 20;
        UpdateScoreUI();
        
        // UI 업데이트
        if (guideText != null)
        {
            guideText.text = "멸균증류수 준비가 완료되었습니다!";
        }
        
        // 버튼 다시 활성화
        if (startInteractionButton != null)
        {
            startInteractionButton.interactable = true;
        }
        
        Debug.Log($"Interaction completed for item {item.itemName}!");
    }
    
    /// <summary>
    /// 상호작용 오류가 발생했을 때 처리하는 이벤트 핸들러입니다.
    /// </summary>
    private void HandleInteractionError(Item item, string errorMessage, int penaltyPoints)
    {
        // 오류로 인한 감점
        score = Mathf.Max(0, score - penaltyPoints);
        UpdateScoreUI();
        
        // UI 업데이트
        if (guideText != null)
        {
            guideText.text = $"오류: {errorMessage}";
        }
        
        Debug.Log($"Interaction error for item {item.itemName}: {errorMessage}. Penalty: {penaltyPoints} points.");
    }
    
    /// <summary>
    /// 점수 UI를 업데이트합니다.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"점수: {score} / {maxScore}";
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (interactionHandler != null)
        {
            interactionHandler.OnStepCompleted -= HandleStepCompleted;
            interactionHandler.OnInteractionCompleted -= HandleInteractionCompleted;
            interactionHandler.OnInteractionError -= HandleInteractionError;
        }
        
        // 버튼 리스너 해제
        if (startInteractionButton != null)
        {
            startInteractionButton.onClick.RemoveListener(StartDistilledWaterInteraction);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 멸균증류수 상호작용 데이터 예시 (ScriptableObject)
/// 실제 사용 시에는 인스펙터에서 값을 설정하거나 변경할 수 있습니다.
/// </summary>
[CreateAssetMenu(fileName = "DistilledWaterInteractionData", menuName = "Nursing/Interactions/Distilled Water")]
public class DistilledWaterInteractionData : ScriptableObject
{
    public string itemId = "distilledWater";
    public List<InteractionStep> interactionSteps = new List<InteractionStep>();
    
    private void OnValidate()
    {
        // 인스펙터에서 값이 변경될 때 기본 데이터 설정
        if (interactionSteps.Count == 0)
        {
            SetupDefaultInteractionSteps();
        }
    }
    
    /// <summary>
    /// 기본 상호작용 단계를 설정합니다.
    /// </summary>
    private void SetupDefaultInteractionSteps()
    {
        // Step 1: 뚜껑을 드래그로 연다 (첫 번째 드래그)
        InteractionStep step1 = new InteractionStep
        {
            interactionType = InteractionType.Drag,
            guideText = "드래그로 뚜껑을 여세요.",
            requiredDragAngle = 270f, // 위에서 아래로 드래그
            dragAngleTolerance = 30f,
            errorMessage = "뚜껑을 열기 위해 위에서 아래로 드래그하세요.",
            penaltyPoints = 5,
            // 화살표 설정
            tutorialArrowSprite = null, // 실제 에셋은 인스펙터에서 설정
            tutorialArrowPosition = new Vector2(0, 100),
            tutorialArrowRotation = 270f
        };
        
        // Step 2: 뚜껑을 드래그로 연다 (두 번째 드래그)
        InteractionStep step2 = new InteractionStep
        {
            interactionType = InteractionType.Drag,
            guideText = "드래그로 뚜껑을 완전히 제거하세요.",
            requiredDragAngle = 180f, // 오른쪽에서 왼쪽으로 드래그
            dragAngleTolerance = 30f,
            errorMessage = "뚜껑을 제거하기 위해 오른쪽에서 왼쪽으로 드래그하세요.",
            penaltyPoints = 5,
            // 화살표 설정
            tutorialArrowSprite = null, // 실제 에셋은 인스펙터에서 설정
            tutorialArrowPosition = new Vector2(50, 0),
            tutorialArrowRotation = 180f
        };
        
        // Step 3: 퀴즈 (실제 게임에서는 QuizPopup 표시)
        InteractionStep step3 = new InteractionStep
        {
            interactionType = InteractionType.SingleClick,
            guideText = "멸균증류수를 부을 곳을 터치하세요.",
            validClickArea = new Rect(100, 100, 50, 50), // 쓰레기통 영역
            errorMessage = "먼저 의료폐기물통에 소량의 물을 부어야 합니다.",
            penaltyPoints = 10,
            // 화살표 설정은 필요 없음 (클릭 영역에 따라 분기)
        };
        
        // Step 4: 종지에 물을 붓기
        InteractionStep step4 = new InteractionStep
        {
            interactionType = InteractionType.SingleClick,
            guideText = "다음으로 멸균증류수를 부을 곳을 터치하세요.",
            validClickArea = new Rect(200, 200, 50, 50), // 종지 영역
            errorMessage = "종지에 멸균증류수를 부어야 합니다.",
            penaltyPoints = 5,
            // 화살표 설정
            tutorialArrowSprite = null, // 실제 에셋은 인스펙터에서 설정
            tutorialArrowPosition = new Vector2(225, 225),
            tutorialArrowRotation = 0f
        };
        
        // 단계 추가
        interactionSteps.Add(step1);
        interactionSteps.Add(step2);
        interactionSteps.Add(step3);
        interactionSteps.Add(step4);
    }
    
    // 인터랙션 매니저에 데이터 등록
    [ContextMenu("Register To Interaction Manager")]
    public void RegisterToInteractionManager()
    {
        if (InteractionManager.Instance != null)
        {
            InteractionManager.Instance.RegisterItemInteraction(itemId, interactionSteps);
            Debug.Log($"Registered {name} interaction steps to Interaction Manager");
        }
        else
        {
            Debug.LogWarning("Interaction Manager instance not found");
        }
    }
}
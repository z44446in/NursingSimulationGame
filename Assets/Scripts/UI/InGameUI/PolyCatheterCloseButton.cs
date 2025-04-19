using UnityEngine;
using UnityEngine.UI;
using Nursing.Managers;
using Nursing.Penalty;

public class PolyCatheterCloseButton : MonoBehaviour
{
    [SerializeField] private string itemId; // HandleItemClick에 전달할 ID
    [SerializeField] private PenaltyData cartNotEmptyPenalty;

    private Button button;
    private ProcedureManager procedureManager;
    private PenaltyManager penaltyManager;
    private CartUI cartUI;

    private void Awake()
    {
        button = GetComponent<Button>();
        procedureManager = FindObjectOfType<ProcedureManager>();
        penaltyManager = FindObjectOfType<PenaltyManager>();
        cartUI = FindObjectOfType<CartUI>();
        
        if (button == null)
        {
            Debug.LogError("PolyCatheterCloseButton: Button 컴포넌트가 없습니다.");
            return;
        }
        
        if (procedureManager == null || penaltyManager == null || cartUI == null)
        {
            Debug.LogError("필요한 매니저 참조를 찾을 수 없습니다.");
        }
        
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        // 카트에 아이템이 없는지 확인
        bool isCartEmpty = cartUI.GetCartItems().Count == 0;
        
        if (isCartEmpty)
        {
            // 조건 만족: 인터랙션 시작
            procedureManager.HandleItemClick(itemId);
            
            // PrepareFinish 상태 변경
            GameManager.Instance.SetPrepareFinished(true);

                // 버튼 자체를 삭제
        Destroy(gameObject);
        }
        else
        {
            // 조건 불만족: 패널티 적용
            if (penaltyManager != null && cartNotEmptyPenalty != null)
            {
                penaltyManager.ApplyPenalty(cartNotEmptyPenalty);
            }
            
            // 오류 메시지 표시 옵션
            DialogueManager.Instance.ShowSmallDialogue("카트를 비워야 합니다.");
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
    }
}
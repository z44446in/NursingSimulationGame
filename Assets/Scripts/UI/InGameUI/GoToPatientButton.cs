using UnityEngine;
using UnityEngine.UI;
using Nursing.Managers;
using Nursing.Penalty;

public class GoToPatientButton : MonoBehaviour
{
    [SerializeField] private string itemId; // HandleItemClick에 전달할 ID
    [SerializeField] private PenaltyData notPreparedPenalty;
    
    private Button button;
    private ProcedureManager procedureManager;
    private PenaltyManager penaltyManager;
    private IntermediateManager intermediateManager;

    private void Awake()
    {
        button = GetComponent<Button>();
        procedureManager = FindObjectOfType<ProcedureManager>();
        penaltyManager = FindObjectOfType<PenaltyManager>();
        intermediateManager = FindObjectOfType<IntermediateManager>();
        
        if (button == null)
        {
            Debug.LogError("GoToPatientButton: Button 컴포넌트가 없습니다.");
            return;
        }
        
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        // PrepareFinish 상태 확인
        bool isPrepared = GameManager.Instance.isPrepareFinished;
        
        if (isPrepared)
        {
            // 1. ProcedureStep 진행 (인터랙션 없이도 스텝 완료)
            procedureManager.HandleItemClick(itemId);
            
            // 2. 화면 전환 처리
            intermediateManager.OnGoToPatientClick();
        }
        else
        {
            // 조건 불만족: 패널티 적용
            if (penaltyManager != null && notPreparedPenalty != null)
            {
                penaltyManager.ApplyPenalty(notPreparedPenalty);
            }
            
            
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
    }
}
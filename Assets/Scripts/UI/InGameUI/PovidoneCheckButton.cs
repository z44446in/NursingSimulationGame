using UnityEngine;
using UnityEngine.UI;
using Nursing.Managers;
using Nursing.Penalty;



public class PovidoneCheckButton : MonoBehaviour
{
    [HideInInspector] public ProcedureManager procedureManager;
    [HideInInspector] public PenaltyManager penaltyManager;
    [SerializeField] private string stepId;
    [SerializeField] private PenaltyData insufficientPovidonePenalty;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
         procedureManager = ProcedureManager.Instance;
        penaltyManager   = PenaltyManager.Instance;
        if (button == null)
        {
            Debug.LogError("PovidoneCheckButton: Button 컴포넌트가 없습니다.");
            return;
        }
        if (procedureManager == null || penaltyManager == null)
        {
            Debug.LogError("PovidoneCheckButton: ProcedureManager 또는 PenaltyManager 참조가 없습니다.");
        }
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        int povidoneCount = GameObject.FindGameObjectsWithTag("CountPovidone").Length;
        if (povidoneCount >= 5)
        {
            procedureManager.ForceCompleteStepById(stepId);
            Debug.Log("스텝완료 작동안해");
        }
        else
        {
            if (insufficientPovidonePenalty != null)
                {penaltyManager.ApplyPenalty(insufficientPovidonePenalty);
                Debug.Log("패널티 시스템 작동안해");}
        }
    }
}

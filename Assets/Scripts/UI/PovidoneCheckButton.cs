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
        int povidoneCount = GameObject.FindGameObjectsWithTag("Povidone").Length;
        if (povidoneCount >= 5)
        {
            procedureManager.ForceCompleteStepById(stepId);
        }
        else
        {
            if (insufficientPovidonePenalty != null)
                penaltyManager.ApplyPenalty(insufficientPovidonePenalty);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using Nursing.Procedure;


public class DebugManager : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private ProcedureType debugProcedureType;
    [SerializeField] private Button applyProcedureTypeButton;


    private void Start()
    {
        InitializeDebugControls();


    }

    private void InitializeDebugControls()
    {
        if (applyProcedureTypeButton != null)
        {
            applyProcedureTypeButton.onClick.AddListener(ApplyDebugProcedureType);
        }
    }

    public void ApplyDebugProcedureType()
    {
        if (debugProcedureType == null)
        {
            Debug.LogError("No ProcedureType assigned in DebugManager!");
            return;
        }

     
        GameManager.Instance.SetCurrentProcedureType(debugProcedureType.ProcdureTypeName);

        // ��Ÿ �ʿ��� ���� (����, �÷��� Ÿ�� ���� ��)
        ApplyProcedureSettings(debugProcedureType);
        Debug.Log("���� ���� Ÿ����" + debugProcedureType.displayName+ "���� ������" + debugProcedureType.versionType + "���� ����"  + debugProcedureType.procedurePlayType);

    }

    private ProcedureTypeEnum GetProcedureTypeEnum(ProcedureType procType)
    {
        // ���� ProcedureTypeEnum ���� ������
        return procType.ProcdureTypeName;

       
    }

    private void ApplyProcedureSettings(ProcedureType procType)
    {
       
        GameManager.Instance.SetProcedureVersionType(procType.versionType);
        GameManager.Instance.SetProcedurePlayType(procType.procedurePlayType);

        Debug.Log($"Applied procedure settings - Version: {procType.versionType}, Play Type: {procType.procedurePlayType}");
    }

    
}
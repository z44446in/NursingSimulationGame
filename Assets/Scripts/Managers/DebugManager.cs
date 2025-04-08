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

        // 기타 필요한 로직 (버전, 플레이 타입 설정 등)
        ApplyProcedureSettings(debugProcedureType);
        Debug.Log("지금 술기 타입은" + debugProcedureType.displayName+ "지금 버전은" + debugProcedureType.versionType + "지금 모드는"  + debugProcedureType.procedurePlayType);

    }

    private ProcedureTypeEnum GetProcedureTypeEnum(ProcedureType procType)
    {
        // 직접 ProcedureTypeEnum 값을 가져옴
        return procType.ProcdureTypeName;

       
    }

    private void ApplyProcedureSettings(ProcedureType procType)
    {
       
        GameManager.Instance.SetProcedureVersionType(procType.versionType);
        GameManager.Instance.SetProcedurePlayType(procType.procedurePlayType);

        Debug.Log($"Applied procedure settings - Version: {procType.versionType}, Play Type: {procType.procedurePlayType}");
    }

    
}
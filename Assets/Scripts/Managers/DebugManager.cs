using UnityEngine;
using UnityEngine.UI;
using Nursing.Procedure;
using System.Collections; 

public class DebugManager : MonoBehaviour
{
    [Header("Procedure Data")]
    [SerializeField] private ProcedureType procedureType;
    [SerializeField] private Button startGameButton;
    [SerializeField] private MonoBehaviour targetScript; // ������ ��ũ��Ʈ

    private void Start()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
    }

    public void StartGame()
    {
        if (GameManager.Instance != null && procedureType != null)
        {
            // ProcedureType ������ GameManager�� ����
            GameManager.Instance.SetCurrentProcedureType(procedureType.ProcdureTypeName);
            GameManager.Instance.SetProcedureVersionType(procedureType.versionType);
            GameManager.Instance.SetProcedurePlayType(procedureType.procedurePlayType);

            // ���� ����
            GameManager.Instance.StartGameScene();
            ResetScript();
        }
        else
        {
            Debug.LogError("GameManager �Ǵ� ProcedureType�� �������� �ʾҽ��ϴ�!");
        }
    }

    private void OnDestroy()
    {
        if (startGameButton != null)
            startGameButton.onClick.RemoveAllListeners();
    }

    private void ResetScript()
    {
        if (targetScript != null)
        {
            // ��ũ��Ʈ ��Ȱ��ȭ �� �ٽ� Ȱ��ȭ
            targetScript.enabled = false;
            targetScript.enabled = true;


            // ������ Start �޼��带 ȣ���ϰ� �ʹٸ� �Ʒ� �ڵ带 �߰�
            StartCoroutine(CallStartMethodNextFrame());
        }
    }


    private IEnumerator CallStartMethodNextFrame()
    {
        yield return null; // ���� �����ӱ��� ���

        // Reflection�� ����Ͽ� Start �޼��� ȣ�� (����� �޼��嵵 ȣ�� ����)
        System.Reflection.MethodInfo startMethod = targetScript.GetType().GetMethod("Start",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (startMethod != null)
        {
            startMethod.Invoke(targetScript, null);
           
        }
    }

}
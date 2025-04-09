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

    [Header("�߰�ȭ�� ������ư ")]
    [SerializeField] private IntermediateManager intermediateManager;
    [SerializeField] private Button debugAddAllRequiredItemsButton;

    private void Start()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        // ��ư �̺�Ʈ ���
        if (debugAddAllRequiredItemsButton != null)
        {
            debugAddAllRequiredItemsButton.onClick.AddListener(DebugAddAllRequiredItems);
        }
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

        // ��ư �̺�Ʈ ����
        if (debugAddAllRequiredItemsButton != null)
        {
            debugAddAllRequiredItemsButton.onClick.RemoveListener(DebugAddAllRequiredItems);
        }
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

    /// <summary>
    /// ������ - ���͹̵��Ʈ ȭ�鿡�� ��� �ʼ� �������� �� ���� �߰�
    /// </summary>
    public void DebugAddAllRequiredItems()
    {
        if (intermediateManager == null)
        {
            Debug.LogError("IntermediateManager reference is missing!");
            return;
        }

        // ��� �ʼ� ������ ��������
        if (intermediateManager.requiredItems != null)
        {
            foreach (var requiredItem in intermediateManager.requiredItems.requiredItems)
            {
                if (!requiredItem.isOptional)  // �ʼ� �����۸� �߰�
                {
                    // �̹� �߰����� ���� ��쿡�� �߰�
                    if (!intermediateManager.requiredPickedItems.Contains(requiredItem.item))
                    {
                        intermediateManager.AddPickedItem(requiredItem.item);
                        Debug.Log($"[DEBUG] Added required item: {requiredItem.item.itemName}");
                    }
                }
            }

            // īƮ UI ���� ��û
            intermediateManager.RefreshCartItems();

            Debug.Log("[DEBUG] All required items have been added");
        }
        else
        {
            Debug.LogError("Required items list is null!");
        }
    }

}
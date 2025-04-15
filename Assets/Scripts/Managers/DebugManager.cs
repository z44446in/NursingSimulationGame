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

    [Header("중간화면 디버그버튼")]
    [SerializeField] private IntermediateManager intermediateManager;
    [SerializeField] private Nursing.Managers.ProcedureManager procedureManager;
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
    /// 디버그 - 인터미디어트 화면에서 모든 필수 아이템을 한 번에 추가
    /// </summary>
    public void DebugAddAllRequiredItems()
    {
        if (intermediateManager == null)
        {
            Debug.LogError("IntermediateManager reference is missing!");
            return;
        }

        if (procedureManager == null)
        {
            Debug.LogError("ProcedureManager reference is missing!");
            return;
        }

        // ProcedureManager에서 중간 단계 필수 아이템 목록 가져오기
        var intermediateRequiredItems = procedureManager.GetIntermediateRequiredItems();

        // 모든 필수 아이템 추가하기
        if (intermediateRequiredItems != null && intermediateRequiredItems.Count > 0)
        {
            foreach (var requiredItem in intermediateRequiredItems)
            {
                if (!requiredItem.isOptional)  // 필수 아이템만 추가
                {
                    // 이미 추가되지 않은 경우에만 추가
                    if (!intermediateManager.requiredPickedItems.Contains(requiredItem.item))
                    {
                        intermediateManager.AddPickedItem(requiredItem.item);
                    }
                }
            }

            // 카트 UI 갱신 요청
            intermediateManager.RefreshCartItems();

            Debug.Log("[DEBUG] All required items have been added from ProcedureData");
        }
        else
        {
            Debug.LogError("No intermediate required items found in ProcedureData!");
        }
    }

}
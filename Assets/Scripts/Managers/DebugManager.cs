using UnityEngine;
using UnityEngine.UI;
using Nursing.Procedure;
using System.Collections; 

public class DebugManager : MonoBehaviour
{
    [Header("Procedure Data")]
    [SerializeField] private ProcedureType procedureType;
    [SerializeField] private Button startGameButton;
    [SerializeField] private MonoBehaviour targetScript; // 리셋할 스크립트

    [Header("중간화면 디버깅버튼 ")]
    [SerializeField] private IntermediateManager intermediateManager;
    [SerializeField] private Button debugAddAllRequiredItemsButton;

    private void Start()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);

        // 버튼 이벤트 등록
        if (debugAddAllRequiredItemsButton != null)
        {
            debugAddAllRequiredItemsButton.onClick.AddListener(DebugAddAllRequiredItems);
        }
    }

    public void StartGame()
    {
        if (GameManager.Instance != null && procedureType != null)
        {
            // ProcedureType 정보를 GameManager에 설정
            GameManager.Instance.SetCurrentProcedureType(procedureType.ProcdureTypeName);
            GameManager.Instance.SetProcedureVersionType(procedureType.versionType);
            GameManager.Instance.SetProcedurePlayType(procedureType.procedurePlayType);

            // 게임 시작
            GameManager.Instance.StartGameScene();
            ResetScript();
        }
        else
        {
            Debug.LogError("GameManager 또는 ProcedureType이 설정되지 않았습니다!");
        }
    }

    private void OnDestroy()
    {
        if (startGameButton != null)
            startGameButton.onClick.RemoveAllListeners();

        // 버튼 이벤트 제거
        if (debugAddAllRequiredItemsButton != null)
        {
            debugAddAllRequiredItemsButton.onClick.RemoveListener(DebugAddAllRequiredItems);
        }
    }

    private void ResetScript()
    {
        if (targetScript != null)
        {
            // 스크립트 비활성화 후 다시 활성화
            targetScript.enabled = false;
            targetScript.enabled = true;


            // 강제로 Start 메서드를 호출하고 싶다면 아래 코드를 추가
            StartCoroutine(CallStartMethodNextFrame());
        }
    }


    private IEnumerator CallStartMethodNextFrame()
    {
        yield return null; // 다음 프레임까지 대기

        // Reflection을 사용하여 Start 메서드 호출 (비공개 메서드도 호출 가능)
        System.Reflection.MethodInfo startMethod = targetScript.GetType().GetMethod("Start",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (startMethod != null)
        {
            startMethod.Invoke(targetScript, null);
           
        }
    }

    /// <summary>
    /// 디버깅용 - 인터미디어트 화면에서 모든 필수 아이템을 한 번에 추가
    /// </summary>
    public void DebugAddAllRequiredItems()
    {
        if (intermediateManager == null)
        {
            Debug.LogError("IntermediateManager reference is missing!");
            return;
        }

        // 모든 필수 아이템 가져오기
        if (intermediateManager.requiredItems != null)
        {
            foreach (var requiredItem in intermediateManager.requiredItems.requiredItems)
            {
                if (!requiredItem.isOptional)  // 필수 아이템만 추가
                {
                    // 이미 추가되지 않은 경우에만 추가
                    if (!intermediateManager.requiredPickedItems.Contains(requiredItem.item))
                    {
                        intermediateManager.AddPickedItem(requiredItem.item);
                        Debug.Log($"[DEBUG] Added required item: {requiredItem.item.itemName}");
                    }
                }
            }

            // 카트 UI 갱신 요청
            intermediateManager.RefreshCartItems();

            Debug.Log("[DEBUG] All required items have been added");
        }
        else
        {
            Debug.LogError("Required items list is null!");
        }
    }

}
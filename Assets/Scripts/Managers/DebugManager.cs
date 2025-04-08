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

    private void Start()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
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

}
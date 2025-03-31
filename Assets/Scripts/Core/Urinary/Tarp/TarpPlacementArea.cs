using UnityEngine;
using UnityEngine.UI;

public class TarpPlacementArea : MonoBehaviour
{
    [SerializeField] private bool isValidPlacement = true;
    [SerializeField] private GameObject tarpminiGamePrefab;
    [SerializeField] private Transform popupParent;

    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnAreaClicked);
        }
    }

    private void OnAreaClicked()
    {
        if (isValidPlacement)
        {
            // 미니게임 팝업 생성
            ShowTarpMiniGame();
        }
        else
        {
            // 잘못된 위치 선택시 피드백
            DialogueManager.Instance.ShowSmallDialogue("이곳에는 방수포를 둘 수 없습니다.");
        }
    }

    private void ShowTarpMiniGame()
    {


        GameObject tarpMiniGameInstance = Instantiate(tarpminiGamePrefab);
        if (tarpMiniGameInstance != null)
        {
            tarpMiniGameInstance.transform.SetParent(popupParent, false);
        }
        // 미니게임 팝업 생성 및 초기화
        // TODO: 프리팹 경로 설정 필요
        GameObject miniGamePrefab = Resources.Load<GameObject>("Prefabs/TarpMiniGame");
        if (miniGamePrefab != null)
        {
            GameObject miniGame = Instantiate(miniGamePrefab);
            MiniGamePopup popup = miniGame.GetComponent<MiniGamePopup>();
            if (popup != null)
            {
                popup.Initialize(
                    onSuccess: () => {
                        // 성공시 실제로 방수포 배치
                        PlaceTarpInGame();
                    },
                    onFail: () => {
                        // 실패시 페널티 적용
                        ProcedureManager.Instance.ApplyScorePenalty(10);
                    },
                    penaltyScore: 10
                );
            }
        }
    }

    private void PlaceTarpInGame()
    {
        // 실제 게임 화면에 방수포 배치
        // TODO: 방수포 프리팹 경로 설정 필요
        GameObject tarpPrefab = Resources.Load<GameObject>("Prefabs/TarpInGame");
        if (tarpPrefab != null)
        {
            Instantiate(tarpPrefab, transform.position, Quaternion.identity);
        }
    }
}
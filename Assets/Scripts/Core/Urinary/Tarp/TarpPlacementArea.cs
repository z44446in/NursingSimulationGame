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
            // �̴ϰ��� �˾� ����
            ShowTarpMiniGame();
        }
        else
        {
            // �߸��� ��ġ ���ý� �ǵ��
            DialogueManager.Instance.ShowSmallDialogue("�̰����� ������� �� �� �����ϴ�.");
        }
    }

    private void ShowTarpMiniGame()
    {


        GameObject tarpMiniGameInstance = Instantiate(tarpminiGamePrefab);
        if (tarpMiniGameInstance != null)
        {
            tarpMiniGameInstance.transform.SetParent(popupParent, false);
        }
        // �̴ϰ��� �˾� ���� �� �ʱ�ȭ
        // TODO: ������ ��� ���� �ʿ�
        GameObject miniGamePrefab = Resources.Load<GameObject>("Prefabs/TarpMiniGame");
        if (miniGamePrefab != null)
        {
            GameObject miniGame = Instantiate(miniGamePrefab);
            MiniGamePopup popup = miniGame.GetComponent<MiniGamePopup>();
            if (popup != null)
            {
                popup.Initialize(
                    onSuccess: () => {
                        // ������ ������ ����� ��ġ
                        PlaceTarpInGame();
                    },
                    onFail: () => {
                        // ���н� ���Ƽ ����
                        ProcedureManager.Instance.ApplyScorePenalty(10);
                    },
                    penaltyScore: 10
                );
            }
        }
    }

    private void PlaceTarpInGame()
    {
        // ���� ���� ȭ�鿡 ����� ��ġ
        // TODO: ����� ������ ��� ���� �ʿ�
        GameObject tarpPrefab = Resources.Load<GameObject>("Prefabs/TarpInGame");
        if (tarpPrefab != null)
        {
            Instantiate(tarpPrefab, transform.position, Quaternion.identity);
        }
    }
}
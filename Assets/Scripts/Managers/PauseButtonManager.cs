using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseButtonManager : MonoBehaviour
{
    [Header("Popup Objects")]
    [SerializeField] private GameObject confirmPopup; // 팝업 패널
    [SerializeField] private Button yesButton; // '예' 버튼
    [SerializeField] private Button noButton;  // '아니오' 버튼

    private void Start()
    {
        // 팝업은 처음에 비활성화
        if (confirmPopup != null)
            confirmPopup.SetActive(false);

        // 버튼 리스너 등록
        if (yesButton != null)
            yesButton.onClick.AddListener(OnYesClicked);
        
        if (noButton != null)
            noButton.onClick.AddListener(OnNoClicked);
    }

    // 정지 버튼을 눌렀을 때 호출되는 메서드
    public void OnPauseButtonClicked()
    {
        if (confirmPopup != null)
        {
            confirmPopup.SetActive(true);
            GameManager.Instance.PauseGame(); // 게임 일시정지
        }
    }

    // '예' 버튼 클릭 시
    private void OnYesClicked()
    {
        GameManager.Instance.LoadScene(GameManager.MAIN_HOME_SCENE);
        Time.timeScale = 1f; // 시간 스케일 원래대로 복구
        
    }

    // '아니오' 버튼 클릭 시ㅇㅇ
    private void OnNoClicked()
    {
        confirmPopup.SetActive(false);
        GameManager.Instance.ResumeGame(); // 게임 재개
    }

    private void OnDestroy()
    {
        // 버튼 리스너 제거
        if (yesButton != null)
            yesButton.onClick.RemoveListener(OnYesClicked);
        
        if (noButton != null)
            noButton.onClick.RemoveListener(OnNoClicked);
    }
}
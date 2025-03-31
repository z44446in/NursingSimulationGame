using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGamePopup : BaseGamePopup
{
    [Header("Mini Game Components")]
    [SerializeField] private Image instructionImage;
    [SerializeField] private TextMeshProUGUI instructionText;  // 추가
    [SerializeField] private Transform contentArea;

    private bool isInstructionShown = true;

    protected override void Start()
    {
        base.Start();

        // 처음에는 게임 시작하지 않고 instruction만 보여줌
        isTimerRunning = false;

        if (instructionImage != null)
        {
            instructionImage.gameObject.SetActive(true);
            // 인스트럭션 이미지 클릭 이벤트 추가
            instructionImage.GetComponent<Button>()?.onClick.AddListener(OnInstructionClick);
        }
    }
    public void SetInstructionText(string text)  // 추가
    {
        if (instructionText != null)
        {
            instructionText.text = text;
        }
    }
    private void OnInstructionClick()
    {
        if (isInstructionShown)
        {
            isInstructionShown = false;
            instructionImage.gameObject.SetActive(false);
            StartGame();
        }
    }

    public Transform GetContentArea()
    {
        return contentArea;
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGamePopup : BaseGamePopup
{
    [Header("Mini Game Components")]
    [SerializeField] private Image instructionImage;
    [SerializeField] private TextMeshProUGUI instructionText;  // �߰�
    [SerializeField] private Transform contentArea;

    private bool isInstructionShown = true;

    protected override void Start()
    {
        base.Start();

        // ó������ ���� �������� �ʰ� instruction�� ������
        isTimerRunning = false;

        if (instructionImage != null)
        {
            instructionImage.gameObject.SetActive(true);
            // �ν�Ʈ���� �̹��� Ŭ�� �̺�Ʈ �߰�
            instructionImage.GetComponent<Button>()?.onClick.AddListener(OnInstructionClick);
        }
    }
    public void SetInstructionText(string text)  // �߰�
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
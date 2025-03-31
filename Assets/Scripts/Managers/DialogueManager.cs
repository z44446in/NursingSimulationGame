using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    public static DialogueManager Instance => instance;

    public enum Speaker
    {
        Character,   // ĳ����
        Patient,     // ȯ�� 
        Guardian,    // ��ȣ��
        Player,      // �÷��̾�
        HeadNurse    // ����ȣ��
    }

    [System.Serializable]
    public class SpeakerData
    {
        public string displayName;      // ȭ�鿡 ǥ�õ� �̸�
        public Sprite speakerSprite;    // ȭ�� �̹���
    }

    [Header("Dialogue Prefabs")]
    [SerializeField] private GameObject smallDialoguePrefab;
    [SerializeField] private GameObject largeDialoguePrefab;

    [Header("Speakers")]
    [SerializeField] private SpeakerData[] speakerData = new SpeakerData[5];

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Image speakerImage;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Parents")]
    [SerializeField] private Transform dialogueParent;
    [SerializeField] private Transform popupParent;

    private GameObject currentSmallDialogue;
    private GameObject currentLargeDialogue;
    private TextMeshProUGUI largeDialogueText;
    private ScrollRect scrollRect;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowSmallDialogue(
     string message,
     bool isOptionalItemMessage = false,  // �� ��° �Ű������� �̵�
     Action onDialogueClosed = null,      // �� ��° �Ű������� �̵�
     Speaker speaker = Speaker.Character   // ������ �Ű������� �̵�
 )
    {
        Button dialogueButton;

        if (currentSmallDialogue == null)
        {
            currentSmallDialogue = Instantiate(smallDialoguePrefab, dialogueParent);

            speakerNameText = currentSmallDialogue.GetComponentInChildren<TextMeshProUGUI>(true);
            speakerImage = currentSmallDialogue.GetComponentInChildren<Image>(true);
            dialogueText = currentSmallDialogue.GetComponentInChildren<TextMeshProUGUI>(true);

            dialogueButton = currentSmallDialogue.GetComponent<Button>();
            if (dialogueButton == null)
            {
                dialogueButton = currentSmallDialogue.AddComponent<Button>();
            }
        }
        else
        {
            dialogueButton = currentSmallDialogue.GetComponent<Button>();
        }

        SpeakerData currentSpeaker = speakerData[(int)speaker];

        if (speakerNameText != null)
        {
            speakerNameText.text = currentSpeaker.displayName;
        }

        if (speakerImage != null)
        {
            speakerImage.sprite = currentSpeaker.speakerSprite;
            speakerImage.gameObject.SetActive(currentSpeaker.speakerSprite != null);
        }

        if (dialogueText != null)
        {
            dialogueText.text = message;
        }

        SetDialogueAsTopmost(currentSmallDialogue);
        currentSmallDialogue.SetActive(true);

        dialogueButton.onClick.RemoveAllListeners();
        dialogueButton.onClick.AddListener(() => {
            currentSmallDialogue.SetActive(false);
            onDialogueClosed?.Invoke();
        });
    }

    public void ShowLargeDialogue(string message)
    {
        if (currentLargeDialogue == null)
        {
            currentLargeDialogue = Instantiate(largeDialoguePrefab, dialogueParent);
            largeDialogueText = currentLargeDialogue.GetComponentInChildren<TextMeshProUGUI>();
            scrollRect = currentLargeDialogue.GetComponentInChildren<ScrollRect>();

            Button dialogueButton = currentLargeDialogue.GetComponent<Button>();
            if (dialogueButton == null)
            {
                dialogueButton = currentLargeDialogue.AddComponent<Button>();
            }
            dialogueButton.onClick.AddListener(() => {
                currentLargeDialogue.SetActive(false);
            });
        }

        SetDialogueAsTopmost(currentLargeDialogue);
        largeDialogueText.text = message;
        if (scrollRect != null)
            scrollRect.normalizedPosition = Vector2.one;
        currentLargeDialogue.SetActive(true);
    }

    private void SetDialogueAsTopmost(GameObject dialogueObject)
    {
        if (dialogueObject != null)
        {
            dialogueObject.transform.SetAsLastSibling();
        }
    }

    private void OnDestroy()
    {
        if (currentSmallDialogue != null)
            Destroy(currentSmallDialogue);
        if (currentLargeDialogue != null)
            Destroy(currentLargeDialogue);
    }
}
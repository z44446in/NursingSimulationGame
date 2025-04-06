using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace Nursing.Managers
{
    public class DialogueManager : MonoBehaviour
    {
        private static DialogueManager instance;
        public static DialogueManager Instance => instance;

        public enum Speaker
        {
            Character,   // 캐릭터
            Patient,     // 환자 
            Guardian,    // 보호자
            Player,      // 플레이어
            HeadNurse    // 수간호사
        }

        [System.Serializable]
        public class SpeakerData
        {
            public string displayName;      // 화면에 표시될 이름
            public Sprite speakerSprite;    // 화자 이미지
        }

        [Header("대화창 프리팹")]
        [SerializeField] private GameObject smallDialoguePrefab; // 작은 대화창 프리팹
        [SerializeField] private GameObject largeDialoguePrefab; // 큰 대화창 프리팹

        [Header("대화창 부모 오브젝트")]
        [SerializeField] private Transform dialogueParent; // 대화창 부모 오브젝트

        [Header("화자 데이터")]
        [SerializeField] private SpeakerData[] speakerData; // 화자 데이터 배열

        [Header("Animation Settings")]
        [SerializeField] private float fadeInDuration = 0.3f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private Ease fadeInEase = Ease.OutQuad;
        [SerializeField] private Ease fadeOutEase = Ease.InQuad;

        // 현재 활성화된 대화창
        private GameObject currentSmallDialogue;
        private GameObject currentLargeDialogue;

        // UI 요소 참조
        private TextMeshProUGUI speakerNameText;
        private Image speakerImage;
        private TextMeshProUGUI dialogueText;

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

        // 기존 메서드 유지
        public void ShowSmallDialogue(
            string message,
            bool isOptionalItemMessage = false,
            Action onDialogueClosed = null,
            Speaker speaker = Speaker.Character
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
            }

            if (dialogueText != null)
            {
                dialogueText.text = message;
            }

            // 페이드인 애니메이션 추가
            CanvasGroup canvasGroup = currentSmallDialogue.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = currentSmallDialogue.AddComponent<CanvasGroup>();
            }

            
            currentSmallDialogue.SetActive(true);

            // 초기 알파값을 0으로 설정하고 페이드인
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase);

            dialogueButton.onClick.RemoveAllListeners();
            dialogueButton.onClick.AddListener(() => {
                // 페이드아웃 애니메이션 후 대화창 비활성화 및 콜백 실행
                canvasGroup.DOFade(0f, fadeOutDuration)
                    .SetEase(fadeOutEase)
                    .OnComplete(() => {
                        currentSmallDialogue.SetActive(false);
                        onDialogueClosed?.Invoke();
                    });
            });

            // 버튼 클릭 이벤트 등록
            if (dialogueButton != null)
            {
                dialogueButton.onClick.RemoveAllListeners();
                dialogueButton.onClick.AddListener(() =>
                {
                    onDialogueClosed?.Invoke();
                    CloseSmallDialogue();
                });
            }

            BringToFront(currentSmallDialogue);
        }

        // 패널티 시스템과 호환되는 오버로드된 메서드 추가
        public void ShowSmallDialogue(
            string message,
            string speakerName
        )
        {
            // 스피커 이름으로 Speaker 열거형 값을 찾음
            Speaker foundSpeaker = Speaker.Character;

            // 기존 메서드 호출
            ShowSmallDialogue(message, false, null, foundSpeaker);
        }

        public void ShowLargeDialogue(string message, Action onDialogueClosed = null)
        {
            Button dialogueButton;

            if (currentLargeDialogue == null)
            {
                currentLargeDialogue = Instantiate(largeDialoguePrefab, dialogueParent);

                dialogueText = currentLargeDialogue.GetComponentInChildren<TextMeshProUGUI>(true);
                dialogueButton = currentLargeDialogue.GetComponent<Button>();
                if (dialogueButton == null)
                {
                    dialogueButton = currentLargeDialogue.AddComponent<Button>();
                }
            }

            else
            {
                dialogueButton = currentLargeDialogue.GetComponent<Button>();
            }
            if (dialogueText != null)
            {
                dialogueText.text = message;
            }


            // 버튼 클릭 이벤트 등록
            if (dialogueButton != null)
            {
                dialogueButton.onClick.RemoveAllListeners();
                dialogueButton.onClick.AddListener(() =>
                {
                    onDialogueClosed?.Invoke();
                    CloseLargeDialogue();
                });
            }

            BringToFront(currentLargeDialogue);
        }
    
        

        public void CloseSmallDialogue()
        {
            if (currentSmallDialogue != null)
            {
                Destroy(currentSmallDialogue);
                currentSmallDialogue = null;
            }
        }

        public void CloseLargeDialogue()
        {
            if (currentLargeDialogue != null)
            {
                Destroy(currentLargeDialogue);
                currentLargeDialogue = null;
            }
        }

        private void BringToFront(GameObject dialogueObject)
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
}
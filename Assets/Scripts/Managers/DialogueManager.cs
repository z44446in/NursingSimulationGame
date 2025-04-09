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
            HeadNurse,   // 수간호사
           
        }

        [System.Serializable]
        public class SpeakerData
        {
            public string displayName = "";      // 화면에 표시될 이름
            public Sprite speakerSprite;         // 화자 이미지
            
            // 기본 생성자
            public SpeakerData() { }
            
            // 매개변수가 있는 생성자
            public SpeakerData(string name, Sprite sprite)
            {
                displayName = name;
                speakerSprite = sprite;
            }
        }

        [Header("대화창 프리팹")]
        [SerializeField] private GameObject smallDialoguePrefab; // 작은 대화창 프리팹
        [SerializeField] private GameObject largeDialoguePrefab; // 큰 대화창 프리팹
        
        [Header("가이드 UI")]
        [SerializeField] private GameObject guidePanel; // 가이드 메시지 패널
        [SerializeField] private TextMeshProUGUI guideText; // 가이드 메시지 텍스트

        [Header("대화창 부모 오브젝트")]
        [SerializeField] private Transform dialogueParent; // 대화창 부모 오브젝트

        [Header("화자 데이터")]
        [SerializeField] private SpeakerData[] speakerData = new SpeakerData[System.Enum.GetValues(typeof(Speaker)).Length]; // 화자 데이터 배열

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
                
                // 대화창의 버튼 컴포넌트 참조
                dialogueButton = currentSmallDialogue.GetComponent<Button>();
                if (dialogueButton == null)
                {
                    dialogueButton = currentSmallDialogue.AddComponent<Button>();
                }
                
                // 작은 대화창 프리팹에서 UI 요소 찾기 - 프리팹 구조에 맞게 요소 찾기
                Transform nameTr = currentSmallDialogue.transform.Find("SmallDBg /name");
                Transform contextTr = currentSmallDialogue.transform.Find("SmallDBg /context");
                Transform speakerImageTr = currentSmallDialogue.transform.Find("SmallDBg /speakerImage");
                
                if (nameTr != null)
                    speakerNameText = nameTr.GetComponent<TextMeshProUGUI>();
                
                if (contextTr != null)
                    dialogueText = contextTr.GetComponent<TextMeshProUGUI>();
                
                if (speakerImageTr != null)
                    speakerImage = speakerImageTr.GetComponent<Image>();
            }
            else
            {
                dialogueButton = currentSmallDialogue.GetComponent<Button>();
            }

            // speaker 열거형에 해당하는 speakerData 가져오기
            SpeakerData currentSpeaker = speakerData[(int)speaker];

            // 화자 이름과 이미지 설정
            if (speakerNameText != null && !string.IsNullOrEmpty(currentSpeaker.displayName))
            {
                speakerNameText.text = currentSpeaker.displayName;
            }
            else if (speakerNameText != null)
            {
                // 이름이 설정되지 않은 경우 기본값 사용
                speakerNameText.text = speaker.ToString();
            }

            if (speakerImage != null && currentSpeaker.speakerSprite != null)
            {
                speakerImage.sprite = currentSpeaker.speakerSprite;
            }

            // 대화 내용 설정
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

            // 여기에 추가 - 이전 Tween 중지 후 새 애니메이션 시작
            DOTween.Kill(canvasGroup);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase);
            currentSmallDialogue.SetActive(true);

            // 초기 알파값을 0으로 설정하고 페이드인
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase);

            dialogueButton.onClick.RemoveAllListeners();
            // DialogueManager.cs의 ShowSmallDialogue 메서드 내 버튼 클릭 핸들러 수정
            dialogueButton.onClick.AddListener(() => {
                // 페이드아웃 애니메이션 후 대화창 비활성화 및 콜백 실행
                canvasGroup.DOFade(0f, fadeOutDuration)
                    .SetEase(fadeOutEase)
                    .OnComplete(() => {
                        currentSmallDialogue.SetActive(false);
                        onDialogueClosed?.Invoke();
            // 여기서 파괴하지 말고, 애니메이션 완료 후 파괴
        });
            });





            BringToFront(currentSmallDialogue);
        }

        // 패널티 시스템과 호환되는 오버로드된 메서드 추가 - 더 이상 사용하지 않음
        [System.Obsolete("이 메서드는 더 이상 사용되지 않습니다. 대신 Speaker enum을 사용하는 오버로드를 사용하세요.")]
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
    
        /// <summary>
        /// 가이드 메시지를 표시합니다.
        /// </summary>
        public void ShowGuideMessage(string message)
        {
            if (guidePanel == null || guideText == null)
            {
                Debug.LogWarning("가이드 패널 또는 텍스트가 설정되지 않았습니다.");
                return;
            }
            
            guideText.text = message;
            
            // 가이드 패널이 비활성화 상태라면 활성화
            if (!guidePanel.activeSelf)
            {
                guidePanel.SetActive(true);
                
                // 페이드인 애니메이션 (선택적)
                CanvasGroup canvasGroup = guidePanel.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase);
                }
            }
            else
            {
                // 이미 활성화 상태라면 텍스트 변경 효과 (선택적)
                guideText.DOFade(0f, 0.1f).OnComplete(() => {
                    guideText.DOFade(1f, 0.3f);
                });
            }
        }
        
        /// <summary>
        /// 가이드 메시지를 닫습니다.
        /// </summary>
        public void HideGuideMessage()
        {
            if (guidePanel == null)
                return;
                
            // 페이드아웃 애니메이션 (선택적)
            CanvasGroup canvasGroup = guidePanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase)
                    .OnComplete(() => {
                        guidePanel.SetActive(false);
                    });
            }
            else
            {
                guidePanel.SetActive(false);
            }
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
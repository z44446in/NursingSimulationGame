using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Nursing.Penalty;

namespace Nursing.Managers
{
    public class PenaltyManager : MonoBehaviour
    {

         public static PenaltyManager Instance { get; private set; }
    

        [Header("패널티 UI 요소")]
        [SerializeField] private Image redScreenFlash;
        [SerializeField] private GameObject smallDialoguePrefab;
        
        [Header("패널티 설정")]
        [SerializeField] private float flashDuration = 0.3f;
        [SerializeField] private float flashInterval = 0.2f;
        
        private DialogueManager dialogueManager;
        private PenaltyDatabase penaltyDatabase;

        

        private void Awake()
        {
            // ② 기존 Awake 로직보다 위에 싱글톤 초기화
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 인스턴스 제거
            return;
        }
        Instance = this;
        
            dialogueManager = FindObjectOfType<DialogueManager>();
            penaltyDatabase = FindObjectOfType<PenaltyDatabase>();
            
            if (redScreenFlash != null)
            {
                redScreenFlash.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 패널티를 적용합니다.
        /// </summary>
        /// <param name="penaltyData">적용할 패널티 데이터</param>
        /// <returns>패널티가 성공적으로 적용되었는지 여부</returns>
        public bool ApplyPenalty(PenaltyData penaltyData)
        {
            if (penaltyData == null)
                return false;
            
            
            // 패널티 시각효과 표시
            if (penaltyData.flashRedScreen && redScreenFlash != null)
            {
                StartCoroutine(FlashScreen(penaltyData.flashCount));
            }
            
            // 패널티 메시지 표시
            if (!string.IsNullOrEmpty(penaltyData.penaltyMessage) && dialogueManager != null)
            {
                dialogueManager.ShowSmallDialogue(penaltyData.penaltyMessage, false, null, penaltyData.speaker);
            }
            
            // 패널티 데이터베이스에 기록
            if (!string.IsNullOrEmpty(penaltyData.databaseMessage) && penaltyDatabase != null)
            {
                penaltyDatabase.RecordPenalty(penaltyData);
            }

            // 동작 취소 실행 (추가된 부분)
            if (penaltyData.undoAction != null)
            {
                penaltyData.undoAction.Invoke();
            }

            return true;

        }
        
        /// <summary>
        /// 화면 가장자리를 빨간색으로 깜빡입니다.
        /// </summary>
        /// <param name="flashCount">깜빡임 횟수</param>
        private IEnumerator FlashScreen(int flashCount)
        {
            for (int i = 0; i < flashCount; i++)
            {
                redScreenFlash.gameObject.SetActive(true);
                
                // 페이드 인
                float t = 0f;
                while (t < flashDuration)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(0f, 0.5f, t / flashDuration);
                    redScreenFlash.color = new Color(1f, 0f, 0f, alpha);
                    yield return null;
                }
                
                // 페이드 아웃
                t = 0f;
                while (t < flashDuration)
                {
                    t += Time.deltaTime;
                    float alpha = Mathf.Lerp(0.5f, 0f, t / flashDuration);
                    redScreenFlash.color = new Color(1f, 0f, 0f, alpha);
                    yield return null;
                }
                
                redScreenFlash.gameObject.SetActive(false);
                
                if (i < flashCount - 1)
                    yield return new WaitForSeconds(flashInterval);
            }
        }
    }
}
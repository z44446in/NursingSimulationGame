using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using DG.Tweening;

public class HandSanitizerMiniGame : MonoBehaviour, Nursing.Managers.MiniGameController
{
    [Header("References")]
    [SerializeField] private RectTransform leftHandTransform;
    [SerializeField] private RectTransform rightHandTransform;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button cancelButton;

    [Header("Settings")]
    [SerializeField] private float maxProgress = 100f;
    [SerializeField] private float progressPerRub = 2f;
    [SerializeField] private float progressDecayRate = 5f;
    [SerializeField] private float minRubDistance = 10f;
    [SerializeField] private float handReturnSpeed = 5f;
    [SerializeField] private Vector2 leftHandInitialPosition;
    [SerializeField] private Vector2 rightHandInitialPosition;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private float currentProgress = 0f;
    private Vector2 leftHandPrevPos;
    private Vector2 rightHandPrevPos;
    private bool leftHandTouched = false;
    private bool rightHandTouched = false;
    private int leftHandPointerId = -1;
    private int rightHandPointerId = -1;
    private bool gameCompleted = false;
    private CanvasGroup canvasGroup;

    // 게임 완료 이벤트
    public event System.Action<bool> OnGameComplete;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // 초기 위치 저장
        if (leftHandInitialPosition == Vector2.zero && leftHandTransform != null)
            leftHandInitialPosition = leftHandTransform.anchoredPosition;
            
        if (rightHandInitialPosition == Vector2.zero && rightHandTransform != null)
            rightHandInitialPosition = rightHandTransform.anchoredPosition;
    }

    private void Start()
    {
        // 초기화
        currentProgress = 0f;
        UpdateProgressBar();
        
        if (statusText != null)
            statusText.text = "손소독 중";
            
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelGame);
            
        // 손 위치 초기화
        if (leftHandTransform != null)
            leftHandTransform.anchoredPosition = leftHandInitialPosition;
            
        if (rightHandTransform != null)
            rightHandTransform.anchoredPosition = rightHandInitialPosition;
    }

    private void Update()
    {
        if (gameCompleted)
            return;

        // 터치/마우스 입력 처리
        HandleInput();
        
        // 손 위치 자동 복귀
        ReturnHandsToPosition();
        
        // 진행상황 감소 (사용자가 계속 문지르지 않으면)
        if (!leftHandTouched || !rightHandTouched)
        {
            DecreaseProgress();
        }
        
        // 게임 완료 체크
        CheckGameCompletion();
    }

    private void HandleInput()
    {
        // 터치 디바이스
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Vector2 touchPosition = touch.position;
                
                // 터치 시작
                if (touch.phase == TouchPhase.Began)
                {
                    // 왼손 확인
                    if (!leftHandTouched && RectTransformUtility.RectangleContainsScreenPoint(leftHandTransform, touchPosition))
                    {
                        leftHandTouched = true;
                        leftHandPointerId = touch.fingerId;
                        leftHandPrevPos = touchPosition;
                    }
                    // 오른손 확인
                    else if (!rightHandTouched && RectTransformUtility.RectangleContainsScreenPoint(rightHandTransform, touchPosition))
                    {
                        rightHandTouched = true;
                        rightHandPointerId = touch.fingerId;
                        rightHandPrevPos = touchPosition;
                    }
                }
                // 터치 이동
                else if (touch.phase == TouchPhase.Moved)
                {
                    // 왼손 이동
                    if (leftHandTouched && touch.fingerId == leftHandPointerId)
                    {
                        Vector2 deltaMove = touchPosition - leftHandPrevPos;
                        MoveHand(leftHandTransform, deltaMove);
                        leftHandPrevPos = touchPosition;
                        
                        if (rightHandTouched)
                        {
                            AddProgress(deltaMove.magnitude);
                        }
                    }
                    // 오른손 이동
                    else if (rightHandTouched && touch.fingerId == rightHandPointerId)
                    {
                        Vector2 deltaMove = touchPosition - rightHandPrevPos;
                        MoveHand(rightHandTransform, deltaMove);
                        rightHandPrevPos = touchPosition;
                        
                        if (leftHandTouched)
                        {
                            AddProgress(deltaMove.magnitude);
                        }
                    }
                }
                // 터치 종료
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // 왼손 터치 해제
                    if (leftHandTouched && touch.fingerId == leftHandPointerId)
                    {
                        leftHandTouched = false;
                        leftHandPointerId = -1;
                    }
                    // 오른손 터치 해제
                    else if (rightHandTouched && touch.fingerId == rightHandPointerId)
                    {
                        rightHandTouched = false;
                        rightHandPointerId = -1;
                    }
                }
            }
        }
        // 마우스 입력 (PC 테스트용)
        else
        {
            Vector2 mousePos = Input.mousePosition;
            
            // 왼쪽 버튼 클릭
            if (Input.GetMouseButtonDown(0))
            {
                if (!leftHandTouched && RectTransformUtility.RectangleContainsScreenPoint(leftHandTransform, mousePos))
                {
                    leftHandTouched = true;
                    leftHandPrevPos = mousePos;
                }
            }
            // 오른쪽 버튼 클릭
            else if (Input.GetMouseButtonDown(1))
            {
                if (!rightHandTouched && RectTransformUtility.RectangleContainsScreenPoint(rightHandTransform, mousePos))
                {
                    rightHandTouched = true;
                    rightHandPrevPos = mousePos;
                }
            }
            
            // 왼쪽 버튼 이동
            if (leftHandTouched && Input.GetMouseButton(0))
            {
                Vector2 deltaMove = mousePos - leftHandPrevPos;
                MoveHand(leftHandTransform, deltaMove);
                leftHandPrevPos = mousePos;
                
                if (rightHandTouched)
                {
                    AddProgress(deltaMove.magnitude);
                }
            }
            
            // 오른쪽 버튼 이동
            if (rightHandTouched && Input.GetMouseButton(1))
            {
                Vector2 deltaMove = mousePos - rightHandPrevPos;
                MoveHand(rightHandTransform, deltaMove);
                rightHandPrevPos = mousePos;
                
                if (leftHandTouched)
                {
                    AddProgress(deltaMove.magnitude);
                }
            }
            
            // 버튼 해제
            if (Input.GetMouseButtonUp(0))
            {
                leftHandTouched = false;
            }
            
            if (Input.GetMouseButtonUp(1))
            {
                rightHandTouched = false;
            }
        }
    }
    
    private void MoveHand(RectTransform handTransform, Vector2 deltaMove)
    {
        if (handTransform == null) return;
        
        handTransform.anchoredPosition += deltaMove;
    }
    
    private void ReturnHandsToPosition()
    {
        // 터치되지 않은 손은 원래 위치로 복귀
        if (!leftHandTouched && leftHandTransform != null)
        {
            leftHandTransform.anchoredPosition = Vector2.Lerp(
                leftHandTransform.anchoredPosition, 
                leftHandInitialPosition, 
                Time.deltaTime * handReturnSpeed
            );
        }
        
        if (!rightHandTouched && rightHandTransform != null)
        {
            rightHandTransform.anchoredPosition = Vector2.Lerp(
                rightHandTransform.anchoredPosition, 
                rightHandInitialPosition, 
                Time.deltaTime * handReturnSpeed
            );
        }
    }
    
    private void AddProgress(float moveMagnitude)
    {
        // 최소 이동 거리보다 적게 움직였다면 진행 상황 추가하지 않음
        if (moveMagnitude < minRubDistance)
            return;
            
        currentProgress += progressPerRub * (moveMagnitude / 100f);
        currentProgress = Mathf.Clamp(currentProgress, 0f, maxProgress);
        UpdateProgressBar();
    }
    
    private void DecreaseProgress()
    {
        currentProgress -= progressDecayRate * Time.deltaTime;
        currentProgress = Mathf.Clamp(currentProgress, 0f, maxProgress);
        UpdateProgressBar();
    }
    
    private void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            progressBar.fillAmount = currentProgress / maxProgress;
        }
    }
    
    private void CheckGameCompletion()
    {
        if (currentProgress >= maxProgress && !gameCompleted)
        {
            gameCompleted = true;
            
            if (statusText != null)
            {
                statusText.text = "손소독 완료";
            }
            
            // 간단한 완료 애니메이션
            StartCoroutine(CompleteWithDelay(1.5f));
        }
    }
    
    private IEnumerator CompleteWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // 페이드 아웃 효과
        canvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() => {
            // 게임 완료 이벤트 발생
            OnGameComplete?.Invoke(true);
        });
    }
    
    private void CancelGame()
    {
        // 페이드 아웃 효과
        canvasGroup.DOFade(0f, fadeOutDuration).OnComplete(() => {
            // 게임 취소 이벤트 발생
            OnGameComplete?.Invoke(false);
        });
    }
    
    private void OnDestroy()
    {
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(CancelGame);
            
        DOTween.Kill(canvasGroup);
    }
}
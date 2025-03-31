using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TarpMiniGame : MonoBehaviour
{
    [Header("Game Components")]
    [SerializeField] private Button liftHipButton;
    [SerializeField] private RectTransform tarpImage;
    [SerializeField] private PatientAnimationController patientController;
    [SerializeField] private float successThreshold = 100f; // ������� �����ؾ� �ϴ� Y ��ġ��

    [Header("Settings")]
    [SerializeField] private float dragSensitivity = 0.01f;
    [SerializeField] private Vector2 initialTarpPosition;
    private bool canDragTarp = false;
    private Vector2[] touchStartPositions = new Vector2[2];
    private float initialTouchDistance;
    private bool isDragging = false;

    private MiniGamePopup miniGamePopup;

    private void Start()
    {
        miniGamePopup = GetComponent<MiniGamePopup>();
        SetupGame();
    }

    private void SetupGame()
    {
        // �ʱ� ��ġ ����
        if (tarpImage != null)
        {
            tarpImage.anchoredPosition = initialTarpPosition;
        }

        // ��ư �̺�Ʈ ����
        if (liftHipButton != null)
        {
            liftHipButton.onClick.AddListener(OnLiftHipButtonClicked);
        }

        // ���� ���۽� ��ư�� Ȱ��ȭ
        canDragTarp = false;
    }

    private void OnLiftHipButtonClicked()
    {
        if (patientController != null)
        {
            patientController.LiftHip();
            canDragTarp = true;
            Invoke("DisableDrag", 0.5f);
        }
    }

    private void Update()
    {
        if (!canDragTarp) return;

        HandleTwoFingerDrag();
    }

    private void HandleTwoFingerDrag()
    {
        // �� �հ��� ��ġ ����
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // �巡�� ����
            if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                StartDrag(touch0, touch1);
            }
            // �巡�� ��
            else if (isDragging && (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved))
            {
                ContinueDrag(touch0, touch1);
            }
            // �巡�� ����
            else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
            {
                EndDrag();
            }
        }
    }

    private void StartDrag(Touch touch0, Touch touch1)
    {
        isDragging = true;
        touchStartPositions[0] = touch0.position;
        touchStartPositions[1] = touch1.position;
        initialTouchDistance = Vector2.Distance(touch0.position, touch1.position);
    }

    private void ContinueDrag(Touch touch0, Touch touch1)
    {
        float currentTouchDistance = Vector2.Distance(touch0.position, touch1.position);
        float dragDistance = currentTouchDistance - initialTouchDistance;

        // ����� �̵�
        Vector2 newPosition = tarpImage.anchoredPosition;
        newPosition.y += dragDistance * dragSensitivity;
        tarpImage.anchoredPosition = newPosition;

        // ���� ���� üũ
        CheckSuccess();
    }

    private void EndDrag()
    {
        isDragging = false;
    }

    private void DisableDrag()
    {
        canDragTarp = false;

        // 0.5�� �ȿ� �������� ���ߴٸ� ���� ó��
        if (!isDragging || tarpImage.anchoredPosition.y < successThreshold)
        {
            miniGamePopup.OnGameFailure();
        }
    }

    private void CheckSuccess()
    {
        if (tarpImage.anchoredPosition.y >= successThreshold)
        {
            // ���� ó��
            canDragTarp = false;
            isDragging = false;
            CancelInvoke("DisableDrag"); // 0.5�� Ÿ�̸� ���
            miniGamePopup.OnGameSuccess();
        }
    }

    private void OnDestroy()
    {
        if (liftHipButton != null)
        {
            liftHipButton.onClick.RemoveAllListeners();
        }
    }
}
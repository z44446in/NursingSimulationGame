using UnityEngine;
using System.Collections;

public class ScreenSizeManager : MonoBehaviour
{
     [Header("Settings")]
    [SerializeField] private float targetAspect = 16f / 9f; // Landscape 기준
    [Tooltip("Safe area가 변경될 때 UI가 부드럽게 이동하는 시간")]
    [SerializeField] private float safeAreaTransitionDuration = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    private Camera mainCamera;
    private RectTransform safeAreaRect;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private ScreenOrientation lastOrientation = ScreenOrientation.Portrait; // Unknown 대신 Portrait로 초기화
    private Vector2Int lastResolution = Vector2Int.zero;
    private Coroutine safeAreaTransitionCoroutine;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found!");
        }

        // 화면 회전 고정
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        

        InitializeScreen();
    }
    private void InitializeScreen()
    {
        UpdateCameraRect();
        UpdateSafeArea(true);
        lastOrientation = Screen.orientation;
        lastResolution = new Vector2Int(Screen.width, Screen.height);
    }

    private void Update()
    {
        // 화면 변경 사항 체크
        if (lastOrientation != Screen.orientation || 
            lastResolution.x != Screen.width || 
            lastResolution.y != Screen.height ||
            lastSafeArea != Screen.safeArea)
        {
            lastOrientation = Screen.orientation;
            lastResolution = new Vector2Int(Screen.width, Screen.height);
            UpdateCameraRect();
            UpdateSafeArea(false);
        }
    }

    private void UpdateCameraRect()
    {
        if (mainCamera == null) return;  // 카메라가 없으면 실행하지 않음

        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;
       

        Rect rect = new Rect(0, 0, 1, 1);

        if (scaleHeight < 1f)
        {
            rect.height = scaleHeight;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            rect.width = scaleWidth;
            rect.x = (1f - scaleWidth) / 2f;
        }

        mainCamera.rect = rect;
    }

    private void UpdateSafeArea(bool immediate)
    {
        if (safeAreaRect == null) return;

        Rect safeArea = Screen.safeArea;
        if (safeArea == lastSafeArea) return;

        // 현재 해상도에 대한 안전 영역 비율 계산
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // 가로 모드에서 anchors 조정
        if (Screen.orientation == ScreenOrientation.LandscapeLeft || 
            Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            float temp = anchorMin.x;
            anchorMin.x = anchorMin.y;
            anchorMin.y = temp;
            temp = anchorMax.x;
            anchorMax.x = anchorMax.y;
            anchorMax.y = temp;
        }

        if (immediate)
        {
            ApplySafeArea(anchorMin, anchorMax);
        }
        else
        {
            if (safeAreaTransitionCoroutine != null)
            {
                StopCoroutine(safeAreaTransitionCoroutine);
            }
            safeAreaTransitionCoroutine = StartCoroutine(AnimateSafeArea(anchorMin, anchorMax));
        }

        lastSafeArea = safeArea;
    }

    private void ApplySafeArea(Vector2 anchorMin, Vector2 anchorMax)
    {
        safeAreaRect.anchorMin = anchorMin;
        safeAreaRect.anchorMax = anchorMax;
        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;
    }

    private IEnumerator AnimateSafeArea(Vector2 targetAnchorMin, Vector2 targetAnchorMax)
    {
        Vector2 startAnchorMin = safeAreaRect.anchorMin;
        Vector2 startAnchorMax = safeAreaRect.anchorMax;
        float elapsedTime = 0f;

        while (elapsedTime < safeAreaTransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / safeAreaTransitionDuration;
            t = Mathf.SmoothStep(0f, 1f, t); // 부드러운 전환을 위한 보간

            safeAreaRect.anchorMin = Vector2.Lerp(startAnchorMin, targetAnchorMin, t);
            safeAreaRect.anchorMax = Vector2.Lerp(startAnchorMax, targetAnchorMax, t);
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;

            yield return null;
        }

        ApplySafeArea(targetAnchorMin, targetAnchorMax);
        safeAreaTransitionCoroutine = null;
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateCameraRect();
        UpdateSafeArea(false);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!showDebugGizmos) return;

        // 디버그 정보 표시
        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label($"Screen Resolution: {Screen.width}x{Screen.height}");
        GUILayout.Label($"Safe Area: {Screen.safeArea}");
        GUILayout.Label($"Orientation: {Screen.orientation}");
        GUILayout.EndArea();
    }
#endif
}
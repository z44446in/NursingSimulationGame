using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Camera))]
public class ScreenSizeManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float targetAspect = 16f / 9f;
    [SerializeField] private float safeAreaTransitionDuration = 0.3f;

    [Header("References")]
    [SerializeField] private RectTransform safeAreaRect;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    private Camera mainCamera;
    private Rect lastSafeArea;
    private ScreenOrientation lastOrientation;
    private Vector2Int lastResolution;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera not found!");
        }

        Screen.orientation = ScreenOrientation.LandscapeLeft;
        InitializeScreen();
    }

    private void InitializeScreen()
    {
        UpdateCameraRect();
        UpdateSafeArea(true);

        lastOrientation = Screen.orientation;
        lastResolution = new Vector2Int(Screen.width, Screen.height);
        lastSafeArea = Screen.safeArea;
    }

    private void Update()
    {
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
        if (mainCamera == null) return;

        float windowAspect = (float)Screen.width / Screen.height;
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

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        if (immediate)
        {
            ApplySafeArea(anchorMin, anchorMax);
        }
        else
        {
            AnimateSafeArea(anchorMin, anchorMax);
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

    private void AnimateSafeArea(Vector2 targetAnchorMin, Vector2 targetAnchorMax)
    {
        if (safeAreaRect == null) return;

        DOTween.Kill(safeAreaRect);

        safeAreaRect.DOAnchorMin(targetAnchorMin, safeAreaTransitionDuration).SetEase(Ease.OutQuad);
        safeAreaRect.DOAnchorMax(targetAnchorMax, safeAreaTransitionDuration).SetEase(Ease.OutQuad);

        safeAreaRect.offsetMin = Vector2.zero;
        safeAreaRect.offsetMax = Vector2.zero;
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!showDebugGizmos) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 100));
        GUILayout.Label($"Screen Resolution: {Screen.width}x{Screen.height}");
        GUILayout.Label($"Safe Area: {Screen.safeArea}");
        GUILayout.Label($"Orientation: {Screen.orientation}");
        GUILayout.EndArea();
    }
#endif
}

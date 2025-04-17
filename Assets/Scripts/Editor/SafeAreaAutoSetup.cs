using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SafeAreaAutoSetup : EditorWindow
{
    [MenuItem("Tools/🛠 SafeArea 자동 세팅")]
    public static void ShowWindow()
    {
        GetWindow<SafeAreaAutoSetup>("SafeArea Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("씬 내 모든 Canvas에 SafeAreaRoot 생성", EditorStyles.boldLabel);
        if (GUILayout.Button("SafeAreaRoot 자동 생성 및 연결"))
        {
            SetupAllCanvases();
        }
    }

    private void SetupAllCanvases()
    {
        var canvases = FindObjectsOfType<Canvas>();
        int setupCount = 0;

        foreach (var canvas in canvases)
        {
            // 이미 있는지 확인
            var existing = canvas.transform.Find("SafeAreaRoot");
            RectTransform safeAreaRoot;

            if (existing == null)
            {
                // 생성
                GameObject root = new GameObject("SafeAreaRoot", typeof(RectTransform));
                safeAreaRoot = root.GetComponent<RectTransform>();
                root.transform.SetParent(canvas.transform, false);

                // 기존 UI 자식들 옮기기
                for (int i = canvas.transform.childCount - 2; i >= 0; i--) // SafeAreaRoot 제외
                {
                    Transform child = canvas.transform.GetChild(i);
                    child.SetParent(safeAreaRoot, true);
                }

                // full stretch
                safeAreaRoot.anchorMin = Vector2.zero;
                safeAreaRoot.anchorMax = Vector2.one;
                safeAreaRoot.offsetMin = Vector2.zero;
                safeAreaRoot.offsetMax = Vector2.zero;

                setupCount++;
                Debug.Log($"[SafeAreaSetup] '{canvas.name}'에 SafeAreaRoot 생성 완료");
            }
            else
            {
                safeAreaRoot = existing.GetComponent<RectTransform>();
            }

            // ScreenSizeManager 연결
            var manager = canvas.GetComponent<ScreenSizeManager>();
            if (manager != null)
            {
                SerializedObject so = new SerializedObject(manager);
                so.FindProperty("safeAreaRect").objectReferenceValue = safeAreaRoot;
                so.ApplyModifiedProperties();

                Debug.Log($"[SafeAreaSetup] '{canvas.name}'의 ScreenSizeManager에 SafeAreaRoot 연결됨");
            }
        }

        EditorUtility.DisplayDialog("완료!", $"{setupCount}개의 SafeAreaRoot가 생성되었습니다.", "OK");
    }
}

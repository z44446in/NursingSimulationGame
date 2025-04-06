using UnityEngine;
using cakeslice;

public class OutlineHighlighter : MonoBehaviour
{
    [ContextMenu("Highlight All Except Tag '제외'")]
    public void HighlightAll()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // 비활성 오브젝트 무시
            if (!obj.activeInHierarchy)
                continue;

            // "제외" 태그 무시
            if (obj.CompareTag("제외"))
                continue;

            // Renderer가 없는 경우 무시
            if (obj.GetComponent<Renderer>() == null)
                continue;

            // 이미 Outline이 있으면 재활성화
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
            {
                outline = obj.AddComponent<Outline>();
            }

            outline.enabled = true;
        }
    }
}

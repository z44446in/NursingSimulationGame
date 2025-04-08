using UnityEngine;
using cakeslice;

public class OutlineHighlighter : MonoBehaviour
{
    [ContextMenu("Highlight All Except Tag '����'")]
    public void HighlightAll()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // ��Ȱ�� ������Ʈ ����
            if (!obj.activeInHierarchy)
                continue;

            // "����" �±� ����
            if (obj.CompareTag("����"))
                continue;

            // Renderer�� ���� ��� ����
            if (obj.GetComponent<Renderer>() == null)
                continue;

            // �̹� Outline�� ������ ��Ȱ��ȭ
            Outline outline = obj.GetComponent<Outline>();
            if (outline == null)
            {
                outline = obj.AddComponent<Outline>();
            }

            outline.enabled = true;
        }
    }
}

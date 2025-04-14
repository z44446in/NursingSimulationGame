using UnityEngine;
using UnityEngine.UI;

public class OutlineEffectController : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial; // �ʰ� ���� outline shader ���
    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        img.material = null; // ���� �ÿ� ��Ȱ��ȭ
    }

    void Update()
    {
        // ���� ����: ���콺 �ö��� �� outline ���̱�
        if (IsMouseOver())
        {
            img.material = outlineMaterial;
        }
        else
        {
            img.material = null;
        }
    }

    bool IsMouseOver()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            Input.mousePosition,
            null
        );
    }
}

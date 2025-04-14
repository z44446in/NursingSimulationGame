using UnityEngine;
using UnityEngine.UI;

public class OutlineEffectController : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial; // 너가 만든 outline shader 사용
    private Image img;

    void Start()
    {
        img = GetComponent<Image>();
        img.material = null; // 시작 시엔 비활성화
    }

    void Update()
    {
        // 예시 조건: 마우스 올라갔을 때 outline 보이기
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

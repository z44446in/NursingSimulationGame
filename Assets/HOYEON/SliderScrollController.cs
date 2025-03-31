using UnityEngine;
using UnityEngine.UI;

public class SliderScrollController : MonoBehaviour
{
    public ScrollRect scrollRect; // Scroll View의 ScrollRect
    public Slider slider;        // 사용자 정의 Slider

    private void Start()
    {
        // Slider의 초기값을 Scroll Rect와 동기화 (반대로 설정)
        slider.value = 1 - scrollRect.verticalNormalizedPosition;

        // Slider 값 변경 시 Scroll Rect 동기화
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        // Slider의 값과 Scroll Rect의 스크롤 위치를 반대로 설정
        scrollRect.verticalNormalizedPosition = 1 - value;
    }

    private void Update()
    {
        // Scroll Rect의 현재 스크롤 위치를 Slider에 동기화
        slider.value = 1 - scrollRect.verticalNormalizedPosition;
    }
}
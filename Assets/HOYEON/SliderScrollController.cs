using UnityEngine;
using UnityEngine.UI;

public class SliderScrollController : MonoBehaviour
{
    public ScrollRect scrollRect; // Scroll View�� ScrollRect
    public Slider slider;        // ����� ���� Slider

    private void Start()
    {
        // Slider�� �ʱⰪ�� Scroll Rect�� ����ȭ (�ݴ�� ����)
        slider.value = 1 - scrollRect.verticalNormalizedPosition;

        // Slider �� ���� �� Scroll Rect ����ȭ
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        // Slider�� ���� Scroll Rect�� ��ũ�� ��ġ�� �ݴ�� ����
        scrollRect.verticalNormalizedPosition = 1 - value;
    }

    private void Update()
    {
        // Scroll Rect�� ���� ��ũ�� ��ġ�� Slider�� ����ȭ
        slider.value = 1 - scrollRect.verticalNormalizedPosition;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;

    private void Start()
    {
        // �ʱ� ���� ����
        volumeSlider.value = AudioListener.volume;
    }

    public void OnVolumeChanged()
    {
        // ���� ����
        AudioListener.volume = volumeSlider.value;
    }
}
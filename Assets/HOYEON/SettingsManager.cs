using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;

    private void Start()
    {
        // ÃÊ±â º¼·ý ¼³Á¤
        volumeSlider.value = AudioListener.volume;
    }

    public void OnVolumeChanged()
    {
        // º¼·ý º¯°æ
        AudioListener.volume = volumeSlider.value;
    }
}
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioSource audioSource;

    private void Start()
    {
        // AudioManager가 유지되도록 설정
        DontDestroyOnLoad(audioSource.gameObject);
        
        // 슬라이더 초기값 설정
        volumeSlider.value = audioSource.volume;
        // 슬라이더 값 변경 이벤트 설정
        volumeSlider.onValueChanged.AddListener(delegate { OnVolumeChanged(); });
    }

    public void OnVolumeChanged()
    {
        // AudioSource의 볼륨을 슬라이더 값으로 설정
        audioSource.volume = volumeSlider.value;
    }
}
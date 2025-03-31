using UnityEngine;
using UnityEngine.UI;

public class AchievementController : MonoBehaviour
{
    [System.Serializable]
    public class Achievement
    {
        public string name;       // Achievement 이름
        public Sprite unlockedImage; // Achievement를 달성했을 때 표시할 이미지
        public bool isUnlocked;  // 달성 여부
        public Image slot;       // 해당 Achievement 슬롯
    }

    public Achievement[] achievements; // 모든 Achievement 리스트

    public void UnlockAchievement(int index)
    {
        if (index < 0 || index >= achievements.Length) return; // 유효성 검사

        // Achievement를 달성
        if (!achievements[index].isUnlocked)
        {
            achievements[index].isUnlocked = true; // 상태 변경
            achievements[index].slot.sprite = achievements[index].unlockedImage; // 이미지 변경
            achievements[index].slot.color = Color.white; // 이미지 표시
        }
    }
}
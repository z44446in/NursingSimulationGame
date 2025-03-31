using UnityEngine;
using UnityEngine.UI;

public class AchievementController : MonoBehaviour
{
    [System.Serializable]
    public class Achievement
    {
        public string name;       // Achievement �̸�
        public Sprite unlockedImage; // Achievement�� �޼����� �� ǥ���� �̹���
        public bool isUnlocked;  // �޼� ����
        public Image slot;       // �ش� Achievement ����
    }

    public Achievement[] achievements; // ��� Achievement ����Ʈ

    public void UnlockAchievement(int index)
    {
        if (index < 0 || index >= achievements.Length) return; // ��ȿ�� �˻�

        // Achievement�� �޼�
        if (!achievements[index].isUnlocked)
        {
            achievements[index].isUnlocked = true; // ���� ����
            achievements[index].slot.sprite = achievements[index].unlockedImage; // �̹��� ����
            achievements[index].slot.color = Color.white; // �̹��� ǥ��
        }
    }
}
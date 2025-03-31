using UnityEngine;
using UnityEngine.UI;

public class PatientAnimationController : MonoBehaviour
{
    [SerializeField] private Image patientImage;
    [SerializeField] private Sprite normalPoseSprite;  // �⺻ ���� �ڼ�
    [SerializeField] private Sprite liftedHipPoseSprite;  // ������ �� �ڼ�

    private void Start()
    {
        if (patientImage != null)
        {
            patientImage.sprite = normalPoseSprite;
        }
    }

    public void LiftHip()
    {
        if (patientImage != null && liftedHipPoseSprite != null)
        {
            patientImage.sprite = liftedHipPoseSprite;
            Invoke("ResetPose", 0.5f);
        }
    }

    private void ResetPose()
    {
        if (patientImage != null && normalPoseSprite != null)
        {
            patientImage.sprite = normalPoseSprite;
        }
    }
}
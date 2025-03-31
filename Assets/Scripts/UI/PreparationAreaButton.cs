using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PreparationAreaButton : MonoBehaviour
{
    [SerializeField] private PreparationAreaType areaType;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        
    }

    private void OnButtonClicked()
    {
        if (PreparationManager.Instance == null)
        {
            Debug.LogError("PreparationManager instance is null!");
            return;
        }
        
        
        PreparationManager.Instance.OnAreaClicked(areaType);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems; // 이거 추가!

public class MainMenu : MonoBehaviour
{
    public void OpenPanel(GameObject nextPanel)
    {
        nextPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        // 현재 클릭된 버튼을 찾고
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        if(clickedButton != null) 
        {
            // 버튼의 부모(패널)을 찾아서 닫기
            clickedButton.transform.parent.gameObject.SetActive(false);
            Debug.Log("패널이 닫혔습니다");
        }
    }

    public void SwitchPanel(GameObject nextPanel)
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        if(clickedButton != null)
        {
            clickedButton.transform.parent.gameObject.SetActive(false);
            nextPanel.SetActive(true);
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("MainGame");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is exiting");
    }
}
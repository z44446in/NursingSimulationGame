using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    public static DialogueManager Instance => instance;

    [Header("UI Prefabs")]
    [SerializeField] private GameObject smallDialoguePrefab;
    [SerializeField] private GameObject largeDialoguePrefab;
    [SerializeField] private Transform dialogueParent;

    private GameObject currentSmallDialogue;
    private GameObject currentLargeDialogue;
    private Coroutine autoCloseCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowSmallDialogue(string message, bool autoClose = true, Action onClose = null)
    {
        // 이미 떠있는 다이얼로그가 있다면 닫기
        CloseSmallDialogue();

        if (smallDialoguePrefab == null || dialogueParent == null)
        {
            Debug.LogError("Small dialogue prefab or parent is null");
            return;
        }

        currentSmallDialogue = Instantiate(smallDialoguePrefab, dialogueParent);
        SmallPopup smallPopup = currentSmallDialogue.GetComponent<SmallPopup>();
        
        if (smallPopup != null)
        {
            smallPopup.Initialize(message, autoClose, onClose);
            
            if (autoClose)
            {
                autoCloseCoroutine = StartCoroutine(AutoCloseDialogue());
            }
        }
        else
        {
            Debug.LogError("Failed to get SmallPopup component");
            Destroy(currentSmallDialogue);
        }
    }

    public void ShowLargeDialogue(string message, Action onClose = null)
    {
        // 이미 떠있는 다이얼로그가 있다면 닫기
        CloseLargeDialogue();

        if (largeDialoguePrefab == null || dialogueParent == null)
        {
            Debug.LogError("Large dialogue prefab or parent is null");
            return;
        }

        currentLargeDialogue = Instantiate(largeDialoguePrefab, dialogueParent);
        SmallPopup largePopup = currentLargeDialogue.GetComponent<SmallPopup>();
        
        if (largePopup != null)
        {
            largePopup.Initialize(message, false, onClose);
        }
        else
        {
            Debug.LogError("Failed to get SmallPopup component for large dialogue");
            Destroy(currentLargeDialogue);
        }
    }

    public void CloseSmallDialogue()
    {
        if (currentSmallDialogue != null)
        {
            Destroy(currentSmallDialogue);
            currentSmallDialogue = null;
        }

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    public void CloseLargeDialogue()
    {
        if (currentLargeDialogue != null)
        {
            Destroy(currentLargeDialogue);
            currentLargeDialogue = null;
        }
    }

    private IEnumerator AutoCloseDialogue()
    {
        yield return new WaitForSeconds(3f);
        CloseSmallDialogue();
    }

    private void OnDestroy()
    {
        CloseSmallDialogue();
        CloseLargeDialogue();
    }
}
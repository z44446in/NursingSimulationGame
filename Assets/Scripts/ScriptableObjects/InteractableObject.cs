using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    public bool isInteractable = true;
    public UnityEvent onInteract;
    
    private void OnMouseEnter()
    {
        if (isInteractable)
        {
            InteractionManager.Instance.HighlightObject(gameObject, true);
        }
    }

    private void OnMouseExit()
    {
        if (isInteractable)
        {
            InteractionManager.Instance.HighlightObject(gameObject, false);
        }
    }

    private void OnMouseDown()
    {
        if (isInteractable)
        {
            onInteract?.Invoke();
        }
    }
}
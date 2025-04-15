using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CentralOutlineManager : MonoBehaviour
{
    public Material outlineMat;
    public float blinkInterval = 0.5f;
    private List<Image> targetImages = new();
    private Coroutine blinkingCoroutine;
    private bool isStaticOutlineActive = false;

    void Start()
    {
        GameObject[] objs = FindObjectsOfType<GameObject>();
        foreach (var obj in objs)
        {
            if (obj.layer == LayerMask.NameToLayer("Water"))
            {
                var img = obj.GetComponent<Image>();
                if (img != null)
                    targetImages.Add(img);
            }
        }
    }

    public void StartBlinking()
    {
        if (blinkingCoroutine == null)
            blinkingCoroutine = StartCoroutine(BlinkRoutine());
    }

    public void StopBlinking()
    {
        if (blinkingCoroutine != null)
        {
            StopCoroutine(blinkingCoroutine);
            blinkingCoroutine = null;

            foreach (var img in targetImages)
                img.material = null;
        }
    }
    public void StartOutline()
    {
        foreach (var img in targetImages)
            img.material = outlineMat;
            
        isStaticOutlineActive = true;

        
    }

    public void StopOutline()
    {
        if (isStaticOutlineActive)
        {
            foreach (var img in targetImages)
                img.material = null;
                
            isStaticOutlineActive = false;
        }
    }
    private IEnumerator BlinkRoutine()
    {
        bool on = true;

        while (true)
        {
            foreach (var img in targetImages)
                img.material = on ? outlineMat : null;

            on = !on;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}

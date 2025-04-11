using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlithTest : MonoBehaviour
{
    [SerializeField] public GameObject cartobjec;

    // Start is called before the first frame update
    void Start()
    {

        cakeslice.Outline outline = cartobjec.GetComponent<cakeslice.Outline>();
        outline.goHigh();
    }

    // Update is called once per frame
    void Update()
    {
        cakeslice.Outline outline = cartobjec.GetComponent<cakeslice.Outline>();
        outline.enabled = true;
    }
}

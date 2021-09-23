using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    // Materials for Shaders
    public Material defaultMaterial;
    public Material spriteOutlineMaterial;

    public void OnMouseEnter()
    {
        GetComponent<Renderer>().material = spriteOutlineMaterial;
    }
    public void OnMouseExit()
    {
        GetComponent<Renderer>().material = defaultMaterial;
    }

    void OnDestroy()
    {
        GetComponent<Renderer>().material = defaultMaterial;
    }
}

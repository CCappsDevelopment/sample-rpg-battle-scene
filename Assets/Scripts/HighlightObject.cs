using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    // Materials for Shaders
    public Material defaultMaterial;
    public Material spriteOutlineMaterial;

    void OnMouseEnter()
    {
        GetComponent<Renderer>().material = spriteOutlineMaterial;
    }
    void OnMouseExit()
    {
        GetComponent<Renderer>().material = defaultMaterial;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    // Materials for Shaders
    public Material defaultMaterial;
    public Material spriteOutlineMaterial;

    // Highlight sprite with outline on mouseover
    public void OnMouseEnter()
    {
        SetMaterial(spriteOutlineMaterial);
    }

    // Remove highlighed outline when mouseover ends
    public void OnMouseExit()
    {
        SetMaterial(defaultMaterial);
    }

    // Reset object's material when this script is removed
    public void OnDestroy()
    {
        SetMaterial(defaultMaterial);
    }

    // Set Material on the Renderer Component on attached object.
    private void SetMaterial(Material _material)
    {
        GetComponent<Renderer>().material = _material;
    }
}

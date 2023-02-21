using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowThresholdCustomEffect5 : MonoBehaviour
{
    public Material shadowMaterial;
    public Color shadowColor;

    [Range(0,0.5f)]
    public float shadowThreshold = 0.5f;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        shadowMaterial.SetFloat("_ShadowThreshold", shadowThreshold);
        shadowMaterial.SetColor("_ShadowColor", shadowColor);
        Graphics.Blit(source, destination, shadowMaterial);
    }
}

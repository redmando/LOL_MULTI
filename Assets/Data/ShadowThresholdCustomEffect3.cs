using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowThresholdCustomEffect3 : MonoBehaviour
{
    public Material shadowMaterial;

    [Range(0,0.5f)]
    public float shadowThreshold;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        shadowMaterial.SetFloat("_ShadowThreshold", shadowThreshold);
        Graphics.Blit(source, destination, shadowMaterial);
    }
}

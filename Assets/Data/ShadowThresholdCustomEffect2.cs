using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowThresholdCustomEffect2 : MonoBehaviour
{
    public Material shadowMaterial;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, shadowMaterial);
    }
}

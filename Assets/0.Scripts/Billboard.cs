using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera cameraToLookAt;

    private void Start()
    {
        cameraToLookAt = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        //transform.LookAt(Camera.main.transform.position);
        transform.LookAt(transform.position + cameraToLookAt.transform.rotation * Vector3.back,
            cameraToLookAt.transform.rotation * Vector3.up);
    }
}

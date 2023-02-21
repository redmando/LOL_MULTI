using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bilboard : MonoBehaviour
{
    public Camera CameraToLookAt;
    private void Start()
    {
        CameraToLookAt = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }
    private void LateUpdate()
    {
        //transform.LookAt(Camera.main.transform.position);
        transform.LookAt(transform.position + CameraToLookAt.transform.rotation * Vector3.back,
            CameraToLookAt.transform.rotation * Vector3.up);


    }

}

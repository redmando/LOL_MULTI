using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        transform.LookAt(GameObject.Find("BillboardTarget").GetComponent<Transform>());
    }
}

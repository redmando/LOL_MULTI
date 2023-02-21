using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_Distance : MonoBehaviour
{
    public Transform pos1;
    public Transform pos2;
    public bool isWriteLog;

    private void Update()
    {
        if(isWriteLog)
        {
            Debug.Log(Vector3.Distance(pos1.position, pos2.position));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CYC_Skill_W : MonoBehaviour
{
    public string allianceTag;
    public int useCount = 5;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == allianceTag)
        {
            other.gameObject.SendMessage("Damage", -200.0f);
            useCount--;
            if (useCount <= 0) { Destroy(gameObject); }
        }
    }
}

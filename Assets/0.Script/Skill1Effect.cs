using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill1Effect : MonoBehaviour
{
    public Transform target;
    public string playerTag;
    private float damage = 50.0f;

    private void Start()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(playerTag == "Top_Unit" &&  other.tag == "Bottom_Unit")
        {
         other.SendMessage("Damage", damage);       
        }
        else if(playerTag == "Bottom_Unit" && other.tag == "Top_Unit")
        {
            other.SendMessage("Damage", damage);
        }

        Destroy(this.gameObject, 3.0f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill3Effect : MonoBehaviour
{
    public Transform target;
    public string playerTag;
    private float damage = 100.0f;

    private void Start()
    {
        Destroy(this.gameObject, 5.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (playerTag == "Top_Unit" && other.tag == "Bottom_Unit")
        {
            other.SendMessage("Damage", damage);
        }
        else if (playerTag == "Bottom_Unit" && other.tag == "Top_Unit")
        {
            other.SendMessage("Damage", damage);
        }      
    }
    
}

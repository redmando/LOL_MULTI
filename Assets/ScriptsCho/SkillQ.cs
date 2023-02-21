using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillQ : MonoBehaviour
{
    public float fAttackDamage = 40.0f;
    public Transform TargetTr;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        TargetTr.SendMessage("QDamage", fAttackDamage);
    }
}

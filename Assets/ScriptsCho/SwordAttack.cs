using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAttack : MonoBehaviour
{
    public float fAttackDamage = 40.0f;

    private float dis;
    private float speed;
    private float wateTime;
    GameObject sword;

    Transform Tr;

    public Transform TargetTr;

    private float dist;
    // Start is called before the first frame update
    void Start()
    {
        Tr = GetComponent<Transform>();
        if (TargetTr == null)
        {
            return;
        }
    }
    // Update is called once per frame
    void Update()
    {
        TargetTr.SendMessage("Damage", fAttackDamage);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_State : MonoBehaviour
{
    public bool isDead;
    public float HP;
    public float MP;

    private float currentHP;
    private float currentMP;

    private void Start()
    {
        currentHP = HP;
        currentMP = MP;
    }

    private void FixedUpdate()
    {
        if (currentHP <= 0)
        {
            isDead = true;
        }
    }
}

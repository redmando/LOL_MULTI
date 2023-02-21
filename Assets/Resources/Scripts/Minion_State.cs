using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Minion_State : MonoBehaviour
{
    [Header("State")]
    public bool isDead;
    public float HP;
    public float damage;
    public float range;
    public float speed;
    public int attackType;

    [Header("UI")]
    [SerializeField]
    private Image Hpbar;

    private Animator anim;
    private Collider col;

    private float currentHP;
    private float timer;

    private void Start()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();

        anim.SetInteger("AttackType", attackType);
        currentHP = HP;
    }


    private void FixedUpdate()
    {
        StateCheck();
        UIUpdate();
    }

    private void StateCheck()
    {
        if (currentHP <= 0)
        {
            if (isDead == false)
            {
                anim.SetTrigger("IsDead");
                col.enabled = false;
                isDead = true;
            }
            else
            {
                timer += Time.deltaTime;

                if(timer >= 5.0f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void UIUpdate()
    {
        Hpbar.fillAmount = currentHP / HP;
    }

    private void Damaged(float _damage)
    {
        currentHP -= _damage;
    }
}

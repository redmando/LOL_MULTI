using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Building_State : MonoBehaviour
{
    [Header("State")]
    public bool isDestroyed;
    public float HP = 1000;
    public float currentHP;

    [Header("UI")]
    [SerializeField]
    private Image Hpbar;

    private void Start()
    {
        currentHP = HP;
    }

    private void FixedUpdate()
    {
        StateCheck();
        UIUpdate();
    }

    private void StateCheck()
    {
        if (HP <= 0)
        {
            isDestroyed = true;
        }
    }

    private void UIUpdate()
    {
        Hpbar.fillAmount = currentHP / HP;
    }

    private void Recover()
    {
        currentHP = HP;
        isDestroyed = false;
    }

    private void Damaged(float _damage)
    {
        currentHP -= _damage;
    }
}

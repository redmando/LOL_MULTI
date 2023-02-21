using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerSkill : MonoBehaviour
{
    public Transform skill1Position;
    public Transform skill2Position;
    public Transform skill3Position;
    public Rigidbody magicBall;
    public GameObject earthShatter;
    public GameObject dustExplosion;
    public NavMeshAgent playerNav;
    private float shootSpeed = 500.0f;
    private float shootForce;
    private float currentSpeed;



    private void Start()
    {
        shootForce = shootSpeed * Time.deltaTime;
    }

    public void Skill1()
    {
        Rigidbody instanceMagicBall = Instantiate(magicBall, skill1Position.position, Quaternion.identity);

        instanceMagicBall.velocity = shootForce * skill1Position.forward;
    }

    public void Skill2()
    {
        Instantiate(dustExplosion, skill2Position.position, Quaternion.identity);

        currentSpeed = playerNav.speed;
        float buffUseSpeed = currentSpeed * 1.5f;

        playerNav.speed = buffUseSpeed;

        Invoke("ReturnSpeed", 8.0f);
    }

    public void ReturnSpeed()
    {
        playerNav.speed = currentSpeed;
    }

    public void Skill3()
    {
        Instantiate(earthShatter, skill3Position.position, Quaternion.identity);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_W : MonoBehaviour
{
    public GameObject myCharacter;
    public ParticleSystem me;
    public string sTargetUnitTag;
    public float DrainHP = 0;
    List<ParticleCollisionEvent> Hit;

    private void Start()
    {
        me = this.GetComponent<ParticleSystem>();
        Hit = new List<ParticleCollisionEvent>();
        sTargetUnitTag = myCharacter.GetComponent<Vampire_CYH>().sAttackUnitTag;
    }
    private void OnParticleCollision(GameObject other)
    {
        if(other.GetComponent<DeadState>() && other.CompareTag(sTargetUnitTag))
        {
            ParticlePhysicsExtensions.GetCollisionEvents(me, other, Hit);
            for(int i = 0; i < Hit.Count; i++)
            {
                other.transform.SendMessage("Damage", 20.0f);
                DrainHP += 6.0f;
            }
        }
    }
    private void Update()
    {
        if (!me.isPlaying)
        {
            CallDrain();
            myCharacter.GetComponent<Vampire_CYH>().StartCoroutine(myCharacter.GetComponent<Vampire_CYH>().Skill_CoolTime(8.0f));
            gameObject.SetActive(false);
        }
    }
    public void CallDrain()
    {
        Debug.Log(DrainHP);
        myCharacter.GetComponent<Vampire_CYH>().Drain(DrainHP);
        DrainHP = 0;
        Hit = null;
    }

}

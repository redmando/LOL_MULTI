using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Q : MonoBehaviour
{
    public float fAttackDamage = 120.0f;

    private float speed = 20.0f;
    private float waitTime;
    public Transform Tr;
    public string sTargetUnitTag;
    private void Start()
    {
        Tr = GetComponent<Transform>();
        StartCoroutine(this.TimeDestroy());
    }

    IEnumerator TimeDestroy()
    {
        yield return new WaitForSeconds(0.5f); // 기본 1.5f
        Destroy(gameObject);
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Q스킬 충돌 감지!");
        if (collision.gameObject.GetComponent<DeadState>() != null && collision.gameObject.CompareTag(sTargetUnitTag))
        {
            collision.transform.SendMessage("Damage", fAttackDamage);
            Debug.Log("스킬 명중!!");
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

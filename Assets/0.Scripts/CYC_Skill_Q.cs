using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CYC_Skill_Q : MonoBehaviour
{
    public Vector3 currPos; // 현재위치
    public Vector3 dir;     // 방향

    public string enemyTag;

    private Vector3 moveDir;
    private void Start()
    {
        currPos = transform.position;
        StartCoroutine(skillWork());
        moveDir = dir - currPos;
        moveDir = Vector3.Normalize(moveDir);
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == enemyTag)
        {
            other.gameObject.SendMessage("Damage", 100.0f);
        }
    }


    IEnumerator skillWork()
    {
        yield return new WaitForSeconds(0.2f);
        while (true)
        {
            //transform.position += moveDir;
            transform.Translate(moveDir);
            yield return null;

            if(Vector3.Distance(currPos, this.transform.position) >= 10.0f) { break; }
        }
        Destroy(gameObject);
        //yield return null;
    }
}

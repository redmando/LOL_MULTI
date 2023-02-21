using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class CYC_Skill_E : MonoBehaviour
{
    public string enemyTag;
    GameObject objOther;// ★★★★★나중에 배열로 교체
    private void Start()
    {
        StartCoroutine(del());
    }
    IEnumerator del()
    {
        yield return new WaitForSeconds(10.0f);
        objOther.GetComponent<NavMeshAgent>().speed = 7.0f;// ★★★★★나중에 배열로 교체
        Destroy(gameObject);
    }
    void SlowRecovery(GameObject gameObject)
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        objOther = other.gameObject; // ★★★★★나중에 배열로 교체
        if (other.gameObject.tag == enemyTag)
        { other.gameObject.GetComponent<NavMeshAgent>().speed = 3.0f; }
    }
    
    private void OnTriggerExit(Collider other)
    {
        other.gameObject.GetComponent<NavMeshAgent>().speed = 7.0f;
    }
}

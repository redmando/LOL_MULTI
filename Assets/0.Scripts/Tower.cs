using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public GameObject objMissile;
    public Transform tsfMissile;        //  미사일의 발사 위치

    [SerializeField]
    Transform tsfTarget;                // 공격 목표.

    public string sAttackUnitTag;       //  Top_Unit, Bottom_Unit

    public float fSearchDist = 15.0f;   // 적 검색 거리..(터렛 사정 거리)

    private float dist;
    private float minimunDist = 20.0f;
    GameObject objTarget;

    bool bColl;     // 접근하는 적이 있는지 체크. 

    void Start()
    {
        StartCoroutine(this.State());
    }

    IEnumerator State()
    {
        while(true)
        {
            yield return new WaitForSeconds(1.0f);  // 지연 시간.

            NearEnemyAttack(transform.position, fSearchDist); // 적을 찾음. 근처에 적이 있는지.
        }
    }

    // 반지름의 거리 안에 적이 있는지 파악.
    public void NearEnemyAttack(Vector3 pos, float radius)
    {
        Collider[] colls = Physics.OverlapSphere(pos, radius);

        minimunDist = radius;

        bColl = false;

        objTarget = null;   // 목표 오브젝트 초기화.
        tsfTarget = null;   // 이동 관련 목표도 초기화.

        for(int i = 0; i < colls.Length; ++i)   //  자기 자신 포함.
        {
            if( colls[i].tag == sAttackUnitTag
                && colls[i].GetComponent<DeadState>().bDead == false )  // 적 유닛이면서 살아있으면..
            {
                Vector3 objectPos = colls[i].transform.position;
                dist = Vector3.Distance(objectPos, transform.position);

                if(minimunDist > dist)
                {
                    objTarget = colls[i].gameObject;
                    minimunDist = dist;
                    bColl = true;
                }
            }
        }

        // 타겟팅 한 놈을 따라간다. 
        if(bColl)
        {
            // 적이 발견되면 미사일 발사.
            GameObject obj = Instantiate(objMissile, tsfMissile.position, tsfMissile.rotation);
            obj.GetComponent<MissileMove>().TargetTr = objTarget.transform;
            //obj.GetComponent<MissileMove>().Tr = tsfMissile;
        }
    }

}

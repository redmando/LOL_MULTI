using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;

public class Minions2 : MonoBehaviourPunCallbacks, IPunObservable
{
    Animator ani;
    NavMeshAgent nav;
    private Transform tr;

    [SerializeField]
    Transform tsfMinion;        //  미니언의 위치
    [SerializeField]
    Transform[] tsfTarget;      // 이동 목표의 배열화 (웨이포인트 배열)
    [SerializeField]
    Transform tsfTargetNow;     //  현재 이동 목표

    public int nTargetCount;    //  이동 목표 지점의 카운트..(웨이포인트 배열 카운트)

    // ///////////////////////////////////////
    public string[] sTargetName;        //  Waypoint의 이름.(문자로 저장)
    public string sTargetNameNow;       //  현재 추적중인 타겟 이름.

    public string sAttackUnitTag;       //  Top_Unit    , Bottom_Unit
    public string sAttackBuildingTag;   //  Top_Building, Bottom_Building

    public enum minState { idle, trace };       //  미니언 네비게이션 상태.
    public minState curState = minState.trace;  // 상태 초기화.

    public float traceDist = 5.0f;      // 추적 도달 거리.

    public float fSearchDist = 15.0f;           //  = 15.0f;  // 적 검색 거리...
    public float attackDist = 3.0f;             //  = 3.0f    // 공격 도달 거리
    public float attackBuildingDist = 15.0f;    //  = 15.0f;  // 공격 도달 거리..

    bool bState = true;         //   처음 거리 측정용. 최종 목적지 도착 한번 체크용.   

    public bool isDie = false;  //  미니언이 죽었는지 확인.

    public int nCountHp;        // HP 조절용,.


    GameObject objTarget;       //  목표 게임 오브젝트.(적 건물, 적 유닛, 이동 위치)

    public Image hpBar;

    public int nPhotonViewId;   //  목표가 되는 오브젝트의 포톤 ID

    private float currHP = 100.0f;  // HP
    private float initHP = 100.0f;

    public float fAttackDamage = 20.0f;     // 공격시 데미지 수치.   

    public GameObject objMissile;       // 미사일 프리팹.
    public Transform tsfMissile;        // 미사일 발사위치(지팡이 끝)

    private void Awake()
    {
        // 처음 이동 지정위치를 선정
        sTargetNameNow = sTargetName[0];

        // 이동위치의 초기화
        for (int i = 0; i < tsfTarget.Length; i++)
        {
            tsfTarget[i] = GameObject.Find(sTargetName[i]).GetComponent<Transform>();
        }

        // 처음 목표 지점으로 세팅
        tsfTargetNow = tsfTarget[0];

        // 초기화
        tsfMinion = GetComponent<Transform>();
        ani = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        tr = GetComponent<Transform>();
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            StartCoroutine(this.State());  // 이동 & 추적 상태 확인..
        }
    }

    // 목표한 적이 가까우면 정지 상태로 변경. 아니면 추격.
    IEnumerator State()
    {
        while (!isDie)   // 안죽었을때..
        {

            yield return new WaitForSeconds(0.1f);    // 지연시간.

            // 적을 찾는 부분
            NearEnemyAttack(tsfMinion.position, fSearchDist);       // 적을 찾음, 없으면 웨이포인트 이동.

            // 공격 하는 부분(건물인경우, 유닛인경우)
            // 상태 변경.
            float dist = Vector3.Distance(tsfTargetNow.position, tsfMinion.position);

            // 만약 건물 이라면.. 사정거리를 길게 체크.
            if (tsfTargetNow.tag == sAttackBuildingTag && dist < attackBuildingDist)
            {
                curState = minState.idle;
                ani.SetBool("cast", true);
                ani.SetBool("walk", false);
            }
            else if (dist < attackDist)  // 유닛 공격일때.
            {
                curState = minState.idle;
                ani.SetBool("cast", true);
                ani.SetBool("walk", false);
            }
            else        // 추적 모드.
            {
                curState = minState.trace;
                ani.SetBool("cast", false);
                ani.SetBool("walk", true);
            }



            // 목표 변경 처리 부분
            if (bState)      //  최종 지점에 도착 안했을 경우.
            {
                // 1. 중간 지점에 도착했는지 확인
                float _dist = Vector3.Distance(tsfTarget[nTargetCount].position, tsfMinion.position);

                //Debug.Log("test");

                // 2. 근처에 도달 했는지 확인
                if (_dist < traceDist)
                {
                    // 3. 다음 목적지로 변경.
                    nTargetCount++;

                    // 4. 다음 목적지의 이름을 변경 sTargetNameNow
                    sTargetNameNow = sTargetName[nTargetCount];

                    // 5. 목적지가 끝이라면 종료.
                    if (tsfTarget.Length - 1 == nTargetCount)
                    {
                        bState = false;
                    }
                }
            }
            // 이동 처리.
            switch (curState)
            {
                case minState.idle:
                    nav.isStopped = true;
                    break;
                case minState.trace:
                    nav.SetDestination(tsfTargetNow.transform.position); // 현재 목표 지점으로 세팅
                    nav.isStopped = false;
                    break;
            }

            // 데미지 처리 부분.
            nCountHp++;

            if (nCountHp > 5 && !isDie)
            {
                switch (curState)
                {
                    case minState.idle:
                        //objTarget.SendMessage("Damage", fAttackDamage);
                        transform.LookAt(objTarget.transform.position);     // 적 방향을 응시.
                        break;
                    case minState.trace:
                        break;
                }
                nCountHp = 0;
            }

            // 목표 오브젝트를 찾는 부분. viewID 처리 부분..
            // 타겟이 없다면 0을 전달..

            if (!objTarget)  // 타겟 오브젝트가 없다면..
            {
                nPhotonViewId = 0;
            }
            else
            {
                // 웨이포인트가 타겟이 되는 경우 예외 처리.
                //nPhotonViewId = objTarget.GetComponent<PhotonView>().ViewID;
                //Debug.Log("Minion2 ViewID:" + nPhotonViewId + "," + objTarget.name);
            }

        }
    }


    public void AttackMissile()
    {
        // 함수가 호출되면 미사일 발사!!
        GameObject obj = Instantiate(objMissile, tsfMissile.position, tsfMissile.rotation);
        obj.GetComponent<MissileMove>().TargetTr = objTarget.transform;
    }

    public void AttackArrow()
    {
        // 함수가 호출되면 화살 발사!!
        GameObject obj = Instantiate(objMissile, tsfMissile.position, tsfMissile.rotation);
        obj.GetComponent<ArrowMove>().TargetTr = objTarget.transform;
    }


    void Update()
    {
        // 조작 부분 수정
        if (!photonView.IsMine)  // 내 미니언이 아닌경우.
        {
            if ((tr.position - currPos).sqrMagnitude >= 10.0f * 10.0f)
            {
                tr.position = currPos;
                tr.rotation = currRot;
            }
            else
            {
                tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 10.0f);
                tr.rotation = Quaternion.Slerp(tr.rotation, currRot, Time.deltaTime * 10.0f);
            }

            currHP = OncurrHp;  // HP 업데이트
            hpBar.fillAmount = currHP / initHP;

            // 목표 오브젝트 포톤 ID 적용.
            nPhotonViewId = OnPhotonViewId;

            // 목표 오브젝트를 저장!!
            objTarget = null;

            if(nPhotonViewId != 0)
                objTarget = PhotonView.Find(nPhotonViewId).gameObject;
        }
    }

    private float dist;  // 적과의 거리 비교용.
    private float minimunDist = 20.0f;  //  가장 가까운적을 저장하기 위한 변수.
    bool bColl;         // 웨이포인트 진행이 아니라 적유닛이나 건물이 타겟팅 되었음을 저장 

    // 반지름 거리 안에 적이 있는지 파악..
    public void NearEnemyAttack(Vector3 pos, float radius)
    {
        // 설정된 거리 안에 적이 있는지 검색하는 부분
        Collider[] colls = Physics.OverlapSphere(pos, radius);

        minimunDist = radius;   // 초기화..
        bColl = false;          // 초기화..

        objTarget = null;       //  목표 오브젝트 초기화.
        tsfTargetNow = null;    //  이동 관련 목표도 초기화.

        for (int i = 0; i < colls.Length; i++)   // 자기 자신도 포함.
        {
            if ((colls[i].tag == sAttackUnitTag || colls[i].tag == sAttackBuildingTag)  // 적 유닛,적 건물
                && colls[i].GetComponent<DeadState>().bDead == false)  // 살아 있으면 타겟팅                
            {
                // 가장 가까운 적을 타겟팅
                Vector3 objectPos = colls[i].transform.position;
                dist = Vector3.Distance(objectPos, transform.position);

                if (minimunDist > dist)
                {
                    objTarget = colls[i].gameObject;
                    minimunDist = dist;
                    // 웨이포인트 진행이 아니라 적유닛이나 건물이 타겟팅 되었음을 저장 
                    bColl = true;
                }
            }
        }
        // 타겟팅이 된 적을 따라가는 부분
        if (bColl)
        {
            tsfTargetNow = objTarget.transform;
        }
        // 웨이포인트 이동 부분..
        else
        {
            objTarget = GameObject.Find(sTargetNameNow);
            tsfTargetNow = objTarget.transform;
        }
    }

    void Damage(float fdamage)
    {
        currHP -= fdamage;
        hpBar.fillAmount = currHP / initHP;     //  HP UI 출력값.

        if (currHP <= 0)
        {
            isDie = true;
            GetComponent<DeadState>().bDead = true;
        }
    }
    void QDamage(float fdamage)
    {
        currHP -= fdamage;
        hpBar.fillAmount = currHP / initHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            isDie = true;
            this.GetComponent<DadeState>().bDead = true;
            Debug.Log("챔피언 죽음");
        }
    }
    void EDamage(float fdamage)
    {
        currHP -= fdamage;
        hpBar.fillAmount = currHP / initHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            isDie = true;
            this.GetComponent<DadeState>().bDead = true;
            Debug.Log("챔피언 죽음");
        }
    }

    private void LateUpdate()
    {
        if (isDie)
        {
            nav.isStopped = true;           //  네비게이션 이동을 정지.
            curState = minState.idle;       //  상태를 대기로 변경.

            ani.SetBool("death", true);
            StartCoroutine(this.Dead());
        }
    }

    IEnumerator Dead()
    {
        yield return new WaitForSeconds(4.0f);      //  지연시간...
        Destroy(gameObject);
    }

    private Vector3 currPos;
    private Quaternion currRot;
    public float OncurrHp;
    public int OnPhotonViewId;      //  목표가 되는 오브젝트의 포톤 ViewID

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);
            stream.SendNext(currHP);
            stream.SendNext(nPhotonViewId);
            stream.SendNext(isDie);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            OncurrHp = (float)stream.ReceiveNext();
            OnPhotonViewId = (int)stream.ReceiveNext();
            isDie = (bool)stream.ReceiveNext();
        }
    }
}

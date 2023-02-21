using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class MiniunManager3 : MonoBehaviour
{
    Animator ani;
    NavMeshAgent nav;

    [SerializeField]
    Transform tsMinion;

    [SerializeField]
    Transform[] tsTarget; //이동 목표의 배열화 (웨이포인트 배열)
    [SerializeField]
    Transform tsfTargetNow; //현재 이동 목표

    public int nTargetCount; // 배열의 이동 목표 지점의 카운트..(웨이포인트 배열 카운트)

    GameObject objTarget; //목표 게임 오브젝트.(적 건물, 적 유닛)

    /////////////////////////////////////////////////////////////
    public string[] sTargetName; //목표 지점의 웨이 포인트 이름을 뮨자로 저장
    public string sTargetNameNow; // 현재 목표 웨이포인트 타겟 이름

    public string sAttackUnitTag; //TopUnit,BottomUnit
    public string sAttackBuildingTag; //TopBuilding,BottomBuilding

    public enum miniunState
    {
        idle, trace
    };
    public miniunState curState = miniunState.trace; //상태 초기화

    public float traceDist = 5.0f; //추적 도달 거리.
    public float fSearchDist = 15.0f; // = 15.0f; //적 검색 거리..
    public float attackDist = 3.0f; //=3.0f //공격 도달 거리

    public float buildingAttackDist = 15.0f; //=15.0f //공격 도달 거리

    bool bSrate = true; //처음 거리 측정용, 한번 체크용

    public bool IsDie = false; //미니언이 죽었는지 확인

    public int nCountHP; //HP 조절용..
    public Animator animator;

    //========================================================================//
    //UI
    public Image Hpbar;
    private float currHP = 100.0f;
    private float InitHP = 100.0f;

    public float fAttackDamage = 20.0f; //공격시 데미지



    private void Awake()
    {
        //처음 이동 지정 위치를 선정
        sTargetNameNow = sTargetName[0];

        //이동 위치의 초기화
        for (int i = 0; i < tsTarget.Length; i++)
        {
            tsTarget[i] = GameObject.Find(sTargetName[i]).GetComponent<Transform>();
        }
        //처음 목표 지점으로 세팅
        tsfTargetNow = tsTarget[0];

        //초기화 
        tsMinion = GetComponent<Transform>();
        ani = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();


    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(this.start());
    }

    IEnumerator start()
    {
        while (!IsDie)
        {
            yield return new WaitForSeconds(0.1f);
            //적을 찾는 부분
            NearEnemyAttack(tsMinion.position, fSearchDist); //적을 찾음, 없으면 웨이 포인트 이동

            //공격하는 부분(건물인 경우 유닛인 경우)

            //상태 변경.
            float dist = Vector3.Distance(tsfTargetNow.position, tsMinion.position);

            //만약 건물이라면.. 사정거리를 길게 체크
            if (tsfTargetNow.tag == sAttackBuildingTag && dist < buildingAttackDist)
            {
                curState = miniunState.idle;
                animator.SetBool("attack 0", true);
                ani.SetBool("wark", false);
            }
            else if (dist < attackDist) //유닛이 공격일때
            {
                curState = miniunState.idle;
                animator.SetBool("attack 0", true);
                ani.SetBool("wark", false);
            }
            else //추적 모드
            {
                curState = miniunState.trace;
                animator.SetBool("attack 0", false);
                ani.SetBool("wark", true);
            }


            //목표 변경 처리 부분
            if (bSrate) //최종 지점에 도착 안했을 경우
            {
                //1. 중간 지점에 도착했는지 확인
                float _dist = Vector3.Distance(tsTarget[nTargetCount].position, tsMinion.position);

                //2. 근처에 도달 했는지 확인
                if (_dist < traceDist)
                {
                    //3.다음 목적지로 변경
                    nTargetCount++;

                    //4.다음 목적지 이름을 변경
                    sTargetNameNow = sTargetName[nTargetCount];

                    //5. 목적지가 끝이라면 종료
                    if (tsTarget.Length - 1 == nTargetCount)
                    {
                        bSrate = false;
                    }
                }
            }

            //이동 처리
            switch (curState)
            {
                case miniunState.idle:
                    nav.isStopped = true;

                    break;
                case miniunState.trace:
                    nav.SetDestination(tsfTargetNow.transform.position); //현재 목표 지점으로 세팅
                    nav.isStopped = false;


                    break;
            }

            //데미지 처리 부분
            //Collider[] colliders = Physics.OverlapSphere(tsMinion.position, attackDist);
            nCountHP++;

            if (nCountHP > 5 && !IsDie)
            {
                switch (curState)
                {
                    case miniunState.idle:
                        objTarget.SendMessage("Damage", fAttackDamage);
                        transform.LookAt(objTarget.transform.position);
                        break;
                    case miniunState.trace:
                        break;
                }
                nCountHP = 0;
            }
            //nav.destination(tsfTargetNow.transform,);
        }
    }

    private float dist; //적과의 거리 비교
    private float minimunDist = 20.0f; //가장 가까운 적을 저장하기 위한 변수

    public bool bColl;
    //반지름 거리안에 적이 있는지 파악..
    public void NearEnemyAttack(Vector3 pos, float radius)
    {
        //설정된 거리 안에 적이 있는지 검색하는 부분
        Collider[] colliders = Physics.OverlapSphere(pos, radius);

        minimunDist = radius;
        bColl = false;

        objTarget = null;
        tsfTargetNow = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            if ((colliders[i].tag == sAttackUnitTag || colliders[i].tag == sAttackBuildingTag)
                && colliders[i].GetComponent<DadeState>().bDead == false)//적 유닛, 적 건물// 살아 있으면 타겟팅
            {
                //가장 가까운 적을 타켓팅
                Vector3 objectPos = colliders[i].transform.position;
                dist = Vector3.Distance(objectPos, transform.position);
                if (minimunDist > dist)
                {
                    objTarget = colliders[i].gameObject;
                    minimunDist = dist;
                    bColl = true; //웨이 포이트가 아니아 적 유닛이나 건물이 타겟팅 되었음을 저장

                }
            }
        }

        //타켓팅이 된 적을 따라 가는 부분
        if (bColl)
        {
            tsfTargetNow = objTarget.transform;
            //transform.LookAt(tsfTargetNow);
            //ani.SetBool("attack 0", true);
            //ani.SetBool("wark", false);
        }
        //웨이 포인트 이동 부분..
        else
        {
            objTarget = GameObject.Find(sTargetNameNow);
            tsfTargetNow = objTarget.transform;
            //transform.LookAt(tsfTargetNow);
            //ani.SetBool("wark", true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (tsfTargetNow == null)
        {
            return;
        }
    }

    void Damage(float fdamage)
    {
        currHP -= fdamage;
        Hpbar.fillAmount = currHP / InitHP; //HP UI 출력값.


        if (currHP <= 0.0f)
        {
            currHP = 0.0f;
            IsDie = true;
            GetComponent<DadeState>().bDead = true;
        }
    }
    void QDamage(float fdamage)
    {
        currHP -= fdamage;
        Hpbar.fillAmount = currHP / InitHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            IsDie = true;
            this.GetComponent<DadeState>().bDead = true;
            Debug.Log("챔피언 죽음");
        }
    }
    void EDamage(float fdamage)
    {
        currHP -= fdamage;
        Hpbar.fillAmount = currHP / InitHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            IsDie = true;
            this.GetComponent<DadeState>().bDead = true;
             Debug.Log("챔피언 죽음");
        }
    }
    private void LateUpdate()
    {
        if (IsDie)
        {
            animator.SetBool("attack 0", false);
            //ani.SetBool("dead", true);
            animator.SetBool("dead", true);
            StartCoroutine(this.Dead());
        }
    }
    IEnumerator Dead()
    {
        yield return new WaitForSeconds(4.0f); //지연 시간..
        Destroy(gameObject);
    }
}

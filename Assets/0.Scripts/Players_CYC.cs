using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class Players_CYC : MonoBehaviourPunCallbacks
//public class Players_CYC : MonoBehaviour
{
    [SerializeField]
    Transform tsfRespawn;       //  생성 위치.

    public bool bAttackMove;    //  공격 이동 체크.

    NavMeshAgent agent;
    public Animator animator;
    public Image hpBar;

    public Text nickName;       //  UI 닉네임.

    [SerializeField]
    private float currAttackDamage = 40.0f;     //   공격 데미지..
    [SerializeField]
    private float currHP = 2000.0f;
    [SerializeField]
    private float initHP = 2000.0f;




    //========================================= 스킬 관련 변수 ================================
    [Header("스킬관련변수")]
    public bool skillUsing = false;
    public GameObject qSkill;
    public GameObject wSkill;
    public GameObject eSkill;
    //=======================================================================================


    private void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();

        StartCoroutine(this.State());       //  이동 추적 .. 상태 확인.
        StartCoroutine(this.AutoHealing()); //  자동 HP 회복 ..        

        curState = minState.idle;       // 기본 상태 대기 초기화.

        // 내 닉네임 가져오기
        nickName.text = photonView.Owner.NickName;

    }

    private void Update()
    {
        // 조작 부분 수정.
        if (!photonView.IsMine)  // 내 챔피언이 아니라면...
        {
            return;
        }


        if (Application.platform == RuntimePlatform.Android) // 안드로이드 터치 이동.
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector3 pos = Input.GetTouch(0).position;

                Ray ray = Camera.main.ScreenPointToRay(pos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    //animator.SetBool("run", true);
                    ani(aniState.run);
                    agent.SetDestination(hit.point);
                    agent.isStopped = false;
                }
            }
            if (agent.remainingDistance <= 0.2f && agent.velocity.magnitude >= 0.2f)
            {
                Debug.Log("idle");
                //animator.SetBool("run", false);
                ani(aniState.run);
            }
        }
        else    // 마우스 클릭 이동..(PC)
        {
            if (Input.GetMouseButtonDown(0) && !this.GetComponent<DeadState>().bDead) // 왼쪽 클릭
            {
                // 마우스가 UI에 적용이 안되었을 경우에만 허용.
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    bAttackMove = true;     // 공격 이동!!

                    // 이동 중에 적이 있는지 파악하고 공격하는 모드로 변경.

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        //animator.SetBool("run", true);
                        ani(aniState.run);
                        agent.SetDestination(hit.point);
                        agent.isStopped = false;
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1) && !this.GetComponent<DeadState>().bDead)   // 오른 클릭!!
            {
                // 마우스가 UI에 적용이 안되었을 경우에만 허용
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    bAttackMove = false;    // 공격모드 이동 아님.
                    bAI = false;            // 자동 공격 모드 끔.

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        //animator.SetBool("run", true);
                        //animator.SetBool("cast", false);
                        ani(aniState.run);
                        agent.SetDestination(hit.point);
                        agent.isStopped = false;
                    }
                }
            }

            if (agent.remainingDistance <= 0.2f && agent.velocity.magnitude >= 0.2f)
            {
                //animator.SetBool("run", false);
                ani(aniState.idle);
                // 멈추게 되면 자동 추적. 자동 공격 모드.
                bAttackMove = true;     // 공격이동 모드
                bAI = true;             // 자동 공격 모드 켜짐.
            }
        }

        //========================================== 스킬 조작 ====================================================
        if (Input.GetKey(KeyCode.Q) && !skillUsing) // 키가 눌렸을때 && 스킬 사용중이 아니라면
        {   // Q - 직선 광역공격
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                transform.LookAt(hit.point);
                skillUsing = true;                          // 스킬 사용중
                StartCoroutine(AnimDelay("skillUse"));
                agent.isStopped = true;


                GameObject qskillobj = Instantiate(qSkill);             // 생성
                qskillobj.transform.position = this.transform.position; // 생성 후 위치 선정
                qskillobj.GetComponent<CYC_Skill_Q>().dir = hit.point;  // 생성된 오브젝트에 이동할 방향 넣어주ㅜㅁ
                if (this.tag == "Top_Unit") { qskillobj.GetComponent<CYC_Skill_Q>().enemyTag = "Bottom_Unit"; }
                else if (this.tag == "Bottom_Unit") { qskillobj.GetComponent<CYC_Skill_Q>().enemyTag = "Top_Unit"; }
            }
        }


        if (Input.GetKeyDown(KeyCode.W) && !skillUsing)
        {   // W - 설치물에 들어오는 아군 회복 횟수제한있음
            GameObject setObj = Instantiate(wSkill);
            setObj.transform.position = this.transform.position;

            if (this.tag == "Top_Unit") { setObj.GetComponent<CYC_Skill_W>().allianceTag = "Top_Unit"; }
            else if (this.tag == "Bottom_Unit") { setObj.GetComponent<CYC_Skill_W>().allianceTag = "Bottom_Unit"; }
        }



        if (Input.GetKeyDown(KeyCode.E) && !skillUsing)
        {   // E - 적 이동속도 감소. 자신의 위치에 소환
            GameObject setObj = Instantiate(eSkill);
            setObj.transform.position = this.transform.position;
            //
            //Top_Unit
            //Bottom_Unit
            if (this.tag == "Top_Unit") { setObj.GetComponent<CYC_Skill_E>().enemyTag = "Bottom_Unit"; }
            else if (this.tag == "Bottom_Unit") { setObj.GetComponent<CYC_Skill_E>().enemyTag = "Top_Unit"; }
        }
        
        
        
        //if (Input.GetKeyDown(KeyCode.R) && !skillUsing)
        //{   // R - 자신기준 범위안 아군 회복
        //    skillUsing = true;                          // 스킬 사용중
        //    Debug.Log("R");
        //}
    }

    //=================================================  추가된 함수  =======================================================
    void SkillUsingOff()
    {       // 애니메이션 이벤트로 실행되는 함수. 스킬 애니메이션이 끝날때 skillUsing을 꺼줍니다
        skillUsing = false;
        agent.isStopped = false;
    }
    IEnumerator AnimDelay(string setBoolName)
    {   // animatior 에 있는 AniState에서 신호를 줄때마다 애니메이션이 다시 시작되서 그냥 딜레이 주고 꺼버림.
        animator.SetBool(setBoolName, true);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool(setBoolName, false);
    }












    //======================================================================================================================
    void Damage(float fdamage)
    {
        currHP -= fdamage;
        hpBar.fillAmount = currHP / initHP;

        if (currHP <= 0.0f && !this.GetComponent<DeadState>().bDead)  //  죽은 상태면..
        {
            isDie = true;
            this.GetComponent<DeadState>().bDead = true;
            bAttackMove = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
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
            bAttackMove = false;
            bAI = false;
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
            bAttackMove = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
        }
    }

    private void LateUpdate()
    {
        if (isDie)
        {
            isDie = false;

            //animator.SetBool("run", false);
            //animator.SetBool("cast", false);
            //animator.SetBool("death", true);
            ani(aniState.death);

            StartCoroutine(this.Dead());

            agent.isStopped = true;

            Debug.Log("죽음");
        }
    }

    IEnumerator Dead()
    {
        yield return new WaitForSeconds(5.0f);      // 지연 시간.

        // 생존값 초기화.
        currHP = 100.0f;
        hpBar.fillAmount = 1;   //   currHP / initHP

        this.GetComponent<DeadState>().bDead = false;
        // 없애지 말고 위치를 이동!!! 챔피언 생성 위치로 이동.
        transform.position = tsfRespawn.position;

        //animator.SetBool("death", false);
        ani(aniState.death);

        agent.Warp(tsfRespawn.position);

        agent.isStopped = false;

        StartCoroutine(this.State());       //  이동 & 추적 상태확인.(다시 복구!!)

        Debug.Log("부활");
    }

    //////////////////////////////////////
    // 추적 부분

    private float dist;                 //  적과의 거리 체크 용.
    private float minimumDist = 20.0f;  //  제일 가까운 적의 위치

    [Header("추적부분")]
    public bool bColl;                         //  적을 찾았는지?
    public bool bAI;                           //  적 자동 추적 모드 인지.
    public GameObject objTarget;               //  목표 오브젝트.
    public Transform tsfTarget;         //  현재 이동 목표. 길 찾기에 사용. 공격시에 사용.

    // 적 유닛 또는 적 건물 이름 처리.
    [Header("공격목표태그")]
    public string sAttackUnitTag;       //  유닛.
    public string sAttackBuildingTag;   //  건물.

    public GameObject objMissile;       //  원거리 프리팹
    public Transform tsfMissile;        //  원거리 발사 위치 (지팡이 끝..)

    // 반지름의 거리안에 적이 있는지 파악.
    public void NearEnemyAttack(Vector3 pos, float radius)
    {
        Collider[] colls = Physics.OverlapSphere(pos, radius);

        minimumDist = radius;       // 초기화
        bColl = false;
        objTarget = null;
        //tsfTarget = null;   // 타겟팅 적도 초기화..

        for (int i = 0; i < colls.Length; ++i)
        {
            if ((colls[i].tag == sAttackUnitTag || colls[i].tag == sAttackBuildingTag)
                && colls[i].GetComponent<DeadState>().bDead == false)
            {
                Vector3 objectPos = colls[i].transform.position;
                dist = Vector3.Distance(objectPos, transform.position);

                if (minimumDist > dist)
                {
                    objTarget = colls[i].gameObject;    // 가장 가까운 적
                    minimumDist = dist;                 // 최대 가까운 적
                    bColl = true;
                }
            }
        }
        if (bColl)   // 타겟팅한 적을 따라간다.
        {
            tsfTarget = objTarget.transform;    // 찾은 적을 추척 하도록 설정.
            bAI = true; // 추적모드 On 시킴.
        }
        else
        {
            bAI = false;    //  추적모드 Off 시킴
        }
    }

    public void AttackMissile()
    {
        if (objTarget) // 오브젝트가 있는 경우.!!!!
        {
            // 함수 호출되면 원거리 미사일 발사.
            GameObject obj = Instantiate(objMissile, tsfMissile.position, tsfMissile.rotation);
            obj.GetComponent<MissileMove>().TargetTr = objTarget.transform;         // 공격 목표.,
            obj.GetComponent<MissileMove>().fAttackDamage = currAttackDamage;       // 공격 데미지.
        }
    }

    //////////////////////////////////////////////
    // 주변에 몬스터가 있는 경우  --> 추적 & 공격.

    public enum minState { idle, trace, attack };       // 상태 (네비게이션 상태)
    public minState curState = minState.idle;

    public float attackDist = 3.0f;             // 공격 도달 거리.
    public float attackBuildingDist = 15.0f;    // 건물 공격 도달 거리.
    public float fSearchDist = 15.0f;           // 적 검색 거리.

    public bool isDie = false;                  // 나의 죽음 상태.


    /////////////////////////////
    // 자동 회복
    IEnumerator AutoHealing()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);  // 지연 시간
            if (currHP < initHP)
            {
                currHP += 2;
                hpBar.fillAmount = currHP / initHP;
            }
        }
    }

    // 목표한 적이 가까우면 정지 상태로 변경, 아니면 추적 후 공격.
    IEnumerator State()
    {
        while (!isDie && !this.GetComponent<DeadState>().bDead)  // 안죽었을때.. 2중 체크.
        {
            yield return new WaitForSeconds(0.1f);      //  지연시간.

            // 내 주변의 적을 찾음. 근처에 적이 있는지...
            NearEnemyAttack(transform.position, fSearchDist);

            // 자동 추적 모드일 경우.
            if (bAI && bAttackMove)
            {
                //// 만약 타겟팅한 적이 없다면.
                //if (tsfTarget.tag != sAttackBuildingTag && tsfTarget.tag != sAttackUnitTag)
                //{
                //    curState = minState.idle;
                //    animator.SetBool("cast", true);
                //    animator.SetBool("run", false);
                //}
                //else
                {
                    float dist = Vector3.Distance(tsfTarget.position, transform.position);

                    // 타겟이 만약 건물이라면 ... 사정거리 길게 체크.
                    if (tsfTarget.tag == sAttackBuildingTag && dist < attackBuildingDist)
                    {
                        curState = minState.attack;
                        //animator.SetBool("cast", true);
                        //animator.SetBool("run", false);
                        ani(aniState.cast);
                    }
                    else if (dist < attackDist)      //  유닛 공격일때.
                    {
                        curState = minState.attack;
                        //animator.SetBool("cast", true);
                        //animator.SetBool("run", false);
                        ani(aniState.cast);
                    }
                    else
                    {
                        curState = minState.trace;
                        //animator.SetBool("cast", false);
                        //animator.SetBool("run", true);
                        ani(aniState.run);
                    }
                }

                ////////////////////////////////////////
                // 이동 처리.
                switch (curState)
                {
                    case minState.attack:
                        agent.isStopped = true;
                        transform.LookAt(objTarget.transform.position);     // 적을 바라본다.
                        break;
                    case minState.trace:
                        agent.SetDestination(tsfTarget.transform.position);
                        agent.isStopped = false;
                        break;
                    case minState.idle:
                        agent.isStopped = true;
                        break;
                }
            }
            // 로직 수정...(일명 땜빵)
            else if (curAni == aniState.cast)    // 공격중일때...
            {
                if (!objTarget)  // 타겟 오브젝트가 없다면..
                {
                    curAni = aniState.idle;     // 대기 상태로 전환.
                    ani(aniState.idle);         // 애니랑 스태이트 모두 대기(idle)상태로 변환.
                    Debug.Log("땜빵코드");
                }
            }
        }
    }

    //=======================================
    // 애니메이션 관리 함수.
    public enum aniState { idle, cast, run, death };    // 4가지 (액션)애니메이션 상태.
    public aniState curAni = aniState.idle;             // 에니메이션 확인용..(인스펙터)    

    void ani(aniState _curAni)      // 애니메이션 상태 변환 함수.
    {
        animator.SetBool("idle", false);
        animator.SetBool("cast", false);
        animator.SetBool("run", false);
        animator.SetBool("death", false);

        curAni = _curAni;   // 확인용.

        switch (_curAni)
        {
            case aniState.idle:
                animator.SetBool("idle", true);
                break;
            case aniState.cast:
                animator.SetBool("cast", true);
                break;
            case aniState.run:
                animator.SetBool("run", true);
                break;
            case aniState.death:
                StartCoroutine(deathAnim());
                break;
        }
    }
    IEnumerator deathAnim()
    {
        animator.SetBool("death", true);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("death", false);

    }




}

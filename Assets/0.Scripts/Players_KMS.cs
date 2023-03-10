using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class Players_KMS : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    Transform tsfRespawn;       //  생성 위치.

    public bool bAttackMove;    //  공격 이동 체크.

    NavMeshAgent agent;
    private Transform tr;

    public Animator animator;
    public Image hpBar;

    public int nPhotonViewId;   //  목표가 되는 오브젝트의 포톤 ID

    public Text nickName;       //  UI 닉네임.

    [SerializeField]
    private float currAttackDamage = 40.0f;     //   공격 데미지..
    [SerializeField]
    private float currHP = 2000.0f;
    [SerializeField]
    private float initHP = 2000.0f;

    private void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();
        tr = this.GetComponent<Transform>();

        if (photonView.IsMine)   // 나 스스로 인경우..
        {
            StartCoroutine(this.State());       //  이동 추적 .. 상태 확인.
            StartCoroutine(this.AutoHealing()); //  자동 HP 회복 ..        
        }

        curState = minState.idle;       // 기본 상태 대기 초기화.

        // 내 닉네임 가져오기
        nickName.text = photonView.Owner.NickName;

        // 내 재 생성 위치 가져오기
        if(gameObject.tag == "Bottom_Unit")
        {
            tsfRespawn = GameObject.Find("WayPointB").gameObject.transform;
        }

        if(gameObject.tag == "Top_Unit")
        {
            tsfRespawn = GameObject.Find("WayPointT").gameObject.transform;
        }
    }

    private void Update()
    {
        // 조작 부분 수정.
        if (!photonView.IsMine)  // 내 챔피언이 아니라면...
        {
            if((tr.position - currPos).sqrMagnitude >= 10.0f * 10.0f)
            {
                tr.position = currPos;
                tr.rotation = currRot;
            }
            else
            {
                tr.position = Vector3.Lerp(tr.position, currPos, Time.deltaTime * 10.0f);
                tr.rotation = Quaternion.Slerp(tr.rotation, currRot, Time.deltaTime * 10.0f);
            }
            currHP = OncurrHp;      // HP 업데이트
            hpBar.fillAmount = currHP / initHP;

            // 목표 오브젝트 포톤 ID 적용.
            nPhotonViewId = OnPhotonViewId;

            // 목표 오브젝트를 저장!!
            objTarget = PhotonView.Find(nPhotonViewId).gameObject;

            //return;
        }
        // 자신인 경우..
        else
        {
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
        }
    }

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
        currHP = initHP;
        hpBar.fillAmount = 1;   //   currHP / initHP

        this.GetComponent<DeadState>().bDead = false;
        // 없애지 말고 위치를 이동!!! 챔피언 생성 위치로 이동.
        transform.position = tsfRespawn.position;

        //animator.SetBool("death", false);
        ani(aniState.death);

        agent.Warp(tsfRespawn.position);

        agent.isStopped = false;

        if (photonView.IsMine)  // 나 스스로 인경우
        {
            StartCoroutine(this.State());       //  이동 & 추적 상태확인.(다시 복구!!)
            
            // 카메라를 자기 진영으로 이동.(바로 이동)
            if (gameObject.tag == "Bottom_Unit")
            {
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMove>().CameraMoveBottomDirect();
            }
            if (gameObject.tag == "Top_Unit")
            {
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMove>().CameraMoveTopDirect();
            }
        }

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

            // 목표 오브젝트를 찾는 부분. viewID 처리 부분..
            // 타겟이 없다면 0을 전달..

            if (!objTarget)  // 타겟 오브젝트가 없다면..
            {
                nPhotonViewId = 0;
            }
            else
            {
                nPhotonViewId = objTarget.GetComponent<PhotonView>().ViewID;
                //Debug.Log("ViewID:" + nPhotonViewId);
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
                animator.SetBool("death", true);
                break;
        }

        // RPC 전송!!
        photonView.RPC("AniRequest", RpcTarget.Others, _curAni);        // 나의 챔피언 애니 전송!!!
    }

    [PunRPC]
    void AniRequest(aniState _curAni)
    {
        Debug.Log("전달받은 애니 데이터:" + _curAni);

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
                animator.SetBool("death", true);
                break;
        }
    }

    private Vector3 currPos;        //  실시간으로 받는 변수.(위치값)
    private Quaternion currRot;     //  실시간으로 받는 변수.(회전값)
    public float OncurrHp = 100.0f; //  실시간으로 받는 변수.(HP)
    public int OnPhotonViewId;      //  목표가 되는 오브젝트의 포톤 ViewID

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
        if(stream.IsWriting)  // 데이터를 계속 전송만..
        {
            stream.SendNext(tr.position);       //  나의 위치값을 보낸다.
            stream.SendNext(tr.rotation);       //  나의 회전값을 보낸다.
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

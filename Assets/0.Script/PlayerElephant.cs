using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems; //버튼같은거 클릭할때 바닥은 클릭안되도록 처리하기위한
using Photon.Pun;
using Photon.Realtime;

public class PlayerElephant : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    Transform tsfRespawn;   //리스폰 위치(처음 생성위치도 동일)

    public bool bAttackMove;    //공격 이동 체크(이동하면서 공격하느냐를 체크)
    public bool bWalk;

    NavMeshAgent agent;
    private Transform tr;
    public Animator animator;
    public Image hpBar;

    public int nPhotonViewId;   //목표가 되는 오브젝트의 포톤 ID

    public Text nickName;       //UI닉네임


    [SerializeField]
    private float currAttackDamage = 40.0f; //공격데미지
    [SerializeField]
    private float curHP = 2000.0f;
    [SerializeField]
    private float initHP = 2000.0f;

    public readonly int attackIndex = Animator.StringToHash("AttackTree");

    public int nCountHp;        // HP 조절용,.

    public float fAttackDamage = 20.0f;     // 공격시 데미지 수치. 


    private void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();
        tr = this.GetComponent<Transform>();



        if (photonView.IsMine)
        {
            //코루틴 2개 쓸거다.
            //1. 이동 추적 관련 상태 체크 코루틴
            //2. 자동 HP 회복 관련 코루틴
            StartCoroutine(this.State());
            StartCoroutine(this.AutoHealing());
        }

        curState = minState.idle;   //기본 상태 대기 초기화.

        //내 닉네임 가져오기
        nickName.text = photonView.Owner.NickName;

        shootForce = shootSpeed * Time.deltaTime;

        //내 재 생성 위치 가져오기
        if(gameObject.tag == "Bottom_Unit")
        {
            tsfRespawn = GameObject.Find("RespawnT").gameObject.transform;
        }

        if (gameObject.tag == "Top_Unit")
        {
            tsfRespawn = GameObject.Find("RespawnB").gameObject.transform;
        }
    }

    private void Update()
    {
        //조작 부분 수정.
        if (!photonView.IsMine) //내 챔피언이 아니라면
        {
            //동기화
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

            curHP = onCurrHp;       //hp업데이트
            hpBar.fillAmount = curHP / initHP;

            //목표 오브젝트 포톤 ID 적용
            nPhotonViewId = onPhotonViewId;

            //목표 오브젝트를 저장
            objTarget = PhotonView.Find(nPhotonViewId).gameObject;


            //return;
        }
        else    //자신인 경우
        {

            //지금은 안쓰지만 나중에 쓸 때 사용.
            //만약 플랫폼이 안드로이드면 안드로이드 터치 이동
            if (Application.platform == RuntimePlatform.Android)
            {
                //0보다 크면 터치가 됐다는 뜻 겟터치쪽 0 의미는 한번 터치
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    //터치된 곳의 위치값을 가져옴
                    Vector3 pos = Input.GetTouch(0).position;

                    Ray ray = Camera.main.ScreenPointToRay(pos);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        //이곳에서 이동처리
                        //animator.SetBool("Walk", true);
                        ani(aniState.walk);
                        agent.SetDestination(hit.point);
                        //false면 agent를 구동해라 라는 뜻
                        agent.isStopped = false;
                    }
                }
                //목적지에 가까워졌으면 정지하라
                if (agent.remainingDistance <= 0.2f && agent.velocity.magnitude >= 0.2f)
                {
                    Debug.Log("IDle");
                    //animator.SetBool("Walk", false);
                    ani(aniState.idle);
                }
            }
            else //여기가 우리가 지금하는 마우스로 클릭해서 이동
            {
                //왼쪽버튼 클릭 그리고 챔피언이 죽지 않았다면.
                if (Input.GetMouseButtonDown(0) && !this.GetComponent<DeadState>().bDead)
                {
                    //마우스가 UI(버튼 등에)에 적용이 안됐을 경우에만 적용.
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        //공격 이동(즉 lefe마우스 클릭)
                        //이때는 이동중에 적이 있는지 파악하면서 이동해야함
                        bAttackMove = true;
                        bWalk = true;
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, 100))
                        {
                            //animator.SetBool("Walk", true);
                            ani(aniState.walk);
                            agent.SetDestination(hit.point);
                            agent.isStopped = false;
                        }
                    }
                }   //오른쪽 마우스 버튼을 눌렀을 경우 (강제 이동만 하도록)
                else if (Input.GetMouseButtonDown(1) && !this.GetComponent<DeadState>().bDead)
                {

                    //마우스가 UI(버튼 등에)에 적용이 안됐을 경우에만 적용.
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        bAttackMove = false; //공격하면서 이동하는 모드가 아님
                        bAI = false;    //자동 공격모드를 끔
                        bWalk = true;

                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit, 100))
                        {
                            //animator.SetBool("Walk", true);
                            //animator.SetBool("Attack", false);
                            ani(aniState.walk);
                            agent.SetDestination(hit.point);
                            agent.isStopped = false;
                        }
                    }
                }

                //목적지에 가까워졌으면 정지하라
                if (agent.remainingDistance <= 0.2f && agent.velocity.magnitude >= 0.2f)
                {
                    //animator.SetBool("Walk", false);
                    ani(aniState.idle);
                    //멈추게 되면 자동 추적. 자동 공격 모드.
                    bAttackMove = true; //공격이동 모드 켬
                    bAI = true; //자동 공격모드 켬
                    bWalk = false;
                }
            }

            if (!isDie && !this.GetComponent<DeadState>().bDead)
            {
                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    Debug.Log("스킬1 발동");
                    bSkillAttack1 = true;
                    ani(aniState.attack1);
                    bAttackMove = false;
                    bAI = false;
                    bWalk = false;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    Debug.Log("스킬2 발동");
                    bSkillAttack2 = true;
                    ani(aniState.attack2);
                    bAttackMove = false;
                    bAI = false;
                    bWalk = false;
                }
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    Debug.Log("스킬3 발동");
                    bSkillAttack3 = true;
                    ani(aniState.attack3);
                    bAttackMove = false;
                    bAI = false;
                    bWalk = false;
                }
            }
        }
    }

    void Damage(float fdamage)
    {
        curHP -= fdamage;
        hpBar.fillAmount = curHP / initHP;

        //hp가 0이 됐는데 아직 상태는 죽지않았다면
        if(curHP <= 0.0f && !this.GetComponent<DeadState>().bDead)
        {
            //죽은상태로 변경
            isDie = true;
            this.GetComponent<DeadState>().bDead = true;
            bAttackMove = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
        }
    }
    void QDamage(float fdamage)
    {
        curHP -= fdamage;
        hpBar.fillAmount = curHP / initHP;

        if (curHP < 0.0f && !this.GetComponent<DadeState>().bDead)
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
        curHP -= fdamage;
        hpBar.fillAmount = curHP / initHP;

        if (curHP < 0.0f && !this.GetComponent<DadeState>().bDead)
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
            //animator.SetBool("Walk", false);
            //animator.SetBool("Attack", false);
            //animator.SetBool("Die", true);
            ani(aniState.die);
            StartCoroutine(this.Dead());

            agent.isStopped = true;

            Debug.Log("죽음");
        }
    }

    IEnumerator Dead()
    {
        yield return new WaitForSeconds(5.0f); //지연 시간

        //챔피언 다시 살려야하니 생존값 초기화
        curHP = initHP;
        hpBar.fillAmount = 1;   //curHP / initHP

        this.GetComponent<DeadState>().bDead = false;

        //챔피언은 Destroy 하지 말고 위치를 이동
        transform.position = tsfRespawn.position;
        //animator.SetBool("Die", false);
        ani(aniState.idle);

        //미니언들과는 다르게 챔피언은 삭제후 다시 생성하는게 아니니까
        //agent가 죽은 위치를 가지고 있다 그렇기 때문에 위치를 리스폰위치로 워프시켜준다.
        agent.Warp(tsfRespawn.position);

        agent.isStopped = false;

        if (photonView.IsMine)
        {
            StartCoroutine(this.State());   //이동&추적 상태 체크 코루틴을 다시 복구


            if (gameObject.tag == "Bottom_Unit")
            {
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMove>().CameraMoveTopDirect();
            }

            if (gameObject.tag == "Top_Unit")
            {
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMove>().CameraMoveBottomDirect();
            }
        }
        Debug.Log("부활");
    }

    //////////////////////////////////////////////
    /////추적 부분
   
    private float dist;                 //적과의 거리 체크
    private float minimumDist = 20.0f;  //제일 가까운 적의 위치

    [Header("추적부분")]
    public bool bColl;                         //적을 찾았는지 체크
    public bool bAI;                            //적 자동 추적 모드인지 체크
    public bool bSkillAttack1;           //첫번째 스킬 발동 중인지 체크
    public bool bSkillAttack2;           //두번째 스킬 발동 중인지 체크
    public bool bSkillAttack3;           //세번째 스킬 발동 중인지 체크
    GameObject objTarget;               //목표 오브젝트.
    public Transform tsfTarget;         //위치에 관한 것. 현재 이동 목표. 길찾기, 공격시에 모두 사용

    //적 유닛 또는 적 건물 이름 처리.
    [Header("공격목표태그")]
    public string sAttackUnitTag;       //유닛
    public string sAttackBuildingTag;   //건물.

    public GameObject objMissile;       //발사체 프리펩
    public Transform tsfmissile;        //발사체 발사 위치(예: 지팡이 끝)

    //반지름의 거리안에 적이 있는지 파악
     public void NearEnemyAttack(Vector3 pos, float radius)
    {
        Collider[] colls = Physics.OverlapSphere(pos, radius);

        minimumDist = radius;
        bColl = false;
        objTarget = null;

        for(int i=0; i<colls.Length; i++)
        {
            if((colls[i].tag == sAttackUnitTag || colls[i].tag == sAttackBuildingTag) 
                && colls[i].GetComponent<DeadState>().bDead == false)
            {
                Vector3 objectPos = colls[i].transform.position;
                dist = Vector3.Distance(objectPos, transform.position);

                if(minimumDist > dist)
                {
                    objTarget = colls[i].gameObject; //가장 가까운 적이 타겟으로 잡힘
                    minimumDist = dist;              //가장 가까운 적의 거리가 최소 거리로 변경됨
                    bColl = true;
                }
            }
        }

        //위 for문에서 적을 찾았다고 한다면 bColl이 true이기에 여기 if문 안으로 들어옴
        if(bColl)
        {
            tsfTarget = objTarget.transform;    //찾은 적을 추적하도록 설정.
            bAI = true;     //추적 모드 On
        }
        else
        {
            bAI = false;    //추적 모드 Off
        }
    }

    //원거리 캐릭터라면 있어야할 함수
    public void AttackMissile()
    {
        if (objTarget)  //오브젝트가 있는 경우.
        {
            //함수 호출되면 발사체가 발사된다.
            GameObject obj = Instantiate(objMissile, tsfmissile.position, tsfmissile.rotation);
            //obj.GetComponent<ProjectileMove>().TargetTr = objTarget.transform;  //공격 목표
            //obj.GetComponent<ProjectileMove>().fAttackDamage = currAttackDamage;  //공격 데미지
        }
    }

    /////////////////////////////////////////
    ///주변에 몬스터가 있는 경우   -- 추적 & 공격
    ///
    
    public enum minState { idle, trace, attack};    //상태(네비게이션 상태)
    public minState curState = minState.idle;  //현재 상태 초기화

    public float attackDist = 3.0f;             //공격 도달 거리
    public float attackBuildingDist = 15.0f;    //건물 공격 도달 거리
    public float fSearchDist = 15.0f;           //적 검색 거리

    public bool isDie = false;                  //나의 죽음 상태

    ////////////////////////////
    ///자동 회복
    IEnumerator AutoHealing()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);  //지연 시간
            if (curHP < initHP)
            {
                curHP += 2;
                hpBar.fillAmount = curHP / initHP;
            }
        }
    }

    //목표한 적이 가까우면 정지 상태로 변경. 아니면  추적.
    IEnumerator State()
    {
        //내가 미사일에 맞아서 죽는 경우가 있고 전체적으로 죽은 경우가 있기에
        //일원화도 좋지만 지금처럼 되어있으면 2중 체크해야한다.
        while(!isDie && !this.GetComponent<DeadState>().bDead)
        {
            yield return new WaitForSeconds(0.1f); //0.1초마다 체크.

            //내 주변의 적을 찾음, 근처에 적이 있는지..
            NearEnemyAttack(transform.position, fSearchDist);

            //자동 추적 모드일 경우
            if(bAI && bAttackMove)
            {
                float dist = Vector3.Distance(tsfTarget.position, transform.position);

                //타겟이 만약 건물이라면 사정거리를 길게 체크.
                if(tsfTarget.tag == sAttackBuildingTag && dist < attackBuildingDist )
                {
                    //idle이 되면 바로 공격함
                    curState = minState.attack;
                    //animator.SetBool("Attack", true);
                    //animator.SetBool("Walk", false);
                    ani(aniState.attack);
                }
                else if(dist < attackDist) //유닛공격
                {
                    curState = minState.attack;
                    //animator.SetBool("Attack", true);
                    //animator.SetBool("Walk", false);
                    ani(aniState.attack);
                }
                else
                {
                    curState = minState.trace;
                    //animator.SetBool("Attack", false);
                    //animator.SetBool("Walk", true);
                    ani(aniState.walk);
                }

                //////////////////////////////////
                ///이동처리
                switch(curState)
                {
                    case minState.attack:
                        agent.isStopped = true;
                        transform.LookAt(objTarget.transform.position);//적을 바라본다.
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
            //로직 수정...(일명 땜빵)
            //현재 공격중일때 문제가 됨. 적이 사라졌는데 계속 떄리고있는
            else if(curAni == aniState.attack)  //이건 지금 공격중일때 의미함
            {
                //공격중인데 오브젝트타겟이 없는상태라면? 보통 적이 죽은상태겠지
                if(!objTarget)
                {
                    curState = minState.idle;     //공격상태를 대기상태로 변경
                    ani(aniState.idle);         //애니랑 스테이트 모두 대기상태로 변경
                    Debug.Log("땜빵코드");
                }
            }
            else if(!bAI && !bAttackMove && !bWalk)
            {
                if(!bSkillAttack1 && !bSkillAttack2 && !bSkillAttack3)
                {
                    curState = minState.idle;     //공격상태를 대기상태로 변경
                    ani(aniState.idle);         //애니랑 스테이트 모두 대기상태로 변경
                }
            }

            //목표 오브젝트 찾는 부분 viewID처리 부분
            //타겟이 없다면 0을 전달
            if (!objTarget)
            {
                nPhotonViewId = 0;
            }
            else
            {
                nPhotonViewId = objTarget.GetComponent<PhotonView>().ViewID;
                Debug.Log("ViewID: "+nPhotonViewId);
            }

            // 데미지 처리 부분.
            nCountHp++;

            if (nCountHp > 5 && !isDie)
            {
                switch (curState)
                {
                    case minState.attack:
                        objTarget.SendMessage("Damage", fAttackDamage);
                        //transform.LookAt(objTarget.transform.position);
                        break;
                    case minState.trace:
                        break;
                }
                nCountHp = 0;
            }
        }
    }

    //==========================================
    //애니메이션 관리 함수
    //지금은 4가지 (액션)애니메이션 상태로 만든다.
    //스킬들이 여러개 생기면 더 추가하면됨.
    public enum aniState { idle, attack, attack1, attack2, attack3, walk, die }
    public aniState curAni = aniState.idle;     //애니메이션 확인용(인스펙터창에서 확인하기 편하게.)
    

    //_는 사용하는 사람에 따라 다른데 지금은 함수 안에서만 쓰는 변수의미로.
    void ani(aniState _curAni)  //애니메이션 상태 변환 함수.
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Die", false);
        animator.SetBool("Attack1", false);
        animator.SetBool("Attack2", false);
        animator.SetBool("Attack3", false);

        //인스펙터에서 현재 애니상태 확인용
        curAni = _curAni;

        //이전처럼 여러개 작성해서 트루 하나에 펄스 여러개 했다면
        //지금은 하나만 트루시키면 나머지는 다 알아서 펄드되게끔 함수로 컨트롤하게끔.
        //기존처럼 했던 애니메이터에도 프로퍼티를 수정해야한다.
        //가는 기준으로 트루만 시켜주면된다. 즉, 런에서 아이들 가리키는 방향으로는 아이들 트루만 해놓으면됨.

        

        switch(_curAni)
        {
            case aniState.idle:
                animator.SetBool("Idle", true);
                break;
            case aniState.attack:
                animator.SetBool("Attack", true);
                break;
            case aniState.attack1:
                if (bSkillAttack1)
                {
                    animator.SetBool("Attack1", true);
                    bSkillAttack1 = false;
                }
                break;
            case aniState.attack2:
                if (bSkillAttack2)
                {
                    animator.SetBool("Attack2", true);
                    bSkillAttack2 = false;
                }
                break;
            case aniState.attack3:
                if (bSkillAttack3)
                {
                    animator.SetBool("Attack3", true);
                    bSkillAttack3 = false;
                }                
                break;
            case aniState.walk:
                animator.SetBool("Walk", true);
                break;
            case aniState.die:
                animator.SetBool("Die", true);
                break;
            default:
                break;
        }

        //RPC 전송!!
        photonView.RPC("AniRequest", RpcTarget.Others, _curAni , bSkillAttack1, bSkillAttack2, bSkillAttack3);
    }

    [PunRPC]
    void AniRequest(aniState _curAni, bool bSkillAttack1, bool bSkillAttack2, bool bSkillAttack3)
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Attack", false);
        animator.SetBool("Walk", false);
        animator.SetBool("Die", false);
        animator.SetBool("Attack1", false);
        animator.SetBool("Attack2", false);
        animator.SetBool("Attack3", false);

        //인스펙터에서 현재 애니상태 확인용
        curAni = _curAni;

        //이전처럼 여러개 작성해서 트루 하나에 펄스 여러개 했다면
        //지금은 하나만 트루시키면 나머지는 다 알아서 펄드되게끔 함수로 컨트롤하게끔.
        //기존처럼 했던 애니메이터에도 프로퍼티를 수정해야한다.
        //가는 기준으로 트루만 시켜주면된다. 즉, 런에서 아이들 가리키는 방향으로는 아이들 트루만 해놓으면됨.

        switch (_curAni)
        {
            case aniState.idle:
                animator.SetBool("Idle", true);
                break;
            case aniState.attack:
                animator.SetBool("Attack", true);
                break;
            case aniState.attack1:
                if (bSkillAttack1)
                {
                    animator.SetBool("Attack1", true);
                    bSkillAttack1 = false;
                }
                break;
            case aniState.attack2:
                if (bSkillAttack2)
                {
                    animator.SetBool("Attack2", true);
                    bSkillAttack2 = false;
                }
                break;
            case aniState.attack3:
                if (bSkillAttack3)
                {
                    animator.SetBool("Attack3", true);
                    bSkillAttack3 = false;
                }
                break;
            case aniState.walk:
                animator.SetBool("Walk", true);
                break;
            case aniState.die:
                animator.SetBool("Die", true);
                break;
            default:
                break;
        }
    }

    public Transform skill1Position;
    public Transform skill2Position;
    public Transform skill3Position;
    public GameObject magicBall;
    public GameObject earthShatter;
    public GameObject dustExplosion;
    public NavMeshAgent playerNav;
    private float shootSpeed = 500.0f;
    private float shootForce;
    private float currentSpeed;

    public void Skill1()
    {
        GameObject instanceMagicBall = Instantiate(magicBall, skill1Position.position, Quaternion.identity);

        instanceMagicBall.GetComponent<Rigidbody>().velocity = shootForce * skill1Position.forward;
        instanceMagicBall.GetComponent<Skill1Effect>().playerTag = transform.tag;
    }

    public void Skill2()
    {
        GameObject InstantiateDustExplosion = Instantiate(dustExplosion, skill2Position.position, transform.rotation);

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
        GameObject InstantiateEarthShatter = Instantiate(earthShatter, skill3Position.position, transform.rotation);
        InstantiateEarthShatter.GetComponent<Skill3Effect>().playerTag = transform.tag;
    }

    private Vector3 currPos;        //실시간으로 받는 위치값 변수
    private Quaternion currRot;     //실시간으로 받는 회전값 변수
    public float onCurrHp = 100.0f; //실시간으로 받는 Hp값 변수
    public int onPhotonViewId;      //목표가 되는 오브젝트의 포톤 ViewID

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
        if(stream.IsWriting) //데이터 계속 전송만
        {
            stream.SendNext(tr.position);   //나의 위치값을 보낸다.
            stream.SendNext(tr.rotation);   //나의 회전값을 보낸다.
            stream.SendNext(curHP);
            stream.SendNext(nPhotonViewId);
            stream.SendNext(isDie);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            onCurrHp = (float)stream.ReceiveNext();
            onPhotonViewId = (int)stream.ReceiveNext();
            isDie = (bool)stream.ReceiveNext();
        }
    }
}

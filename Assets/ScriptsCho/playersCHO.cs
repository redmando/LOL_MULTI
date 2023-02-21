using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class playersCHO : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField]
    Transform tsRespawn;

    Rigidbody rd;

    public bool bAttack; //공격이동 체크
    //public bool bAI; //자동 공격 모드 체크
    //public bool isDie;

    NavMeshAgent agent;
    private Transform Playertr;
    public Animator animator;
    public Image hpbar;

    public Text NicName;

    public float fAttackDamage;

    [SerializeField]
    private float currHP = 2000.0f;
    [SerializeField]
    private float initHP = 2000.0f;

    public float fdamage;
    public int nCountHP;
    public GameObject Sword;
    [Header("스킬 이펙트 포지션 및 스킬넘")]
    public int nSkillNum;
    public Transform QskillPos;
    public Transform WskillPos;
    public GameObject Skill;
    public GameObject[] Skills;

    public int nPhotonviewID;


    // Start is called before the first frame update
    void Start()
    {
        Playertr = this.GetComponent<Transform>();
        rd = this.GetComponent<Rigidbody>();
        agent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();

        if (photonView.IsMine)
        {

            StartCoroutine(this.State());
            StartCoroutine(this.AutoHealing()); //자동 HP회복..

        }
        tsTarget = null;
        currState = minState.idle; //기본 상태는 대기로 초기화
        rd.isKinematic = false;
        rd.isKinematic = true;

        //내 닉네임 가져오기
        NicName.text = photonView.Owner.NickName;

        // 내 재 생성 위치 가져오기
        if (gameObject.tag == "Bottom_Unit")
        {
            tsRespawn = GameObject.Find("WayPointB").GetComponent<Transform>();
        }
        if (gameObject.tag == "Top_Unit")
        {
            tsRespawn = GameObject.Find("WayPointT").GetComponent<Transform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //내가 아닌경우
        if (!photonView.IsMine)
        {
            if ((transform.position = currPos).sqrMagnitude >= 10.0f * 10.0f)
            {
                Playertr.position = currPos;
                Playertr.rotation = currRot;
            }
            else
            {
                Playertr.position = Vector3.Lerp(Playertr.position, currPos, Time.deltaTime * 10.0f);
                Playertr.rotation = Quaternion.Lerp(Playertr.rotation, currRot, Time.deltaTime * 10.0f);
            }
            currHP = OncurrHp;
            hpbar.fillAmount = currHP / initHP;

            //목표 오브젝트 포톤 ID적용
            nPhotonviewID = OnPhotonViewID;

            //목표 오브젝트를 저장
            ObjTarget = PhotonView.Find(nPhotonviewID).gameObject;
            //animator = currAnime;


            //return;
        }
        //자신인 경우
        else
        {
            //안드로이드
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
                {
                    Vector3 pos = Input.GetTouch(0).position;

                    Ray ray = Camera.main.ScreenPointToRay(pos);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {
                        //animator.SetBool("run", true
                        ani(aniState.run);
                        agent.SetDestination(hit.point);
                        agent.isStopped = false;
                    }
                }
                if (agent.remainingDistance <= 0.2f && agent.velocity.magnitude >= 0.2f)
                {
                    Debug.Log("idle");
                    ani(aniState.run);
                    agent.isStopped = true;

                }
            }
            else //마우스 클릭 이동
            {
                if (Input.GetMouseButtonDown(0) && !this.GetComponent<DadeState>().bDead) //왼쪽 클릭
                {
                    //마우스가 UI에 적용이 안되었을 경우
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        Debug.Log("이동");
                        bAttack = true;
                        //이동 중에 적이 있는지 파악하고 공격하는 모드로 변경
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100))
                        {
                            ani(aniState.run);
                            agent.SetDestination(hit.point);

                            agent.isStopped = false;
                        }
                    }

                }
                else if (Input.GetMouseButtonDown(1) && !this.GetComponent<DadeState>().bDead)
                {
                    //마우스 가 UI에 적용이 안되었을 경우 에만 허용
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        Debug.Log("이동2");
                        bAttack = false;
                        bAI = false; // 자동 공격 모드를 끔
                                     //이동 중에 적이 있는지 파악하고 공격하는 모드로 변경
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 100))
                        {
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
                    bAttack = true;
                    bAI = true;
                    //agent.isStopped = true;
                }
            }
            if (Input.GetKeyDown(KeyCode.Q))
            {
                nSkillNum = 1;

                ani(aniState.Q);

                Skills = new GameObject[2];
                Skills[0] = PhotonNetwork.Instantiate("skillFolder/QSkill", QskillPos.transform.position, Quaternion.identity);
                Skills[1] = PhotonNetwork.Instantiate("skillFolder/QSkill2", QskillPos.transform.position, Quaternion.identity);
                Skills[0].transform.parent = QskillPos.transform;
                //Skills[1].GetComponent<SkillQ>().TargetTr = ObjTarget.transform;
                Skills[1].transform.parent = QskillPos.transform;

                Invoke("SkillDerete", 5.0f);

            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                nSkillNum = 2;
                ani(aniState.E);
                Skill = PhotonNetwork.Instantiate("skillFolder/Wskill", WskillPos.transform.position, Quaternion.identity);
                Skill.transform.parent = WskillPos.transform;
                Invoke("SkillDerete", 5.0f);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                nSkillNum = 3;
                ani(aniState.R);


            }

        }
    }

    void SkillDerete()
    {
        if (Skill != null)
        {
            Destroy(Skill);
            return;
        }
        if (Skills != null)
        {
            Destroy(Skills[0]);
            Destroy(Skills[1]);

        }

    }
    void Damage(float fdamage)
    {
        currHP -= fdamage;
        hpbar.fillAmount = currHP / initHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            isDie = true;
            this.GetComponent<DadeState>().bDead = true;
            bAttack = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
        }
    }
    void QDamage(float fdamage)
    {
        currHP -= fdamage;
        hpbar.fillAmount = currHP / initHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            isDie = true;
            this.GetComponent<DadeState>().bDead = true;
            bAttack = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
        }
    }
    void EDamage(float fdamage)
    {
        currHP -= fdamage;
        hpbar.fillAmount = currHP / initHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            isDie = true;
            this.GetComponent<DadeState>().bDead = true;
            bAttack = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
        }
    }
    void RDamage(float fdamage)
    {
        currHP -= fdamage;
        hpbar.fillAmount = currHP / initHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            isDie = true;
            this.GetComponent<DadeState>().bDead = true;
            bAttack = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
        }
    }
    private void LateUpdate()
    {
        if (isDie)
        {
            isDie = false;
            ani(aniState.death);
            StartCoroutine(this.Dead());

            agent.isStopped = true;
            Debug.Log("죽음");
        }
    }
    IEnumerator Dead()
    {
        yield return new WaitForSeconds(5.0f); //지연시간.
        //생존 값 초기화
        currHP = initHP;
        hpbar.fillAmount = 1; //curr/initHP
        this.GetComponent<DadeState>().bDead = false;
        transform.position = tsRespawn.position;
        ani(aniState.idle);
        agent.Warp(tsRespawn.position);
        agent.isStopped = false;
        rd.isKinematic = false;
        rd.isKinematic = true;
        if (photonView.IsMine)
        {
            StartCoroutine(this.State());
            //카메라를 자기 진형으로 이동
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
        //Destroy(gameObject);
    }


    ////////////////////////////////////////////////////////
    ///추적 부분
    private float dist;
    private float minimumdist = 20.0f;

    [Header("추적부분")]
    public bool bColl;
    bool bAI;
    GameObject ObjTarget; //목표 오브젝트
    public Transform tsTarget;

    //적 유닛 또는 적 건물 이동 처리
    [Header("공격목표 테그")]
    public string sAttackUnitTag;
    public string sAttackBuildingTag;

    [Header("원거리")]
    public GameObject objMissile;
    public Transform TsMissile;

    //바지름 안에 거리안에 적이 있는 지 파악
    public void NearEnemyAttack(Vector3 pos, float rafius)
    {

        Collider[] colls = Physics.OverlapSphere(pos, rafius);
        minimumdist = rafius;
        bColl = false;
        ObjTarget = null;

        for (int i = 0; i < colls.Length; i++)
        {
            if ((colls[i].tag == sAttackBuildingTag || colls[i].tag == sAttackUnitTag)
                && colls[i].GetComponent<DadeState>().bDead == false)
            {
                Vector3 objectPos = colls[i].transform.position;
                dist = Vector3.Distance(objectPos, transform.position);

                if (minimumdist > dist)
                {
                    ObjTarget = colls[i].gameObject;
                    minimumdist = dist;
                    bColl = true;

                }

            }
        }
        if (bColl)
        {
            tsTarget = ObjTarget.transform;
            bAI = true;
        }
        else
        {
            bAI = false;
        }

    }
    public void AttackMissile()
    {
        GameObject obj = Instantiate(objMissile, TsMissile.position, TsMissile.rotation);
        obj.GetComponent<MissileMove>().TargetTr = ObjTarget.transform;
    }
    //public void AttackSword()
    //{

    //    Sword.GetComponent<SwordAttack>().TargetTr.SendMessage("Damage", fAttackDamage);
    //    Debug.Log("근거리 공격");
    //}

    ///////////////////////////////////////////////////
    ///주변에 몬스터가 있는 경우  --> 추적 &공격,
    public enum minState { idle, trace, attack };
    public minState currState = minState.trace;

    public float attackDist = 3.0f;
    public float attackBuildingDist = 15.0f;
    public float fSearchDist = 15.0f;

    public bool isDie = false;

    public string playerKINDS;

    /// /////////////////////
    //자동 회복
    IEnumerator AutoHealing()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.11f);
            if (currHP < initHP)
            {
                currHP += 2;
                hpbar.fillAmount = currHP / initHP;
            }
        }

    }

    //목표 한 적이 가까우면 정지 상태로, 아니라면 추적 후 공격
    IEnumerator State()
    {
        //if (!photonView.IsMine) { }
        while (!isDie && !this.GetComponent<DadeState>().bDead) //안죽었을 때 해당 하는 2중 체크
        {
            yield return new WaitForSeconds(0.1f); //지연시간

            //내 주변의 적을 찾음, 근처에 적이 있는 지..
            NearEnemyAttack(transform.position, fSearchDist);

            //자동 모드 추적일경우
            if (bAI && bAttack)
            {
                float dist = Vector3.Distance(tsTarget.position, transform.position);

                //만약 건물이라면
                if (tsTarget.tag == sAttackBuildingTag && dist < attackBuildingDist)
                {
                    currState = minState.attack;
                    ani(aniState.attack);
                    if (playerKINDS == "A")
                    {
                        if (nSkillNum == 1) { tsTarget.SendMessage("QDamage", 30.0f); nSkillNum = 0; }
                        if (nSkillNum == 2) { tsTarget.SendMessage("EDamage", 35.0f); nSkillNum = 0; }
                        if (nSkillNum == 3) { tsTarget.SendMessage("RDamage", fAttackDamage); nSkillNum = 0; }
                        else
                        {
                            tsTarget.SendMessage("Damage", fAttackDamage);
                            Debug.Log("근거리 공격");
                        }
                    }
                }
                else if (dist < attackDist)
                {
                    currState = minState.attack;
                    ani(aniState.attack);
                    if (playerKINDS == "A")
                    {
                        if (nSkillNum == 1) { tsTarget.SendMessage("QDamage", fAttackDamage); }
                        if (nSkillNum == 2) { tsTarget.SendMessage("EDamage", fAttackDamage); }
                        if (nSkillNum == 3) { tsTarget.SendMessage("RDamage", fAttackDamage); }
                        else
                        {
                            tsTarget.SendMessage("Damage", fAttackDamage);
                            Debug.Log("근거리 공격");
                        }
                    }
                }
                else
                {
                    currState = minState.trace;
                    ani(aniState.run);
                }
                switch (currState)
                {
                    case minState.attack:
                        agent.isStopped = true;
                        transform.LookAt(ObjTarget.transform.position);
                        break;
                    case minState.trace:
                        agent.SetDestination(tsTarget.transform.position);
                        agent.isStopped = false;
                        break;
                    case minState.idle:
                        break;

                }


            }
            //로직 수정..(일명 땜빵)
            else if (curAni == aniState.attack) //공격중일때
            {
                if (!ObjTarget) //타겟 오브젝트가 없다면
                {
                    curAni = aniState.idle; //대기 상태로 전환
                    ani(aniState.idle);
                    Debug.Log("땜 빵 코 드");
                }

            }
            //목표 오브젝트를 찾는 부분,ViewID처리
            //타겟이 없다면 0을 전달
            if (!ObjTarget)
            {
                nPhotonviewID = 0;
            }
            else
            {
                nPhotonviewID = ObjTarget.GetComponent<PhotonView>().ViewID;
                Debug.Log("ViewID" + nPhotonviewID);
            }
        }
    }

    //===================================
    //애니메이션 관리 함수
    public enum aniState { idle, attack, run, death, Q, E, R };
    public aniState curAni = aniState.idle;

    void ani(aniState _curAni) //애니이션 상태 함수
    {
        animator.SetBool("idle", false);
        animator.SetBool("attack", false);
        animator.SetBool("run", false);
        animator.SetBool("death", false);
        animator.SetBool("Q", false);
        animator.SetBool("E", false);
        animator.SetBool("R", false);

        curAni = _curAni; //확인용

        switch (_curAni)
        {
            case aniState.idle:
                animator.SetBool("idle", true);
                break;
            case aniState.attack:
                animator.SetBool("attack", true);
                break;
            case aniState.run:
                animator.SetBool("run", true);
                break;
            case aniState.death:
                animator.SetBool("death", true);
                break;
            case aniState.Q:
                animator.SetTrigger("Q");
                break;
            case aniState.R:
                animator.SetTrigger("W");
                break;
            case aniState.E:
                animator.SetTrigger("E");
                break;
        }
        photonView.RPC("AniRequest", RpcTarget.Others, curAni);
    }

    [PunRPC]
    void AniRequest(aniState _curAni)
    {
        Debug.Log("전달 받은 애니 데이터:" + _curAni);
        animator.SetBool("idle", false);
        animator.SetBool("attack", false);
        animator.SetBool("run", false);
        animator.SetBool("death", false);
        animator.SetBool("Q", false);
        animator.SetBool("E", false);
        animator.SetBool("R", false);

        curAni = _curAni; //확인용

        switch (_curAni)
        {
            case aniState.idle:
                animator.SetBool("idle", true);
                break;
            case aniState.attack:
                animator.SetBool("attack", true);
                break;
            case aniState.run:
                animator.SetBool("run", true);
                break;
            case aniState.death:
                animator.SetBool("death", true);
                break;
            case aniState.Q:
                animator.SetTrigger("Q");
                break;
            case aniState.R:
                animator.SetTrigger("W");
                break;
            case aniState.E:
                animator.SetTrigger("E");
                break;
        }
    }
    private Vector3 currPos;
    private Quaternion currRot;
    public float OncurrHp = 100.0f;
    public int OnPhotonViewID;

    //private Animator currAni;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
        if (stream.IsWriting)
        {
            stream.SendNext(Playertr.position);
            stream.SendNext(Playertr.rotation);
            stream.SendNext(currHP);
            stream.SendNext(nPhotonviewID);
            stream.SendNext(isDie);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            currRot = (Quaternion)stream.ReceiveNext();
            OncurrHp = (float)stream.ReceiveNext();
            OnPhotonViewID = (int)stream.ReceiveNext();
            isDie = (bool)stream.ReceiveNext();

        }
    }
}

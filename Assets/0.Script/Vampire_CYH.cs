using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class Vampire_CYH : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("UI")]
    public GameObject MainCam;
    public Image hpBar;
    public Text NickName;

    [Header("유저 데이터")]
    public GameObject GameUserData;
    [SerializeField]
    public Transform RespawnTr;
    private Transform Tr;
    public Animator anim;
    NavMeshAgent agent;
    public int nPhotonViewID;
    [SerializeField]
    private float curHP = 2000.0f;
    [SerializeField]
    private float InitHP = 2000.0f;
    public float damage = 50.0f;

    public bool AttackMove;     // 공격 중 이동


    [Header("스킬")]
    public GameObject Q_skill;          // Q 스킬 투사체
    public GameObject W_skill;          // W 스킬 파티클
    public GameObject E_skill;

    public GameObject Q_Dist;           // Q 사정거리를 보여주는 이미지
    public GameObject W_Dist;           // W 사정거리를 보여주는 이미지
    public GameObject E_Dist;

    public bool isSkill = false;        // 스킬 사용중 체크
    public bool Skill_Q_Set;            // Q 스킬 시전 준비
    public bool Skill_W_Set;            // W 스킬 시전 준비
    public bool Skill_E_Set;

    public bool Skill_Q_ready = true;   // Q 스킬 쿨타임 완료
    public bool Skill_W_ready = true;   // W 스킬 쿨타임 완료
    public bool Skill_E_ready = true;










    private void Start()
    {
        MainCam = GameObject.FindGameObjectWithTag("MainCamera");
        agent = this.GetComponent<NavMeshAgent>();
        anim = this.GetComponent<Animator>();
        GameUserData = GameObject.Find("GameUserData").gameObject;
        Tr = transform;

        if (photonView.IsMine) // 자기 자신일 경우
        {
            StartCoroutine(this.State());               // 이동, 추적 등등 상태 확인
            StartCoroutine(this.AutoHealing());         // HP 자동 회복
        }

        MainCam.transform.position = new Vector3(transform.position.x, MainCam.transform.position.y,
            transform.position.z - MainCam.GetComponent<CameraMove>().nDes);

        curState = minState.idle;

        NickName.text = photonView.Owner.NickName;
    }

    private void Update()
    {
        if (!photonView.IsMine) // 내 캐릭터가 아니라면
        {
            if ((Tr.position - currPos).sqrMagnitude >= 10.0f * 10.0f)
            {
                Tr.position = currPos;
                Tr.rotation = CurrRot;
            }
            else
            {
                Tr.position = Vector3.Lerp(Tr.position, currPos, Time.deltaTime * 10.0f);
                Tr.rotation = Quaternion.Lerp(Tr.rotation, CurrRot, Time.deltaTime * 10.0f);
            }// 동기화가 맞지 않아서 위치나 회전값이 오차가 생기면 포톤으로 받아온 값으로 변경한다.
            curHP = OncurrHp;       // HP는 받아온 값 그대로 사용.
            hpBar.fillAmount = curHP / InitHP;
        }
        // State에서 목표가 포착되면 목표의 PhotonView ID를 가져오고
        // 해당 View ID를 PhotonView.Find로 검색하여 게임오브젝트로 가져와 저장

        // 목표 오브젝트 포톤 ID 적용
        nPhotonViewID = OnPhotonViewID;
        // 목표 오브젝트 저장
        if(nPhotonViewID == 0)
        {

        }
        else
        {
            TargetObj = PhotonView.Find(nPhotonViewID).gameObject;
        }

        if(Application.platform == RuntimePlatform.Android)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector3 pos = Input.GetTouch(0).position;

                Ray ray = Camera.main.ScreenPointToRay(pos);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit))
                {
                    //anim.SetBool("Run", true);
                    Animation(animState.Run);
                    agent.SetDestination(hit.point);
                    agent.isStopped = false;
                }
            }
            if (agent.remainingDistance <= 0.2f && agent.velocity.magnitude >= 0.2f)
            {
                //anim.SetBool("Run", false);
                Animation(animState.Idle);
            } // 안드로이드 이동 멈춤
        } // 안드로이드 터치 이동
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (isSkill)
            {
                agent.isStopped = true;
            }
            if (Input.GetMouseButtonDown(0) && !this.GetComponent<DeadState>().bDead && !isSkill)
            {
                // 마우스가 UI에 적용이 안됐을 경우에만 허용
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    AttackMove = true; // 공격 이동 > 이동 중에 색적과 공격으로 상태 변경

                    if (Physics.Raycast(ray, out hit))
                    {
                        Animation(animState.Run);
                        agent.SetDestination(hit.point);
                        agent.isStopped = false;
                    }
                }
            }
            else if (Input.GetMouseButtonDown(1) && !this.GetComponent<DeadState>().bDead && !isSkill)
            {
                // 마우스가 UI에 적용이 안됐을 경우에만 허용
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    AttackMove = false; // 공격 이동 아님
                    bAI = false;        // 자동 공격 모드 끔

                    if (Physics.Raycast(ray, out hit))
                    {
                        Animation(animState.Run);
                        agent.SetDestination(hit.point);
                        agent.isStopped = false;
                    } // 클릭한 곳을 목적지로 설정
                }
            }
            Vector3 MousePos;
            Ray MouseRay;
            RaycastHit MouseHit;

            MouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if ((Input.GetKeyDown(KeyCode.Q) && !this.GetComponent<DeadState>().bDead && Skill_Q_ready == true) || Skill_Q_Set == true)
            {
                isSkill = true;
                Skill_Q_Set = true;
                Skill_W_Set = false;
                Q_Dist.SetActive(true);
                W_Dist.SetActive(false);

                if(Physics.Raycast(MouseRay, out MouseHit, 100.0f))
                {
                    MousePos = MouseHit.point - Tr.position;
                    MousePos.y = 0.0f;
                    Tr.rotation = Quaternion.LookRotation(MousePos);
                }
                if (Input.GetMouseButtonDown(0))
                {
                    anim.SetBool("Q", true);
                    GameObject Q = Instantiate(Q_skill, MissileTr.position, Tr.rotation);
                    Q.GetComponent<Skill_Q>().sTargetUnitTag = sAttackUnitTag;
                    Skill_Q_Set = false;
                    Q_Dist.SetActive(false);
                    Skill_Q_ready = false;
                    isSkill = false;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    Skill_Q_Set = false;
                    Q_Dist.SetActive(false);
                    isSkill = false;
                }
            }// Q 스킬 사용
            else if (Input.GetKeyDown(KeyCode.W) && !this.GetComponent<DeadState>().bDead && Skill_W_ready == true || Skill_W_Set == true)
            {
                isSkill = true;
                Skill_W_Set = true;
                Skill_Q_Set = false;
                W_Dist.SetActive(true);
                Q_Dist.SetActive(false);

                if (Physics.Raycast(MouseRay, out MouseHit, 100.0f))
                {
                    MousePos = MouseHit.point - Tr.position;
                    MousePos.y = 0.0f;
                    Tr.rotation = Quaternion.LookRotation(MousePos);
                }
                if (Input.GetMouseButtonDown(0))
                {
                    anim.SetBool("W", true);
                    W_skill.SetActive(true);
                    W_skill.GetComponent<ParticleSystem>().Play();
                    Skill_W_Set = false;
                    W_Dist.SetActive(false);
                    Skill_W_ready = false;
                    isSkill = false;
                }
                if (Input.GetMouseButtonDown(1))
                {
                    Skill_W_Set = false;
                    W_Dist.SetActive(false);
                }
            }//W 스킬 사용
            if (agent.remainingDistance <= 0.2f && agent.velocity.magnitude >= 0.2f)
            {
                Animation(animState.Idle);
                // 멈추게 되면 자동 추적. 자동 공격 모드

                AttackMove = true;      // 공격 이동
                bAI = true;             // 자동 공격 모드

            }
        }    // 마우스 클릭 이동(PC)
    }

    void Damage(float fdamage)
    {
        curHP -= fdamage;
        if (curHP < 0)
            curHP = 0;
        hpBar.fillAmount = curHP / InitHP;

        // 죽은 상태면
        if(curHP <= 0 && !this.GetComponent<DeadState>().bDead)
        {
            isDie = true;
            this.GetComponent<DeadState>().bDead = true;
            AttackMove = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
        }
    }
    public void Drain(float DrainHP)
    {
        curHP += DrainHP;
        if(curHP > InitHP)
        {
            curHP = InitHP;
        }
    }
    void QDamage()
    {
        curHP -= 30;
        hpBar.fillAmount = curHP / InitHP;

        if (curHP < 0.0f && !this.GetComponent<DeadState>().bDead)
        {
            isDie = true;
            this.GetComponent<DeadState>().bDead = true;
            bColl = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
        }
    }
    void EDamage()
    {
        curHP -= 35;
        hpBar.fillAmount = curHP / InitHP;

        if (curHP < 0.0f && !this.GetComponent<DeadState>().bDead)
        {
            isDie = true;
            this.GetComponent<DeadState>().bDead = true;
            bColl = false;
            bAI = false;
            Debug.Log("챔피언 죽음");
        }
    }
    public void Q_End()
    {
        anim.SetBool("Q", false);
        StartCoroutine(Skill_CoolTime(4.0f));
    }
    public void W_End()
    {
        anim.SetBool("W", false);
    }
    private void LateUpdate()
    {
        if (isDie)
        {
            isDie = false;
            Animation(animState.Dead);


            StartCoroutine(this.Dead());

            agent.isStopped = true;
            Debug.Log("죽음");
        }
    }

    public IEnumerator Skill_CoolTime(float Skill)
    {
        if(Skill == 4.0f)
        {
            yield return new WaitForSeconds(Skill);
            Skill_Q_ready = true;
            Debug.Log("Q 사용 가능!");
        }
        else if (Skill == 8.0f)
        {
            yield return new WaitForSeconds(Skill);
            Skill_W_ready = true;
            Debug.Log("W 사용 가능!");
        }
        else if (Skill == 6.0f)
        {
            yield return new WaitForSeconds(Skill);
            Skill_E_ready = true;
            Debug.Log("E 사용 가능!");
        }

    }// Q스킬 쿨타임
    IEnumerator Dead()
    {
        if (photonView.IsMine)
            //MainCam.GetComponent<CameraMove>().Death();
        yield return new WaitForSeconds(5.0f);
        // 리스폰 후 초기화
        curHP = 2000.0f;
        hpBar.fillAmount = 1.0f;

        this.GetComponent<DeadState>().bDead = false;
        // Destroy는 코스트가 높으니 약간의 편법으로 맵 바깥으로 옮긴 후에
        // 리스폰 시간이 지나면 리스폰 위치로 다시 이동시키는 방법도 괜찮음.

        transform.position = RespawnTr.position;
        Animation(animState.Idle);

        agent.Warp(RespawnTr.position);
        agent.isStopped = false;

        if (photonView.IsMine)
        {
            StartCoroutine(this.State());       // 이동 & 추적 상태확인(다시 복구)

            MainCam.transform.position = new Vector3(transform.position.x, MainCam.transform.position.y,
                transform.position.z - MainCam.GetComponent<CameraMove>().nDes);
            //MainCam.GetComponent<CameraMove>().Respawn();
        }
    }

    ///////////////////// 추적

    private float dist;                 // 적과의 거리 체크
    private float minimumDist = 20.0f;  // 제일 가까운 적의 위치

    [Header("추적 부분")]
    public bool bColl;                         // 적을 찾았는지?
    public bool bAI;                           // 자동 추적 모드
    public GameObject TargetObj;               // 목표 오브젝트
    public Transform TargetTr;          // 현재 이동 목표. 길 찾기, 공격 시에 사용

    // 적 유닛 또는 적 건물 이름 처리
    [Header("공격목표태그")]
    public string sAttackUnitTag;
    public string sAttackTowerTag;

    public GameObject MissileObj;
    public Transform MissileTr;

    // 반지름의 거리안에 적이 있는지 파악
    public void NearEnemyAttack(Vector3 pos, float radius)
    {
        Collider[] colls = Physics.OverlapSphere(pos, radius);

        // 초기화
        minimumDist = radius;
        bColl = false;
        TargetObj = null;

        for (int i = 0; i < colls.Length; i++)
        {
            if ((colls[i].tag == sAttackUnitTag || colls[i].tag == sAttackTowerTag)
                && colls[i].GetComponent<DeadState>().bDead == false)
            {
                Vector3 objectPos = colls[i].transform.position;
                dist = Vector3.Distance(objectPos, transform.position);

                if(minimumDist > dist)
                {
                    TargetObj = colls[i].gameObject;    // 가장 가까운 적
                    minimumDist = dist;
                    bColl = true;
                }
            }
        }
        if (bColl)      // 타겟팅한 적을 따라간다.
        {
            TargetTr = TargetObj.transform; // 찾은 적 추적
            bAI = true;     // 추적 모드 ON
        }
        else
        {
            bAI = false;    // 추적 모드 OFF
        }
    }

    public void ShotMagic()
    {
        if (TargetObj)
        {
            // 함수 호출되면 원거리 미사일 발사.
            GameObject obj = Instantiate(MissileObj, MissileTr.position, MissileTr.rotation);
            obj.GetComponent<MissileMove>().TargetTr = TargetObj.transform;
            obj.GetComponent<MissileMove>().fAttackDamage = damage;
        }
    }

    // 주변에 몬스터가 있을 경우 --> 추적 & 공격

    public enum minState { idle, trace, attack };
    public minState curState = minState.idle;

    public float AttackDist = 10.0f;            // 공격 모드 도달 거리
    public float AttackBuildingDist = 10.0f;    // 타워 공격 도달 거리
    public float fSearchDist = 15.0f;           // 적 검색 거리

    public bool isDie = false;                  // 죽음 상태

    IEnumerator AutoHealing()       // HP 자동 회복
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if(curHP < InitHP)
            {
                curHP += 2;
                hpBar.fillAmount = curHP / InitHP;
            }
        }
    }

    IEnumerator State()
    {
        while (!isDie && !this.GetComponent<DeadState>().bDead)     // 안죽었을 때(2중 체크)
        {
            yield return new WaitForSeconds(0.1f);

            // 색적 ON
            NearEnemyAttack(transform.position, fSearchDist);

            // 자동 추적 모드일 경우
            if (bAI && AttackMove)
            {
                float dist = Vector3.Distance(TargetTr.position, transform.position);

                // 타워 공격
                if (TargetTr.tag == sAttackTowerTag && dist > AttackBuildingDist)
                {
                    curState = minState.attack;
                    Animation(animState.Atk);

                }
                else if (dist < AttackDist) // 유닛 공격
                {
                    curState = minState.attack;
                    Animation(animState.Atk);

                }
                else
                {
                    curState = minState.trace;
                    Animation(animState.Run);

                }


                // 이동 처리

                switch (curState)
                {
                    case minState.attack:
                        agent.isStopped = true;
                        transform.LookAt(TargetObj.transform.position); // 적을 바라본다
                        break;
                    case minState.trace:
                        agent.SetDestination(TargetTr.transform.position);
                        agent.isStopped = false;
                        break;
                    case minState.idle:
                        agent.isStopped = true;
                        break;
                }
            }
            // 로직 수정(일명 땜빵) 혹 나중에 팀단위로 개발할 경우 이런 땜빵 코드는 항상 기록해둘것!!
            else if (curAnim == animState.Atk)  // 공격중일 때
            {
                if (!TargetObj) // 타겟이 없다면
                {
                    curAnim = animState.Idle;       // 대기 상태로 전환
                    Animation(animState.Idle);      // 애니와 state 모두 대기상태로 변환
                }
            }

            // 목표 오브젝트를 찾는 부분 viewID 처리 부분
            // 타겟이 없다면 0 전달
            if (!TargetObj)
            {
                nPhotonViewID = 0;
            }
            else
            {
                nPhotonViewID = TargetObj.GetComponent<PhotonView>().ViewID;
            }
        
        }       
    }


    public enum animState { Idle, Atk, Run, Dead };  // 4가지 액션 상태.
    public animState curAnim = animState.Idle;      // 인스펙터 창에서 확인용

    // 애니메이션 관리 함수
    private void Animation(animState _curAnim)      // 애니메이션 상태 변환 함수
    {
        anim.SetBool("Idle", false);
        anim.SetBool("Run", false);
        anim.SetBool("Atk", false);
        anim.SetBool("Dead", false);

        curAnim = _curAnim;     // 확인용

        switch (_curAnim)
        {
            case animState.Idle:
                anim.SetBool("Idle", true);

                break;
            case animState.Run:
                anim.SetBool("Run", true);

                break;
            case animState.Atk:
                anim.SetBool("Atk", true);

                break;
            case animState.Dead:
                anim.SetBool("Dead", true);

                break;
        }

        // RPC 전송
        photonView.RPC("AniRequest", RpcTarget.Others, _curAnim);    // 나의 챔피언 애니 전송
    }

    [PunRPC]
    void AniRequest(animState _curAnim)
    {
        anim.SetBool("Idle", false);
        anim.SetBool("Run", false);
        anim.SetBool("Atk", false);
        anim.SetBool("Dead", false);

        curAnim = _curAnim;     // 확인용

        switch (_curAnim)
        {
            case animState.Idle:
                anim.SetBool("Idle", true);
                break;
            case animState.Run:
                anim.SetBool("Run", true);
                break;
            case animState.Atk:
                anim.SetBool("Atk", true);
                break;
            case animState.Dead:
                anim.SetBool("Dead", true);
                break;
        }
    }
    private Vector3 currPos;        // 실시간으로 받는 위치 값
    private Quaternion CurrRot;     // 실시간으로 받는 회전 값
    public float OncurrHp = 100.0f; // 실시간으로 받는 현재 HP
    public int OnPhotonViewID;      // 실시간으로 받는 목표의 포톤 ViewID
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
        if (stream.IsWriting) // 데이터 전송만 함
        {
            stream.SendNext(Tr.position);   // 나의 위치 값
            stream.SendNext(Tr.rotation);   // 나의 회전 값
            stream.SendNext(curHP);         // 나의 현재 HP
            stream.SendNext(nPhotonViewID);
            stream.SendNext(isDie);
        }
        else
        {
            currPos = (Vector3)stream.ReceiveNext();
            CurrRot = (Quaternion)stream.ReceiveNext();
            OncurrHp = (float)stream.ReceiveNext();
            OnPhotonViewID = (int)stream.ReceiveNext();
            isDie = (bool)stream.ReceiveNext();
        }
    }
}

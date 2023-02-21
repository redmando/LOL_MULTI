using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{ 
    // 게임 정보.
    [Header("게임정보")]
    public int nMyActorNumber;          //  내 접속 번호. (룸 내 고유번호)
    public GameObject objGameUserData;  //  게임 유저 데이터.

    public GameObject objVictoryPanel;  //  승리 패널.
    public GameObject objLosePanel;     //  패배 패널.


    // 전사 미니언
    [Header("전사 미니언")]
    public GameObject Minions_B_Type01; // 중앙 공격
    public GameObject Minions_B_Type02; // top 공격
    public GameObject Minions_B_Type03; // bottom 공격
    public GameObject Minions_T_Type01; // 중앙 공격
    public GameObject Minions_T_Type02; // top 공격
    public GameObject Minions_T_Type03; // bottom 공격

    // 마법사 미니언
    [Header("마법사 미니언")]
    public GameObject Minions2_B_Type01; // 중앙 공격
    public GameObject Minions2_B_Type02; // top 공격
    public GameObject Minions2_B_Type03; // bottom 공격
    public GameObject Minions2_T_Type01; // 중앙 공격
    public GameObject Minions2_T_Type02; // top 공격
    public GameObject Minions2_T_Type03; // bottom 공격

    // 공성 미니언
    [Header("공성 미니언")]
    public GameObject Minions3_B_Type01; // 중앙 공격
    public GameObject Minions3_B_Type02; // top 공격
    public GameObject Minions3_B_Type03; // bottom 공격
    public GameObject Minions3_T_Type01; // 중앙 공격
    public GameObject Minions3_T_Type02; // top 공격
    public GameObject Minions3_T_Type03; // bottom 공격

    // 슈퍼(공성) 미니언
    [Header("슈퍼(공성) 미니언")]
    public GameObject Minions4_B_Type01; // 중앙 공격
    public GameObject Minions4_B_Type02; // top 공격
    public GameObject Minions4_B_Type03; // bottom 공격
    public GameObject Minions4_T_Type01; // 중앙 공격
    public GameObject Minions4_T_Type02; // top 공격
    public GameObject Minions4_T_Type03; // bottom 공격


    [Header("생성 위치")]
    public Transform tsfRespawnB;       // 생성 위치. 
    public Transform tsfRespawnT;       // 생성 위치.

    //public Transform tsfNexusB;       // 넥서스 생성 위치.(Bottom)
    //public Transform tsfNexusT;       // 넥서스 생성 위치.(TOP)

    public Transform tsf_T_Nexus;
    public Transform tsf_T_Inhibitor_B;
    public Transform tsf_T_Inhibitor_M;
    public Transform tsf_T_Inhibitor_T;
    public Transform tsf_T_Turret_B;
    public Transform tsf_T_Turret_B_1;
    public Transform tsf_T_Turret_B_2;
    public Transform tsf_T_Turret_M;
    public Transform tsf_T_Turret_M1;
    public Transform tsf_T_Turret_M2;
    public Transform tsf_T_Turret_T;
    public Transform tsf_T_Turret_T1;
    public Transform tsf_T_Turret_T2;

    public Transform tsf_B_Nexus;
    public Transform tsf_B_Inhibitor_B;
    public Transform tsf_B_Inhibitor_M;
    public Transform tsf_B_Inhibitor_T;
    public Transform tsf_B_Turret_B;
    public Transform tsf_B_Turret_B1;
    public Transform tsf_B_Turret_B2;
    public Transform tsf_B_Turret_M;
    public Transform tsf_B_Turret_M1;
    public Transform tsf_B_Turret_M2;
    public Transform tsf_B_Turret_T;
    public Transform tsf_B_Turret_T1;
    public Transform tsf_B_Turret_T2;

    public bool bUpgradeTopTop;           // 윗마을 공성 유닛이 업그레이드 되었다면.!! (top)
    public bool bUpgradeTopMiddle;        // 윗마을 공성 유닛이 업그레이드 되었다면.!! (middle)
    public bool bUpgradeTopBottom;        // 윗마을 공성 유닛이 업그레이드 되었다면.!! (bottom)
    public bool bUpgradeBottomTop;        // 아랫마을 공성 유닛이 업그레이드 되었다면.!! (top)
    public bool bUpgradeBottomMiddle;     // 아랫마을 공성 유닛이 업그레이드 되었다면.!! (middle)
    public bool bUpgradeBottomBottom;     // 아랫마을 공성 유닛이 업그레이드 되었다면.!! (bottom)

    public bool bDestoryTopNexus;         // 윗마을 넥서스가 파괴 되었다면..
    public bool bDestoryBottomNexus;      // 아랫마을 넥서스가 파괴 되었다면..

    // 게임 정지
    public bool bGamePause;                // 게임을 일시 정지 시킴.

    public enum TeamState { top, bottom };      // 2가지 팀 상태
    public TeamState MyTeam = TeamState.top;    // 초기화..


    private static GameManager instance = null;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public static GameManager Instance
    {
        get
        {
            if(instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    void Start()
    {
        bGamePause = false;

        // 네크워크 복구
        PhotonNetwork.IsMessageQueueRunning = true;

        //InvokeRepeating("SpawnMinions", 0, 15.0f);       //  근거리 미니언
        //InvokeRepeating("SpawnMinions2", 1, 15.0f);      //  원거리 미니언
        //InvokeRepeating("SpawnMinions3", 2, 15.0f);      //  공성 미니언

        // GameUserData 에서 게임 오브젝트로 연결.
        objGameUserData = GameObject.Find("GameUserData");

        nMyActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        Debug.Log("nMyActorNumber: " + nMyActorNumber);

        GameObject objChampion;

        for(int i = 0; i < 10;i++)
        {
            // 나의 정보를 찾음.
            if(objGameUserData.GetComponent<GameUserData>().nActorNumber[i] == nMyActorNumber)
            {
                int _nChampionNum =  objGameUserData.GetComponent<GameUserData>().nChampionNum[i];

                // 윗 마을 캐릭터인 경우
                if ( i < 5 )     // 슬롯 번호를 체크.
                {
                    //  팀 설정.
                    MyTeam = TeamState.top;

                    // 챔피언 번호에 따라 소환.
                    Debug.Log("nActorNumber[i] top:" + objGameUserData.GetComponent<GameUserData>().nActorNumber[i]);

                    // 챔피언 설정
                    if (_nChampionNum == 0)
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_T", tsfRespawnT.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 1)
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_T", tsfRespawnT.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 3)
                    {
                        objChampion = PhotonNetwork.Instantiate("3.Champion_T", tsfRespawnT.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 4)
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_T", tsfRespawnT.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 5)
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_T", tsfRespawnT.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 6)
                    {
                        objChampion = PhotonNetwork.Instantiate("6.vampire_a_lusth_T", tsfRespawnT.position, Quaternion.identity);
                    }
                    else
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_T", tsfRespawnT.position, Quaternion.identity);
                    }
                }
                else
                {
                    //  팀 설정.
                    MyTeam = TeamState.bottom;

                    // 챔피언 번호에 따라 소환.
                    Debug.Log("nActorNumber[i] bottom:" + objGameUserData.GetComponent<GameUserData>().nActorNumber[i]);

                    // 챔피언 설정
                    if (_nChampionNum == 0)
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_B", tsfRespawnB.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 1)
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_B", tsfRespawnB.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 3)
                    {
                        objChampion = PhotonNetwork.Instantiate("3.Champion_B", tsfRespawnB.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 4)
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_B", tsfRespawnB.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 5)
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_B", tsfRespawnB.position, Quaternion.identity);
                    }
                    else if (_nChampionNum == 6)
                    {
                        objChampion = PhotonNetwork.Instantiate("6.vampire_a_lusth_B", tsfRespawnB.position, Quaternion.identity);
                    }
                    else
                    {
                        objChampion = PhotonNetwork.Instantiate("TT_Mage_B", tsfRespawnB.position, Quaternion.identity);
                    }
                }

                // 메인 카메라에 대상 게임 오브젝트를 지정.!!
                GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMove>().obj[0] = objChampion;

                break;
            }
        }

        // 방장인 경우 넥서스 생성해 주기..
        if (PhotonNetwork.IsMasterClient)
        {
            InvokeRepeating("SpawnMinions", 0, 15.0f);       //  근거리 미니언
            InvokeRepeating("SpawnMinions2", 1, 15.0f);      //  원거리 미니언
            InvokeRepeating("SpawnMinions3", 2, 15.0f);      //  공성 미니언

            PhotonNetwork.Instantiate("T_Nexus"      , tsf_T_Nexus      .position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Inhibitor_B", tsf_T_Inhibitor_B.position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Inhibitor_M", tsf_T_Inhibitor_M.position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Inhibitor_T", tsf_T_Inhibitor_T.position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Turret_B"   , tsf_T_Turret_B   .position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Turret_B_1" , tsf_T_Turret_B_1 .position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Turret_B_2" , tsf_T_Turret_B_2 .position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Turret_M"   , tsf_T_Turret_M   .position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Turret_M1"  , tsf_T_Turret_M1  .position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Turret_M2"  , tsf_T_Turret_M2  .position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Turret_T"   , tsf_T_Turret_T   .position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Turret_T1"  , tsf_T_Turret_T1  .position, Quaternion.identity);
            PhotonNetwork.Instantiate("T_Turret_T2"  , tsf_T_Turret_T2  .position, Quaternion.identity);
            //.....건물들 다..
            PhotonNetwork.Instantiate("B_Nexus"      , tsf_B_Nexus      .position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Inhibitor_B", tsf_B_Inhibitor_B.position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Inhibitor_M", tsf_B_Inhibitor_M.position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Inhibitor_T", tsf_B_Inhibitor_T.position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Turret_B"   , tsf_B_Turret_B   .position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Turret_B1"  , tsf_B_Turret_B1  .position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Turret_B2"  , tsf_B_Turret_B2  .position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Turret_M"   , tsf_B_Turret_M   .position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Turret_M1"  , tsf_B_Turret_M1  .position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Turret_M2"  , tsf_B_Turret_M2  .position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Turret_T"   , tsf_B_Turret_T   .position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Turret_T1"  , tsf_B_Turret_T1  .position, Quaternion.identity);
            PhotonNetwork.Instantiate("B_Turret_T2"  , tsf_B_Turret_T2  .position, Quaternion.identity);
        }


    }

    void SpawnMinions()
    {
        // Bottom
        PhotonNetwork.Instantiate("B_T_0_0", tsfRespawnB.position, Quaternion.identity);
        PhotonNetwork.Instantiate("B_T_1_0", tsfRespawnB.position, Quaternion.identity);
        PhotonNetwork.Instantiate("B_T_2_0", tsfRespawnB.position, Quaternion.identity);

        // Top
        PhotonNetwork.Instantiate("T_B_0_0", tsfRespawnT.position, Quaternion.identity);
        PhotonNetwork.Instantiate("T_B_1_0", tsfRespawnT.position, Quaternion.identity);
        PhotonNetwork.Instantiate("T_B_2_0", tsfRespawnT.position, Quaternion.identity);
    }

    void SpawnMinions2()
    {
        // Bottom
        PhotonNetwork.Instantiate("B_T_0_1", tsfRespawnB.position, Quaternion.identity);
        PhotonNetwork.Instantiate("B_T_1_1", tsfRespawnB.position, Quaternion.identity);
        PhotonNetwork.Instantiate("B_T_2_1", tsfRespawnB.position, Quaternion.identity);

        // Top
        PhotonNetwork.Instantiate("T_B_0_1", tsfRespawnT.position, Quaternion.identity);
        PhotonNetwork.Instantiate("T_B_1_1", tsfRespawnT.position, Quaternion.identity);
        PhotonNetwork.Instantiate("T_B_2_1", tsfRespawnT.position, Quaternion.identity);
    }

    void SpawnMinions3()
    {
        if (!bUpgradeTopTop)      // 업그레이드가 안되었다면
        {
            // Top
            PhotonNetwork.Instantiate("T_B_0_2", tsfRespawnT.position, Quaternion.identity);
        }
        else
        {
            // Top
            PhotonNetwork.Instantiate("T_B_0_3", tsfRespawnT.position, Quaternion.identity);
        }

        if (!bUpgradeTopMiddle)      // 업그레이드가 안되었다면
        {
            // Top
            PhotonNetwork.Instantiate("T_B_1_2", tsfRespawnT.position, Quaternion.identity);
        }
        else
        {
            // Top
            PhotonNetwork.Instantiate("T_B_1_3", tsfRespawnT.position, Quaternion.identity);
        }


        if (!bUpgradeTopBottom)      // 업그레이드가 안되었다면
        {
            // Top
            PhotonNetwork.Instantiate("T_B_2_2", tsfRespawnT.position, Quaternion.identity);
        }
        else
        {
            // Top
            PhotonNetwork.Instantiate("T_B_2_3", tsfRespawnT.position, Quaternion.identity);
        }

        //---------------------------------------------------------------------------------

        if (!bUpgradeBottomTop)      // 업그레이드가 안되었다면
        {
            // Bottom
            PhotonNetwork.Instantiate("B_T_0_2", tsfRespawnB.position, Quaternion.identity);
        }
        else
        {
            // Bottom
            PhotonNetwork.Instantiate("B_T_0_3", tsfRespawnB.position, Quaternion.identity);
        }

        if (!bUpgradeBottomMiddle)      // 업그레이드가 안되었다면
        {
            // Bottom
            PhotonNetwork.Instantiate("B_T_1_2", tsfRespawnB.position, Quaternion.identity);
        }
        else
        {
            // Bottom
            PhotonNetwork.Instantiate("B_T_1_3", tsfRespawnB.position, Quaternion.identity);
        }

        if (!bUpgradeBottomBottom)      // 업그레이드가 안되었다면
        {
            // Bottom
            PhotonNetwork.Instantiate("B_T_2_2", tsfRespawnB.position, Quaternion.identity);
        }
        else
        {
            // Bottom
            PhotonNetwork.Instantiate("B_T_2_3", tsfRespawnB.position, Quaternion.identity);
        }
    }

    // 게임 일시 정지
    public void GamePause()
    {
        if(bGamePause)
        {
            Time.timeScale = 0;
            bGamePause = false;
        }
        else
        {
            Time.timeScale = 1;     // 배속 x1   2 = x2
            bGamePause = true;
        }
    }

    // 게임 종료 처리
    public void GameOver()
    {
        // 아랫마을 넥서스가 부셔 졌다면. 윗마을 승리.
        //f(GameManager.Instance.bDestoryBottomNexus)
        if (bDestoryBottomNexus)
        {
            // 1. 부서진 넥서스 위치로 카메라 이동. 본진으로 이동.
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMove>().CameraMoveBottomDirect();

            // 2. 게임 정지(생략)

            // 3. 게임팀에 따라 각각 다른 결과 UI를 보여줌.
            if (MyTeam == TeamState.top)
                objVictoryPanel.SetActive(true);

            if (MyTeam == TeamState.bottom)
                objLosePanel.SetActive(true);
        }

        // 아랫 마을 승리 라면..
        if(bDestoryTopNexus)
        {
            // 1. 부서진 넥서스 위치로 카메라 이동. 본진으로 이동.
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMove>().CameraMoveTopDirect();

            // 2. 게임 정지(생략)

            // 3. 게임팀에 따라 각각 다른 결과 UI를 보여줌.
            if (MyTeam == TeamState.bottom)
                objVictoryPanel.SetActive(true);

            if (MyTeam == TeamState.top)
                objLosePanel.SetActive(true);
        }


    }


    // 게임 나가기 버튼.
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();     //  어플리케이션 종료
#endif
    }
}

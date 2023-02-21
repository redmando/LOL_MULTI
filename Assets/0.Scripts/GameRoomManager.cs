using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class UserData
{
    // 클라이언트가 접속할 때에 어느쪽도 정의가 안되어 있는 경우에는 유저ID를 송신하지 않습니다.
    // Photon Server는 그 경우 GUID를 송신 합니다.
    // userID가 클라이언트로 부터 접속시에 송신되는 경우,
    // PhotonNetwork.AuthValues,  NickName/PlayerName은 입실 시에 송신 됩니다.

    // 유저 정보
    public int nActorNumber;    //  고유 아이디 권장 안함. 방이 생성후 액터 고유 넘버로 사용.
    //public string sUserID;      //  유저 고유 아이디.
    public string sNickname;    //  유저 닉네임.
    public bool bMasterClient;  //  방장인지 저장.

    // 게임 정보
    public bool bReady;         //  레디 상태 인가.
    //public int nSlotNum;      //  몇번째 슬롯인가?(UserData 자체를 배열로 선언 => 필요 없음)
    public int nChampionNum;    //  챔피언 선택 정보.

    // 초기화..    
    public UserData(int _nActorNumber, string _sNickname, bool _bMasterClient, bool _bReady, int _nChampionNum )
    {
        this.nActorNumber = _nActorNumber;
        //this.sUserID = _sUserID;
        this.sNickname = _sNickname;
        this.bMasterClient = _bMasterClient;
        this.bReady = _bReady;
        this.nChampionNum = _nChampionNum;
    }

    public UserData(UserData C)     // 복사 생성자.
    {
        this.nActorNumber = C.nActorNumber;
        this.sNickname = C.sNickname;
        this.bMasterClient = C.bMasterClient;
        this.bReady = C.bReady;
        this.nChampionNum = C.nChampionNum;
    }

    public void Clear()
    {
        this.nActorNumber = 0;
        this.sNickname = "";
        this.bMasterClient = false;
        this.bReady = false;
        this.nChampionNum = 0;
    }
}

public class GameRoomManager : MonoBehaviourPunCallbacks
{
    // test용..  출력 & 확인 용
    public Text[] NickName;             //  닉네임 표시 UI
    public Text[] ChampionName;         //  캐릭터 닉네임 표시
    public GameObject[] objReady;       //  레디 표시 UI
    public Image[] imgCrown;            //  방장 마크
    public Transform[] cameraCurrTr;    //  현재 카메라 위치.(챔피언 각각)
    public Transform[] cameraInitTr;    //  초기값 카메라 위치.

    [Header("게임오브젝트")]
    public GameObject[] objSlot;        //  팀 위치 선택 슬롯!!

    // 나의 챔피언 선택 버튼
    public GameObject objSelectButton;  //  챔피언 선택 좌우 버튼.
    public GameObject objStartButton;   //  게임 시작 버튼
    public GameObject objReadyButton;   //  게임 레디 버튼

    public int nChampionCount;          //  챔피언 카운트 (챔피언 개수)
    public string[] sChampionName;      //  챔피언 이름 (인스팩터 저장)

    public bool[] bSelect;              //  선택되어있는지. 유저가 있는지 슬롯 체크.  0~4 윗마을 5~9 아랫마을

    // 유저의 데이터를 선언.(저장소)
    private UserData[] userData = new UserData[]
    {
        new UserData (0, "", false, false, 0),
        new UserData (0, "", false, false, 0),
        new UserData (0, "", false, false, 0),
        new UserData (0, "", false, false, 0),
        new UserData (0, "", false, false, 0),
        new UserData (0, "", false, false, 0),
        new UserData (0, "", false, false, 0),
        new UserData (0, "", false, false, 0),
        new UserData (0, "", false, false, 0),
        new UserData (0, "", false, false, 0)
    };

    // 채팅 관련
    public Text msgList;            //  채팅 내용 출력
    public InputField ifSendMsg;    //  채팅 입력
    public Text playerCount;        //  참여 플레이어 수

    // 게임 정보
    [Header("게임정보")]
    public int nMyActorNumber;      //  내 접속 번호. (룸 내 고유번호)
    //public string sMyNickName;      //  닉네임
    //public int nMyPlayerLoginNum;   //  접속 순서
    //public int nMyPlayerSelectNum;  //  선택 위치..                                 
    //public int nMySlotSelect;          // 나의 슬롯 선택    
    //public int nMyChampionSelect;       // 나의 챔피언 선택

    [Header("게임데이터")]
    public GameObject GameUserData;     //  게임 유저 데이터 게임오브젝트.

    void Start()
    {
        //// test
        //for(int i = 0; i  < NickName.Length ; i++)
        //{
        //    NickName[i].text = "test " + i.ToString();
        //}
        //// test2
        //NickName[0].text = PhotonNetwork.NickName;

        // 내 정보 업데이트
        nMyActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;     //  나의 접속 번호.(나름 고유 번호)        
        //sMyNickName = PhotonNetwork.NickName;                   //  나의 닉네임
        //nMyPlayerLoginNum = PhotonNetwork.PlayerList.Length;    //  유저가 접속할떄마다 체크 & 업데이트.(필요)

        //Debug.Log("마스터인가?:" + PhotonNetwork.IsMasterClient);
        //Debug.Log("마스터 이름:" + PhotonNetwork.MasterClient);
        //Debug.Log("닉네임:" + PhotonNetwork.NickName);
        //Debug.Log("LocalPlayer:" + PhotonNetwork.LocalPlayer);
        //Debug.Log("LocalPlayer UserId:" + PhotonNetwork.LocalPlayer.UserId);
        //Debug.Log("LocalPlayer NickName:" + PhotonNetwork.LocalPlayer.NickName);
        //Debug.Log("LocalPlayer ActorNumber:" + PhotonNetwork.LocalPlayer.ActorNumber);
        //Debug.Log("PlayerList:" + PhotonNetwork.PlayerList);


        // 메세지 큐를 다시 활성화. PhotonNetwork의 데이터 통신을 다시 연결해 준다.
        PhotonNetwork.IsMessageQueueRunning = true;
        // 입장 플레이어 확인.
        Invoke("CheckPlayerCount", 0.5f);


        // 방장(Master Client) 이라면 (테스트로 끝자리(10번째)를 차지하도록 수정)
        if(PhotonNetwork.IsMasterClient)
        {
            //// 레디 UI 표시
            //ResetReadyView(9);
            //// 방장 UI 표시
            //ResetMasterView(9);
            //// 닉네임 슬롯에서 다 지우기
            //ResetNicknameView();
            //// 닉네임 UI 표시
            //SetNicknameView(9, PhotonNetwork.NickName);
            //// 챔피언 관련 초기화
            //ResetChampionView();
            //// 챔피언 기본 표시
            //SetChampionView(9, 0);
            //// 나의 슬롯 (캐릭터 자리) 저장
            //nMySlotSelect = 9;
            //// 나의 챔피언 초기화
            //nMyChampionSelect = 0;
            //// 챔피언 선택버튼 위치 이동
            //objSelectButton.transform.position = objSlot[9].transform.position;

            Debug.Log("=== 마스터 클라이언트다아아아~~");

            //--------------------------------------------

            // 1. 정보 갱신. (5가지 데이터 갱신)   test위해 2번째 칸으로 초기화. 원래 0번 칸..
            SetActorNumber(0, nMyActorNumber);      //  액터 넘버 갱신.   PhotonNetwork.LocalPlayer.ActorNumber;
            SetNickname(0, PhotonNetwork.NickName); //  닉네임 저장 갱신
            SetMaster(0, true);                     //  방장 정보 갱신
            SetReady(0, true);                      //  레디 정보 갱신
            SetChampionNum(0, 0);                   //  챔피언 번호 0번으로 갱신

            // 2. UI & 화면 출력
            UpdateSlot();

            // 3. Start 버튼 활성화.
            objStartButton.SetActive(true);
            objReadyButton.SetActive(false);

            // 4. Start 버튼 기능 활성화 확인.
            UpdateStartButton();

        }
        else        // 방장이 아니라면... MC 요청!!!
        {
            // 0. 초기화?
            for(int i = 0; i < 10;i++)
            {
                userData[i].Clear();
            }

            photonView.RPC("McDataRequest", RpcTarget.MasterClient, nMyActorNumber);    // 나의 액터 정보 전송..

            // 1. Ready 버튼 활성화.
            objStartButton.SetActive(false);
            objReadyButton.SetActive(true);
        }       
    }

    [PunRPC]
    void McDataRequest(int _nActorNumber)   //  마스터 클라이언트 정보 요청, 처음 로그인해서 방장 및 다른 사람들 정보를 전달.
    {
        for (int i = 0; i < 10; i++)
        {
            // 요청자(유저) 정보 빼고 다 전송.
            if(userData[i].nActorNumber != 0 && userData[i].nActorNumber != _nActorNumber)
            {
                // 메세지 전송 부분
                photonView.RPC("NewUserMsg", RpcTarget.Others, i,
                    userData[i].nActorNumber, userData[i].sNickname, userData[i].bMasterClient, userData[i].bReady, userData[i].nChampionNum);

                Debug.Log("방장 : NewUserMsg " + i);
            }
        }
    }

    [PunRPC]
    void NewUserMsg(int _i, int _ActorNumber, string _NickName, bool _bMasterClient, bool _bReady, int _nChampionNum)
    {
        SetActorNumber(_i, _ActorNumber);
        SetNickname(_i, _NickName);
        SetMaster(_i, _bMasterClient);
        SetReady(_i, _bReady);
        SetChampionNum(_i, _nChampionNum);

        // 업데이트
        UpdateSlot();
    }





    //==================================================
    // UI 관련.

    // 레디 초기화 함수(방장용)
    void ResetReadyView(int nSlot)
    {
        for(int i = 0; i < 10;i++)
        {
            objReady[i].SetActive(false);
        }
        objReady[nSlot].SetActive(true);
    }
    // 레디 초기화 함수
    void ResetReadyView()
    {
        for (int i = 0; i < 10; i++)
        {
            objReady[i].SetActive(false);
        }
    }
    // 레디 세팅 함수
    void SetReadyView(int nSlot, bool bView)
    {
        objReady[nSlot].SetActive(bView);
    }

    // 방장 마크 초기화 함수
    void ResetMasterView(int nSlot)
    {
        for (int i = 0; i < 10; i++)
        {
            imgCrown[i].enabled = false;
        }
        imgCrown[nSlot].enabled = true;
    }
    // 방장 마크 세팅 함수
    void SetMasterView(int nSlot, bool bView)
    {
        imgCrown[nSlot].enabled = bView;
    }

    // 닉네임 초기화 함수
    void ResetNicknameView()
    {
        for (int i = 0; i < 10; i++)
        {
            NickName[i].text = "";
        }
    }
    // 닉네임 세팅 함수
    void SetNicknameView(int nSlot, string sName)
    {
        NickName[nSlot].text = sName; // PhotonNetwork.NickName;
    }

    // 챔피언 이름 다 지우기.
    void ResetChampionNameView()
    {
        for (int i = 0; i < 10; i++)
        {
            ChampionName[i].text = "";
        }
    }
    // 챔피언 이름 세팅
    void SetChampionNameView(int nSlot, string sName)
    {
        ChampionName[nSlot].text = sName;
    }

    // 챔피언 선택 카메라 초기화 함수
    void ResetChampionView()
    {
        // 전부 다 안보이도록 처리..
        for (int i = 0; i < 10; i++)
        {
            cameraCurrTr[i].transform.position = cameraInitTr[10].transform.position;
        }
    }
    // 챔피언 선택 세팅 함수
    void SetChampionView(int nCameraNum, int nChampionNum)
    {
        // 선택된 챔피언 위치로 카메라 이동.
        cameraCurrTr[nCameraNum].transform.position = cameraInitTr[nChampionNum].transform.position;
    }

    // 시작 버튼 활성화 처리.
    void SetStartButton(bool bSet)
    {
        objStartButton.GetComponent<Button>().interactable = bSet;
    }


    //==================================================
    // UserData Set 관련.

    // 액터 넘버 세팅
    void SetActorNumber(int _nSlot, int _nNum)
    {
        userData[_nSlot].nActorNumber = _nNum;
    }
    // 닉네임 세팅
    void SetNickname(int nSlot, string sName)
    {
        userData[nSlot].sNickname = sName;
    }
    // 방장 마크 세팅
    void SetMaster(int nSlot, bool bSet)
    {
        userData[nSlot].bMasterClient = bSet;
    }
    // 레디 세팅
    void SetReady(int nSlot, bool bSet)
    {
        userData[nSlot].bReady = bSet;
    }
    // 챔피언 세팅
    void SetChampionNum(int nSlot, int nNum)
    {
        userData[nSlot].nChampionNum = nNum;
    }
    //-----------------------------------------------
    // UI

    // 챔피언 선택 버튼 세팅.
    void SetChampionButton(int nSlot)
    {
        objSelectButton.transform.position = objSlot[nSlot].transform.position;
    }

    // 게임룸 인원수 관련
    void CheckPlayerCount()
    {
        int currPlayer = PhotonNetwork.PlayerList.Length;           //  현재 접속 인원
        int maxPlayer = PhotonNetwork.CurrentRoom.MaxPlayers;       //  룸 최대 인원.
        playerCount.text = string.Format("[{0} / {1}]", currPlayer, maxPlayer);     //  UI에 출력
    }

    //-----------------------------------------------------
    // 채팅 관련
    public void OnSendChatMsg()
    {
        string msg = string.Format("[{0}] {1}", PhotonNetwork.LocalPlayer.NickName, ifSendMsg.text);
        photonView.RPC("ReceiveMsg", RpcTarget.Others, msg);
        ReceiveMsg(msg);
    }

    [PunRPC]
    void ReceiveMsg(string msg)
    {
        msgList.text += "\n" + msg;
    }

    //============================================
    // 기능 부분

    public void OnGameStartClick()
    {
        Debug.Log("MC 게임 입장!!");

        photonView.RPC("BroadcastMsgGameStart", RpcTarget.All);     //  방장인 경우.
    }

    [PunRPC]
    void BroadcastMsgGameStart()
    {
        Debug.Log("유저 게임 입장!");

        // 전달 데이터 저장
        for(int i = 0; i < 10; i++)
        {
            GameUserData.GetComponent<GameUserData>().nActorNumber[i] = userData[i].nActorNumber;
            GameUserData.GetComponent<GameUserData>().sNickName[i] = userData[i].sNickname;
            GameUserData.GetComponent<GameUserData>().bMasterClient[i] = userData[i].bMasterClient;
            GameUserData.GetComponent<GameUserData>().bReady[i] = userData[i].bReady;
            GameUserData.GetComponent<GameUserData>().nChampionNum[i] = userData[i].nChampionNum;
        }

        PhotonNetwork.IsMessageQueueRunning = false;
        // 씬 전환.
        SceneManager.LoadScene("3.play");
    }

    public void OnExitClick()
    {
        PhotonNetwork.LeaveRoom();  // 내부에서 OnLeftRoom을 불러줌..
    }

    public override void OnLeftRoom()
    {
        //base.OnLeftRoom();
        //PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("1.Lobby");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        //base.OnPlayerEnteredRoom(newPlayer);
        CheckPlayerCount();     // UI 인원수 변경.
        string msg = string.Format("\n<color=#00ff00>[{0}]님이 입장했습니다.</color>", newPlayer.NickName);
        ReceiveMsg(msg);

        Debug.Log("newplayer.ActorNumber: " + newPlayer.ActorNumber);   // 리셋되지 않음. 들어오는 순서대로 추가!! 방장이 바뀌어도 유지.

        // 1. 유저 배치 부분. 마스터 클라이언트만 일함..
        if (PhotonNetwork.IsMasterClient)
        {
            // 2. 빈자리 찾음
            for (int i = 0; i < 10; i++)
            {
                // 3. 비어있는 슬롯이라면 처리.
                if (userData[i].nActorNumber == 0)
                {
                    // 4. 데이터 입력
                    SetActorNumber(i, newPlayer.ActorNumber);
                    SetNickname(i, newPlayer.NickName);
                    SetMaster(i, false);
                    SetReady(i, false);
                    SetChampionNum(i, 0);    // 0번으로 초기화.

                    // 5. 화면 업데이트
                    UpdateSlot();

                    // 6. 메세지 전송 부분
                    photonView.RPC("NewUserMsg", RpcTarget.Others, i, newPlayer.ActorNumber, newPlayer.NickName, false, false, 0);

                    string msg2 = string.Format("[{0}],[{1}],[{2}]", i, newPlayer.NickName, newPlayer.UserId);
                    Debug.Log("newUserMsg " + msg2);

                    // 7. Start 버튼 기능 활성화 확인
                    UpdateStartButton();

                    break;
                }
            }
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        //base.OnPlayerLeftRoom(otherPlayer);
        CheckPlayerCount();     // UI 인원수 변경.
        string msg = string.Format("\n<color=#ff0000>[{0}]님이 퇴장했습니다.</color>", otherPlayer.NickName);
        ReceiveMsg(msg);

        // 1. 유저 제거 부분. 마스터 클라이언트만 일함..
        if (PhotonNetwork.IsMasterClient)
        {
            // 내가 방장임을 스스로 데이터에 저장.
            SetMaster(GetUserSlotNum(nMyActorNumber), true);
            SetReady(GetUserSlotNum(nMyActorNumber), true);

            // UI. Start 버튼 활성화..
            objStartButton.SetActive(true);
            objReadyButton.SetActive(false);

            // 2. 나간 유저 찾음
            for (int i = 0; i < 10; i++)
            {
                // 3. 나간 유저 정보 비교..
                if (userData[i].nActorNumber == otherPlayer.ActorNumber)
                {
                    // 4. 데이터 입력 (초기화)
                    SetActorNumber(i, 0);
                    SetNickname(i, "");
                    SetMaster(i, false);
                    SetReady(i, false);
                    SetChampionNum(i, 0);    // 0번으로 초기화.

                    // 5. 화면 업데이트
                    UpdateSlot();

                    // 6. 메세지 전송 부분
                    photonView.RPC("OutUserMsg", RpcTarget.Others, i, otherPlayer.ActorNumber , GetUserSlotNum(nMyActorNumber) );

                    string msg2 = string.Format("[{0}],[{1}],[{2}]", i, otherPlayer.NickName, otherPlayer.UserId);
                    Debug.Log("OutUserMsg " + msg2);

                    // 7. Start 버튼 기능 활성화 확인
                    UpdateStartButton();

                    break;
                }
            }
        }
    }

    [PunRPC]
    void OutUserMsg(int _i, int _ActorNumber, int _nMasterSlot)     // 방장 슬롯 데이터 추가!!
    {
        // 방장 데이터 갱신
        SetMaster(_nMasterSlot, true);
        SetReady(_nMasterSlot, true);

        // 1. 데이터 갱신.
        SetActorNumber(_i, 0);
        SetNickname(_i, "");
        SetMaster(_i, false);
        SetReady(_i, false);
        SetChampionNum(_i, 0);

        // 2. 화면 갱신.
        UpdateSlot();
    }
    
    void Update()
    {
        
    }

    //==============================================
    // UI 이벤트 처리

    public void ClickSlot(int nSlotNum)
    {
        //// 이전 자리 지우기.. (레디, 방장 표시, 닉네임, 챔피언)
        //SetReadyView(nMySlotSelect, false);
        //SetMasterView(nMySlotSelect, false);
        //SetNicknameView(nMySlotSelect, "");
        //SetChampionView(nMySlotSelect, 10);     //  초기화 10번.

        //// 선택된 슬롯에 적용..
        //SetReadyView(nSlotNum, true);
        //SetMasterView(nSlotNum, true);
        //SetNicknameView(nSlotNum, sMyNickName);  //PhotonNetwork.NickName;
        //SetChampionView(nSlotNum, nMyChampionSelect);   // 현재 선택된 캐릭터 보이기..

        //nMySlotSelect = nSlotNum;       //  선택 슬롯 갱신!!
    }

    //public void ClickLeftChampion()
    //{
    //    //nMyChampionSelect--;

    //    //if (nMyChampionSelect < 0)            nMyChampionSelect = 0;

    //    //SetChampionView(nMySlotSelect, nMyChampionSelect);
    //}

    //public void ClickRightChampion()
    //{
    //    //nMyChampionSelect++;

    //    //if (nMyChampionSelect > 9)            nMyChampionSelect = 9;

    //    //SetChampionView(nMySlotSelect, nMyChampionSelect);
    //}

    //=========================================================
    //  슬롯 이동 부분

    // 방장에게 요청하는 방식 #2
    public void ClickSlot2(int nSlotNum)
    {
        // 내가 방장이라면 ? = 바로 처리 (유저들에게 알림 처리를 위한 별도의 작업 필요. 패킷 처리를 하나 줄임. ㅡ,.ㅡa)
        if(PhotonNetwork.IsMasterClient)
        {
            if(userData[nSlotNum].nActorNumber == 0)    // 지금 자리가 비어있다면 가능.
            {
                Debug.Log("000");
                // 0. 액터 번호로 데이터 위치 얻어 오기.
                int _nOldSlotNum = GetUserSlotNum(nMyActorNumber);

                // 1. 데이터 복사. (깊은 복사 필요)
                //userData[nSlotNum] = userData[_nOldSlotNum];
                CopyUserData(nSlotNum, _nOldSlotNum);

                // 2. 이전 데이터 삭제. 
                userData[_nOldSlotNum].Clear();

                // UI 처리

                // 3. 이전 선택 자리 지우기. (레디, 방장, 닉네임, 챔피언)
                SetReadyView(_nOldSlotNum, false);
                SetMasterView(_nOldSlotNum, false);
                SetNicknameView(_nOldSlotNum, "");
                SetChampionNameView(_nOldSlotNum, "");
                SetChampionView(_nOldSlotNum, 10);      // 초기화.

                // 4. 선정된 자리 보이기
                SetReadyView(nSlotNum, true);
                SetMasterView(nSlotNum, true);
                SetNicknameView(nSlotNum, userData[nSlotNum].sNickname);
                SetChampionNameView(nSlotNum, sChampionName[userData[nSlotNum].nChampionNum]);  // 챔피언 (고유)이름 보이기..
                SetChampionView(nSlotNum, userData[nSlotNum].nChampionNum);                     //  현재 선택된 챔피언 보이기..

                // 4.5 UI 업데이트는 선택사항.
                UpdateSlot();

                // 5. 모든 유저에게 처리 메세지 보냄.
                photonView.RPC("BroadcastMsgSlotChange", RpcTarget.Others, nSlotNum, nMyActorNumber);   // 1.이동할 자리 2.액터 넘버
            }
        }
        else    // 유저 라면 방장에게 이동 하겠다는 요청을 보냄.
        {
            if(userData[nSlotNum].nActorNumber == 0)     // 자리가 비어있을 경우에만 요청.   
            {
                photonView.RPC("McReceiveMsgSlotChange", RpcTarget.MasterClient, nSlotNum, nMyActorNumber);     // 1.이동할 자리 2. 액터 넘버
            }
        }
    }

    [PunRPC]
    void McReceiveMsgSlotChange(int _nSlotNum, int _nMyActorNumber)
    {
        // 지금 자리가 비어있다면 이동처리 
        if (userData[_nSlotNum].nActorNumber == 0)
        {
            Debug.Log("000McReceiveMsgSlotChange");
            // 0. 액터 번호로 데이터 위치 얻어오기.
            int _nOldSlotNum = GetUserSlotNum(_nMyActorNumber);

            Debug.Log("번호:" + _nSlotNum + "," + _nOldSlotNum);

            // 1. 데이터 복사.
            CopyUserData(_nSlotNum, _nOldSlotNum);

            // 2. 이전 데이터 삭제
            userData[_nOldSlotNum].Clear();

            // 3. (UI처리) 이전 선택자리 지우기
            SetReadyView(_nOldSlotNum, false);
            SetMasterView(_nOldSlotNum, false);
            SetNicknameView(_nOldSlotNum, "");
            SetChampionNameView(_nOldSlotNum, "");
            SetChampionView(_nOldSlotNum, 10);      // 초기화.

            // 4. (UI처리) 선정 자리 보이기
            SetReadyView(_nSlotNum, true);
            SetMasterView(_nSlotNum, true);
            SetNicknameView(_nSlotNum, userData[_nSlotNum].sNickname);
            SetChampionNameView(_nSlotNum, sChampionName[userData[_nSlotNum].nChampionNum]);  // 챔피언 (고유)이름 보이기..
            SetChampionView(_nSlotNum, userData[_nSlotNum].nChampionNum);                     //  현재 선택된 챔피언 보이기..

            // 4.5 (UI처리) 업데이트        
            UpdateSlot();

            // 5. 모든 유저에게 처리 메시지 보냄.
            photonView.RPC("BroadcastMsgSlotChange", RpcTarget.Others, _nSlotNum, _nMyActorNumber);  // 1. 이동할 자리  2.액터 넘버
        }
    }

    [PunRPC]
    void BroadcastMsgSlotChange(int _nSlotNum, int _nMyActorNumber)   // 방장을 제외한 일반 유저들 처리.
    {
        // 지금 자리가 비어있다면 이동처리 
        if (userData[_nSlotNum].nActorNumber == 0)
        {
            Debug.Log("000BroadcastMsgSlotChange");
            // 0. 액터 번호로 데이터 위치 얻어오기.
            int _nOldSlotNum = GetUserSlotNum(_nMyActorNumber);

            // 1. 데이터 복사.
            CopyUserData(_nSlotNum, _nOldSlotNum);

            // 2. 이전 데이터 삭제
            userData[_nOldSlotNum].Clear();

            // 3. (UI처리) 이전 선택자리 지우기
            SetReadyView(_nOldSlotNum, false);
            SetMasterView(_nOldSlotNum, false);
            SetNicknameView(_nOldSlotNum, "");
            SetChampionNameView(_nOldSlotNum, "");
            SetChampionView(_nOldSlotNum, 10);      // 초기화.

            // 4. (UI처리) 선정 자리 보이기
            SetReadyView(_nSlotNum, true);
            SetMasterView(_nSlotNum, true);
            SetNicknameView(_nSlotNum, userData[_nSlotNum].sNickname);
            SetChampionNameView(_nSlotNum, sChampionName[userData[_nSlotNum].nChampionNum]);  // 챔피언 (고유)이름 보이기..
            SetChampionView(_nSlotNum, userData[_nSlotNum].nChampionNum);                     //  현재 선택된 챔피언 보이기..

            // 4.5 (UI처리) 업데이트        
            UpdateSlot();
        }
    }

    //--------------------------------------------------
    // 레디 누르기 (방장은 OnGameStartClick)

    // 방장에게 요청하는 방식 #3
    public void ClickReady()
    {
        // 방장에게 레디 데이터 날림.
        photonView.RPC("McReceiveMsgReady", RpcTarget.MasterClient, nMyActorNumber);    // 1.액터 넘버
    }

    [PunRPC]
    void McReceiveMsgReady(int _nMyActorNumber)
    {
        // 0. 액터 번호로 데이터 위치 얻어오기.
        int _nSlotNum = GetUserSlotNum(_nMyActorNumber);

        // 1. 방장이므로 userData 우선 처리. (반전 처리)
        userData[_nSlotNum].bReady = !userData[_nSlotNum].bReady;

        // 2. UI 출력
        SetReadyView(_nSlotNum, userData[_nSlotNum].bReady);

        // 3. 모든 유저에게 처리 메세지 보냄.
        photonView.RPC("BroadcastMsgReady", RpcTarget.Others, _nSlotNum, userData[_nSlotNum].bReady);   // 1.적용할 자리 2. 레디 상태

        // 4. Start 버튼 기능 활성화 확인.
        UpdateStartButton();
    }

    [PunRPC]
    void BroadcastMsgReady(int _nSlotNum, bool _bReady)
    {
        // 1. 데이터 적용
        userData[_nSlotNum].bReady = _bReady;
        // 2. UI 출력
        SetReadyView(_nSlotNum, _bReady);
    }



    //=============================================================
    // 유저 데이터 부분.

    // 액터 넘버로 존재 슬롯 받아오기.
    int GetUserSlotNum(int _nActorNumber)
    {
        for(int i = 0; i < 10; i++)
        {
            if(userData[i].nActorNumber == _nActorNumber)
            {
                return i;
            }
        }

        Debug.LogWarning("경고!! 유저 없음:" + _nActorNumber);
        return 999;
    }

    // 액터 넘버로 챔피언 번호 받아오기.
    int GetChampionNum(int _nActorNumber)
    {
        for(int i = 0; i < 10; i++)
        {
            if(userData[i].nActorNumber == _nActorNumber)
            {
                return userData[i].nChampionNum;
            }
        }

        Debug.LogWarning("경고!! 챔피언 번호 없음! 에러 액터:" + _nActorNumber);
        return 999;
    }

    // 유저 정보 복사. 
    void CopyUserData(int _nSlotNum, int _nOldSlotNum)
    {
        userData[_nSlotNum].nActorNumber  = userData[_nOldSlotNum].nActorNumber;
        userData[_nSlotNum].sNickname     = userData[_nOldSlotNum].sNickname;
        userData[_nSlotNum].bMasterClient = userData[_nOldSlotNum].bMasterClient;
        userData[_nSlotNum].bReady        = userData[_nOldSlotNum].bReady;
        userData[_nSlotNum].nChampionNum  = userData[_nOldSlotNum].nChampionNum;
    }


    //=============================================================
    // 챔피언 선택 부분.

    // 왼쪽 이동.
    public void ClickLeftChampion()
    {
        // 방장일 경우 바로 처리
        if (PhotonNetwork.IsMasterClient)
        {
            // 0. 액터 번호로 데이터 위치 얻어오기
            int _nSlotNum = GetUserSlotNum(nMyActorNumber);

            // 1. 유저 데이터 처리
            userData[_nSlotNum].nChampionNum--;

            if(userData[_nSlotNum].nChampionNum < 0)
            {
                userData[_nSlotNum].nChampionNum = nChampionCount - 1;      // 챔피언 수량, 1부터..
            }

            // 2. 데이터로 출력하기 (보여주기)
            SetChampionNameView(_nSlotNum, sChampionName[userData[_nSlotNum].nChampionNum]);    // 챔피언 이름 보이기.
            SetChampionView(_nSlotNum, userData[_nSlotNum].nChampionNum);                       //  현재 선택된 챔피언 보이기.

            // 3. 브로드 캐스팅
            photonView.RPC("ChangeChampion", RpcTarget.Others, nMyActorNumber, userData[_nSlotNum].nChampionNum ); // 1.챔피언 변경될 액터 2.챔피언 넘버
        }
        // 유저일 경우 처리
        else
        {
            // 1. MC에게 변경 요청.
            photonView.RPC("McChangeChampionLeft",RpcTarget.MasterClient, nMyActorNumber );
        }
    }
    [PunRPC]
    void McChangeChampionLeft(int _nActorNumber)
    {
        // 0. 액터 번호로 데이터 위치 가져오기
        int _nSlotNum = GetUserSlotNum(_nActorNumber);

        // 1. 유저 데이터 처리
        userData[_nSlotNum].nChampionNum--;

        if(userData[_nSlotNum].nChampionNum < 0)
        {
            userData[_nSlotNum].nChampionNum = nChampionCount - 1;  
        }

        // 2. 데이터로 출력하기(보여주기)
        SetChampionNameView(_nSlotNum, sChampionName[userData[_nSlotNum].nChampionNum]);    // 챔피언 이름 보여주기
        SetChampionView(_nSlotNum, userData[_nSlotNum].nChampionNum);                       // 챔피언 보여주기

        // 3. 브로드 캐스팅
        photonView.RPC("ChangeChampion", RpcTarget.Others, _nActorNumber, userData[_nSlotNum].nChampionNum); // 1.챔피언 변경될 액터 2.챔피언 넘버
    }

    [PunRPC]
    void ChangeChampion(int _nActorNumber, int _nChampionNum)       //  액터 넘버. 챔피언 번호 --> 챔피언 변경
    {
        // 0. 액터 번호로 데이터 위치 가져오기
        int _nSlotNum = GetUserSlotNum(_nActorNumber);

        // 1. 유저 데이터 처리
        userData[_nSlotNum].nChampionNum = _nChampionNum;

        // 2. 데이터로 출력하기(보여주기)
        SetChampionNameView(_nSlotNum, sChampionName[userData[_nSlotNum].nChampionNum]);  // 챔피언 이름 보여주기
        SetChampionView(_nSlotNum, userData[_nSlotNum].nChampionNum);          // 챔피언 보여주기
    }


    // 오른쪽 이동.
    public void ClickRightChampion()
    {
        // 방장일 경우 처리 .바로 처리!! (유저들에게 알림 처리를 위한 별도 작업 필요. 패킷 처리를 하나 줄임. ㅡ,.ㅡa)

        if (PhotonNetwork.IsMasterClient)
        {
            // 0. 액터 번호로 데이터 위치 얻어오기.
            int _nSlotNum = GetUserSlotNum(nMyActorNumber);

            // 1. 유저 데이터 처리
            userData[_nSlotNum].nChampionNum++;

            if (userData[_nSlotNum].nChampionNum > nChampionCount - 1)
            {
                userData[_nSlotNum].nChampionNum = 0;
            }

            // 2. 데이터로 출력 하기(보여주기)
            SetChampionNameView(_nSlotNum, sChampionName[userData[_nSlotNum].nChampionNum]);   // 챔피언 이름 보이기..
            SetChampionView(_nSlotNum, userData[_nSlotNum].nChampionNum);                      // 현재 선택된 캐릭터 보이기..

            // 3. 브로드 캐스팅
            photonView.RPC("ChangeChampion", RpcTarget.Others, nMyActorNumber, userData[_nSlotNum].nChampionNum);  // 1.이동할 자리  2.액터 넘버

        }
        else    // 유저일 경우 처리.
        {
            // 1. MC 에게 변경 요청
            photonView.RPC("McChangeChampionRight", RpcTarget.MasterClient, nMyActorNumber);     // 방장에게 챔피언 왼쪽 변경 요청
        }
    }

    [PunRPC]
    void McChangeChampionRight(int _nActorNumber)
    {
        // 0. 액터 번호로 데이터 위치 얻어오기.
        int _nSlotNum = GetUserSlotNum(_nActorNumber);

        // 1. 유저 데이터 처리
        userData[_nSlotNum].nChampionNum++;

        if (userData[_nSlotNum].nChampionNum > nChampionCount - 1)
        {
            userData[_nSlotNum].nChampionNum = 0;
        }

        // 2. 데이터로 출력 하기 (보여주기)
        SetChampionNameView(_nSlotNum, sChampionName[userData[_nSlotNum].nChampionNum]);   // 챔피언 이름 보이기..
        SetChampionView(_nSlotNum, userData[_nSlotNum].nChampionNum);                      // 현재 선택된 캐릭터 보이기..

        // 3. 브로드 캐스팅
        photonView.RPC("ChangeChampion", RpcTarget.Others, _nActorNumber, userData[_nSlotNum].nChampionNum);  // 1.이동할 자리  2.액터 넘버
    }



    //==========================================
    // UI 업데이트 : 슬롯의 정보를 전부 다 업데이트 

    void UpdateSlot()
    {
        // 1번 슬롯 부터 10번 슬롯까지 업데이트
        for(int i = 0; i < 10; i++)
        {
            // 고유 아이디가 있다면.(유저가 슬롯에 존재 한다면..)
            //if(userData[i].sUserID != "")
            if(userData[i].nActorNumber != 0)
            {
                // 각 슬롯 자리에 데이터 처리
                // 1. 방장(MC)
                if (userData[i].bMasterClient)
                {
                    SetMasterView(i, true);
                }
                else
                {
                    SetMasterView(i, false);
                }
                // 2. 레디
                if(userData[i].bReady)
                {
                    SetReadyView(i, true);
                }
                else
                {
                    SetReadyView(i, false);
                }
                // 3. 닉네임
                SetNicknameView(i, userData[i].sNickname);
                // 4. 챔피언 종류
                SetChampionView(i, userData[i].nChampionNum);
                // 5. 챔피언 이름
                SetChampionNameView(i, sChampionName[userData[i].nChampionNum]);
            }
            else    // 유저가 슬롯에 없다면 초기화.
            {
                SetMasterView(i, false);
                SetReadyView(i, false);
                SetNicknameView(i, "");
                SetChampionNameView(i, "");
                SetChampionView(i, 10);     // 초기화 10번.
            }

            // 내 아이디를 확인해서 챔피언 선택 버튼을 이동.
            if(userData[i].nActorNumber == nMyActorNumber)
            {
                // 챔피언 선택창을 이동..
                SetChampionButton(i);   // 캐릭터 선택 버튼 이동.
            }
        }
    }

    // 유저 전원 체크 해서 전원 Ready 인 경우, 게임 시작 버튼 활성화.
    void UpdateStartButton()
    {
        // 방장인 경우에만 처리!!
        if(PhotonNetwork.IsMasterClient)
        {
            // 게임 시작할 때 확인.
            // 레디 버튼이 요청왔을때 확인.
            // 유저 입장시 확인.
            // 유저 퇴장시 확인.

            bool _bStartOk = true;

            // 1. 참여 인원수 확인(액터 넘버)
            for (int i = 0; i < 10; i++)
            {
                // 2. 전원 ready 상태이면 버튼 활성화. 아니면 비활성화.
                if (userData[i].nActorNumber != 0)
                {
                    //  2.5 만약에 레디상태가 아닌 유저있다면 처리.
                    if(!userData[i].bReady)
                    {
                        _bStartOk = false;
                        break;
                    }
                }
            }
            //  2.6 전원 레디면.
            if(_bStartOk)
            {
                SetStartButton(true);
            }
            //  2.7 아니라면
            else
            {
                SetStartButton(false);
            }

            // 3. UI 처리
            //SetStartButton(_bStartOk);
            // 생략..

        }
    }       
}

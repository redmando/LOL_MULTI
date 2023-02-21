using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    public enum ActivePanel
    {
        LOGIN = 0,
        ROOMS = 1
    }
    public ActivePanel activePanel = ActivePanel.LOGIN;

    private string gameVersion = "1.0";     // 클라이언트 버전.
    public string userID = "player01";      // 플레이어 아이디.
    public byte maxplayer = 10;             // 플레이 인원수.

    public InputField txtUserId;        
    public InputField txtRoomName;

    public GameObject[] panels;     // 로그인 패널과 룸 리스트 패널.
    public GameObject room;
    public Transform gridTr;

    [Header("게임오브젝트")]
    public GameObject objCreateButton;      //  게임방 생성 버튼
    public GameObject objJoinButton;        //  게임방 참가 버튼

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        txtUserId.text = PlayerPrefs.GetString("USER_ID", "USER_" + Random.Range(1, 999));
        txtRoomName.text = PlayerPrefs.GetString("ROOM_NAME", "ROOM_" + Random.Range(1, 999));

        if(PhotonNetwork.IsConnected)
        {
            ChangePanel(ActivePanel.ROOMS);
        }

        // 방만들기 & 방 참가 버튼 비활성화
        SetCreateNJoinButton(false);
    }

    private void ChangePanel(ActivePanel panel)
    {
        foreach(GameObject _panel in panels)
        {
            Debug.Log(panels);
            _panel.SetActive(false);
        }
        panels[(int)panel].SetActive(true);
    }

    #region SELF_CALLBACK_FUNCTIONS
    public void OnLogin()
    {
        PhotonNetwork.GameVersion = this.gameVersion;
        PhotonNetwork.NickName = txtUserId.text;
        PhotonNetwork.ConnectUsingSettings();
        PlayerPrefs.SetString("USER_ID", PhotonNetwork.NickName);
        ChangePanel(ActivePanel.ROOMS);
    }

    public void OnCreateRoomClick()
    {
        PhotonNetwork.CreateRoom(txtRoomName.text, new RoomOptions { MaxPlayers = this.maxplayer });
    }

    public void OnJoinRandomRoomClick()
    {
        PhotonNetwork.JoinRandomRoom();
    }
    #endregion

    #region PHOTON_CALLBACK_FUNCTIONS
    public override void OnConnectedToMaster()
    {
        //base.OnConnectedToMaster();
        Debug.Log("접속 연결");
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        //base.OnJoinedLobby();
        Debug.Log("로비 입장");

        // 방 생성 버튼 및 방 참가버튼 활성화.
        SetCreateNJoinButton(true);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        //base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("랜덤 룸 입장 실패");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = this.maxplayer });
    }
    public override void OnJoinedRoom()
    {
        //base.OnJoinedRoom();
        Debug.Log("룸 입장");
        PhotonNetwork.IsMessageQueueRunning = false;
        SceneManager.LoadScene("2.Room");
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //base.OnRoomListUpdate(roomList);
        //{
        //}
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("ROOM"))
        {
            Destroy(obj);
        }
        foreach(RoomInfo roomInfo in roomList)
        {
            GameObject _room = Instantiate(room, gridTr);
            RoomData roomData = _room.GetComponent<RoomData>();
            roomData.roomName = roomInfo.Name;
            roomData.maxPlayer = roomInfo.MaxPlayers;
            roomData.PlayerCount = roomInfo.PlayerCount;
            roomData.UpDateInfo();
            roomData.GetComponent<Button>().onClick.AddListener
                (
                delegate
                {
                    onClickRoom(roomData.roomName);
                }

                );
        }        
    }
    #endregion

    void onClickRoom(string roomName)
    {
        PhotonNetwork.NickName = txtUserId.text;
        PhotonNetwork.JoinRoom(roomName, null);
        PlayerPrefs.SetString("USER_ID", PhotonNetwork.NickName);
    }

    // 접속 버튼 활성화 처리..
    void SetCreateNJoinButton(bool bSet)
    {
        objCreateButton.GetComponent<Button>().interactable = bSet;
        objJoinButton.GetComponent<Button>().interactable = bSet;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Cameramove_CHOGUNHEE : MonoBehaviour
{
    //시작 위치 표인트 설정.
    public GameObject objTopPoint; //윗마을 시작 포인트
    public GameObject objBottomPoint; //아랫마을 시작 포인트

    //시야 처리 FOG OF WAR
    public FogOfWarManager FogWManager;

    public GameObject[] obj;
    public int nNum;
    public int nDes;

    public bool useKeyboardinput = true;

    public bool bseScreenEdgeInput = true;
    public bool useScreenEdgeInput = true;
    public float screenEdgeBorder = 25.0f;
    public float panningSpeed = 10.0f;

    public bool usePannding = true;
    public KeyCode panningkey = KeyCode.Mouse2;

    // 캐릭터를 따라가는 중이라면
    public string horizontaAxis = "Horizontal";
    public string verticaAxis = "Vertical";

    public float keyboardMoveMentSpeed = 5.0f;
    public float screenEdgeMoveMentSpeed = 3.0f;

    private Transform m_Tramsform; //카메라 트렌스폼
    // Start is called before the first frame update
    public bool bMouseLock = true;
    /*Cursor.lockState = CursorLockMode.Confined;*/ //게임 창 마우스가 안나감/
    ////Cursor.lockState = CursorLockMode.Locked;
    ////Cursor.lockState = CursorLockMode.None;
    
    void Start()
    {
        //플레이 시작 시에 시작 위치(진형으로 이동)
        int _nSlot = 0;

        for (int i = 0; i < 10; i++) 
        {
            if (GameObject.Find("GameUserData").GetComponent<GameUserData>().sNickName[i] == PhotonNetwork.NickName)
            {
                _nSlot = i;
                break;
            }
        }
        //윗마을 일 경우
        if (_nSlot < 5)
        {
            //윗마을 팀이면 윗마을 포인트로 카메라 고정
            gameObject.transform.position = new Vector3(objTopPoint.transform.position.x, gameObject.transform.position.y,
                objTopPoint.transform.position.z - nDes);
            //안개 진영처리
            FogWManager.ShowFaction(1);
        }
        //아렛마을 일 경우
        else 
        {
            //아렛마을 팀이면 아렛마을 포인트로 카메라 고정
            gameObject.transform.position = new Vector3(objBottomPoint.transform.position.x, gameObject.transform.position.y,
                objBottomPoint.transform.position.z - nDes);
            //안개 진영처리
            FogWManager.ShowFaction(0);
           
        }

        m_Tramsform = transform;

        Cursor.lockState = CursorLockMode.Confined; //게임 창 마우스가 안나감/
        ////Cursor.lockState = CursorLockMode.Locked;
        ////Cursor.lockState = CursorLockMode.None;
    }

    private Vector2 MouseInput 
    {
        get { return Input.mousePosition; }
    }
    private Vector2 keyboardInput 
    {
        get { return useKeyboardinput ? new Vector2(Input.GetAxis(horizontaAxis), Input.GetAxis(verticaAxis)) : Vector2.zero; }
    }
    private Vector2 MouseAxis 
    {
        get { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));}
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }
    private void Move() 
    {
        if (useKeyboardinput) 
        {
            Vector3 desireMove = new Vector3(keyboardInput.x, 0, keyboardInput.y);

            desireMove *= keyboardMoveMentSpeed; 
            desireMove *= Time.deltaTime;
            desireMove = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f))*desireMove;
            desireMove = m_Tramsform.InverseTransformDirection(desireMove);

            m_Tramsform.Translate(desireMove, Space.Self);
        }
        if (useScreenEdgeInput) 
        {
            Vector3 desiredMove = new Vector3();

            Rect lefRect = new Rect(0, 0, screenEdgeBorder, Screen.height);
            Rect rightRect = new Rect(Screen.width - screenEdgeBorder, 0, screenEdgeBorder, Screen.height);
            Rect upRect = new Rect(0, Screen.height - screenEdgeBorder, Screen.width, screenEdgeBorder);
            Rect downRect = new Rect(0,0, Screen.height, screenEdgeBorder);

            desiredMove.x = lefRect.Contains(MouseInput) ? -1 : rightRect.Contains(MouseInput) ? 1 : 0;
            desiredMove.z = upRect.Contains(MouseInput) ? 1 : downRect.Contains(MouseInput) ? -1 : 0;

            desiredMove *= screenEdgeMoveMentSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f)) * desiredMove;
            desiredMove = m_Tramsform.InverseTransformDirection(desiredMove);

            m_Tramsform.Translate(desiredMove, Space.Self);
        }
        if (usePannding&&Input.GetKey(panningkey)&&MouseAxis != Vector2.zero) 
        {

            Vector3 desiredMove = new Vector3(-MouseAxis.x,0,-MouseAxis.y);

            desiredMove *= screenEdgeMoveMentSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f)) * desiredMove;
            desiredMove = m_Tramsform.InverseTransformDirection(desiredMove);

            m_Tramsform.Translate(desiredMove, Space.Self);
        }
        if (Input.GetKey(KeyCode.Space)) 
        {
            Vector3 TargetPos = new Vector3(obj[nNum].transform.position.x, 
                gameObject.transform.position.y, obj[nNum].transform.position.z - nDes);
            transform.position = Vector3.Lerp(transform.position, TargetPos, Time.deltaTime * 20.0f);
        }
    }
}

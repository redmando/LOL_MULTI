using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CameraMove : MonoBehaviour
{
    // 시작 위치 포인트 설정.
    public GameObject objTopPoint;      // 윗마을 시작 포인트.
    public GameObject objBottomPoint;    // 아랫마을 시작 포인트.

    // fog of war 진영 처리.
    public FogOfWarManager FOWManager;

    public GameObject[] obj;        // 유닛을 따라가기 위해 선정.
    public int nNum;                // 유닛의 인덱스.
    public int nDes;                // 카메라의 거리값.

    public bool useKeyboardInput = true;    // 키보드 입력.

    public bool useScreenEdgeInput = true;  // 화면 가장자리 처리.
    public float screenEdgeBorder = 25.0f;  // 화면 가장자리 두께
    public float panningSpeed = 10.0f;      // 이동 속도.

    public bool usePanning = true;
    public KeyCode panningKey = KeyCode.Mouse2;

    // 캐릭터를 따라가는 중이라면.
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";

    public float keyboardMovementSpeed = 5.0f;
    public float screenEdgeMovementSpeed = 3.0f;

    private Transform m_Transform;          //  카메라 트랜스폼.

    //=================================================
    public bool bMouseLock = true;      //  마우스를 잠그는 플래그. 초기값 잠겨있음.

    //Cursor.lockState = CursorLockMode.Confined;       // 게임창 밖으로 마우스가 안나감.
    //Cursor.lockState = CursorLockMode.Locked;         // 마우스를 게임 중앙 좌표에 고정 시키고 마우스 커서가 안보임.
    //Cursor.lockState = CursorLockMode.None;           // 마우스 커서 정상.

    void Start()
    {
        // 플레이 시작시에 시작 위치(진영으로 이동)
        int _nSlot = 0;

        for(int i = 0; i < 10; i++)
        {
            if(GameObject.Find("GameUserData").GetComponent<GameUserData>().sNickName[i] == PhotonNetwork.NickName )
            {
                _nSlot = i;
                break;
            }
        }

        // 윗마을일 경우(top)
        if(_nSlot < 5)
        {
            // 윗마을 팀 이면 윗마을 포인트로 카메라 고정.
            gameObject.transform.position =
                new Vector3(objTopPoint.transform.position.x, gameObject.transform.position.y,
                objTopPoint.transform.position.z - nDes);

            // 안개 진영 처리.
            FOWManager.ShowFaction(1);
        }
        // 아랫마을일 경우(Bottom)
        else
        {
            // 아랫마을 팀이면 아랫마을 포인트로 카메라 고정.
            gameObject.transform.position =
                new Vector3(objBottomPoint.transform.position.x, gameObject.transform.position.y,
                objBottomPoint.transform.position.z - nDes);

            // 안개 진영 처리.
            FOWManager.ShowFaction(0);
        }

        m_Transform = transform;                        // 기본 트랜스폼.

        //Cursor.lockState = CursorLockMode.Confined;     // 게임창 밖으로 마우스가 안나감.        // test 할때는 꺼놓음..
    }

    //=========================
    // 입력
    private Vector2 MouseInput
    {
        get { return Input.mousePosition; }
    }

    private Vector2 KeyboardInput
    {
        get { return useKeyboardInput ? new Vector2(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis)): Vector2.zero; }
    }

    private Vector2 MouseAxis
    {
        get { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); }
    }
           
    void Update()
    {
        Move();        
    }

    private void Move()
    {
        // 화살표 키 이동.
        if(useKeyboardInput)
        {
            Vector3 desiredMove = new Vector3(KeyboardInput.x, 0, KeyboardInput.y);

            desiredMove *= keyboardMovementSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f)) * desiredMove;
            desiredMove = m_Transform.InverseTransformDirection(desiredMove);

            m_Transform.Translate(desiredMove, Space.Self);
        }

        // 화면 가장자리 이용 이동.
        if(useScreenEdgeInput)
        {
            Vector3 desiredMove = new Vector3();

            Rect leftRect = new Rect(0, 0, screenEdgeBorder, Screen.height);
            Rect rightRect = new Rect(Screen.width - screenEdgeBorder, 0, screenEdgeBorder, Screen.height);
            Rect upRect = new Rect(0, Screen.height - screenEdgeBorder, Screen.width, screenEdgeBorder);
            Rect downRect = new Rect(0,0,Screen.width, screenEdgeBorder);

            desiredMove.x = leftRect.Contains(MouseInput) ? -1 : rightRect.Contains(MouseInput) ? 1 : 0;
            desiredMove.z = upRect.Contains(MouseInput) ? 1 : downRect.Contains(MouseInput) ? -1 : 0;

            desiredMove *= screenEdgeMovementSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f)) * desiredMove;
            desiredMove = m_Transform.InverseTransformDirection(desiredMove);

            m_Transform.Translate(desiredMove, Space.Self);
        }

        // 휠 클릭 드래그 이동.
        if(usePanning && Input.GetKey(panningKey) && MouseAxis != Vector2.zero)
        {
            Vector3 desiredMove = new Vector3(-MouseAxis.x, 0, -MouseAxis.y);

            desiredMove *= screenEdgeMovementSpeed;
            desiredMove *= Time.deltaTime;
            desiredMove = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f)) * desiredMove;
            desiredMove = m_Transform.InverseTransformDirection(desiredMove);

            m_Transform.Translate(desiredMove, Space.Self);
        }

        // Spacebar 가 눌리면 캐릭터로 화면 이동.
        if(Input.GetKey(KeyCode.Space))
        {
            Vector3 TargetPos = new Vector3(obj[nNum].transform.position.x, gameObject.transform.position.y,
                obj[nNum].transform.position.z - nDes);
            transform.position = Vector3.Lerp(transform.position, TargetPos, Time.deltaTime * 20.0f);
        }
    }

    // TOP 본진으로 이동.
    public void CameraMoveTop()
    {
        Vector3 TargetPos = new Vector3(objTopPoint.transform.position.x, gameObject.transform.position.y, objTopPoint.transform.position.z - nDes);

        transform.position = Vector3.Lerp(transform.position, TargetPos, Time.deltaTime * 20.0f);
    }

    // Bottom 본진으로 이동.
    public void CameraMoveBottom()
    {
        Vector3 TargetPos = new Vector3(objBottomPoint.transform.position.x, gameObject.transform.position.y, objBottomPoint.transform.position.z - nDes);

        transform.position = Vector3.Lerp(transform.position, TargetPos, Time.deltaTime * 20.0f);
    }

    // TOP 본진으로 이동.(바로 이동)
    public void CameraMoveTopDirect()
    {
        transform.position = new Vector3(objTopPoint.transform.position.x, gameObject.transform.position.y, objTopPoint.transform.position.z - nDes);
    }

    // Bottom 본진으로 이동.(바로 이동)
    public void CameraMoveBottomDirect()
    {
        transform.position = new Vector3(objBottomPoint.transform.position.x, gameObject.transform.position.y, objBottomPoint.transform.position.z - nDes);
    }

}

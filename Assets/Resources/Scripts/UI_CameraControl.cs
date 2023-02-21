using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_CameraControl : MonoBehaviour
{
    [Header("CameraMove")]
    public bool mouseLock = true;
    public bool screenEdgeInput = false;
    public bool keyboardInput = true;
    public float screenEdgeBorder = 25.0f;
    public float keyboardMoveSpeed = 100.0f;
    public float screenEdgeMoveSpeed = 3.0f;

    [Header("CameraZoom")]
    public float zoomSpeed;
    private float maxDistance = 100.0f;
    private float minDistance = 20.0f;

    private Transform tr;
    private Camera cam;

    private Vector2 MouseInput
    {
        get { return Input.mousePosition; }
    }
    private Vector2 KeyboardInput
    {
        get { return keyboardInput ? new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) : Vector2.zero; }
    }
    private float wheelInput
    {
        get { return Input.GetAxis("Mouse ScrollWheel"); }
    }

    private void Start()
    {
        cam = GetComponent<Camera>();
        tr = GetComponent<Transform>();
        //Cursor.lockState = CursorLockMode.Confined;     // 게임창 밖으로 마우스가 안 나감
        // Kinds of Cursor.lockStace
        /*
         * Cursor.lockState = CursorLockMode.Confined;      // 게임창 밖으로 마우스가 안 나감
         * Cursor.lockState = CursorLockMode.Locked;        // 마우스를 게임 중앙 좌표에 고정 시키고 마우스 커서가 안 보임
         * Cursor.lockState = CursorLockMode.None;          // 마우스 커서 정상
         */
    }

    private void Update()
    {
        CameraZoom();
        CameraControl();
    }

    private void CameraZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        if (cam.fieldOfView <= minDistance && scroll < 0)
        {
            cam.fieldOfView = minDistance;
        }
        else if (cam.fieldOfView >= maxDistance && scroll > 0)
        {
            cam.fieldOfView = maxDistance;
        }
        else
        {
            cam.fieldOfView += scroll;
        }
    }

    private void CameraControl()
    {
        

        if (screenEdgeInput == true)
        {
            Vector3 intendedMove = new Vector3();

            Rect leftRect = new Rect(0, 0, screenEdgeBorder, Screen.height);
            Rect rightRect = new Rect(Screen.width - screenEdgeBorder, 0, screenEdgeBorder, Screen.height);
            Rect upRect = new Rect(0, Screen.height - screenEdgeBorder, Screen.width, screenEdgeBorder);
            Rect downRect = new Rect(0, 0, Screen.width, screenEdgeBorder);

            intendedMove.x = leftRect.Contains(MouseInput) ? -1 : rightRect.Contains(MouseInput) ? 1 : 0;
            intendedMove.z = upRect.Contains(MouseInput) ? 1 : downRect.Contains(MouseInput) ? -1 : 0;

            intendedMove *= screenEdgeMoveSpeed;
            intendedMove *= Time.deltaTime;
            intendedMove = Quaternion.Euler(new Vector3(0.0f, tr.eulerAngles.y, 0.0f)) * intendedMove;
            // Quaternion.Euler : z축 주위로 z, x축 주위로 x, y축 주위로 y 각도만큼 회전한(순서대로) Rotation을 반환합니다
            // transform.eulerAngles : 회전(rotation)의 오일러(euler) 각도를 반환합니다.
            intendedMove = tr.InverseTransformDirection(intendedMove);

            tr.Translate(intendedMove, Space.Self);
        }
        if (keyboardInput == true)
        {
            Vector3 intendedMove = new Vector3(KeyboardInput.x, 0, KeyboardInput.y);
            intendedMove *= keyboardMoveSpeed;
            intendedMove *= Time.deltaTime;
            intendedMove = Quaternion.Euler(new Vector3(0.0f, tr.eulerAngles.y, 0.0f)) * intendedMove;
            intendedMove = tr.InverseTransformDirection(intendedMove);
            // 월드 좌표와 로컬 좌표 변환
            /*
             * InverseTransformDirection : 월드 방향의 벡터를 로컬 벡터로 변환시킨다.
             * ex) 로컬 벡터 = this.transform.InverseTransformDirection(월드 벡터);
             * TransformPoint : 로컬 위치 벡터를 월드 위치 벡터로 바꾼다.
             * ex) 월드 위치 벡터 = this.transform.TransformPoint(로컬 위치 벡터);
            */
            tr.Translate(intendedMove, Space.Self);
        }
    }
}

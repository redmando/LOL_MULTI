using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    public LayerMask layerMask;
    public float moveSpeed;
    public float rotateSpeed;

    private Animator anim;
    private Player_State state;
    private Transform tr;

    private Vector3 direction;
    private float angle;
    private float distance;

    private void Start()
    {
        anim = GetComponent<Animator>();
        state = GetComponent<Player_State>();
        tr = GetComponent<Transform>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                distance = Vector3.Distance(tr.position, hit.point);
                direction = (hit.point - tr.position).normalized;
                StopAllCoroutines();
                StartCoroutine(Move());
                StartCoroutine(Turn());
            }
        }
    }

    IEnumerator Move()
    {
        float currentDistance = 0;
        anim.SetBool("IsRun", true);

        while (distance > 0.0f)
        {
            float delta = moveSpeed * Time.deltaTime;

            if (currentDistance + delta > distance)
            {
                delta = distance - currentDistance;
            }

            tr.position += direction * delta;
            currentDistance += delta;

            if (currentDistance >= distance)
            {
                distance = 0;
                currentDistance = 0;
                anim.SetBool("IsRun", false);
            }
            yield return null;
        }
    }

    IEnumerator Turn()
    {
        float currentAngle = 0;
        float dotValue = Vector3.Dot(tr.forward, direction);
        angle = Mathf.Acos(dotValue) * Mathf.Rad2Deg;   // Mathf.Acos(dotValue)에서 Radian 값이 나옴
                                                        // 1 radian은 약 57.3도에 해당하는 값
                                                        // 반지름이 3이고, 중심각이 2 radian이면 호의 길이는 6
                                                        // 180 degree = π radian
                                                        // A dot B = |A||B|cos@  이다
                                                        // cos@ = A dot B / | A || B |
                                                        // @ = cos^ -1(A dot B / | A || B |)
                                                        // 구하려는 각 = 아크코사인(코사인의 역함수) * A와 B의 내적 * |A벡터의 크기| * |B벡터의 크기|
                                                        // 여기서 A벡터와 B벡터는 노말벡터 임으로 둘이 곱해도 1
                                                        // 그 다음 구해진 라디안 값을 degree로 변환시켜야 함으로 Mathf.Rad2Deg를 곱해준다

        dotValue = Vector3.Dot(tr.right, direction);
        if (dotValue < 0)
        {
            angle = -angle;
        }

        while (Mathf.Abs(angle) > 0.0f)
        {
            float delta = rotateSpeed * Time.deltaTime;

            if (dotValue < 0)
            {
                if (currentAngle - delta < angle)
                {
                    delta = angle - currentAngle;
                }

                if (delta < 0)
                {
                    delta = -delta;
                }

                tr.Rotate(Vector3.up * -delta);
                currentAngle -= delta;
                if (currentAngle <= angle)
                {
                    angle = 0;
                    currentAngle = 0;
                }
            }
            else
            {
                if(currentAngle + delta > angle)
                {
                    delta = angle - currentAngle;
                }

                tr.Rotate(Vector3.up * delta);
                currentAngle += delta;

                if(currentAngle >= angle)
                {
                    angle = 0;
                    currentAngle = 0;
                }
            }
            yield return null;
        }
    }
}

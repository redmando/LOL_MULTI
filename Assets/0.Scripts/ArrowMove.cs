using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMove : MonoBehaviour
{
    public float fAttackDamage = 40.0f;

    private float dis;
    private float speed;
    private float waitTime;

    public float fSpeed;

    public Transform Tr;

    public Transform TargetTr;

    private float dist;

    void Start()
    {
        Tr = GetComponent<Transform>();

        if (TargetTr == null)
        {
            Destroy(gameObject);
        }

        dis = Vector3.Distance(Tr.position, TargetTr.position);

        // 미사일 생성후 초반에 벌어지듯이 연출하기 위해
        // 미사일의 회전을 캐릭터 위치에서 포탄의 위치의 방향으로 놓는다.
        // transform.rotation = Quaternion.LookRotation(transform.position - 캐릭터의 위치);
        // transform.rotation = Quaternion.LookRotation(transform.position - Tr.position);

        StartCoroutine(this.TimeDestory());  // 일정 시간(5초)가 지나면 자동으로 삭제.
    }

    IEnumerator TimeDestory()
    {
        yield return new WaitForSeconds(10.0f);      //  지연시간.
        Destroy(gameObject);
    }

    void Update()
    {
        DiffusionMissileMoveOperation();

        if (TargetTr == null) { return; }

        dist = Vector3.Distance(TargetTr.position, transform.position);

        if (dist < 0.1f)     //  적에게 명중한 경우.
        {
            if(TargetTr.gameObject)
                TargetTr.SendMessage("Damage", fAttackDamage);
            Destroy(gameObject);
        }
    }

    void DiffusionMissileMoveOperation()
    {
        if (TargetTr == null)
        {
            Destroy(gameObject);
            return;
        }

        waitTime += Time.deltaTime;
        // 1.5초 동안 천천히 forward 방향으로 전진 한다.
        if (waitTime < 1.5f)
        {
            speed = Time.deltaTime;
            //transform.Translate(Tr.forward * speed, Space.World);
            transform.Translate(Tr.forward * speed, Space.World);
        }
        else
        {
            // 1.5초 이후에 타겟 방향으로 Lerp 위치 이동 한다.
            speed += Time.deltaTime;
            float t = speed / dis;

            //Tr.position = Vector3.LerpUnclamped(Tr.position, TargetTr.position, t);

            if(TargetTr.gameObject != null)     // 타겟이 있을 경우에만...적용.
                Tr.position = Vector3.Lerp(Tr.position, TargetTr.position, Time.deltaTime * 5.0f);
        }

        // 매 프레임 마다 타겟 방향으로 미사일이 방향을 바꿈.
        // 타겟 위치 - 미사일 위치 = 미사일이 타겟에서의 방향
        Vector3 directionVec = TargetTr.position - Tr.position;
        Quaternion qua = Quaternion.LookRotation(directionVec);
        Tr.rotation = Quaternion.Slerp(Tr.rotation, qua, Time.deltaTime * 2.0f);
    }

}

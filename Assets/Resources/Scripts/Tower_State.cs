using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_State : MonoBehaviour
{
    [Header("State")]
    public Transform crystal;
    public Transform firePos;
    public GameObject bullet;
    public float range;
    public float coolTime;
    public float turnSpeed;
    public string enemyUnitTag;

    [Header("SFX")]
    [SerializeField] private ParticleSystem attackEffect;

    private GameObject target;
    public bool canAttack;
    public bool hasTarget;
    public float leftCoolTime;

    private Building_State state;

    private void Start()
    {
        state = GetComponent<Building_State>();
        leftCoolTime = coolTime;
    }

    private void FixedUpdate()
    {
        if (state.isDestroyed == false)
        {
            canAttack = CanAttackCheck();
            EnemySearch(firePos.position, range);
            EnemyAttack();
        }
    }

    private bool CanAttackCheck()
    {
        if (leftCoolTime >= 0)
        {
            leftCoolTime -= Time.deltaTime;
            return false;
        }
        else
        {
            return true;
        }
    }

    private void EnemySearch(Vector3 _pos, float _radius)
    {
        // [임시] 크리스탈 회전
        crystal.Rotate(Vector3.up, turnSpeed * Time.deltaTime);

        // 반지름의 거리 안에 적이 있는지 파악
        Collider[] colls = Physics.OverlapSphere(_pos, _radius);

        target = null;
        hasTarget = false;

        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i].CompareTag(enemyUnitTag))
            {
                float distance = Vector3.Distance(firePos.position, colls[i].transform.position);

                if (range >= distance && colls[i].gameObject.GetComponent<Minion_State>().isDead == false)
                {
                    target = colls[i].gameObject;
                    hasTarget = true;
                }
            }
        }
    }

    private void EnemyAttack()
    {
        if (hasTarget == true && canAttack == true)
        {
            attackEffect.Play();
            GameObject missile = Instantiate(bullet, firePos.position, firePos.rotation);
            missile.GetComponent<Tower_Missile>().target = target.transform;
            leftCoolTime = coolTime;
        }
        else
        {
            attackEffect.Stop();
        }
    }
}

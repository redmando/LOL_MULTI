using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow_Missile : MonoBehaviour
{
    public float speed;
    public float damage;
    public Transform target;

    private void Update()
    {
        GuidedMissile();
    }

    private void EnemySet(Transform _target)
    {
        target = _target;
    }

    private void DamageSet(float _damage)
    {
        damage = _damage;
    }

    private void GuidedMissile()
    {
        if (target == null)
        {
            Destroy(gameObject);
        }

        Vector3 direction = target.position - transform.position;
        float distance = Vector3.Distance(target.position, transform.position);
        float delta = speed * Time.deltaTime;

        direction.Normalize();
        transform.Translate(direction * delta, Space.World);
        transform.position += new Vector3(0, Mathf.Sin(0.1f), 0);
        // 적을 바라보게 만들 함수 넣을 자리

        if (distance <= 1.0f)
        {
            target.SendMessage("Damaged", damage);
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower_Missile : MonoBehaviour
{
    public Transform target;
    public float damage;

    [SerializeField]
    private float speed;

    private Transform tr;
    private float distance;

    private void Start()
    {
        tr = GetComponent<Transform>();

        if (target == null)
        {
            Destroy(gameObject);
        }
        else
        {
            tr.rotation = Quaternion.LookRotation(target.position - tr.position);
        }
    }

    private void Update()
    {
        GuidedMissile();
    }

    private void GuidedMissile()
    {
        if (target == null)
        {
            Destroy(gameObject);
        }

        float delta = speed * Time.deltaTime;
        Vector3 direction =  target.position - tr.position;
        direction.Normalize();
        distance = Vector3.Distance(tr.position, target.position);

        transform.Translate(direction * delta, Space.World);

        if (distance <= 0.1f)
        {
            target.SendMessage("Damaged", damage);
            Destroy(gameObject);
        }
    }
}

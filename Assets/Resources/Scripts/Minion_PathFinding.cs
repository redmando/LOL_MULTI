using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Minion_PathFinding : MonoBehaviour
{
    public enum Team { BLUE, RED }

    [Header("PathFinding And Attack")]
    public Team team;
    public string line;
    public string enemyUnitTag;
    public string enemyBuildingTag;

    [Header("Check")]
    public GameObject enemy = null;

    [SerializeField]
    private Transform currenttargetTr;

    private Animator anim;
    private NavMeshAgent nav;
    private Transform[] wayPoints = new Transform[8]; // if you don't make instance, there could be the error that shows nullException.
    private Minion_State state;

    private float distance;
    private float range;
    private int count = 0;

    private void Start()
    {
        anim = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        state = GetComponent<Minion_State>();

        range = state.range;

        NavMeshSet();
        StartCoroutine(MinionMove());
    }

    private void LateUpdate()
    {
        if (state.isDead)
        {
            nav.isStopped = true;
        }
        NearEnemyCheck(transform.position, range);
    }

    private void NavMeshSet()
    {
        nav.speed = state.speed;
        string lineWaypoint = line;

        if (team == Team.BLUE)
        {
            for (int i = 0; i < 8; i++)
            {
                wayPoints[i] = GameObject.Find("Waypoints").transform.Find(lineWaypoint).transform.Find(lineWaypoint + "_0" + i).GetComponent<Transform>();
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                wayPoints[i] = GameObject.Find("Waypoints").transform.Find(lineWaypoint).transform.Find(lineWaypoint + "_0" + (wayPoints.Length - 1 - i)).GetComponent<Transform>();
            }
        }
    }

    IEnumerator MinionMove()
    {
        while (state.isDead == false)
        {
            yield return new WaitForSeconds(0.5f);

            // If enemy exists follow enemy, else follow waypoint.
            if (enemy != null)
            {
                currenttargetTr = enemy.transform;
                distance = Vector3.Distance(enemy.transform.position, transform.position);
            }
            else
            {
                currenttargetTr = wayPoints[count];
            }
            // Check the distance between minion and current target and set destination.
            nav.SetDestination(currenttargetTr.position);

            if (enemy != null && range >= distance)
            {
                transform.forward = (enemy.transform.position - transform.position).normalized;
                nav.isStopped = true;
                anim.SetBool("IsWalk", false);
                anim.SetBool("IsAttack", true);
            }
            else
            {
                nav.isStopped = false;
                anim.SetBool("IsWalk", true);
                anim.SetBool("IsAttack", false);
            }

            if (Vector3.Distance(transform.position, wayPoints[count].position) <= 5.0f)
            {
                if (count < 8)
                {
                    count++;
                }
                else
                {
                    count = 8;
                }
            }
        }
    }

    private void MeleeAttack()
    {
        if(enemy != null)
        {
            enemy.SendMessage("Damaged", state.damage);
        }
        else
        {
            enemy = null;
            return;
        }
    }

    private void NearEnemyCheck(Vector3 _pos, float _radius)
    {
        Collider[] colls = Physics.OverlapSphere(_pos, _radius * 2);
        float minDistance = _radius * 2;
        float comparativeDistance;

        for (int i = 0; i < colls.Length; i++)
        {
            if ((colls[i].CompareTag(enemyUnitTag) && colls[i].GetComponent<Minion_State>().isDead == false) ||
               (colls[i].CompareTag(enemyBuildingTag) && colls[i].GetComponent<Building_State>().isDestroyed == false))
            {
                comparativeDistance = Vector3.Distance(transform.position, colls[i].gameObject.transform.position);
                if (minDistance > comparativeDistance)
                {
                    minDistance = comparativeDistance;
                    enemy = colls[i].gameObject;
                }
            }
        }
    }
}

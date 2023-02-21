using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minion_Range : MonoBehaviour
{
    [Header("Weapon")]
    public GameObject arrowPrefab;

    public Transform bow;
    public Transform arrow;

    private Minion_State state;
    private Minion_PathFinding pathFinding;

    private void Start()
    {
        pathFinding = GetComponent<Minion_PathFinding>();
        state = GetComponent<Minion_State>();
        WeaponSet();
    }

    private void WeaponSet()
    {
        bow.localPosition = new Vector3(0, -1.34f, 0);
        arrow.localPosition = new Vector3(2, -0.5f, 0.7f);
        arrow.localRotation = Quaternion.Euler(-1f, -20f, 70f);
        arrow.gameObject.SetActive(false);
    }

    private void ArrowSet()
    {
        arrow.gameObject.SetActive(true);
    }

    private void ArrowShoot()
    {
        if (pathFinding.enemy != null)
        {
            arrow.gameObject.SetActive(false);
            GameObject obj = Instantiate(arrowPrefab, arrow.position, arrow.rotation);
            obj.GetComponent<Arrow_Missile>().target = pathFinding.enemy.transform;
            obj.GetComponent<Arrow_Missile>().damage = state.damage;
        }
        else
        {
            return;
        }
    }
}

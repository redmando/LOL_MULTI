using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inhibitor_State : MonoBehaviour
{
    public enum LineType { TOP, MID, BOTTOM };

    [Header("State")]
    public LineType lineType;
    public Transform crystal;
    public GameObject nexus;
    public float moveSpeed;
    public float recoverTime;

    private bool isRecover;

    private Building_State state;

    private void Start()
    {
        state = GetComponent<Building_State>();
        isRecover = false;
    }

    private void FixedUpdate()
    {
        SuperSpawnCheck();
    }
    private void SuperSpawnCheck()
    {
        if (state.isDestroyed == true && isRecover == false)
        {
            StartCoroutine(InhibitorRecover());
            //nexus.SendMessage("SuperMinion", lineType);
            isRecover = true;
        }
        else
        {
            float delta = 0.5f;
            Vector3 movePos = crystal.position;
            movePos.y = delta * Mathf.Sin(Time.time * moveSpeed) + 5.0f;
            crystal.position = movePos;
        }
    }

    IEnumerator InhibitorRecover()
    {
        float timer = 0;

        while (isRecover == true)
        {
            yield return new WaitForSeconds(1.0f);
            timer += Time.time;

            if (timer >= recoverTime)
            {
                state.SendMessage("Recover");
                isRecover = false;
                timer = 0;
            }
        }
    }
}

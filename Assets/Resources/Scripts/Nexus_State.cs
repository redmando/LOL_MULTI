using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nexus_State : MonoBehaviour
{
    [Header("State")]
    public Transform crystal;
    public float moveSpeed;
    public float turnSpeed;

    [Header("SpawnPos")]
    public Transform topLinePos;
    public Transform midLinePos;
    public Transform botLinePos;

    [Header("SpawnTime")]
    public bool gameStart = true;
    public float startDealy = 65.0f;
    public float spawnInterval = 1.0f;
    public float spawnCooltime;

    [Header("Minion")]
    public GameObject meeleMinion;
    public GameObject rangeMinion;
    public GameObject superMinion;

    private enum LineType { TOP, MID, BOT };
    private LineType lineType;

    private void Start()
    {
        StartCoroutine(SpawanTimeCheck());
    }

    IEnumerator SpawanTimeCheck()
    {
        if(gameStart == true)
        {
            yield return new WaitForSeconds(startDealy);
            MinionSpawn(meeleMinion, LineType.TOP);
            MinionSpawn(meeleMinion, LineType.MID);
            MinionSpawn(meeleMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(meeleMinion, LineType.TOP);
            MinionSpawn(meeleMinion, LineType.MID);
            MinionSpawn(meeleMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(meeleMinion, LineType.TOP);
            MinionSpawn(meeleMinion, LineType.MID);
            MinionSpawn(meeleMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(rangeMinion, LineType.TOP);
            MinionSpawn(rangeMinion, LineType.MID);
            MinionSpawn(rangeMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(rangeMinion, LineType.TOP);
            MinionSpawn(rangeMinion, LineType.MID);
            MinionSpawn(rangeMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(rangeMinion, LineType.TOP);
            MinionSpawn(rangeMinion, LineType.MID);
            MinionSpawn(rangeMinion, LineType.BOT);
            gameStart = false;
        }
        else
        {
            yield return new WaitForSeconds(spawnCooltime);
            MinionSpawn(meeleMinion, LineType.TOP);
            MinionSpawn(meeleMinion, LineType.MID);
            MinionSpawn(meeleMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(meeleMinion, LineType.TOP);
            MinionSpawn(meeleMinion, LineType.MID);
            MinionSpawn(meeleMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(meeleMinion, LineType.TOP);
            MinionSpawn(meeleMinion, LineType.MID);
            MinionSpawn(meeleMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(rangeMinion, LineType.TOP);
            MinionSpawn(rangeMinion, LineType.MID);
            MinionSpawn(rangeMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(rangeMinion, LineType.TOP);
            MinionSpawn(rangeMinion, LineType.MID);
            MinionSpawn(rangeMinion, LineType.BOT);
            yield return new WaitForSeconds(spawnInterval);
            MinionSpawn(rangeMinion, LineType.TOP);
            MinionSpawn(rangeMinion, LineType.MID);
            MinionSpawn(rangeMinion, LineType.BOT);
        }
    }

    private void MinionSpawn(GameObject _minion, LineType _lineType)
    {
        Transform spawnPosition = null;

        switch (_lineType)
        {
            case LineType.TOP:
                spawnPosition = topLinePos;
                break;
            case LineType.MID:
                spawnPosition = midLinePos;
                break;
            case LineType.BOT:
                spawnPosition = botLinePos;
                break;
            default:
                Debug.Log("Wrong allocation");
                break;
        }

        GameObject obj = Instantiate(_minion, spawnPosition.position, spawnPosition.rotation);
        obj.GetComponent<Minion_PathFinding>().line = _lineType.ToString();
    }

    private void FixedUpdate()
    {
        CrystalMove();
    }

    private void CrystalMove()
    {
        float heightChange = spawnInterval;
        Vector3 movePos = crystal.position;
        movePos.y = heightChange * Mathf.Sin(Time.time * moveSpeed) + 5.0f;
        crystal.position = movePos;
        crystal.Rotate(Vector3.up * turnSpeed * Time.deltaTime);
    }
}

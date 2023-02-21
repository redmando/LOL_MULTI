using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomData : MonoBehaviour
{
    public string roomName = "";
    public int PlayerCount = 0;
    public int maxPlayer = 0;

    public Text roomDataTxt;

    private void Awake()
    {
        roomDataTxt = GetComponentInChildren<Text>();
    }
    public void UpDateInfo()
    {
        roomDataTxt.text = string.Format(" {0} [{1}/{2}]", roomName, PlayerCount.ToString("00"), maxPlayer);
    }
}

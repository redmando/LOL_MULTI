using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomDate : MonoBehaviour
{
    public string roomName = "";
    public int playerCount = 0;
    public int maxPlayer = 0;

    public Text roomDateText;

    private void Awake()
    {
        roomDateText = GetComponentInChildren<Text>();
    }
    public void UpDateInfo()
    {
        roomDateText.text = string.Format(" {0} [{1}/{2}] ", roomName, playerCount.ToString("00"), maxPlayer);
    }
}

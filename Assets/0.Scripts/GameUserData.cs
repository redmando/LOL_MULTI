using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUserData : MonoBehaviour
{
    // 게임 데이터 저장.
    public int[] nActorNumber;      //  고유 아이디 권장 안함. 방 생성후 액터 고유 넘버를 사용.
    public string[] sNickName;      //  유저 닉네임
    public bool[] bMasterClient;    //  방장인지 저장.
    // 게임 정보
    public bool[] bReady;           //  레디 상태 인가?
    public int[] nChampionNum;      //  몇 번 챔피언을 선택했는가?

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}

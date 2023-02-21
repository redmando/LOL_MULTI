using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Building : MonoBehaviourPunCallbacks, IPunObservable
{
    // true일 경우 Top, false일 경우 Bottom.
    public enum eTeam { none, Top, Bottom };
    public eTeam curTeam = eTeam.none;   // 상태 초기화.
    // 억제기 인지 넥서스 인지..
    public enum eBuilding { tower, top, middle, bottom , nexus };     //  타워, 억제기 건물 상, 중, 하, 넥서스
    public eBuilding curBuilding = eBuilding.tower;  // 상태 초기화.

    public Image hpBar;
    [SerializeField]
    private float currHP = 5000.0f;
    [SerializeField]
    private float initHP = 5000.0f;

    public bool isDie = false;

    // 죽는것과 HP만 동기화 (HP 처리후 자동 삭제.)

    void Damage(float fdamage)
    {
        if (photonView.IsMine)
        {
            currHP -= fdamage;
            hpBar.fillAmount = currHP / initHP;

            if (currHP <= 0)    // 파괴 되면..
            {
                isDie = true;
                GetComponent<DeadState>().bDead = true;
            }
        }
        else
        {
            currHP = OncurrHp;      //  HP 업데이트.
            hpBar.fillAmount = currHP / initHP;
        }
    }
    void QDamage(float fdamage)
    {
        currHP -= fdamage;
        hpBar.fillAmount = currHP / initHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            isDie = true;
            this.GetComponent<DadeState>().bDead = true;
            Debug.Log("챔피언 죽음");
        }
    }
    void EDamage(float fdamage)
    {
        currHP -= fdamage;
        hpBar.fillAmount = currHP / initHP;

        if (currHP < 0.0f && !this.GetComponent<DadeState>().bDead)
        {
            isDie = true;
            this.GetComponent<DadeState>().bDead = true;
              Debug.Log("챔피언 죽음");
        }
    }

    private void LateUpdate()
    {
        // 조작 부분 수정
        if(!photonView.IsMine)      // 내가 소환한 건물이 아니라면
        {
            currHP = OncurrHp;      //  HP 업데이트
            hpBar.fillAmount = currHP / initHP;
        }

        if(isDie)
        {
            StartCoroutine(this.Dead());
        }
    }
    IEnumerator Dead()
    {
        yield return new WaitForSeconds(1.0f);  // 지연시간.
        
        // 팀인지를 체크..
        if(curTeam == eTeam.Top)
        {
            // 억제기 or 넥서스 인경우 처리..
            switch (curBuilding)
            {
                case eBuilding.top:
                    GameManager.Instance.bUpgradeBottomTop = true;
                    break;
                case eBuilding.middle:
                    GameManager.Instance.bUpgradeBottomMiddle = true;
                    break;
                case eBuilding.bottom:
                    GameManager.Instance.bUpgradeBottomBottom = true;
                    break;
                case eBuilding.nexus:
                    GameManager.Instance.bDestoryTopNexus = true;
                    GameManager.Instance.bGamePause = true;     //  게임 정지 요청.
                    GameManager.Instance.GamePause();           //  게임 정지.
                    GameManager.Instance.GameOver();            //  게임 종료
                    Debug.Log("아랫마을 승리!! 게임오버!!");
                    break;
            }
        }
        else if(curTeam == eTeam.Bottom )
        {
            // 억제기 or 넥서스 인경우 처리..
            switch (curBuilding)
            {   
                case eBuilding.top:
                    GameManager.Instance.bUpgradeTopTop = true;
                    break;
                case eBuilding.middle:
                    GameManager.Instance.bUpgradeTopMiddle = true;
                    break;
                case eBuilding.bottom:
                    GameManager.Instance.bUpgradeTopBottom = true;
                    break;
                case eBuilding.nexus:
                    GameManager.Instance.bDestoryBottomNexus = true;
                    GameManager.Instance.bGamePause = true;     //  게임 정지 요청.
                    GameManager.Instance.GamePause();           //  게임 정지.
                    GameManager.Instance.GameOver();            //  게임 종료
                    Debug.Log("윗마을 승리!! 게임오버!!");
                    break;

            }
        }

        Destroy(gameObject);
    }

    public float OncurrHp = 5000.0f;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
        if(stream.IsWriting)        //  데이터를 계속 전송만..
        {
            stream.SendNext(currHP);
            stream.SendNext(isDie);
        }
        else
        {
            OncurrHp =  (float)stream.ReceiveNext();
            isDie = (bool)stream.ReceiveNext();
        }
    }
}

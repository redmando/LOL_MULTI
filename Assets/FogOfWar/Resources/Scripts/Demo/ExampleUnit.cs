using UnityEngine;
using System.Collections;

public class ExampleUnit : MonoBehaviour
{
    private Revealer _Revealer;
    public float VisionRange = 6f;
    public FogOfWar.Players Faction = 0;
    [Range(0, 255)]
    public int UpVision = 10;
    public MeshRenderer meshRenderer;
    public float TurnOffDelay = 0.0f;

    public GameObject objHide;      //숨길 오브젝트

    public void Start()
    {
        if(!meshRenderer)
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

        _Revealer = new Revealer(VisionRange,
            Faction,
            UpVision,
            gameObject);

        FogOfWar.RegisterRevealer(_Revealer);

        if (FogOfWar.IsPositionRevealedByFaction(transform.position, FogOfWar.RevealFaction))
        {
            meshRenderer.enabled = true;
            objHide.SetActive(true);
        }
        else {
            meshRenderer.enabled = false;
            objHide.SetActive(false);
        }
    }

    private bool Hide = false;
    private float StartTime = 0;
    
    void Update()
    {
        if (Hide)
        {
            if (Time.time >= StartTime + TurnOffDelay)
            {
                meshRenderer.enabled = false;
                objHide.SetActive(false);

            }
        }

        _Revealer.VisionRange = VisionRange;

        if (Faction != FogOfWar.RevealFaction)
        {
            if (FogOfWar.IsPositionRevealedByFaction(transform.position, FogOfWar.RevealFaction))
            {
                if (Hide)
                {
                    Hide = false;
                }
                meshRenderer.enabled = true;
                objHide.SetActive(true);

            }
            else {
                if (!Hide)
                {
                    StartTime = Time.time;
                    Hide = true;
                }
            }
        } else {
            if (!meshRenderer.enabled)
            {
                meshRenderer.enabled = true;
            }
            if (!objHide.activeSelf)
            {
                objHide.SetActive(true);
            }
        }
    }
    
    public void OnDisable()
    {
        if (_Revealer != null)
            FogOfWar.UnRegisterRevealer(_Revealer);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position + Vector3.up, "Revealer.png", true);
    }

    public void ChangeFaction(FogOfWar.Players _Faction)
    {
        Debug.Log("Changeing to: "+_Faction);
        FogOfWar.UnRegisterRevealer(_Revealer);
        _Revealer.Faction = _Faction;
        FogOfWar.RegisterRevealer(_Revealer);
    }
}

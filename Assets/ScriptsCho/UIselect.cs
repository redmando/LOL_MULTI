using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIselect : MonoBehaviour
{
    public GameObject objSelectMenu;

    // Start is called before the first frame update
    void Start()
    {
        objSelectMenu = GameObject.Find("Panel_Playervel");
    }
    public void click()
    {
        objSelectMenu.transform.position = transform.position;
    }
}

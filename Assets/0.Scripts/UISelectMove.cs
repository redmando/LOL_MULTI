using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISelectMove : MonoBehaviour
{
    public GameObject objSelectMenu;

    void Start()
    {
        objSelectMenu = GameObject.Find("Panel-player");
    }

    public void Click()
    {
        objSelectMenu.transform.position = transform.position;
    }

}

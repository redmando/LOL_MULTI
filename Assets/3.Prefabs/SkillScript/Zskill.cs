using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zskill : MonoBehaviour
{
    public float movespeed;
    


    //미사일 퍼지게 할건데
    //오브젝트들 스크립트에 하나씩 넣어 자리선정
     [SerializeField]
     private GameObject apear12;
     [SerializeField]
     private GameObject apear1;
     [SerializeField]
     private GameObject apear2;
     [SerializeField]
     private GameObject apear3;
     [SerializeField]
     private GameObject apear4;
     [SerializeField]
     private GameObject apear5;
     [SerializeField]
     private GameObject apear6;
     [SerializeField]
     private GameObject apear7;
     [SerializeField]
     private GameObject apear8;
     [SerializeField]
     private GameObject apear9;
     [SerializeField]
     private GameObject apear10;
     [SerializeField]
     private GameObject apear11;

   // public GameObject[] apear;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       // transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
        

       apear12.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear1.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear2.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear3.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear4.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear5.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear6.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear7.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear8.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear9.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear10.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);
       apear11.transform.Translate(Vector3.forward * Time.deltaTime * movespeed);

    }
}

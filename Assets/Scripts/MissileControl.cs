using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileControl : MonoBehaviour {

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "Asteroid")
        {
            Destroy(col.gameObject);
            Destroy(gameObject);
        }
    }
}
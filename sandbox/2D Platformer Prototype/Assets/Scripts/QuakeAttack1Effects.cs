using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuakeAttack1Effects : MonoBehaviour {

    void Start()
    {
        GameObject.Destroy(gameObject, 1);
    }

}

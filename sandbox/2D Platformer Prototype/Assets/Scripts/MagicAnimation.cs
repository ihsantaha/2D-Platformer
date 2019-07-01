using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicAnimation : MonoBehaviour {

    private Animator anim;




    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void Die()
    {
        anim.SetTrigger("Die");
    }

}

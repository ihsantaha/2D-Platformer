using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void Duck(bool duck)
    {
        anim.SetBool("Duck", duck);
    }

    public void Jump(bool jump)
    {
        anim.SetBool("Jump", jump);
    }

    public void Fall(bool fall)
    {
        anim.SetBool("Fall", fall);
    }

    public void WallSlide(bool wallSlide)
    {
        anim.SetBool("WallSlide", wallSlide);
    }

    public void Move(float move)
    {
        if (anim.gameObject.activeSelf)
            anim.SetFloat("Move", Mathf.Abs(move));
    }

    public void Dodge(bool dodge)
    {
        anim.SetBool("Dodge", dodge);
    }

    public void Push(bool push)
    {
        anim.SetBool("Push", push);
    }

    public void Pull(bool pull)
    {
        anim.SetBool("Pull", pull);
    }

    public void Attack()
    {
        anim.SetTrigger("Attack");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator _anim;

    void Start()
    {
        _anim = GetComponentInChildren<Animator>();
    }

    public void Duck(bool duck)
    {
        _anim.SetBool("Duck", duck);
    }

    public void Jump(bool jump)
    {
        _anim.SetBool("Jump", jump);
    }

    public void Move(float move)
    {
        if (_anim.gameObject.activeSelf)
            _anim.SetFloat("Move", Mathf.Abs(move));
    }

    public void Dodge(bool dodge)
    {
        _anim.SetBool("Dodge", dodge);
    }

    public void Push(bool push)
    {
        _anim.SetBool("Push", push);
    }

    public void Pull(bool pull)
    {
        _anim.SetBool("Pull", pull);
    }

    public void Attack()
    {
        _anim.SetTrigger("Attack");
    }
}

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

    public void Attack()
    {
        _anim.SetTrigger("Attack");
    }
}

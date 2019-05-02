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


    public void Move(float move)
    {
        if (anim.gameObject.activeSelf)
            anim.SetFloat("Move", Mathf.Abs(move));
    }


    public void Defend(int defend)
    {
        anim.SetFloat("Defend", Mathf.Abs(defend));
    }


    public void Slide(bool value)
    {
        anim.SetBool("Slide", value);
    }


    public void Dodge(bool dodge)
    {
        anim.SetBool("Dodge", dodge);
    }


    public void InCrawlSpace(bool inCrawlSpace)
    {
        anim.SetBool("InCrawlSpace", inCrawlSpace);
    }


    public void WallSlide(bool wallSlide)
    {
        anim.SetBool("WallSlide", wallSlide);
    }


    public void InWater(bool inWater)
    {
        anim.SetBool("InWater", inWater);
    }


    public void FloatInWater(bool floatInWater)
    {
        anim.SetBool("FloatInWater", floatInWater);
    }


    public void Swim(bool swim)
    {
        anim.SetBool("Swim", swim);
    }


    public void Climb(bool climb, int speed = 1)
    {
        anim.SetBool("Climb", climb);
        anim.speed = speed;
    }


    public void ClimbInProfileView(bool climbInProfileView, int speed = 1)
    {
        anim.SetBool("ClimbInProfileView", climbInProfileView);
        anim.speed = speed;
    }


    public void Hang(bool hang, int speed = 1)
    {
        anim.SetBool("Hang", hang);
        anim.speed = speed;
    }


    public void HangOnCliff(bool hangOnCliff)
    {
        anim.SetBool("HangOnCliff", hangOnCliff);
    }


    public void Push(bool push)
    {
        anim.SetBool("Push", push);
    }


    public void Pull(bool pull)
    {
        anim.SetBool("Pull", pull);
    }


    public void ClimbUpCliff()
    {
        anim.SetTrigger("ClimbUpCliff");
    }


    public void Attack()
    {
        anim.SetTrigger("Attack");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // Fields
    // --------------------------------------------------------------------------------

    private Rigidbody2D rB2D;
    private Animator anim;
    private Player player;
    private string status;

    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start () {
        rB2D = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        status = "closed";
    }
	

	void Update () {
        Transfer();
        Open();
        Close();
	}


    bool Transfer()
    {
        return false;
    }


    void Open()
    {
        if (IsNearPlayer() && Input.GetKeyDown(KeyCode.O) && status == "closed")
        {
            status = "opened";
            anim.SetTrigger("Open");
            StartCoroutine(Fade());
        }
    }

    void Close()
    {
        if (IsNearPlayer() && Input.GetKeyDown(KeyCode.C) && status == "opened")
        {
            status = "closed";
            anim.SetTrigger("Close");
        }
    }

    IEnumerator Fade()
    {
        float fadeTime = GameObject.Find("Fade").GetComponent<Fading>().BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        GameObject test = GameObject.FindWithTag("Block");
        player.transform.position = new Vector3(test.transform.position.x, test.transform.position.y);
        yield return new WaitForSeconds(fadeTime);
        GameObject.Find("Fade").GetComponent<Fading>().OnLevelWasLoaded();
        status = "closed";
        anim.SetTrigger("Close");

    }

    // void RotateToSide(bool rotateToSide)
    // {
    //    anim.SetTrigger("RotateToSide");
    // }

    // void RotateToFront(bool open)
    // {
    //    anim.SetTrigger("RotateToFront");
    // }




    // ----------------------------------------
    // Raycast Status
    // ----------------------------------------

    bool IsNearPlayer()
    {
        RaycastHit2D hitPlayerRight = Physics2D.Raycast(transform.position, Vector2.right, 0.55f, 1 << 10);
        RaycastHit2D hitPlayerLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.55f, 1 << 10);
        return (hitPlayerRight.collider != null && hitPlayerLeft.collider != null) ? true : false;
    }
}

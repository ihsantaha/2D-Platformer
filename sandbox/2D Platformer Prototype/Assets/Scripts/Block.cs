using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // Fields
    // --------------------------------------------------------------------------------

    private Rigidbody2D rB2D;
    private Player player;

    bool inAir;
    

    
    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start() {
        rB2D = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }
    
	void Update() {
        IsMovable();
        IsOnCliff();

        Debug.Log(transform.rotation.z);
        if (!IsGrounded())
        {
            inAir = true;
        }

        if (inAir)
        {
            if (IsGrounded())
            {
                StartCoroutine(HasLandedRoutine());
                inAir = false;
            }
        }
    }

    void IsMovable()
    { 
        if (player.canMoveBlock && IsNearPlayer())
        {
            if ((Input.GetKey(KeyCode.M) && (player.playerState.pushingRight || player.playerState.pushingLeft)) || !IsGrounded())
            {
                player.playerAnimation.Push(true);
                rB2D.bodyType = RigidbodyType2D.Dynamic;
            }
            else if (IsGrounded())
            {
                rB2D.bodyType = RigidbodyType2D.Kinematic;
            }

            // Move in the direction of the player if he is pulling and let the box collider naturally push him away
            if (player.playerState.pullingLeft || player.playerState.pullingRight)
            {
                // rB2D.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * 0.75f, rB2D.velocity.y);
                transform.Translate(Input.GetAxisRaw("Horizontal") * 0.01f, 0, 0);
                player.transform.Translate(Input.GetAxisRaw("Horizontal") * 0.01f, 0, 0);
                player.playerAnimation.Pull(true);
            }
        }
    }

    void IsOnCliff()
    {
        rB2D.drag = IsGrounded() ? 10 : 1;
    }

    IEnumerator HasLandedRoutine()
    {
        yield return new WaitForSeconds(1);
        rB2D.bodyType = RigidbodyType2D.Kinematic;
        transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, 0, 0);
    }



    // ----------------------------------------
    // Raycast Status
    // ----------------------------------------

    bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 0.55f, 1 << 8);
        return hitGround.collider != null ? true : false;
    }

    bool IsNearPlayer()
    {
        RaycastHit2D hitPlayerRight = Physics2D.Raycast(transform.position, Vector2.right, 0.55f, 1 << 10);
        RaycastHit2D hitPlayerLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.55f, 1 << 10);
        return (hitPlayerRight.collider != null || hitPlayerLeft.collider != null) ? true : false;
    }
}

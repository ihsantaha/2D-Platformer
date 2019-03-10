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

    void Start()
    {
        rB2D = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }


    void Update()
    {
        IsMovable();
        IsOnCliff();
        IsInAir();
        IsNearWall();
        UpdateCanMoveBlock();
    }


    void IsMovable()
    {
        if (!IsGrounded())
        {
            rB2D.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            rB2D.bodyType = RigidbodyType2D.Static;
        }

        if (player.canMoveBlock && IsNearPlayer() && IsGrounded())
        {
            if ((player.interactionState.pushingLeft || player.interactionState.pushingRight) && !IsNearWall())
            {
                transform.Translate(Input.GetAxisRaw("Horizontal") * 0.005f, 0, 0);
                player.transform.Translate(Input.GetAxisRaw("Horizontal") * 0.005f, 0, 0);
                player.playerAnimation.Push(true);
            }

            if ((player.interactionState.pullingLeft || player.interactionState.pullingRight) && !IsPlayerNearWall())
            {
                transform.Translate(Input.GetAxisRaw("Horizontal") * 0.005f, 0, 0);
                player.transform.Translate(Input.GetAxisRaw("Horizontal") * 0.005f, 0, 0);
                player.playerAnimation.Pull(true);
            }
        }
        // else if (IsNearPlayer())
        // {
        //     rB2D.bodyType = RigidbodyType2D.Kinematic;
        // }
    }


    void IsOnCliff()
    {
        rB2D.drag = IsGrounded() ? 10 : 0;
    }


    void IsInAir()
    {
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


    void UpdateCanMoveBlock()
    {
		if (Input.GetKey(KeyCode.M) && player.GetComponent<Controller2D>().collisions.below && PlayerIsNearBlock() && !player.playerState.defending)
        {
            player.canRun = false;
            player.canMoveBlock = true;
            player.playerAnimation.Move(0);

            if ((!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) || (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow)))
            {
                UpdateBlockGripStatus(false, false, false, false);
                player.playerAnimation.Push(false);
                player.playerAnimation.Pull(false);
            }
            else if (Input.GetKey(KeyCode.RightArrow) && player.interactionState.facingRightNearBlock && !player.playerSpriteRenderer.flipX)
            {
                UpdateBlockGripStatus(true, false, false, false);
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && player.interactionState.facingLeftNearBlock && player.playerSpriteRenderer.flipX)
            {
                UpdateBlockGripStatus(false, true, false, false);
            }
            else if (Input.GetKey(KeyCode.RightArrow) && player.interactionState.facingLeftNearBlock && player.playerSpriteRenderer.flipX)
            {
                UpdateBlockGripStatus(false, false, false, true);
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && player.interactionState.facingRightNearBlock && !player.playerSpriteRenderer.flipX)
            {
                UpdateBlockGripStatus(false, false, true, false);
            }
            else
            {
                UpdateBlockGripStatus(false, false, false, false);
            }
        }
        else
        {
            player.canMoveBlock = false;
            player.interactionState.facingRightNearBlock = false;
            player.interactionState.facingLeftNearBlock = false;
            UpdateBlockGripStatus(false, false, false, false);
            player.playerAnimation.Push(false);
            player.playerAnimation.Pull(false);
        }
    }


    void UpdateBlockGripStatus(bool pushingRight, bool pushingLeft, bool pullingRight, bool pullingLeft)
    {
        player.interactionState.pushingRight = pushingRight;
        player.interactionState.pushingLeft = pushingLeft;
        player.interactionState.pullingRight = pullingLeft;
        player.interactionState.pullingLeft = pullingRight;
    }




    // --------------------------------------------------------------------------------
    // Coroutines
    // --------------------------------------------------------------------------------

    IEnumerator HasLandedRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        rB2D.bodyType = RigidbodyType2D.Static;
        rB2D.velocity = new Vector2(0, 0);
        transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, 0, 0);
    }




    // ----------------------------------------
    // Raycast Status
    // ----------------------------------------

    bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 0.51f, 1 << 8);
        return hitGround.collider != null ? true : false;
    }


    bool IsNearPlayer()
    {
        RaycastHit2D hitPlayerRight = Physics2D.Raycast(transform.position, Vector2.right, 0.55f, 1 << 10);
        RaycastHit2D hitPlayerLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.55f, 1 << 10);
        return (hitPlayerRight.collider != null || hitPlayerLeft.collider != null) ? true : false;
    }


    bool PlayerIsNearBlock()
    {
        RaycastHit2D hitBlockRight = Physics2D.Raycast(player.transform.position, Vector2.right, 0.2f, 1 << 9);
        RaycastHit2D hitBlockLeft = Physics2D.Raycast(player.transform.position, Vector2.left, 0.2f, 1 << 9);
        if (hitBlockRight.collider != null)
        {
            player.interactionState.facingRightNearBlock = true;
            player.interactionState.facingLeftNearBlock = false;
            return true;
        }
        else if (hitBlockLeft.collider != null)
        {
            player.interactionState.facingRightNearBlock = false;
            player.interactionState.facingLeftNearBlock = true;
            return true;
        }
        return false;
    }

    bool IsNearWall()
    {
        RaycastHit2D hitWallRight = Physics2D.Raycast(transform.position, new Vector2(0.85f, 0.4f), 0.55f, 1 << 8);
        RaycastHit2D hitWallLeft = Physics2D.Raycast(transform.position, new Vector2(-0.85f, 0.4f), 0.55f, 1 << 8);
        if (hitWallRight.collider != null || hitWallLeft.collider != null)
        {
            return true;
        }
        return false;
    }

    bool IsPlayerNearWall()
    {
        RaycastHit2D hitWallRight = Physics2D.Raycast(player.transform.position, new Vector2(0.3f, 0.3f), 0.4f, 1 << 8);
        RaycastHit2D hitWallLeft = Physics2D.Raycast(player.transform.position, new Vector2(-0.3f, 0.3f), 0.4f, 1 << 8);
        if (hitWallRight.collider != null || hitWallLeft.collider != null)
        {
            player.interactionState.pullingLeft = false;
            player.interactionState.pullingRight = false;
            return true;
        }
        return false;
    }
}

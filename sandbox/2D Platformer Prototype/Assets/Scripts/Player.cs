using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Controller2D))]
public class Player : MonoBehaviour
{
	// --------------------------------------------------------------------------------
	// Structs
	// --------------------------------------------------------------------------------

	public struct PlayerStates
	{
        public bool interacting;

        public bool walking;
        public bool ducking;
        public bool dashing;

        public bool floating;
		public bool jumping;
		public bool wallJumping;
    }


    public struct InteractionStates
    {
        // Block Interaction (to be used by the Block script)
        public bool facingRightNearBlock;
        public bool facingLeftNearBlock;
        public bool pushingRight;
        public bool pushingLeft;
        public bool pullingRight;
        public bool pullingLeft;
    }




    // --------------------------------------------------------------------------------
    // Fields
    // --------------------------------------------------------------------------------

    // Global Variables
    public PlayerStates playerState;
    public InteractionStates interactionState;
    public PlayerAnimation playerAnimation;
    public Vector2 velocity;

    // Class Variables
    SpriteRenderer playerSprite;
    Animator animator;
    Controller2D controller;
	BoxCollider2D boxCollider;

    Vector2 directionalInput;
    Vector2 wallVelocity;

    Coroutine jumpTimer;
    Coroutine wallJumpTimer;

    float colliderHeight;

	float gravity;
    float maxJumpVelocity;
	float minJumpVelocity;
	float velocityXSmoothing;
	float velocityYSmoothing;
    float moveSpeed = 2;
    float dashSpeed = 30;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;

    float wallFriction = 3;
    float wallStickTime = .25f;
    float timeToWallUnstick;

    int wallDirX;
    int jumpCounter;

    bool wallSliding;

    // Coroutines
    bool hasWalked;

    // Status
    public bool canRun;

    // Block Interaction
    public bool canMoveBlock = false;




    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start()
	{
        playerSprite = GetComponentInChildren<SpriteRenderer>();
        playerAnimation = GetComponent<PlayerAnimation>();

        controller = GetComponent<Controller2D> ();
		boxCollider = GetComponent<BoxCollider2D> ();
		colliderHeight = boxCollider.size.y;

        gravity = -40;
	}


	void Update()
	{
        // Status
        MoveStatus();

        // Basic
        PlayerDirection ();
        Duck();
        Move();

        // Interaction
        CheckWallCollisions();
        CheckVerticalCollisions();
        IsGrounded();
    }




    // --------------------------------------------------------------------------------
    // Input Detection methods called by the PlayerInput layer
    // --------------------------------------------------------------------------------
   
    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }


    public void OnJumpInputDown()
    {
        if (wallSliding)
        {
            wallJumpTimer = StartCoroutine(WallJumpRoutine());
        }
        else if (jumpCounter > 0)
        {
            if (!controller.collisions.slidingDownMaxSlope && !playerState.wallJumping)
            {
                jumpCounter--;
                jumpTimer = StartCoroutine(JumpRoutine());
            }
        }
    }


    public void OnJumpInputUp ()
    {
        if (jumpTimer != null)
        {
            StopCoroutine(jumpTimer);
        }
        playerState.jumping = false;
    }




    // --------------------------------------------------------------------------------
    // Movement Status
    // --------------------------------------------------------------------------------

    void MoveStatus()
    {
        // Player will not move normally if interacting with objects
        playerState.interacting = (interactionState.facingRightNearBlock == true || interactionState.facingLeftNearBlock == true);

        if (controller.collisions.below)
        {
            if (hasWalked == false)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                    StartCoroutine(HasWalkedRoutine());
            }
            if (hasWalked == true)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    canRun = true;
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            canRun = false;
        }
    }




    // --------------------------------------------------------------------------------
    // Basic Movement
    // --------------------------------------------------------------------------------

    void PlayerDirection()
    {
        if (directionalInput.x < 0 && !playerState.interacting)
        {
            playerSprite.flipX = true;
        }
        else if (directionalInput.x > 0 && !playerState.interacting)
        {
            playerSprite.flipX = false;
        }
    }


    public void Duck()
    {
        if (controller.collisions.below && directionalInput.y == -1)
        {
            controller.CalculateRaySpacing();
            if (boxCollider.offset == Vector2.zero)
            {
                boxCollider.offset = boxCollider.offset + Vector2.down * colliderHeight / 4;
            }
            boxCollider.size = new Vector2(boxCollider.size.x, colliderHeight * 0.5f);
            playerState.ducking = true;
            playerAnimation.Duck(playerState.ducking);
        }
        else if (controller.CeilingCheck())
        {
            controller.CalculateRaySpacing();
            if (boxCollider.offset != Vector2.zero)
            {
                boxCollider.offset = Vector2.zero;
            }
            boxCollider.size = new Vector2(boxCollider.size.x, colliderHeight);
        }

        if ((directionalInput.y != -1) || (playerState.jumping) || !controller.collisions.below)
        {
            playerState.ducking = false;
            playerAnimation.Duck(playerState.ducking);
        }
    }


    void Move()
    {
        float targetVelocityX;

        if (!interactionState.pullingRight && !interactionState.pullingLeft)
        {
            if (playerState.ducking == false)
            {
                if (canRun == false)
                {
                    // Walk
                    targetVelocityX = directionalInput.x * moveSpeed;
                }
                else
                {
                    // Run
                    targetVelocityX = directionalInput.x * moveSpeed * 2;
                }
            }
            else
            {
                // Crawl
                canRun = false;
                targetVelocityX = directionalInput.x * moveSpeed * 0.25f;
                playerAnimation.Move(targetVelocityX);
            }

            // The basic forces that act upon the player, based on its state
            Vector2 smoothRef = new Vector2(velocityXSmoothing, velocityYSmoothing);
            playerAnimation.Move(targetVelocityX);

            if (playerState.jumping)
            {
                velocity = Vector2.SmoothDamp(velocity, new Vector2(targetVelocityX, 10), ref smoothRef, Time.deltaTime);
                playerAnimation.Jump(playerState.jumping);
            }
            else if (playerState.wallJumping)
            {
                velocity = Vector2.SmoothDamp(velocity, new Vector2(-wallDirX * 10, 5), ref smoothRef, Time.deltaTime);
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;
                float runVelocity = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
                velocity.x = runVelocity;
            }

            controller.Move(velocity * Time.deltaTime, directionalInput);
        }
	}


    public void Dash()
    {
        dashSpeed = 15 * directionalInput.x;
        playerState.dashing = true;
    }




    // --------------------------------------------------------------------------------
    // Interaction Movement
    // --------------------------------------------------------------------------------

    void CheckWallCollisions()
    {
        if (playerState.wallJumping)
        {
            if (controller.collisions.right && wallDirX == -1)
            {
                playerState.wallJumping = false;
                StopCoroutine(wallJumpTimer);
            }
            else if (controller.collisions.left && wallDirX == 1)
            {
                playerState.wallJumping = false;
                StopCoroutine(wallJumpTimer);
            }
        }
        else
        {
            wallDirX = (controller.collisions.left) ? -1 : (controller.collisions.right) ? 1 : 0;
        }

        wallSliding = false;
        playerAnimation.WallSlide(wallSliding);
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0)
        {
            wallSliding = true;
            playerAnimation.WallSlide(wallSliding);
            if (velocity.y < -wallFriction)
            {
                velocity.y = -wallFriction;
            }

            if (timeToWallUnstick > 0)
            {
                velocityXSmoothing = 0;
                velocity.x = 0;

                if (directionalInput.x == -wallDirX)
                {
                    timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }
            else
            {
                timeToWallUnstick = wallStickTime;
            }
        }
    }


    void CheckVerticalCollisions()
    {
        if (controller.collisions.below)
        {
            if (!playerState.jumping)
            {
                jumpCounter = 2;
            }

            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
            }

            playerAnimation.Jump(playerState.jumping);
            playerState.floating = false;
            playerAnimation.Fall(playerState.floating);
        }
        else
        {
            playerState.floating = true;
            playerAnimation.Fall(playerState.floating);
        }

        if (controller.collisions.above)
        {
            // Stop the jump logic immediately when the player hits a ceiling
            velocity.y = 0;
            if (jumpTimer != null)
            {
                StopCoroutine(jumpTimer);
                playerState.jumping = false;
            }
        }
    }




    // --------------------------------------------------------------------------------
    // Coroutines
    // --------------------------------------------------------------------------------

    IEnumerator HasWalkedRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        hasWalked = true;
        yield return new WaitForSeconds(0.2f);
        hasWalked = false;
    }


    IEnumerator JumpRoutine()
	{
		playerState.jumping = true;
		yield return new WaitForSeconds (0.2f);
		playerState.jumping = false;
	}


	IEnumerator WallJumpRoutine()
	{
		playerState.wallJumping = true;
		yield return new WaitForSeconds (0.25f);
		playerState.wallJumping = false;
	}




    // ----------------------------------------
    // Raycast Status
    // ----------------------------------------

    public bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 0.55f, 1 << 8);
        return hitGround.collider != null ? true : false;
    }
}
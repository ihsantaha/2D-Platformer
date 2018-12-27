using UnityEngine;
using System.Collections;
using System.Reflection;

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
        public bool crawling;
        public bool dashing;

        public bool floating;
		public bool jumping;
		public bool wallJumping;
        public bool climbing;
        public bool hanging;

        public object playerStateRef;

        public void SetBool(string field, bool value)
        {
            // There is no way to make .SetValue work on a reference of a Struct, so we must box it into an object
            playerStateRef = this;
            this.GetType().GetField(field).SetValue(playerStateRef, value);
           this = (PlayerStates)playerStateRef;
        }
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

        public object playerStateRef;

        public void SetBool(string field, bool value)
        {
            // There is no way to make .SetValue work on a reference of a Struct, so we must box it into an object
            playerStateRef = this;
            this.GetType().GetField(field).SetValue(playerStateRef, value);
            playerStateRef = (PlayerStates) playerStateRef;
        }
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
    public SpriteRenderer playerSprite;
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
    float moveSpeed = 4;
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
        CanClimb();
        CanHang();

        // Raycast Status
        IsGrounded();
        IsInCrawlSpace();
        IsInClimbableSpace();
        IsUnderGripCeiling();

        // Animation Speed
        IsNotHangingNorClimbing();
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
        if (!playerState.ducking && !IsInCrawlSpace())
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
                    jumpTimer = StartCoroutine(Timer(0.2f, "jumping"));    
                }
            }
        } else
        {
            StartCoroutine(Timer(0.2f, "dashing"));
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


    public void IsNotHangingNorClimbing()
    {
        if (playerState.climbing && velocity.y == 0)
        {
            playerAnimation.Climb(playerState.climbing, 0);
        }
        if (playerState.hanging && velocity.x == 0)
        {
            playerAnimation.Hang(playerState.hanging, 0);
        }
    }




    // --------------------------------------------------------------------------------
    // Basic Movement
    // --------------------------------------------------------------------------------

    void PlayerDirection()
    {
        if (!playerState.interacting && Input.GetAxisRaw("Horizontal") < 0)
        {
            playerSprite.flipX = true;
        }
        else if (!playerState.interacting && Input.GetAxisRaw("Horizontal") > 0)
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
        else if (controller.CeilingCheck() && !playerState.dashing)
        {
            controller.CalculateRaySpacing();
            if (boxCollider.offset != Vector2.zero)
            {
                boxCollider.offset = Vector2.zero;
            }

            boxCollider.size = new Vector2(boxCollider.size.x, colliderHeight);

            if ((directionalInput.y != -1) || (playerState.jumping) || !controller.collisions.below)
            {
                boxCollider.size = new Vector2(boxCollider.size.x, colliderHeight);
                playerState.ducking = false;
                playerAnimation.Duck(playerState.ducking);
            }
        }
    }


    void Move()
    {
        float targetVelocityX;

        if (!playerState.interacting && !interactionState.pullingRight && !interactionState.pullingLeft)
        {
            if (playerState.ducking == false && !IsInCrawlSpace())
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
                targetVelocityX = directionalInput.x * moveSpeed;
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
            else if (playerState.dashing)
            {
                // Slide
                velocity = Vector2.SmoothDamp(velocity, new Vector2(controller.collisions.faceDir * 10, velocity.y), ref smoothRef, Time.deltaTime);
            }
            else if (playerState.climbing && IsInClimbableSpace())
            {
                Climb();
            }
            else if (playerState.hanging && IsUnderGripCeiling())
            {
                Hang();
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

    public void Climb()
    {
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKey(KeyCode.Space))
        {
            playerState.climbing = false;
            playerState.jumping = true;
        }
        else if (Input.GetKey(KeyCode.UpArrow) && !IsOnTopMostLadder())
        {
            velocity.y = 2;
            velocity.x = 0;
            playerAnimation.Climb(playerState.climbing);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            velocity.y = -2;
            velocity.x = 0;
            playerAnimation.Climb(playerState.climbing);

            if (IsGrounded())
            {
                playerState.climbing = false;
                playerAnimation.Climb(playerState.climbing);
            }
        }
        else
        {
            velocity.y = 0;
            velocity.x = 0;
        }
    }


    public void Hang()
    {
        if (Input.GetKey(KeyCode.DownArrow))
        {

            playerState.hanging = false;
            playerState.jumping = true;
        }
        else  if (Input.GetKey(KeyCode.LeftArrow))
        {
            velocity.x = -1;
            playerAnimation.Hang(playerState.hanging);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            velocity.x = 1;
            playerAnimation.Hang(playerState.hanging);
        }
        else
        {
            velocity.y = 0;
            velocity.x = 0;
        }
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


    public void CanClimb()
    {
        if (IsInClimbableSpace() && Input.GetKeyDown(KeyCode.UpArrow))
        {
            playerState.climbing = true;
        }
    }

    public void CanHang()
    {
        if (IsUnderGripCeiling() && Input.GetKey(KeyCode.UpArrow))
        {
            playerState.hanging = true;
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

    IEnumerator Timer(float delay,string property){
        playerState.SetBool(property, true);

        if (property == "dashing")
        {
            playerAnimation.Slide(true);
        }

        yield return new WaitForSeconds(delay);
        playerState.SetBool(property, false);

        if (property == "dashing")
        {
            playerAnimation.Slide(false);
        }     
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
        RaycastHit2D hitBlock = Physics2D.Raycast(transform.position, Vector2.down, 0.55f, 1 << 9);
        return hitGround.collider!= null || hitBlock.collider != null ? true : false;
    }


    public bool IsInCrawlSpace()
    {
        RaycastHit2D hitCeiling = Physics2D.Raycast(transform.position, Vector2.up, 0.55f, 1 << 8);
        if (hitCeiling.collider != null && IsGrounded())
        {
            playerAnimation.InCrawlSpace(true);
            return true;
        }
        playerAnimation.InCrawlSpace(false);
        return false;
    }


    public bool IsInClimbableSpace()
    {
        RaycastHit2D hitClimbRight = Physics2D.Raycast(transform.position, Vector2.right, 0.1f, 1 << 11);
        RaycastHit2D hitClimbLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.1f, 1 << 11);

        if (hitClimbLeft.collider != null && hitClimbRight.collider != null)
        {
            return true;
        }

        playerState.climbing = false;
        playerAnimation.Climb(playerState.climbing);
        return false;
    }
    

    public bool IsOnTopMostLadder()
    {
        RaycastHit2D hitEndOfLadder = Physics2D.Raycast(transform.position + Vector3.up * 0.5f, Vector2.up, 0.5f, 1 << 11);

        if (hitEndOfLadder.collider == null)
        {
            return true;
        }
        return false;
    }


    public bool IsUnderGripCeiling()
    {
        RaycastHit2D hitGripCeiling = Physics2D.Raycast(transform.position, Vector2.up, 0.55f, 1 << 13);

        if (hitGripCeiling.collider != null)
        {
            return true;
        }

        playerState.hanging = false;
        playerAnimation.Hang(playerState.hanging);
        return false;
    }
}
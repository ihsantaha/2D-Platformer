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
        public LayerMask wallLadder;
        public bool interacting;

        public bool walking;
        public bool ducking;
        public bool crawling;
        public bool dashing;
        public bool defending;
        public bool defendingUpwards;
        public bool defendingDownwards;

        public bool hasWeapon;
        public bool swordDrawn;

        public bool floating;
		public bool jumping;
		public bool wallJumping;
        public bool climbing;
        public bool climbingInProfileView;
        public bool hanging;
        public bool hangingOnCliff;

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
    public Animator playerAnimator;
    public Vector2 velocity;

    // Class Variables
    public Transform playerSpriteTransform;
    public SpriteRenderer playerSpriteRenderer;
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

    float wallSlideSpeed = 2;
    float wallStickDelay = .1f;
	float wallHoldDuration;

    int direction;
    int wallDirX;
    int jumpCounter;

    bool wallSliding;

    // Coroutines
    bool hasWalked;
    bool hasDodged;
    bool hasClimbedUpCliff;

    // Status
    public bool canRun;
    public bool canDodge;

    // Block Interaction
    public bool canMoveBlock = false;

    // For Testing
    public bool test;
    public bool flag;




    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start()
	{
        playerSpriteTransform = GetComponentInChildren<Transform>();
        playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerAnimator = GetComponentInChildren<Animator>();
        playerAnimation = GetComponent<PlayerAnimation>();

        controller = GetComponent<Controller2D> ();
		boxCollider = GetComponent<BoxCollider2D> ();
		colliderHeight = boxCollider.size.y;

        gravity = -40;

        playerState.hasWeapon = true;
	}


	void Update()
	{
        // Status
        MoveStatus();

        // Basic
        PlayerDirection();
        Duck();
        Move();
        Defend();

        // Interaction
        CheckWallCollisions();
        CheckVerticalCollisions();
        CheckWaterCollision();
        CanClimb();
        CanClimbInProfileView();
        CanHang();
        CanHangOnCliff();

        // Raycast Status
        IsNearWall();
        IsInCrawlSpace();
        IsInWater();
        IsInClimbableSpace();
        IsInClimbableSpaceInProfileView();
        IsUnderGripCeiling();
        IsHangingOnCliff();

        // Animation Speed
        IsNotHangingNorClimbing();

        // Animation Transition Support
        CliffWallToCliffSurface();

        // Weapon
        DrawSword();
        SwordAttack1();
    }




    // --------------------------------------------------------------------------------
    // Input Detection methods called by the PlayerInput layer
    // --------------------------------------------------------------------------------
   
    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnAttackInputDown() {

        if (directionalInput.y < 0) {
            jumpCounter = 0;
            velocity.y = -20f;
        }

    }


    public void OnJumpInputDown()
    {
        if (!playerState.ducking && !IsInCrawlSpace())
        {
            if (wallSliding)
            {
                wallJumpTimer = StartCoroutine(Timer(0.25f,"wallJumping"));
            }
            else if (IsInWater() || IsHangingOnCliff())
            {
                jumpCounter = 1;
                jumpTimer = StartCoroutine(Timer(0.2f, "jumping"));
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

    /// <summary>
    /// 
    /// </summary>
    void MoveStatus()
    {
        // Player will not move normally if interacting with objects
        playerState.interacting = (interactionState.facingRightNearBlock == true || interactionState.facingLeftNearBlock == true);

        if (controller.collisions.below)
        {
            //if (hasWalked == false)
            //{
            //    if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            //        StartCoroutine(HasWalkedRoutine());
            //}
            //if (hasWalked == true)
            //{
            //    if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
            //    {
            //        canRun = true;
            //    }
            //}
            if (hasDodged == false && !IsInWater())
            {
                if (Input.GetKeyDown(KeyCode.Z) && directionalInput.x == 0)
                {
                    StartCoroutine(HasDodgedRoutine());
                    StartCoroutine(DodgeCooldownRoutine());
                }
            }
        }

        //if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        //{
        //    canRun = false;
        //}

        // Player will not move if attacking
        if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("SwordAttack1"))
        {
            velocity.x = 0;
        }

    }


    public void IsNotHangingNorClimbing()
    {
        if (playerState.climbing && velocity.y == 0)
        {
            playerAnimation.Climb(playerState.climbing, 0);
        }
        if ((playerState.climbingInProfileView && velocity.y == 0) || (playerState.climbingInProfileView && directionalInput.y == 0 && directionalInput.x != 0))
        {
            playerAnimation.ClimbInProfileView(playerState.climbingInProfileView, 0);
        }
        if (playerState.hanging && velocity.x == 0)
        {
            playerAnimation.Hang(playerState.hanging, 0);
        }
    }


    public void SwimDirection(float direction, Quaternion angle, bool swim)
    {
        
        velocity =  angle * new Vector2(direction, 0);
        playerAnimation.Swim(swim);
    }




    // --------------------------------------------------------------------------------
    // Basic Movement
    // --------------------------------------------------------------------------------

    void PlayerDirection()
    {
		if (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("SwordAttack1") && !playerState.interacting && directionalInput.x != 0) {
		    playerSpriteRenderer.flipX = (directionalInput.x < 0);
		}

        direction = playerSpriteRenderer.flipX ? -1 : 1;
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
        moveSpeed = IsInWater() ? 1 : 2;

        if(directionalInput.x == 0 && !IsInWater())
        {
            velocity.x = 0;
        }

        if (!playerState.interacting && !playerState.defending && !interactionState.pullingRight && !interactionState.pullingLeft)
        {
            if (!playerState.ducking && !IsInCrawlSpace())
            {
                targetVelocityX = directionalInput.x * moveSpeed * 2;
                //if (!canRun)
                //{
                //    if (Input.GetKey(KeyCode.X) && !IsInWater())
                //    {
                //        // Sneak
                //        targetVelocityX = directionalInput.x * moveSpeed * 0.1f;
                //        playerAnimation.Move(targetVelocityX);
                //    } else
                //    {
                //        // Walk
                //        targetVelocityX = directionalInput.x * moveSpeed;
                //    }
                //}
                //else
                //{
                //    // Run
                //    targetVelocityX = directionalInput.x * moveSpeed * 2;
                //}

                //if (canDodge && hasDodged && directionalInput.x == 0)
                //{
                //    Dodge();
                //}
            }
            else
            {
                // Crawl
                canRun = false;
                targetVelocityX = directionalInput.x * moveSpeed * 0.5f;
                playerAnimation.Move(targetVelocityX);
            }

            // The basic forces that act upon the player, based on its state
            Vector2 smoothRef = new Vector2(velocityXSmoothing, velocityYSmoothing);
            playerAnimation.Move(targetVelocityX);

            if (playerState.jumping)
            {
                velocity = Vector2.SmoothDamp(velocity, new Vector2(targetVelocityX, 7.5f), ref smoothRef, Time.deltaTime);
                playerAnimation.Jump(playerState.jumping);
            }
            else if (playerState.wallJumping)
            {
                velocity = Vector2.SmoothDamp(velocity, new Vector2(-wallDirX * 7.5f, 7.5f), ref smoothRef, Time.deltaTime);
            }
            else if (playerState.dashing)
            {
                // Slide
                velocity = Vector2.SmoothDamp(velocity, new Vector2(direction * 7.5f, velocity.y), ref smoothRef, Time.deltaTime);
            }
            else if (playerState.climbing)
            {
                Climb();
            }
            else if (playerState.climbingInProfileView)
            {
                ClimbInProfileView();
            }
            else if (playerState.hanging && IsUnderGripCeiling())
            {
                Hang();
            } else if (playerState.hangingOnCliff)
            {
                HangOnCliff();
            }
            else
            {
                if (!IsInWater())
                {
                    if (!controller.collisions.below && Input.GetKey(KeyCode.G))
                    {
                        // Glide
                        velocity.y = -1;
                    }
                    else
                    {
                        velocity.y += gravity * Time.deltaTime;
                    }
                }
                float runVelocity = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
                velocity.x = runVelocity;
            }

            controller.Move(velocity * Time.deltaTime, directionalInput);
        }
	}


    public void Climb()
    {
      if (directionalInput.y != 0 || directionalInput.x != 0 )
        {
            velocity.y = (IsOnTopMostLadder() && directionalInput.y > 0) ? 0 : 2 * directionalInput.y;
            velocity.x = 2 * directionalInput.x;
            playerAnimation.Climb(playerState.climbing);

            if (controller.collisions.below && directionalInput.y == -1)
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


    public void ClimbInProfileView()
    {
        if (directionalInput.y != 0 || directionalInput.x != 0)
        {
            velocity.y = (IsOnTopMostSideLadder() && directionalInput.y > 0) ? 0 : 2 * directionalInput.y;
            velocity.x = 2 * directionalInput.x;
            playerAnimation.ClimbInProfileView(playerState.climbingInProfileView);

            if (controller.collisions.below && directionalInput.y == -1)
            {
                playerState.climbingInProfileView = false;
                playerAnimation.ClimbInProfileView(playerState.climbingInProfileView);
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
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            velocity.x = directionalInput.x;
            playerAnimation.Hang(playerState.hanging);
        }
        else
        {
            velocity = Vector2.zero;
        }
    }


    public void HangOnCliff()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            playerAnimation.ClimbUpCliff();
            playerState.hangingOnCliff = false;
        } else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            playerState.hangingOnCliff = false;
            playerAnimation.HangOnCliff(playerState.hangingOnCliff);
        } else
        {
            velocity = Vector2.zero;
        }
    }


    public void Defend()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && controller.collisions.below)
        {
            playerState.defending = true;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                playerAnimation.Defend(3);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                playerAnimation.Defend(1);
            } else
            {
                playerAnimation.Defend(2);
            }
        }
        else
        {
            playerState.defending = false;
            playerAnimation.Defend(0);
        }
    }


    public void Dodge()
    {
        float dodgeDirection = playerSpriteRenderer.flipX ? 1 : -1;
        velocity += new Vector2(7.5f * dodgeDirection, 0);
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
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y <=0 && directionalInput.x == wallDirX)
        {
			wallHoldDuration += Time.deltaTime;
			if (wallHoldDuration >= wallStickDelay && !IsHangingOnCliff()) {
				wallSliding = true;
				if (velocity.y < -wallSlideSpeed && directionalInput.y >= 0) {
					velocity.y = -wallSlideSpeed;
				} else {
					velocity.y = -wallSlideSpeed * 2;
				}
			}				
        }
		playerAnimation.WallSlide(wallSliding);
    }


    void CheckVerticalCollisions()
    {
        if (controller.collisions.below)
        {
			wallHoldDuration = 0;
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


    public void CheckWaterCollision()
    {
        if (IsInWater())
        {
            // Play float in water animation if no directional input is detected
            playerAnimation.FloatInWater(velocity == Vector2.zero && Input.GetKey(KeyCode.C));
            if (Input.GetKey(KeyCode.C))
            {
                //lock gravity
                velocity.y = 0;
                int polarity = playerSpriteRenderer.flipX ? -1 : 1;
                int force = directionalInput != Vector2.zero ? 1 : 0;
                if (directionalInput.y > 0){
                    force = !IsOnWaterSurface() ? force : 0;
                }
                float angle = directionalInput.x==0 ? directionalInput.y*90*force*polarity : directionalInput.y * 45 * force*polarity;
                SwimDirection(polarity*force, Quaternion.Euler(0, 0, angle), force!=0);     
            }
            else
            {
                velocity.y = -0.4f;
                SwimDirection(0,Quaternion.Euler(0, 0, 0),false);
                if (!controller.collisions.below)
                {
                    playerAnimation.Fall(true);
                }
            }
        }
        else
        {
            playerAnimation.FloatInWater(false);
            playerAnimation.Swim(false);
        }
    }


    public void CanClimb()
    {
        if (IsInClimbableSpace() && Input.GetKeyDown(KeyCode.UpArrow))
        {
            playerState.climbing = true;
        }
    }


    public void CanClimbInProfileView()
    {
        if (IsInClimbableSpaceInProfileView() && Input.GetKeyDown(KeyCode.UpArrow))
        {
            playerState.climbingInProfileView = true;
        }
    }


    public void CanHang()
    {
        if (IsUnderGripCeiling() && Input.GetKey(KeyCode.UpArrow))
        {
            playerState.hanging = true;
        }
    }


    public void CanHangOnCliff()
    {
        if ((IsHangingOnCliff() && Input.GetKey(KeyCode.UpArrow)) || playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("ClimbUpCliff"))
        {
            playerState.hangingOnCliff = true;
            playerAnimation.HangOnCliff(playerState.hangingOnCliff);
        } else
        {
            playerState.hangingOnCliff = false;
            playerAnimation.HangOnCliff(playerState.hangingOnCliff);
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

        yield return new WaitForSeconds(delay);
        playerState.SetBool(property, false);

    }

    IEnumerator HasDodgedRoutine()
    {
        hasDodged = true;
        yield return new WaitForSeconds(1);
        hasDodged = false;
    }

    // Cooldowns
    IEnumerator DodgeCooldownRoutine()
    {
        canDodge = true;
        playerAnimation.Dodge(canDodge);
        yield return new WaitForSeconds(0.2f);
        canDodge = false;
        playerAnimation.Dodge(canDodge);
    }




    // ----------------------------------------
    // Raycast Status
    // ----------------------------------------


    public bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 0.55f, 1 << 8);
        RaycastHit2D hitBlock = Physics2D.Raycast(transform.position, Vector2.down, 0.55f, 1 << 9);
        return hitGround.collider != null || hitBlock.collider != null ? true : false;
    }



    public bool IsNearWall()
    {
        RaycastHit2D hitWallRight = Physics2D.Raycast(transform.position, Vector2.right, 0.55f, 1 << 8);
        RaycastHit2D hitWallLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.55f, 1 << 8);
        return hitWallRight.collider != null || hitWallLeft.collider != null ? true : false;
    }


    public bool IsInCrawlSpace()
    {
        RaycastHit2D hitCrawlSpace = Physics2D.Raycast(transform.position, Vector2.up, 0.55f, 1 << 12);
        bool isInCrawlSpace = hitCrawlSpace && controller.collisions.below;

        return isInCrawlSpace;
    }


    public bool IsInClimbableSpace()
    {
        RaycastHit2D hitClimbRight = Physics2D.Raycast(transform.position, Vector2.right, 0.1f, 1 << 11);
        RaycastHit2D hitClimbLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.1f, 1 << 11);

        playerState.climbing = false;
        playerAnimation.Climb(playerState.climbing);
        return false;
    }


    public bool IsInClimbableSpaceInProfileView()
    {
        RaycastHit2D hitClimbInProfileViewRight = Physics2D.Raycast(transform.position, Vector2.right, 0.1f, 1 << 15);
        RaycastHit2D hitClimbInProfileViewLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.1f, 1 << 15);



        playerState.climbingInProfileView = false;
        playerAnimation.ClimbInProfileView(playerState.climbingInProfileView);
        return false;
    }


    public bool IsOnTopMostLadder()
    {
        RaycastHit2D hitEndOfLadder = Physics2D.Raycast(transform.position + Vector3.up * 0.5f, Vector2.up, 0.5f, 1 << 11);
        return hitEndOfLadder.collider == null ? true : false;
    }


    public bool IsOnTopMostSideLadder()
    {
        RaycastHit2D hitEndOfRightSideLadder = Physics2D.Raycast(transform.position + Vector3.up * 0.5f, Vector2.right, 0.25f, 1 << 15);
        RaycastHit2D hitEndOfLeftSideLadder = Physics2D.Raycast(transform.position + Vector3.up * 0.5f, Vector2.left, 0.25f, 1 << 15);
        return (hitEndOfRightSideLadder.collider == null && hitEndOfLeftSideLadder.collider == null) ? true : false;
    }


    public bool IsUnderGripCeiling()
    {
        RaycastHit2D hitGripCeiling = Physics2D.Raycast(transform.position, Vector2.up, 0.51f, 1 << 13);

        if (hitGripCeiling.collider != null)
        {
            return true;
        }

        playerState.hanging = false;
        playerAnimation.Hang(playerState.hanging);
        return false;
    }


    public bool IsInWater()
    {
        RaycastHit2D hitWater = Physics2D.Raycast(transform.position + Vector3.up * 0.9f, Vector2.down, 0.55f, 1 << 14);

        if (hitWater.collider != null)
        {
            playerAnimation.InWater(true);
            return true;
        }
        playerAnimation.InWater(false);
        return false;
    }


    public bool IsOnWaterSurface()
    {
        RaycastHit2D hitSurface = Physics2D.Raycast(transform.position + Vector3.up * 0.7f, Vector2.up, 0.55f, 1 << 14);
        return hitSurface.collider == null ? true : false;
    }


    public bool IsHangingOnCliff()
    {
        RaycastHit2D hitCliff = Physics2D.Raycast(transform.position + new Vector3(direction * 0.2f, 0.325f, 0), Vector2.down, 0.1f, 1 << 8);
        return (hitCliff.collider != null && IsNearWall()) ? true : false;
    }




    // --------------------------------------------------------------------------------
    // Animation Support
    // --------------------------------------------------------------------------------

    public void CliffWallToCliffSurface() {
        if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("ClimbUpCliff"))
        {
            flag = true;
            hasClimbedUpCliff = true;
        }
        else if (hasClimbedUpCliff == true && flag == true)
        {
            transform.Translate(new Vector3(direction * 0.5f, 1, transform.position.z));
            flag = false;
        }
    }




    // --------------------------------------------------------------------------------
    // Weapon
    // --------------------------------------------------------------------------------

    void DrawSword()
    {
        if (Input.GetKeyDown(KeyCode.F) && playerState.hasWeapon)
        {
            if (!playerState.swordDrawn)
            {
                playerAnimation.drawSword();
                playerState.swordDrawn = true;
            }
            else
            {
                playerAnimation.returnSword();
                playerState.swordDrawn = false;
            }
        }
    }

    void SwordAttack1()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && playerState.hasWeapon)
        {
            playerAnimation.SwordAttack1();
        }
    }
}

﻿using UnityEngine;
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
        public bool defending;
        public bool defendingUpwards;
        public bool defendingDownwards;

        public bool floating;
		public bool jumping;
		public bool wallJumping;
        public bool climbing;
        public bool climbingInProfileView;
        public bool hanging;
        public bool hangingOnCliff;

        public bool castingMagic;

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
	float velocityYSmoothing = 0;
    float moveSpeed = 4;
    // float dashSpeed = 30;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;

    float wallSlideSpeed = 2;
    float wallStickDelay = .1f;
	float wallHoldDuration;

    public int direction;
    int wallDirX;
    int jumpCounter;

    bool wallSliding;

    // Coroutines
    bool hasWalked;
    bool hasDodged;
    bool hasClimbedUpCliff;
    bool hasInitiatedCombo1Availability;
    bool hasInitiatedCombo2Availability;
    bool hasInitiatedCombo3Availability;

    // Status
    public bool canRun;
    public bool canDodge;
    public bool canDrawWeapon;
    public bool canCastMagic;
    public bool magicCooldownComplete;
    public string currentMagicType;
    // public bool swordDrawn;

    // Block Interaction
    public bool canMoveBlock = false;

    // For Testing
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

        canDrawWeapon = true;
        canCastMagic = true;
        magicCooldownComplete = true;
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
        IsGrounded();
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
        // DrawSword();
        SwordAttack();

        // Magic
        // ToggleMagic();
        MagicAttack();
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
            if (hasDodged == false && !IsInWater())
            {
                if (Input.GetKeyDown(KeyCode.Z) && directionalInput.x == 0)
                {
                    StartCoroutine(HasDodgedRoutine());
                    StartCoroutine(DodgeCooldownRoutine());
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            canRun = false;
        }

        // Player will not move if attacking
        if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("SwordAttack1") || 
            playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("DuckSwordAttack") ||
            playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("EarthAttack1"))
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
        velocity.x = direction;
        playerSpriteTransform.rotation = angle;
        playerAnimation.Swim(swim);
    }




    // --------------------------------------------------------------------------------
    // Basic Movement
    // --------------------------------------------------------------------------------

    void PlayerDirection()
    {
		if (!playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("SwordAttack1") && 
            !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("DuckSwordAttack") &&
            !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("JumpSwordAttack") &&
            !playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("EarthAttack1") &&
            !playerState.interacting && directionalInput.x != 0) {
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
                if (!canRun)
                {
                    if (Input.GetKey(KeyCode.X) && !IsInWater())
                    {
                        // Sneak
                        targetVelocityX = directionalInput.x * moveSpeed * 0.1f;
                        playerAnimation.Move(targetVelocityX);
                    } else
                    {
                        // Walk
                        targetVelocityX = directionalInput.x * moveSpeed;
                    }
                }
                else
                {
                    // Run
                    targetVelocityX = directionalInput.x * moveSpeed * 2;
                }

                if (canDodge && hasDodged && directionalInput.x == 0)
                {
                    Dodge();
                }
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
                    if (!IsGrounded() && Input.GetKey(KeyCode.G))
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

            if (IsGrounded() && directionalInput.y == -1)
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

            if (IsGrounded() && directionalInput.y == -1)
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
        if (Input.GetKey(KeyCode.LeftAlt) && IsGrounded())
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
            if (Input.GetKey(KeyCode.C))
            {
                velocity.y = 0;

                // Play float in water animation if no directional input is detected
                if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow) &&
                    !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
                {
                    playerAnimation.FloatInWater(true);
                } else
                {
                    playerAnimation.FloatInWater(false);
                }

                if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
                {
                    int angle;

                    // Swim in the up left or up right direction and up only if a wall is deteced to not go through it
                    if (Input.GetKey(KeyCode.UpArrow) && !IsOnWaterSurface())
                    {
                        if (playerSpriteRenderer.flipX)
                        {
                            angle = IsNearWall() ? -90 : -45;
                            SwimDirection(-1, Quaternion.Euler(0, 0, angle), true);
                        }
                        else
                        {
                            angle = IsNearWall() ? 90 : 45;
                            SwimDirection(1, Quaternion.Euler(0, 0, angle), true);
                        }
                    }
                    // Swim in the down left or down right direction and down only if a wall is deteced to not go through it
                    else if (Input.GetKey(KeyCode.DownArrow) && !IsGrounded())
                    {
                        if (playerSpriteRenderer.flipX)
                        {
                            angle = IsNearWall() ?  90 : 45;
                            SwimDirection(-1, Quaternion.Euler(0, 0, angle), true);
                        }
                        else
                        {
                            angle = IsNearWall() ?  -90 : -45;
                            SwimDirection(1, Quaternion.Euler(0, 0, angle), true);
                        }
                    }
                    else
                    {
                        SwimDirection(directionalInput.x, Quaternion.Euler(0, 0, 0), true);
                    }
                }
                else if (Input.GetKey(KeyCode.UpArrow) && !IsOnWaterSurface())
                {
                    if (playerSpriteRenderer.flipX)
                    {
                        SwimDirection(-1, Quaternion.Euler(0, 0, -90), true);
                    }
                    else
                    {
                        SwimDirection(1, Quaternion.Euler(0, 0, 90), true);
                    }
                }
                else if (Input.GetKey(KeyCode.DownArrow) && !IsGrounded())
                {
                    if (playerSpriteRenderer.flipX)
                    {
                        SwimDirection(-1, Quaternion.Euler(0, 0, 90), true);
                    }
                    else
                    {
                        SwimDirection(1, Quaternion.Euler(0, 0, -90), true);
                    }
                }
                else
                {
                    SwimDirection(0, Quaternion.Euler(0, 0, 0), false);
                }
            }
            else
            {
                velocity.y = -0.4f;
                playerSpriteTransform.rotation = Quaternion.Euler(0, 0, 0);
                playerAnimation.FloatInWater(false);
                playerAnimation.Swim(false);

                if (!IsGrounded())
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

    IEnumerator HasDodgedRoutine()
    {
        hasDodged = true;
        yield return new WaitForSeconds(1);
        hasDodged = false;
    }

    IEnumerator HasInitiatedCombo1Availability()
    {
        hasInitiatedCombo1Availability = true;
        yield return new WaitForSeconds(0.5f);
        hasInitiatedCombo1Availability = false;
    }

    IEnumerator HasInitiatedCombo2Availability()
    {
        hasInitiatedCombo2Availability = true;
        yield return new WaitForSeconds(0.5f);
        hasInitiatedCombo2Availability = false;
        playerAnimation.Combo2(hasInitiatedCombo2Availability);
    }

    IEnumerator HasInitiatedCombo3Availability()
    {
        hasInitiatedCombo3Availability = true;
        yield return new WaitForSeconds(0.5f);
        hasInitiatedCombo3Availability = false;
        playerAnimation.Combo3(hasInitiatedCombo3Availability);
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

    IEnumerator MagicCooldownRoutine()
    {
        magicCooldownComplete = false;
        yield return new WaitForSeconds(2);
        magicCooldownComplete = true;
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
        if (hitCrawlSpace.collider != null && IsGrounded())
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


    public bool IsInClimbableSpaceInProfileView()
    {
        RaycastHit2D hitClimbInProfileViewRight = Physics2D.Raycast(transform.position, Vector2.right, 0.1f, 1 << 15);
        RaycastHit2D hitClimbInProfileViewLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.1f, 1 << 15);

        if (hitClimbInProfileViewLeft.collider != null || hitClimbInProfileViewRight.collider != null)
        {
            return true;
        }

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

    // void DrawSword()
    // {
    //    if (Input.GetKeyDown(KeyCode.F) && playerState.hasWeapon)
    //    {
    //        if (!playerState.swordDrawn)
    //        {
    //            playerAnimation.drawSword();
    //            playerState.swordDrawn = true;
    //        }
    //        else
    //        {
    //            playerAnimation.returnSword();
    //            playerState.swordDrawn = false;
    //        }
    //    }
    // }

    void SwordAttack()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (playerState.ducking)
            {
                playerAnimation.SwordAttack("DuckSwordAttack");
            }
            else if (!IsGrounded())
            {
                playerAnimation.SwordAttack("JumpSwordAttack");
            }
            else
            { 
                if (!hasInitiatedCombo1Availability && !hasInitiatedCombo2Availability && !hasInitiatedCombo3Availability)
                {
                    StartCoroutine(HasInitiatedCombo1Availability());
                    playerAnimation.SwordAttack("SwordAttack1");
                } else if (!hasInitiatedCombo2Availability && !hasInitiatedCombo3Availability)
                {
                    StartCoroutine(HasInitiatedCombo2Availability());
                    playerAnimation.Combo2(hasInitiatedCombo2Availability);
                } else if (!hasInitiatedCombo3Availability)
                {
                    StartCoroutine(HasInitiatedCombo3Availability());
                    playerAnimation.Combo3(hasInitiatedCombo3Availability);
                }
            }
        }
    }




    // --------------------------------------------------------------------------------
    // Magic
    // --------------------------------------------------------------------------------

    void MagicAttack()
    {
        if (canCastMagic && magicCooldownComplete)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                currentMagicType = "FireballAttack1";
                playerState.castingMagic = true;
                StartCoroutine(MagicCooldownRoutine());
            } else if (Input.GetKeyDown(KeyCode.E))
            {
                currentMagicType = "QuakeAttack1";
                playerState.castingMagic = true;
                StartCoroutine(MagicCooldownRoutine());
            }
        }
    }

}

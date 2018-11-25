using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Controller2D))]
public class Player : MonoBehaviour
{
	// --------------------------------------------------------------------------------
	// Struct
	// --------------------------------------------------------------------------------

	public struct PlayerStates
	{
		public bool floating;
		public bool jumping;
		public bool wallJumping;
		public bool dashing;
	}



    // --------------------------------------------------------------------------------
    // Properties
    // --------------------------------------------------------------------------------

    public PlayerStates PlayerState;

    // --------------------------------------------------------------------------------
    // Class Variables
    // --------------------------------------------------------------------------------
    Animator animator;
    Controller2D controller;
	BoxCollider2D boxCollider;

    Vector2 directionalInput;
    Vector2 wallVelocity;
    Vector2 velocity;

    Coroutine jumpTimer;
    Coroutine wallJumpTimer;

    float colliderHeight;

	float gravity;
    float maxJumpVelocity;
	float minJumpVelocity;
	float velocityXSmoothing;
	float velocityYSmoothing;
    float moveSpeed = 6;
    float dashSpeed = 30;
    float accelerationTimeAirborne = .2f;
    float accelerationTimeGrounded = .1f;

    float wallFriction = 3;
    float wallStickTime = .25f;
    float timeToWallUnstick;

    int wallDirX;
    int jumpCounter;

    bool wallSliding;








	// --------------------------------------------------------------------------------
	// Methods
	// --------------------------------------------------------------------------------

	void Start ()
	{
		controller = GetComponent<Controller2D> ();
		boxCollider = GetComponent<BoxCollider2D> ();
		colliderHeight = boxCollider.size.y;

		gravity = -50;
	}

	// -------------------------------------------------------
	// Input Detection methods called by the PlayerInput layer
	// -------------------------------------------------------
	public void SetDirectionalInput (Vector2 input)
	{
		directionalInput = input;
	}

	public void OnJumpInputDown ()
	{
		if (wallSliding) {
			wallJumpTimer = StartCoroutine (WallJumpRoutine ());
		} else if (jumpCounter > 0) {
			if (!controller.Collisions.slidingDownMaxSlope && !PlayerState.wallJumping) {
				jumpCounter--;
				jumpTimer = StartCoroutine (JumpRoutine ());
			}
		}
	}

	public void OnJumpInputUp ()
	{
		if (jumpTimer != null) {
			StopCoroutine (jumpTimer);
		}
		PlayerState.jumping = false;
	}
	// -------------------------------------------------------


	void Update ()
	{
		CheckWallCollisions ();
		CheckVerticalCollisions ();
		Move ();
		Duck ();
	}

	void CheckWallCollisions ()
	{
		if (PlayerState.wallJumping) {
			if (controller.Collisions.right && wallDirX == -1) {
				PlayerState.wallJumping = false;
				StopCoroutine (wallJumpTimer);
			} else if (controller.Collisions.left && wallDirX == 1) {
				PlayerState.wallJumping = false;
				StopCoroutine (wallJumpTimer);
			}
		} else {
			wallDirX = (controller.Collisions.left) ? -1 : (controller.Collisions.right) ? 1 : 0;
		}

		wallSliding = false;
		if ((controller.Collisions.left || controller.Collisions.right) && !controller.Collisions.below && velocity.y < 0) {
			wallSliding = true;
			if (velocity.y < -wallFriction) {
				velocity.y = -wallFriction;
			}

			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (directionalInput.x == -wallDirX) {
					timeToWallUnstick -= Time.deltaTime;
				} else {
					timeToWallUnstick = wallStickTime;
				}
			} else {
				timeToWallUnstick = wallStickTime;
			}
		}
	}

	void CheckVerticalCollisions ()
	{
		if (controller.Collisions.below) {
			if (!PlayerState.jumping) {
				jumpCounter = 2;
			}

			if (controller.Collisions.slidingDownMaxSlope) {

				velocity.y += controller.Collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else {
				velocity.y = 0;
			}
		}

		if (controller.Collisions.above) {
			// Stop the jump logic immediately when the player hits a ceiling
			velocity.y = 0;
			if (jumpTimer != null) {
				StopCoroutine (jumpTimer);
				PlayerState.jumping = false;
			}
		}
	}

	void Move ()
	{
		// The basic forces that act upon the player, based on its state 
		float targetVelocityX = directionalInput.x * moveSpeed;
		Vector2 smoothRef = new Vector2 (velocityXSmoothing, velocityYSmoothing);

		if (PlayerState.jumping) {
			velocity = Vector2.SmoothDamp (velocity, new Vector2 (targetVelocityX, 10), ref smoothRef, Time.deltaTime);
		} else if (PlayerState.wallJumping) {
			velocity = Vector2.SmoothDamp (velocity, new Vector2 (-wallDirX * 10, 5), ref smoothRef, Time.deltaTime);
		} else {
			velocity.y += gravity * Time.deltaTime;
			float runVelocity = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.Collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
			velocity.x = runVelocity;
		}

		controller.Move (velocity * Time.deltaTime, directionalInput);
	}

	public void Dash ()
	{
		dashSpeed = 15 * directionalInput.x;
		PlayerState.dashing = true;
	}

	public void Duck ()
	{
	
		if (directionalInput.y == -1) {
			controller.CalculateRaySpacing ();
			if (boxCollider.offset == Vector2.zero) {

                boxCollider.offset = boxCollider.offset + Vector2.down * colliderHeight / 4;
			}
            boxCollider.size = new Vector2 (boxCollider.size.x, colliderHeight * 0.5f);

		} else if (controller.CeilingCheck ()){
			controller.CalculateRaySpacing ();
			if (boxCollider.offset != Vector2.zero) {

                boxCollider.offset = Vector2.zero;
			}

            boxCollider.size = new Vector2 (boxCollider.size.x, colliderHeight);

			
		}

	
	}



	// --------------------------------------------------------------------------------
	// Coroutines
	// --------------------------------------------------------------------------------

	IEnumerator JumpRoutine ()
	{
		PlayerState.jumping = true;
		yield return new WaitForSeconds (0.2f);
		PlayerState.jumping = false;
	}

	IEnumerator WallJumpRoutine ()
	{
		PlayerState.wallJumping = true;
		yield return new WaitForSeconds (0.25f);
		PlayerState.wallJumping = false;
	}
}
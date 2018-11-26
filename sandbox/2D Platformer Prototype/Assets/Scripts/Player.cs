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
    // Fields
    // --------------------------------------------------------------------------------

    public PlayerStates playerState;

    // Class Variables
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
			if (!controller.collisions.slidingDownMaxSlope && !playerState.wallJumping) {
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
		playerState.jumping = false;
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
		if (playerState.wallJumping) {
			if (controller.collisions.right && wallDirX == -1) {
				playerState.wallJumping = false;
				StopCoroutine (wallJumpTimer);
			} else if (controller.collisions.left && wallDirX == 1) {
				playerState.wallJumping = false;
				StopCoroutine (wallJumpTimer);
			}
		} else {
			wallDirX = (controller.collisions.left) ? -1 : (controller.collisions.right) ? 1 : 0;
		}

		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
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
		if (controller.collisions.below) {
			if (!playerState.jumping) {
				jumpCounter = 2;
			}

			if (controller.collisions.slidingDownMaxSlope) {

				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else {
				velocity.y = 0;
			}
		}

		if (controller.collisions.above) {
			// Stop the jump logic immediately when the player hits a ceiling
			velocity.y = 0;
			if (jumpTimer != null) {
				StopCoroutine (jumpTimer);
				playerState.jumping = false;
			}
		}
	}

	void Move ()
	{
		// The basic forces that act upon the player, based on its state 
		float targetVelocityX = directionalInput.x * moveSpeed;
		Vector2 smoothRef = new Vector2 (velocityXSmoothing, velocityYSmoothing);

		if (playerState.jumping) {
			velocity = Vector2.SmoothDamp (velocity, new Vector2 (targetVelocityX, 10), ref smoothRef, Time.deltaTime);
		} else if (playerState.wallJumping) {
			velocity = Vector2.SmoothDamp (velocity, new Vector2 (-wallDirX * 10, 5), ref smoothRef, Time.deltaTime);
		} else {
			velocity.y += gravity * Time.deltaTime;
			float runVelocity = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
			velocity.x = runVelocity;
		}

		controller.Move (velocity * Time.deltaTime, directionalInput);
	}

	public void Dash ()
	{
		dashSpeed = 15 * directionalInput.x;
		playerState.dashing = true;
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
		playerState.jumping = true;
		yield return new WaitForSeconds (0.2f);
		playerState.jumping = false;
	}

	IEnumerator WallJumpRoutine ()
	{
		playerState.wallJumping = true;
		yield return new WaitForSeconds (0.25f);
		playerState.wallJumping = false;
	}
}
using UnityEngine;
using System.Collections;

public class CircleController2D : SphereRaycastController2D
{
	// --------------------------------------------------------------------------------
	// Struct
	// --------------------------------------------------------------------------------

	public struct CollisionInfo
	{
		public bool above;
		public bool below;
		public bool left;
		public bool right;

		public bool ascendingSlope;
		public bool descendingSlope;
		public bool slidingDownMaxSlope;
 
		public bool fallingThroughPlatform;

		public float slopeAngle;
		public float slopeAngleOld;

		public Vector2 slopeNormal;
		public Vector2 moveAmountOld;

		public int faceDir;


		public void Reset()
		{
			above = false;
			below = false;
			left = false;
			right = false;

			ascendingSlope = false;
			descendingSlope = false;
			slidingDownMaxSlope = false;

			slopeNormal = Vector2.zero;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}




	// --------------------------------------------------------------------------------
	// Fields
	// --------------------------------------------------------------------------------

	public CollisionInfo collisions;
	public Vector2 playerInput;
    public float maxSlopeAngle = 80;

    string current = "";




    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    public override void Start()
	{
		base.Start();
		collisions.faceDir = 1;
	}


	public bool CeilingCheck()
	{
		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up, 0.5f, collisionMask);
			if (hit) {
				return false;
			}
		}
		return true;
	}


	public void Move(Vector2 moveAmount, bool standingOnPlatform)
	{
		Move(moveAmount, Vector2.zero, standingOnPlatform);
	}


	public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false)
	{
		// Constantly refresh collision information to keep "move amount" agnostic of forces
		UpdateRaycastOrigins();
		collisions.Reset();

		collisions.moveAmountOld = moveAmount;
		playerInput = input;

		// Short-circuit the DescendSlope method if we are not going downward
		if (moveAmount.y < 0) {
			DescendSlope(ref moveAmount);
		}

		// Raycast in the direction of the last movement
		if (moveAmount.x != 0) {
			collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
		}

		// Affect the moveAmount according to the collisions
		Horizontalcollisions(ref moveAmount);
		Verticalcollisions(ref moveAmount);

		// Move amount according to all of the calculated "physics" above
		transform.Translate(moveAmount);

		if (standingOnPlatform) {
			collisions.below = true;
		}
	}


	void Horizontalcollisions(ref Vector2 moveAmount)
	{
		float directionX = collisions.faceDir;
		float rayLength = Mathf.Abs(moveAmount.x) + padding;

		for (int i = 0; i < horizontalRayCount; i++) {
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * (horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				// Use the bottom most horizontal raycast to calculate slope angles
				if (i == 0 && slopeAngle <= maxSlopeAngle) {
					if (collisions.descendingSlope) {
						// If there is land in front of the player, he is not descending a slope anymore
						collisions.descendingSlope = false;
						moveAmount = collisions.moveAmountOld;
					}
					AscendSlope(ref moveAmount, slopeAngle, hit.normal);
				}

				// If the hit is not a climbable slope, the player is approaching a wall. So stop the move force, and report a collision
				if (!collisions.ascendingSlope || slopeAngle > maxSlopeAngle) {
					moveAmount.x = (hit.distance - padding) * directionX;
					rayLength = hit.distance;
					collisions.left = directionX == -1;
					collisions.right = directionX == 1;
				}
			}
		}
	}


	void Verticalcollisions(ref Vector2 moveAmount)
	{
		float directionY = Mathf.Sign(moveAmount.y);
		float rayLength = Mathf.Abs(moveAmount.y) + padding;

		for (int i = 0; i < verticalRayCount; i++) {
			Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast (rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			if (hit) {
				if (hit.collider.tag == "Through") {
					if (directionY == 1 || hit.distance == 0) {
						continue;
					}
					if (collisions.fallingThroughPlatform) {
						continue;
					}
					if (playerInput.y == -1) {
						collisions.fallingThroughPlatform = true;
						Invoke ("ResetFallingThroughPlatform", .5f);
						continue;
					}
				}

				// Overrides Y velocity when close to the floor, preventing player from going through the floor
				moveAmount.y = (hit.distance - padding) * directionY;
				rayLength = hit.distance;

				if (collisions.ascendingSlope) {
					moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
				}

				collisions.below = directionY == -1;
				collisions.above = directionY == 1;
			}
		}
	}


	void AscendSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
	{
		float moveDistance = Mathf.Abs(moveAmount.x);
		float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

		// Check jump status and transfer the force to the angle of the slope accordingly if not jumping
		if (moveAmount.y <= climbmoveAmountY) {
			moveAmount.y = climbmoveAmountY;
			moveAmount.x *= Mathf.Cos (slopeAngle * Mathf.Deg2Rad);
			collisions.below = true;
			collisions.ascendingSlope = true;
			collisions.slopeAngle = slopeAngle;
			collisions.slopeNormal = slopeNormal;
		}
	}


	void DescendSlope(ref Vector2 moveAmount)
	{
		RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + padding, collisionMask);
		RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + padding, collisionMask);
		if (maxSlopeHitLeft ^ maxSlopeHitRight) {
			SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
			SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
		}

		if (!collisions.slidingDownMaxSlope) {
			float directionX = Mathf.Sign(moveAmount.x);
			Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
				if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle) {
					if (Mathf.Sign(hit.normal.x) == directionX) {
						if (hit.distance - padding <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x)) {
							float moveDistance = Mathf.Abs(moveAmount.x);
							float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
							moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
							moveAmount.y -= descendmoveAmountY;

							collisions.slopeAngle = slopeAngle;
							collisions.descendingSlope = true;
							collisions.below = true;
							collisions.slopeNormal = hit.normal;
						}
					}
				}
			}
		}
	}


	void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
	{
		if (hit) {
			float slopeAngle = Vector2.Angle (hit.normal, Vector2.up);
			if (slopeAngle > maxSlopeAngle) {
				moveAmount.x = Mathf.Sign (hit.normal.x) * (Mathf.Abs (moveAmount.y) - hit.distance) / Mathf.Tan (slopeAngle * Mathf.Deg2Rad);

				collisions.slopeAngle = slopeAngle;
				collisions.slidingDownMaxSlope = true;
				collisions.slopeNormal = hit.normal;
			}
		}
	}


	void ResetFallingThroughPlatform()
	{
		collisions.fallingThroughPlatform = false;
	}
}

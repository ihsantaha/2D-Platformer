using UnityEngine;
using System.Collections;

public class Controller2D : RaycastController2D
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
    // Properties
    // --------------------------------------------------------------------------------

    public float _maxSlopeAngle = 80;

    public CollisionInfo _collisions;
    public Vector2 _playerInput;



    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    public override void Start()
    {
        base.Start();
        _collisions.faceDir = 1;

    }

    public void Move(Vector2 moveAmount, bool standingOnPlatform)
    {
        Move(moveAmount, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false)
    {
        // Constantly refresh collision information to keep "move amount" agnostic of forces
        UpdateRaycastOrigins();
        _collisions.Reset();

        _collisions.moveAmountOld = moveAmount;
        _playerInput = input;

        // Short-circuit the DescendSlope method if we are not going downward
        if (moveAmount.y < 0)
        {
            DescendSlope(ref moveAmount);
        }

        // Raycast in the direction of the last movement
        if (moveAmount.x != 0)
        {
            _collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
        }

        // Affect the moveAmount according to the collisions
        HorizontalCollisions(ref moveAmount);
        VerticalCollisions(ref moveAmount);

        // Move amount according to all of the calculated "physics" above
        transform.Translate(moveAmount);

        if (standingOnPlatform)
        {
            _collisions.below = true;
        }
    }

    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = _collisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + _padding;

        for (int i = 0; i < _horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, _collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                // Use the bottom most horizontal raycast to calculate slope angles
                if (i == 0 && slopeAngle <= _maxSlopeAngle)
                {
                    if (_collisions.descendingSlope)
                    {
                        // If there is land in front of the player, he is not descending a slope anymore
                        _collisions.descendingSlope = false;
                        moveAmount = _collisions.moveAmountOld;
                    }
                    AscendSlope(ref moveAmount, slopeAngle, hit.normal);
                }

                // If the hit is not a climbable slope, the player is approaching a wall. So stop the move force, and report a collision
                if (!_collisions.ascendingSlope || slopeAngle > _maxSlopeAngle)
                {
                    moveAmount.x = (hit.distance - _padding) * directionX;
                    rayLength = hit.distance;
                    _collisions.left = directionX == -1;
                    _collisions.right = directionX == 1;
                }
            }
        }
    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + _padding;

        for (int i = 0; i < _verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? _raycastOrigins.bottomLeft : _raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (_verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, _collisionMask);

            if (hit)
            {
                if (hit.collider.tag == "Through")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    if (_collisions.fallingThroughPlatform)
                    {
                        continue;
                    }
                    if (_playerInput.y == -1)
                    {
                        _collisions.fallingThroughPlatform = true;
                        Invoke("ResetFallingThroughPlatform", .5f);
                        continue;
                    }
                }

                // Overrides Y velocity when close to the floor, preventing player from going through the floor
                moveAmount.y = (hit.distance - _padding) * directionY;
                rayLength = hit.distance;

                if (_collisions.ascendingSlope)
                {
                    moveAmount.x = moveAmount.y / Mathf.Tan(_collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);
                }

                _collisions.below = directionY == -1;
                _collisions.above = directionY == 1;
            }
        }
    }

    void AscendSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        // Check jump status and transfer the force to the angle of the slope accordingly if not jumping
        if (moveAmount.y <= climbmoveAmountY)
        {
            moveAmount.y = climbmoveAmountY;
            moveAmount.x *= Mathf.Cos(slopeAngle * Mathf.Deg2Rad);
            _collisions.below = true;
            _collisions.ascendingSlope = true;
            _collisions.slopeAngle = slopeAngle;
            _collisions.slopeNormal = slopeNormal;
        }
    }

    void DescendSlope(ref Vector2 moveAmount)
    {
        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(_raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + _padding, _collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(_raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + _padding, _collisionMask);
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
            SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
        }

        if (!_collisions.slidingDownMaxSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.bottomRight : _raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, _collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= _maxSlopeAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - _padding <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendmoveAmountY;

                            _collisions.slopeAngle = slopeAngle;
                            _collisions.descendingSlope = true;
                            _collisions.below = true;
                            _collisions.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > _maxSlopeAngle)
            {
                moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);

                _collisions.slopeAngle = slopeAngle;
                _collisions.slidingDownMaxSlope = true;
                _collisions.slopeNormal = hit.normal;
            }
        }
    }

    void ResetFallingThroughPlatform()
    {
        _collisions.fallingThroughPlatform = false;
    }
}

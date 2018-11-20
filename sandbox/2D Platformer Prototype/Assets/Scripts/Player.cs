using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
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

    public Controller2D _controller;
    public PlayerStates _playerState;
    Animator _animator;

    float _accelerationTimeAirborne = .2f;
    float _accelerationTimeGrounded = .1f;
    float _moveSpeed = 6;
    float _dashSpeed = 30;

    public float _wallFriction = 3;
    public float _wallStickTime = .25f;
    float _timeToWallUnstick;

    public float _gravity;
    float _maxJumpVelocity;
    float _minJumpVelocity;
    float _velocityXSmoothing;
    float _velocityYSmoothing;
    Vector3 _velocity;
    Vector2 _wallVelocity;

    Vector2 _directionalInput;
    bool _wallSliding;
    public int _wallDirX;
    public int _jumpCounter;

    Coroutine _jumpTimer;
    Coroutine _wallJumpTimer;



    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start()
    {
        _controller = GetComponent<Controller2D>();
        _gravity = -50;
    }

    // -------------------------------------------------------
    // Input Detection methods called by the PlayerInput layer
    // -------------------------------------------------------
    public void SetDirectionalInput(Vector2 input)
    {
        _directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        if (_wallSliding)
        {
            _wallJumpTimer = StartCoroutine(WallJumpRoutine());
        }
        else if (_jumpCounter > 0)
        {
            if (!_controller._collisions.slidingDownMaxSlope && !_playerState.wallJumping)
            {
                _jumpCounter--;
                _jumpTimer = StartCoroutine(JumpRoutine());
            }
        }
    }

    public void OnJumpInputUp()
    {
        if (_jumpTimer != null)
        {
            StopCoroutine(_jumpTimer);
        }
        _playerState.jumping = false;
    }
    // -------------------------------------------------------


    void Update()
    {
        CheckWallCollisions();
        CheckVerticalCollisions();
        Move();
    }

    void CheckWallCollisions()
    {
        if (_playerState.wallJumping)
        {
            if (_controller._collisions.left && _wallDirX == -1)
            {
                _playerState.wallJumping = false;
                StopCoroutine(_wallJumpTimer);
            }
            else if (_controller._collisions.right && _wallDirX == 1)
            {
                _playerState.wallJumping = false;
                StopCoroutine(_wallJumpTimer);
            }
        }
        else
        {
            _wallDirX = (_controller._collisions.left) ? -1 : (_controller._collisions.right) ? 1 : 0;
        }

        _wallSliding = false;
        if ((_controller._collisions.left || _controller._collisions.right) && !_controller._collisions.below && _velocity.y < 0)
        {
            _wallSliding = true;
            if (_velocity.y < -_wallFriction)
            {
                _velocity.y = -_wallFriction;
            }

            if (_timeToWallUnstick > 0)
            {
                _velocityXSmoothing = 0;
                _velocity.x = 0;

                if (_directionalInput.x == -_wallDirX)
                {
                    _timeToWallUnstick -= Time.deltaTime;
                }
                else
                {
                    _timeToWallUnstick = _wallStickTime;
                }
            }
            else
            {
                _timeToWallUnstick = _wallStickTime;
            }

        }
    }

    void CheckVerticalCollisions()
    {
        if (_controller._collisions.below)
        {
            if (!_playerState.jumping)
            {
                _jumpCounter = 2;
            }

            if (_controller._collisions.slidingDownMaxSlope)
            {

                _velocity.y += _controller._collisions.slopeNormal.y * -_gravity * Time.deltaTime;
            }
            else
            {
                _velocity.y = 0;
            }
        }

        if (_controller._collisions.above)
        {
            // Stop the jump logic immediately when the player hits a ceiling
            _velocity.y = 0;
            if (_jumpTimer != null)
            {
                StopCoroutine(_jumpTimer);
                _playerState.jumping = false;
            }
        }
    }

    void Move()
    {
        // The basic forces that act upon the player, based on its state 
        float targetVelocityX = _directionalInput.x * _moveSpeed;
        Vector2 smoothRef = new Vector2(_velocityXSmoothing, _velocityYSmoothing);

        if (_playerState.jumping)
        {
            _velocity = Vector2.SmoothDamp(_velocity, new Vector2(targetVelocityX, 10), ref smoothRef, _accelerationTimeAirborne);
        }
        else if (_playerState.wallJumping)
        {
            _velocity = Vector2.SmoothDamp(_velocity, new Vector2(-_wallDirX * 10, 5), ref smoothRef, Time.deltaTime);
        }
        else
        {
            _velocity.y += _gravity * Time.deltaTime;
            float runVelocity = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing, (_controller._collisions.below) ? _accelerationTimeGrounded : _accelerationTimeAirborne);
            _velocity.x = runVelocity;
        }
        _controller.Move(_velocity * Time.deltaTime, _directionalInput);
    }

    public void Dash()
    {
        _dashSpeed = 15 * _directionalInput.x;
        _playerState.dashing = true;
    }



    // --------------------------------------------------------------------------------
    // Coroutines
    // --------------------------------------------------------------------------------

    IEnumerator JumpRoutine()
    {
        _playerState.jumping = true;
        yield return new WaitForSeconds(0.2f);
        _playerState.jumping = false;
    }

    IEnumerator WallJumpRoutine()
    {
        _playerState.wallJumping = true;
        yield return new WaitForSeconds(0.25f);
        _playerState.wallJumping = false;
    }
}
using System.Collections;
using UnityEngine;
public class Player : MonoBehaviour {

    // --------------------------------------------------------------------------------
    // Properties
    // --------------------------------------------------------------------------------

    // Components
    private Rigidbody2D _rB2D;
    private PlayerAnimation _playerAnimation;
    private SpriteRenderer _playerSprite;
    private Animation _animation;

    // Variables
    private float _direction;
    private float _horizontalInput;
    private float _speed = 2.5f;
    private float _jumpForce = 8.5f;

    // Coroutines
    private bool _hasWalked = false;
    private bool _hasJumped = false;

    // Status
    private bool _canRun = false;
    private bool _grounded = false;
    private bool _ducked = false;



    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------
    void Start() {
        _rB2D = GetComponent<Rigidbody2D>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update() {                 // TODOS
        Movement();                 // See Movement
        Attack();                   // 11
    }

    void Movement() { 
        PlayerDirection();          // 00
        Duck();                     // 01
        Jump();                     // 02
        WalkOrRun();                // 04 - 05
    }



    // --------------------------------------------------------------------------------
    // Movement
    // --------------------------------------------------------------------------------
    void PlayerDirection()
    {
        _direction = Input.GetAxisRaw("Horizontal");
        if (_direction < 0)
        {
            _playerSprite.flipX = true;
        }
        else if (_direction > 0)
        {
            _playerSprite.flipX = false;
        }
    }
    // --------------------------------------------------------------------------------
    void Duck()
    {
        if (Input.GetKey(KeyCode.DownArrow) && IsGrounded())
        {
            _ducked = true;
            _playerAnimation.Duck(_ducked);
        }
        else
        {
            _ducked = false;
            _playerAnimation.Duck(_ducked);
        }
    }
    // --------------------------------------------------------------------------------
    void WalkOrRun()
    {
        MoveStatus();

        if (_ducked == false)
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            if (_canRun == false)
            {
                Walk();
            }
            else
            {
                Run();
            }
        }
        else
        {
            _horizontalInput = 0;
        }
    }

    void Walk()
    {
        _speed = 2.5f;
        _rB2D.velocity = new Vector2(_horizontalInput * _speed, _rB2D.velocity.y);
        _playerAnimation.Move(_horizontalInput);
    }

    void Run()
    {
        _speed = 5f;
        _rB2D.velocity = new Vector2(_horizontalInput * _speed, _rB2D.velocity.y);
        _playerAnimation.Move(_horizontalInput);
    }
    // --------------------------------------------------------------------------------
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            _rB2D.velocity = new Vector2(_rB2D.velocity.x, _jumpForce);
            StartCoroutine(HasJumpedRoutine());
            _playerAnimation.Jump(true);
        }
        _grounded = IsGrounded();
    }



    // ----------------------------------------
    // Movement Support
    // ----------------------------------------
    void MoveStatus()
    {
        if (IsGrounded())
        {
            if (_hasWalked == false)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                    StartCoroutine(HasWalkedRoutine());
            }
            if (_hasWalked == true)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                    _canRun = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
            _canRun = false;
    }

    bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 0.75f, 1 << 8);
        if (hitGround.collider != null)
        {
            if (_hasJumped == false)
            {
                _playerAnimation.Jump(false);
                return true;
            }
        }
        return false;
    }

    IEnumerator HasWalkedRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        _hasWalked = true;
        yield return new WaitForSeconds(1f);
        _hasWalked = false;
    }

    IEnumerator HasJumpedRoutine()
    {
        _hasJumped = true;
        yield return new WaitForSeconds(0.1f);
        _hasJumped = false;
    }



    // --------------------------------------------------------------------------------
    // Attack
    // --------------------------------------------------------------------------------
    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.RightControl))
            _playerAnimation.Attack();
    }
}

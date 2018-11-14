using System.Collections;
using UnityEngine;
public class Player_Ihsan : MonoBehaviour
{

    // --------------------------------------------------------------------------------
    // Properties
    // --------------------------------------------------------------------------------

    // Components
    private Rigidbody2D _rB2D;
    private BoxCollider2D _bC2D;
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
    private bool _ducked = false;
    private bool _canDodge = true;

    public bool _canMoveBlock = false;
    public float _dodgeTimer = 0;



    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start()
    {
        _rB2D = GetComponent<Rigidbody2D>();
        _bC2D = GetComponent<BoxCollider2D>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerSprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {                               // TODOS
        Movement();                 // See Movement
    }

    void Movement()
    {
        PlayerDirection();          // 00
        Duck();                     // 01
        Jump();                     // 02
        WalkRunCrawl();             // 04 - 05
        MoveBlock();
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
        // Adjust the player's box collider 2D edges accordingly
        Vector2 bC2DSize = _bC2D.size;
        Vector2 bC2DOffset = _bC2D.offset;

        if (Input.GetKey(KeyCode.DownArrow) && IsGrounded())
        {
            bC2DSize.y = 0.5f;
            bC2DOffset.y = -0.25f;
            _bC2D.size = bC2DSize;
            _bC2D.offset = bC2DOffset;

            _ducked = true;
            _playerAnimation.Duck(_ducked);
        }
        else if (!IsInCrawlSpace())
        {
            bC2DSize.y = 1;
            bC2DOffset.y = 0;
            _bC2D.size = bC2DSize;
            _bC2D.offset = bC2DOffset;

            _ducked = false;
            _playerAnimation.Duck(_ducked);
        }
    }
    // --------------------------------------------------------------------------------
    void WalkRunCrawl()

    {
        MoveStatus();
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        if (_ducked == false)
        {
            if (_canRun == false)
            {
                Walk();
            }
            else
            {
                Run();
            }

            if (Input.GetKeyDown(KeyCode.Z) && IsGrounded())
            {
                Dodge();
            }
        }
        else
        {
            Crawl();
        }
    }

    void Walk()
    {
        _speed = 1f;
        _rB2D.velocity = new Vector2(_horizontalInput * _speed, _rB2D.velocity.y);
        _playerAnimation.Move(_horizontalInput);
    }

    void Run()
    {
        _speed = 3f;
        _rB2D.velocity = new Vector2(_horizontalInput * _speed, _rB2D.velocity.y);
        _playerAnimation.Move(_speed);
    }

    void Crawl()
    {
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
        {
            _speed = 0.5f;
            _rB2D.velocity = new Vector2(_horizontalInput * _speed, _rB2D.velocity.y);
            _playerAnimation.Move(_speed);
        }
        else
        {
            _speed = 0;
            _rB2D.velocity = new Vector2(_horizontalInput * _speed, _rB2D.velocity.y);
            _playerAnimation.Move(_speed);
        }
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
    }
    //---------------------------------------------------------------------------------
    void Dodge()
    {
        if (_canDodge == true)
        {
            _canDodge = false;
            _dodgeTimer = Time.time + .001f;
            _speed = 50f;
            _rB2D.velocity = new Vector2(_horizontalInput * _speed, 0);
            //_playerAnimation.Move(_horizontalInput);

        }
        else
        {
            if (_dodgeTimer <= Time.time)
            {
                _canDodge = true;
            }
        }
    }
    //---------------------------------------------------------------------------------
    void MoveBlock()
    {
        if (Input.GetKey(KeyCode.M) && IsNearBlock())
        {
            _canMoveBlock = true;
        }
        else
        {
            _canMoveBlock = false;
        }
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
                {
                    _canRun = true;
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            _canRun = false;
        }
    }



    // ----------------------------------------
    // Raycast Status
    // ----------------------------------------

    bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 0.75f, 1 << 8 | 1 << 9);
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

    bool IsInCrawlSpace()
    {
        RaycastHit2D hitCrawlSpace = Physics2D.Raycast(transform.position, Vector2.up, 0.3f, 1 << 8);
        if (hitCrawlSpace.collider != null)
        {
            return true;
        }
        return false;
    }

    bool IsNearBlock()
    {
        RaycastHit2D hitBlockRight = Physics2D.Raycast(transform.position, Vector2.right, 0.3f, 1 << 9);
        RaycastHit2D hitBlockLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.3f, 1 << 9);
        if (hitBlockRight.collider != null || hitBlockLeft.collider != null)
        {
            return true;
        }
        return false;
    }



    // ----------------------------------------
    // Coroutines
    // ----------------------------------------

    IEnumerator HasWalkedRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        _hasWalked = true;
        yield return new WaitForSeconds(0.2f);
        _hasWalked = false;
    }

    IEnumerator HasJumpedRoutine()
    {
        _hasJumped = true;
        yield return new WaitForSeconds(0.1f);
        _hasJumped = false;
    }

}

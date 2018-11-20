using System.Collections;
using UnityEngine;
public class OldPlayer : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // Properties
    // --------------------------------------------------------------------------------

    // Components
    private Rigidbody2D _rB2D;
    private BoxCollider2D _bC2D;
    private SpriteRenderer _playerSprite;
    private Animation _animation;
    public PlayerAnimation _playerAnimation;

    // Variables
    private float _direction;
    private float _horizontalInput;
    private float _speed = 2.5f;
    private float _jumpForce = 8.5f;

    // Coroutines
    private bool _hasWalked = false;
    private bool _hasJumped = false;
    private bool _hasDodged = false;

    // Status
    private bool _canRun = false;
    private bool _canDodge = false;
    private bool _ducking = false;
    private bool _interacting = false;

    // Block Interaction
    public bool _canMoveBlock = false;
    public bool _facingRightNearBlock = false;
    public bool _facingLeftNearBlock  = false;
    public bool _pullingRight = false;
    public bool _pullingLeft = false;



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
    {
        Movement();
    }

    void Movement()
    {
        // Basic
        MoveStatus();
        PlayerDirection();
        Duck();
        Jump();
        Move();

        // Interaction
        MoveBlock();
    }



    // --------------------------------------------------------------------------------
    // Basic Movement
    // --------------------------------------------------------------------------------

    void PlayerDirection()
    {
        _direction = Input.GetAxisRaw("Horizontal");
        if (_direction < 0 && !_interacting)
        {
            _playerSprite.flipX = true;
        }
        else if (_direction > 0 && !_interacting)
        {
            _playerSprite.flipX = false;
        }
    }

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

            _ducking = true;
            _playerAnimation.Duck(_ducking);
        }
        else if (!IsInCrawlSpace())
        {
            bC2DSize.y = 1;
            bC2DOffset.y = 0;
            _bC2D.size = bC2DSize;
            _bC2D.offset = bC2DOffset;

            _ducking = false;
            _playerAnimation.Duck(_ducking);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            _rB2D.velocity = new Vector2(_rB2D.velocity.x, _jumpForce);
            StartCoroutine(HasJumpedRoutine());
            _playerAnimation.Jump(true);
        }
    }

    void Move()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        if (!_interacting)
        {
            if (_ducking == false)
            {
                if (_canRun == false)
                {
                    Walk();
                }
                else
                {
                    Run();
                }

                if (_canDodge && _hasDodged && _horizontalInput == 0)
                {
                    Dodge();
                }
            } 
            else
            {
                Crawl();
            }
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
        _canRun = false;
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
    
    void Dodge()
    {
        float dodgeDirection = _playerSprite.flipX ? 1 : -1;
        _rB2D.velocity += new Vector2(5 * dodgeDirection, 0);
        _playerAnimation.Dodge(_canDodge);
    }



    // --------------------------------------------------------------------------------
    // Interaction Movement
    // --------------------------------------------------------------------------------

    void MoveBlock()
    {
        if (Input.GetKey(KeyCode.M) && IsNearBlock())
        {
            _canRun = false;
            _canMoveBlock = true;
            _playerAnimation.Move(0);

            if (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.LeftArrow))
            {
                _playerAnimation.Push(false);
                _playerAnimation.Pull(false);
            }
            else if ((Input.GetKey(KeyCode.RightArrow) && _facingRightNearBlock) || (Input.GetKey(KeyCode.LeftArrow) && _facingLeftNearBlock))
            {
                _rB2D.velocity = new Vector2(_horizontalInput * 1.25f, _rB2D.velocity.y);
                UpdateBlockGripStatus(false, false, true);
            }
            else if (Input.GetKey(KeyCode.RightArrow) && _facingLeftNearBlock)
            {
                UpdateBlockGripStatus(true, false, false);
            }
            else if (Input.GetKey(KeyCode.LeftArrow) && _facingRightNearBlock)
            {
                UpdateBlockGripStatus(false, true, false);
            }
            else
            {
                UpdateBlockGripStatus(false, false, false);
            }
        }
        else
        {
            _canMoveBlock = false;
            _facingRightNearBlock = false;
            _facingLeftNearBlock = false;
            _playerAnimation.Pull(false);
            UpdateBlockGripStatus(false, false, false);
        }
    }



    // ----------------------------------------
    // Movement Support
    // ----------------------------------------

    void MoveStatus()
    {
        // Player will not move normally if interacting with objects
        _interacting = (_facingRightNearBlock == true || _facingLeftNearBlock == true);

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

            if (_hasDodged == false)
            {
                if (Input.GetKeyDown(KeyCode.Z) && _horizontalInput == 0)
                {
                    StartCoroutine(HasDodgedRoutine());
                    StartCoroutine(DodgeCooldownRoutine());
                }
            }
        }

        if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            _canRun = false;
        }
    }

    void UpdateBlockGripStatus(bool pullingLeft, bool pullingRight, bool pushing)
    {
        _pullingLeft = pullingLeft;
        _pullingRight = pullingRight;
        _playerAnimation.Push(pushing);
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
        if (hitBlockRight.collider != null)
        {
            _facingRightNearBlock = true;
            _facingLeftNearBlock = false;
            return true;
        }
        else if (hitBlockLeft.collider != null)
        {
            _facingRightNearBlock = false;
            _facingLeftNearBlock  = true;
            return true;
        }
        return false;
    }



    // ----------------------------------------
    // Coroutines & Cooldowns
    // ----------------------------------------

    // CoRoutines
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

    IEnumerator HasDodgedRoutine()
    {
        _hasDodged = true;
        yield return new WaitForSeconds(1f);
        _hasDodged = false;
    }

    // Cooldowns
    IEnumerator DodgeCooldownRoutine()
    {
        _canDodge = true;
        yield return new WaitForSeconds(0.2f);
        _canDodge = false;
        _playerAnimation.Dodge(_canDodge);
    }
}

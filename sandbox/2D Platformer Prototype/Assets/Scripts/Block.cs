using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // Properties
    // --------------------------------------------------------------------------------

    private Rigidbody2D _rB2D;
    private OldPlayer _player;
    

    
    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start () {
        _rB2D = GetComponent<Rigidbody2D>();
        _player = GameObject.FindWithTag("Player").GetComponent<OldPlayer>();
    }
    
	void Update () {
        IsMovable();
        IsOnCliff();
    }

    void IsMovable()
    { 
        if (_player._canMoveBlock && IsNearPlayer())
        {
            _rB2D.mass = 1;

            // Move in the direction of the player if he is pulling and let the box collider naturally push him away
            if (_player._pullingLeft || _player._pullingRight)
            {
                _rB2D.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * 0.75f, _rB2D.velocity.y);
                _player._playerAnimation.Pull(true);
            }
        }
        else
        {
            _rB2D.mass = 1000000;
        }
    }

    void IsOnCliff()
    {
        _rB2D.drag = IsGrounded() ? 10 : 1;
    }



    // ----------------------------------------
    // Raycast Status
    // ----------------------------------------

    bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 0.55f, 1 << 8);
        return hitGround.collider != null ? true : false;
    }

    bool IsNearPlayer()
    {
        RaycastHit2D hitPlayerRight = Physics2D.Raycast(transform.position, Vector2.right, 0.55f, 1 << 10);
        RaycastHit2D hitPlayerLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.55f, 1 << 10);
        return (hitPlayerRight.collider != null || hitPlayerLeft.collider != null) ? true : false;
    }
}

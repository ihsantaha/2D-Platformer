using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {

    // --------------------------------------------------------------------------------
    // Properties
    // --------------------------------------------------------------------------------

    // Components
    private Rigidbody2D _rB2D;

    // Status
    private bool _isMovable;
    

    
    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start () {
        _rB2D = GetComponent<Rigidbody2D>();
	}
    
	void Update () {
        IsMovable();
        IsOnCliff();
	}

    void IsMovable()
    {
        _isMovable = GameObject.FindWithTag("Player").GetComponent<Player_Ihsan>()._canMoveBlock;

        if (_isMovable)
        {
            _rB2D.mass = 1;
        }
        else
        {
            _rB2D.mass = 1000000;
        }
    }

    void IsOnCliff()
    {
        if (IsGrounded())
        {
            _rB2D.drag = 10;
        }
        else
        {
            _rB2D.drag = 1;
        }
    }



    // ----------------------------------------
    // Raycast Status
    // ----------------------------------------

    bool IsGrounded()
    {
        RaycastHit2D hitGround = Physics2D.Raycast(transform.position, Vector2.down, 0.55f, 1 << 8);
        if (hitGround.collider != null)
        {
            return true;
        }
        return false;
    }

}

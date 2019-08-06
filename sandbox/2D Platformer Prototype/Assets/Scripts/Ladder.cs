using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{

    // --------------------------------------------------------------------------------
    // Fields
    // --------------------------------------------------------------------------------

    // private Rigidbody2D rB2D;
    private Player player;

    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start()
    {
        // rB2D = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }

    private void Update()
    {
        Climb();
    }

    public void Climb()
    {
        if (CanClimb() && Input.GetKeyUp("up"))
        {
            player.playerState.climbing = true;
        }
    }

    bool CanClimb()
    {
        RaycastHit2D hitClimbRight = Physics2D.Raycast(transform.position, Vector2.right, 0.3f, 1 << 11);
        RaycastHit2D hitClimbLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.3f, 1 << 11);

        if (hitClimbRight.collider != null || hitClimbLeft.collider != null)
        {
            return true;
        }
        return false;
    }
}

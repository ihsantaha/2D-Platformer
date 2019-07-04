using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballAttack1 : MonoBehaviour {

    // --------------------------------------------------------------------------------
    // Fields
    // --------------------------------------------------------------------------------

    public Transform magicSpriteTransform;
    public SpriteRenderer magicSpriteRenderer;
    public Animator magicAnimator;
    public MagicAnimation magicAnimation;
    
    private RaycastHit2D hitWallRight;
    private RaycastHit2D hitWallLeft;
    private Player player;

    private int direction;




    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start () {
        magicSpriteTransform = GetComponentInChildren<Transform>();
        magicSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        magicAnimator = GetComponentInChildren<Animator>();
        magicAnimation = GetComponent<MagicAnimation>();

        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        direction = player.direction;
        magicSpriteRenderer.flipX = direction == -1 ? true : false;
    }
	
	
	void Update () {
        Move();
        IsInCollision();
	}


    void Move()
    {
        if (!hitWallRight && !hitWallRight)
        {
            transform.Translate(4.5f * Time.deltaTime * direction, 0, 0);
        }
    }

    bool IsInCollision()
    {
        hitWallRight = Physics2D.Raycast(transform.position, Vector2.right, 0.5f, 1 << 8);
        hitWallLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.5f, 1 << 8);
        if (hitWallRight.collider != null || hitWallLeft.collider != null)
        {
            magicAnimation.Die();
            GameObject.Destroy(gameObject, 0.65f);
            return true;
        }
        return false;
    }

    void Die()
    {
        magicAnimator.SetTrigger("Die");
    }
}

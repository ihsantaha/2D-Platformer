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
    private RaycastHit2D hitWallUp;
    private RaycastHit2D hitWallDown;
    private Player player;
    private MagicAttack magicAttack;

    private float xDirection;
    private float yDirection;




    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start () {
        magicSpriteTransform = GetComponentInChildren<Transform>();
        magicSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        magicAnimator = GetComponentInChildren<Animator>();
        magicAnimation = GetComponent<MagicAnimation>();

        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        magicAttack = GameObject.FindWithTag("MagicAttack").GetComponent<MagicAttack>();

        xDirection = player.direction;
        yDirection = magicAttack.direction;
        magicSpriteTransform.rotation = magicAttack.angle;
        magicSpriteRenderer.flipX = xDirection == -1 ? true : false;
    }
	
	
	void Update () {
        Move();
        IsInCollision();
	}


    void Move()
    {
        if (!hitWallRight && !hitWallRight && !hitWallUp && !hitWallDown)
        {
            transform.Translate(4.5f * Time.deltaTime * xDirection, 4.5f * Time.deltaTime * yDirection, Time.deltaTime, 0);
        }
    }

    bool IsInCollision()
    {
        hitWallRight = Physics2D.Raycast(transform.position, Vector2.right, 0.5f, 1 << 8);
        hitWallLeft = Physics2D.Raycast(transform.position, Vector2.left, 0.5f, 1 << 8);
        hitWallUp = Physics2D.Raycast(transform.position, Vector2.up, 0.5f, 1 << 8);
        hitWallDown = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, 1 << 8);
        if (hitWallRight.collider != null || hitWallLeft.collider != null || hitWallUp.collider != null || hitWallDown.collider != null)
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

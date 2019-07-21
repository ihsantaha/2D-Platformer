using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuakeAttack1 : MonoBehaviour {

    // --------------------------------------------------------------------------------
    // Fields
    // --------------------------------------------------------------------------------

    public Transform magicSpriteTransform;
    public SpriteRenderer magicSpriteRenderer;
    public Animator magicAnimator;
    public MagicAnimation magicAnimation;

    private Player player;
    private MagicAttack magicAttack;




    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start () {
        GameObject.Destroy(gameObject, 1);
    }

}

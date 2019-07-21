using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicAttack : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // Fields
    // --------------------------------------------------------------------------------

    public GameObject fireball_1;
    public GameObject quake_1;
    public GameObject quake_1_effects;
    public Quaternion angle;
    public float initialWaitTime;
    public int direction;

    private Player player;




    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }


    void Update()
    {
        CastMagic();
    }


    void CastMagic()
    {
        if (player.playerState.castingMagic)
        {

            player.playerState.castingMagic = false;
            switch (player.currentMagicType)
            {
                case "QuakeAttack1":
                    player.playerAnimation.SwordAttack("EarthAttack1");
                    StartCoroutine(HasCastedEarthAttack1());
                    break;
                case "FireballAttack1":
                    player.playerAnimation.SwordAttack("SwordAttack3");
                    StartCoroutine(HasCastedFireballAttack1());
                    break;
                default:
                    break;
            }
        }
    }


    IEnumerator HasCastedFireballAttack1()
    {
        yield return new WaitForSeconds(player.IsGrounded() ? 0.45f : 0);
        Vector3 startingPosition = player.transform.position + new Vector3(1 * player.direction, 0, 0);

        if (Input.GetKey(KeyCode.UpArrow))
        {
            direction = 1;
            angle = player.playerSpriteRenderer.flipX ? Quaternion.Euler(0, 0, -45) : Quaternion.Euler(0, 0, 45);

        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            direction = -1;
            angle = player.playerSpriteRenderer.flipX ? Quaternion.Euler(0, 0, 45) : Quaternion.Euler(0, 0, -45);
        }
        else
        {
            direction = 0;
            angle = Quaternion.Euler(0, 0, 0);
        }

        Instantiate(fireball_1, startingPosition, angle);
    }

    IEnumerator HasCastedEarthAttack1()
    {
        yield return new WaitForSeconds(1);
        Vector3 startingPosition = player.transform.position + new Vector3(3 * player.direction, 0, 0);
        Instantiate(quake_1, startingPosition, transform.rotation);
        Instantiate(quake_1_effects, startingPosition, transform.rotation);
    }

}

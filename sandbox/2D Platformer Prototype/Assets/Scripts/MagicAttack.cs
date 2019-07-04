using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicAttack : MonoBehaviour {
    // --------------------------------------------------------------------------------
    // Fields
    // --------------------------------------------------------------------------------
 
    public GameObject fireball_1;
    public float initialWaitTime;

    private Player player;





    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start () {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
    }
	

	void Update () {
        CastMagic();
	}


   void CastMagic()
    {
        if (player.playerState.castingMagic)
        {
            
            player.playerState.castingMagic = false;
            switch (player.currentMagicType)
            {
                case "FireballAttack1":
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
        Instantiate(fireball_1, startingPosition, player.transform.rotation);
    }
}

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // Fields
    // --------------------------------------------------------------------------------

    public KeyCode jumpKey;

    // Class Variables
    Player player;
    KeyCode lastKey;
    float lastPressed;
    float cooldown;

    // Member Variables
    private readonly float _delay = 0.2f;




    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start()
    {
        player = GetComponent<Player>();
        lastPressed = -1;
    }


    void Update()
    {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);

        if (Input.GetKeyDown(jumpKey))
        {
            player.OnJumpInputDown();
        }
        if (Input.GetKeyUp(jumpKey))
        {
            player.OnJumpInputUp();
        }
    }


    bool DoubleTap(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            if (cooldown <= 0 && key == lastKey && Time.time - lastPressed <= _delay)
            {
                cooldown = 0.4f;
                return true;
            }
            else
            {
                lastKey = key;
                lastPressed = Time.time;
            }
        }
        return false;
    }
}

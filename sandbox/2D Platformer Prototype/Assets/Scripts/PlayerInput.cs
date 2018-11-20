using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Player))]
public class PlayerInput : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // Properties
    // --------------------------------------------------------------------------------

    Player _player;
    KeyCode _lastKey;
    public KeyCode _jumpKey;

    float _lastPressed;
    float _cooldown;
    private readonly float _delay = 0.2f;



    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start()
    {
        _player = GetComponent<Player>();
        _lastPressed = -1;
    }

    void Update()
    {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _player.SetDirectionalInput(directionalInput);

        if (Input.GetKeyDown(_jumpKey))
        {
            _player.OnJumpInputDown();
        }
        if (Input.GetKeyUp(_jumpKey))
        {
            _player.OnJumpInputUp();
        }
    }

    bool DoubleTap(KeyCode key)
    {
        if (Input.GetKeyDown(key))
        {
            if (_cooldown <= 0 && key == _lastKey && Time.time - _lastPressed <= _delay)
            {
                _cooldown = 0.4f;
                return true;
            }
            else
            {
                _lastKey = key;
                _lastPressed = Time.time;
            }
        }
        return false;
    }
}

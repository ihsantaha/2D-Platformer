using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --------------------------------------------------------------------------------
    // Properties
    // --------------------------------------------------------------------------------

    private float _baseSpeed;
    private float _targetSpeed;
    private float _xSpeed;
    private float _xRef;

    public Vector2 _velocity;



    // --------------------------------------------------------------------------------
    // Methods
    // --------------------------------------------------------------------------------

    void Start()
    {
        _baseSpeed = 2;
        _targetSpeed = 6;
        _xRef = 0;
    }

    void FixedUpdate()
    {
        float xInput = Input.GetAxisRaw("Horizontal");

        Move(xInput);
    }

    void Move(float xDir)
    {
        _xSpeed = xDir != 0 ? Mathf.SmoothDamp(_xSpeed, _targetSpeed, ref _xRef, 4f * Time.deltaTime) : _baseSpeed;
        transform.Translate(Time.fixedDeltaTime * Vector3.right * xDir * _xSpeed);
    }

    void VerticalForce()
    {

    }

}

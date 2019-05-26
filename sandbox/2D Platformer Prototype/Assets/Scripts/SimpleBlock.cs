using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBlock : RaycastController2D {

   Controller2D controller;
    Vector2 velocity;
    bool hitRight;
    bool hitLeft;
	// Use this for initialization
	public override void Start () {
        CalculateRaySpacing();
        UpdateRaycastOrigins();
        controller = GetComponent<Controller2D>();
    }

    // Update is called once per frame
    void Update() {
        Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        UpdateRaycastOrigins();
        DetectPlayer();

        if (hitRight && Input.GetKey(KeyCode.C))
        {

            velocity.x = directionalInput.x < 0 ? -2 : 0;
        }

        if (hitLeft && Input.GetKey(KeyCode.C))
        {

            velocity.x = directionalInput.x > 0 ? 2 : 0;
        }

        if (!hitRight && !hitLeft)
        {
            velocity.x = 0;
        }

        if (!controller.collisions.below)
        {

            velocity.y = -5;
        }

        controller.Move(velocity * Time.deltaTime, directionalInput);


    }

    void DetectPlayer() {
 
        bool currentLeft = false;
        bool currentRight = false;
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOriginLeft = raycastOrigins.bottomLeft;
            rayOriginLeft += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D left = Physics2D.Raycast(rayOriginLeft, Vector2.left, 2*padding, collisionMask);
            Debug.DrawLine(rayOriginLeft, rayOriginLeft + new Vector2(padding, 0)); Vector2 rayOriginRight = raycastOrigins.bottomRight;
            if (left)
            {
                currentLeft = true;
 
            }
            rayOriginRight += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D right = Physics2D.Raycast(rayOriginRight, Vector2.right, 2*padding, collisionMask);
            Debug.DrawLine(rayOriginRight,rayOriginRight + new Vector2(padding, 0));
            if (right)
            {
                currentRight = true;

            }
  
        }

        hitLeft = currentLeft;
        hitRight = currentRight;
    }
}

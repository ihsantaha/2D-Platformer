using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour {
	float distToGround;
	Rigidbody2D rb;
	CircleCollider2D collider;
	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		collider = GetComponent<CircleCollider2D> ();
		distToGround = collider.bounds.extents.y;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	bool isGrounded(){
		return Physics2D.Raycast (transform.position, Vector2.down, distToGround + 0.1f);
	}

	void OnCollisionEnter2D(Collision2D col){
		
		if (isGrounded() && col.gameObject.tag == "Attack") {
			rb.velocity = new Vector2 (rb.velocity.x, 6);
		}
	
	}
}

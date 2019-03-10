using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour {
	CircleController2D controller;
	Vector2 velocity;
	public float threshold;
	public float aerialDragCoefficient;
	public float groundDragCoefficient;
	// Use this for initialization
	void Start () {
		controller = GetComponent<CircleController2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		Drag ();
		if (controller.collisions.below && velocity.y <0) {
			velocity.y = 0;
		} else {
			velocity.y -= 10 * Time.deltaTime;
		}

		if (controller.collisions.above && velocity.y > 0) {
		
			velocity.y = 0;
		}

		controller.Move (velocity * Time.deltaTime,false);
	}

	void Drag(){

		if (Mathf.Abs(velocity.x) > threshold) {

			velocity.x *= (controller.collisions.below) ? groundDragCoefficient : aerialDragCoefficient;

		} else {
			velocity.x = 0;
		}
	
	}

	void OnTriggerEnter2D(Collider2D col){
	
		if (col.gameObject.tag == "Attack") {
			velocity = col.gameObject.GetComponent<AttackScript>().knockback;
		}
	
	}

}

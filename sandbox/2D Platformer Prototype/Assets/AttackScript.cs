using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour {
	public Vector2 polarized;
	public Vector2 knockback;
	Vector2 knockbackRef;
	public float damage;
	// Use this for initialization
	void Start () {
		knockbackRef = knockback;
	}
	
	// Update is called once per frame
	void Update () {

		knockback.x = knockbackRef.x*-transform.parent.localScale.x;
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour {
	
	Animator attackAnimator;
	AnimatorStateInfo attackState;
	float polarity;
	float tilt;
	readonly Vector3 LEFT = new Vector3(-1,1,1);
	readonly Vector3 RIGHT = new Vector3(1,1,1);
	// Use this for initialization
	void Start () {
		attackAnimator = GetComponent<Animator> ();
	}

	// Update is called once per frame
	void Update () {
		attackState = attackAnimator.GetCurrentAnimatorStateInfo(0);
		polarity = Input.GetAxisRaw ("Horizontal");
		tilt = Input.GetAxisRaw ("Vertical");
		if (attackState.IsName ("Idle")) {
			if (polarity != 0) {
				transform.localScale = (polarity > 0) ? LEFT : RIGHT;

			}
			if (Input.GetKeyDown (KeyCode.Z)) {
				if (tilt == 0) {
					attackAnimator.Play ("Jab", -1, 0);
				} else if (tilt < 0) {
					attackAnimator.Play ("DownTilt", -1, 0);
				}
			}
		}
	}
}

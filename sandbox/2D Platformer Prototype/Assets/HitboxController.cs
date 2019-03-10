using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxController : MonoBehaviour {

	public PolygonCollider2D[] colliders;

	// Use this for initialization
	void Start () {
		
	}

	public void TriggerHitbox(int hitboxId){
		Debug.Log ("hitbox activated");
		colliders [hitboxId].enabled = true;
	}

	public void DisableHitbox(int hitboxId){
		Debug.Log ("hitbox deactivated");
		colliders [hitboxId].enabled = false;
	}

	public void DisableAllHitboxes(){
	
	}

}

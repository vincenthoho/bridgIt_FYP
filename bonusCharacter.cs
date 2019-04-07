using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bonusCharacter : character {
	public AudioClip getSE;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//activate bonus character's animation
		//if (startMove && aiMode_init.mustConnect) {
		//	getBonus ();
		//}
	}

	void OnTriggerEnter2D(Collider2D other){
		if (other.gameObject.tag == "PlayerChar") {
			GetComponent<Animator> ().SetBool ("get", true);
			AudioSource.PlayClipAtPoint (getSE, GetComponent<RectTransform>().anchoredPosition3D);
		}
	}
}

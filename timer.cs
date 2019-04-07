using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class timer : MonoBehaviour {

	private float time;
	private static int seconds = 0;
	private static bool continueTimer = true;
	public GameObject timerText;

	// Use this for initialization
	void Start () {
		time = 0;
		seconds = 0;
		continueTimer = true;
	}

	// Update is called once per frame
	void Update () {
		if (continueTimer)
			updateTimer ();
	}

	public static int getTime(){
		return seconds;
	}

	void updateTimer(){
		time += Time.deltaTime;
		seconds = (int)time;
		timerText.GetComponent<Text> ().text = seconds.ToString ();
	}

	public static void setTimer(bool b){
		continueTimer = b;
	}
}
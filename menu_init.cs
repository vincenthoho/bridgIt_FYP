using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class menu_init : MonoBehaviour {

	public GameObject instructionPanel;
	public GameObject[] levelButtons = new GameObject[2];
	public GameObject modeSlider, moveFirstSlider, modeText, moveFirstText;
	public Sprite[] ableButtons = new Sprite[2];
	public Sprite[] disabledButtons = new Sprite[2];
	public Sprite blueHandle, redHandle;
	public GameObject hintText, LoginInfo, LevelReachedInfo;
	public static bool aiFirst = true;
	public static bool limitMoveMode = false;
	private int levelReached = 1;

	void Awake(){
	}

	// Use this for initialization
	void Start () {
		limitMoveMode = false;
		if (login_controller.getLoginState ()) {
			StartCoroutine(getLevelReached (login_controller.getUserID ()));
			LoginInfo.GetComponent<Text> ().text = "Account: " + login_controller.getUserEmail ();
			LoginInfo.SetActive (true);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void switchMoveFirst(GameObject handle){
		if (moveFirstSlider.GetComponent<Slider> ().value == 1) {
			moveFirstText.GetComponent<Text> ().text = "AI Move First";
			moveFirstText.GetComponent<Text>().color = new Color32(0xF6, 0x44, 0x53, 0xFF);//red
			handle.GetComponent<Image> ().sprite = redHandle;
			aiFirst = true;
		} else {
			moveFirstText.GetComponent<Text> ().text = "Player Move First";
			moveFirstText.GetComponent<Text>().color = new Color32(0x3F, 0x98, 0xEA, 0xFF);//blue
			handle.GetComponent<Image> ().sprite = blueHandle;
			aiFirst = false;
		}
	}

	public void switchMode(GameObject handle){
		if (modeSlider.GetComponent<Slider> ().value == 1) {
			modeText.GetComponent<Text> ().text = "Limit Move Mode";
			modeText.GetComponent<Text>().color = new Color32(0xF6, 0x44, 0x53, 0xFF);//red
			handle.GetComponent<Image> ().sprite = redHandle;
			limitMoveMode = true;
			loadLevelButtons_limitMove ();
		} else {
			modeText.GetComponent<Text> ().text = "Simple Mode";
			modeText.GetComponent<Text>().color = new Color32(0x3F, 0x98, 0xEA, 0xFF);//blue
			handle.GetComponent<Image> ().sprite = blueHandle;
			limitMoveMode = false;
			loadLevelButtons_simple ();
		}
	}

	public IEnumerator updateDataSnapshot(string userID){
		DataSnapshot snapshot;
		var getTask = FirebaseDatabase.DefaultInstance.GetReference ("users").Child (userID).GetValueAsync();
		yield return new WaitUntil (() => getTask.IsCompleted || getTask.IsFaulted);
		if (getTask.IsCompleted)
			snapshot = getTask.Result;
	}

	public void callGetLevelReached(){
		StartCoroutine(getLevelReached (login_controller.getUserID ()));
	}

	public IEnumerator getLevelReached(string userID){
		//get data and check if it exists
		DataSnapshot snapshot = null;
		var getTask = FirebaseDatabase.DefaultInstance.GetReference ("users").Child (userID).GetValueAsync();
		yield return new WaitUntil (() => getTask.IsCompleted || getTask.IsFaulted);
		if (getTask.IsCompleted)
			snapshot = getTask.Result;

		//read levelReached from data
		if (!snapshot.Exists)
			levelReached = 1;
		else
			levelReached = int.Parse(snapshot.Child ("levelReached").Value.ToString());

		loadLevelButtons_simple ();
		LevelReachedInfo.GetComponent<Text> ().text = "Level Reached: " + levelReached;
		LevelReachedInfo.SetActive (true);
	}

	private void loadLevelButtons_simple(){
		int currentLevels = 1;

		hintText.SetActive (!login_controller.getLoginState ());

		//if not login, use levelCleared in quick save's data
		if (!login_controller.getLoginState ()) {
			if (PlayerPrefs.HasKey ("clearedLevel"))
				currentLevels = PlayerPrefs.GetInt ("clearedLevel");
			else {
				PlayerPrefs.SetInt ("clearedLevel", 1);
				PlayerPrefs.Save ();
			}
		} else
			currentLevels = levelReached;

		for (int i = 0; i < levelButtons.Length; i++) {
			if (i < currentLevels) {
				levelButtons [i].GetComponent<Image> ().sprite = ableButtons [i];
				levelButtons [i].GetComponent<Button> ().interactable = true;
			} else {
				levelButtons [i].GetComponent<Image> ().sprite = disabledButtons [i];
				levelButtons [i].GetComponent<Button> ().interactable = false;
			}
		}

		//If not login, lock level 4-6
		for (int i = 3; i < levelButtons.Length; i++) {
			if (!login_controller.getLoginState ()) {
				levelButtons [i].GetComponent<Image> ().sprite = disabledButtons [i];
				levelButtons [i].GetComponent<Button> ().interactable = false;
			}
		}

	}

	private void loadLevelButtons_limitMove(){
		int currentLevels = 0;

		hintText.SetActive (!login_controller.getLoginState ());

		if (!login_controller.getLoginState ()) {
			if (PlayerPrefs.HasKey ("clearedLevel"))
				currentLevels = PlayerPrefs.GetInt ("clearedLevel");
		} else
			currentLevels = levelReached;

		for (int i = 0; i < levelButtons.Length; i++) {
			if (i < currentLevels-1) {
				levelButtons [i].GetComponent<Image> ().sprite = ableButtons [i];
				levelButtons [i].GetComponent<Button> ().interactable = true;
			} else {
				levelButtons [i].GetComponent<Image> ().sprite = disabledButtons [i];
				levelButtons [i].GetComponent<Button> ().interactable = false;
			}
		}

		//If not login, lock level 4-6
		for (int i = 3; i < levelButtons.Length; i++) {
			if (!login_controller.getLoginState ()) {
				levelButtons [i].GetComponent<Image> ().sprite = disabledButtons [i];
				levelButtons [i].GetComponent<Button> ().interactable = false;
			}
		}

	}
}

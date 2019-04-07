using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class userdata_controller : MonoBehaviour {
	public static bool firebaseReady = true;
	public static DatabaseReference databaseRef;
	public static DataSnapshot snapshot;

	private static userdata_controller instance { get; set; }

	void Awake(){
		DontDestroyOnLoad(this);
		instance = this;
	}

	// Use this for initialization
	void Start () {
		if (firebaseReady) {
			//Set database reference for userdata_controller
			FirebaseApp.DefaultInstance.SetEditorDatabaseUrl ("https://bridg-it.firebaseio.com/");
			databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public static void writeNewData(string userID, string email, int levelReached){
		User user = new User (email, levelReached);
		string json = JsonUtility.ToJson(user);
		databaseRef.Child ("users").Child (userID).SetRawJsonValueAsync (json);
	}

	public static void updateLevelReached(string userID, int newLevelReached){
		databaseRef.Child ("users").Child (userID).Child ("levelReached").SetValueAsync (newLevelReached);
	}
}

public class User{
	public string email;		//username -> email
	public int levelReached;	//max level reached

	public User(string email, int levelReached){
		this.email = email;
		this.levelReached = levelReached;
	}

	public string getEmail(){
		return email;
	}

	public int getLevelReached(){
		return levelReached;
	}
}
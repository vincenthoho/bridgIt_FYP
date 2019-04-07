using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Unity.Editor;

public class login_controller : MonoBehaviour {
	public static bool firebaseReady = true;
	public static Firebase.Auth.FirebaseAuth auth = null;
	public GameObject loginPanel, logoutPanel, emailTextBox, passwordTextBox, warningText, emailText, loginInfo;

	void Awake(){
		DontDestroyOnLoad(this);
	}

	// Use this for initialization
	void Start () {
		//Set auth reference
		auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void register(string email, string password){
		//If inputted email and password is correct
		if (firebaseReady) {
			auth.CreateUserWithEmailAndPasswordAsync (email, password).ContinueWith (task => {
				if (task.IsCanceled) {
					Debug.LogError ("CreateUserWithEmailAndPasswordAsync was canceled.");
					return;
				}
				if (task.IsFaulted) {
					Debug.LogError ("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
					return;
				}

				// Firebase user has been created.
				Firebase.Auth.FirebaseUser newUser = task.Result; 
				warningText.GetComponent<Text>().text = "Sign up complete";
				warningText.SetActive(true);
				Debug.LogFormat ("Firebase user created successfully: {0} ({1})",
					newUser.DisplayName, newUser.UserId);
			});
		}
	}

	public void logIn(string email, string password){
		auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
			if (task.IsCanceled) {
				Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				warningText.GetComponent<Text>().text = "Invalid email or password";
				warningText.SetActive(true);
				Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
				return;
			}

			Firebase.Auth.FirebaseUser newUser = task.Result;
			Application.LoadLevel("scene_menu"); //Refresh menu scene when login successful
			Debug.LogFormat("User signed in successfully: {0} ({1})",
				newUser.DisplayName, newUser.UserId);
		});
	}

	public void logOut(){
		if(firebaseReady){
			auth.SignOut ();
			Application.LoadLevel("scene_menu"); //Refresh menu scene when logout
		}
	}

	public static string getUsername(){
		if (firebaseReady) {
			Firebase.Auth.FirebaseUser user = auth.CurrentUser;
			if (user != null)
				return user.DisplayName;
			else
				return null;
		}
		return null;
	}

	public static string getUserEmail(){
		if (firebaseReady) {
			Firebase.Auth.FirebaseUser user = auth.CurrentUser;
			if (user != null)
				return user.Email;
			else
				return "";
		}
		return null;
	}

	public static string getUserID(){
		if (firebaseReady) {
			Firebase.Auth.FirebaseUser user = auth.CurrentUser;
			if (user != null)
				return user.UserId;
			else
				return null;
		}
		return null;
	}

	public static bool getLoginState(){
		if (firebaseReady) {
			Firebase.Auth.FirebaseUser user = auth.CurrentUser;
			return user != null;
		}
		return false;
	}

	public void accountBtn(){
		Firebase.Auth.FirebaseUser user = auth.CurrentUser;

		if (user == null)
			loginPanel.SetActive (true);
		else {
			loadLogoutPanel ();
			logoutPanel.SetActive (true);
		}
	}

	public void loadLogoutPanel(){
		emailText.GetComponent<Text> ().text = "Email: " + getUserEmail ();
	}

	public void signInBtn(){
		string email = emailTextBox.GetComponent<InputField> ().text;
		string password = passwordTextBox.GetComponent<InputField> ().text;

		logIn (email, password);
	}

	public void signUpBtn(){
		string email = emailTextBox.GetComponent<InputField> ().text;
		string password = passwordTextBox.GetComponent<InputField> ().text;

		if (false) {
			warningText.GetComponent<Text> ().text = "Invalid email";
			warningText.SetActive (true);
			return;
		}

		if (password.Length < 6) {
			warningText.GetComponent<Text> ().text = "Password's length must be over 6 digits";
			warningText.SetActive (true);
			return;
		}

		register (email, password);
	}
}

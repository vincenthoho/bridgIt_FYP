using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class playerData : MonoBehaviour
{
	public static string FILE_PATH;

	void Awake()
	{
		Singleton = this;

		DontDestroyOnLoad(this);

		FILE_PATH = Application.persistentDataPath + "/profile.txt";
		Load();
	}

	public static playerData Singleton { get; private set; }

	//public int LoginTime { get; private set; }
	public int clearedLevel { get; set; }

	private void Load()
	{
		Debug.Log (Application.persistentDataPath);
		if (!File.Exists(FILE_PATH))
		{
			Debug.Log("Player data not find");
			UpdateValues (1);
			return;
		}

		string[] splitProfile = File.ReadAllText(FILE_PATH).Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		clearedLevel = int.Parse(splitProfile[0]);
	}

	public void UpdateValues(int clearedLevel)
	{
		File.WriteAllText(FILE_PATH, string.Format("{0}", clearedLevel));
		Load();
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class character : MonoBehaviour {
	
	private static Transform[,] blueNodeArray;
	private static Transform[,] redNodeArray;
	protected static Vector3[] bonusCharacterPos;
	//private static ArrayList path;
	private static ArrayList path;
	private float speed = 300.0f;
	public static bool startMove = false;
	public static bool endMove = false;
	private static int index = 0;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (startMove && index < path.Count) {
			timer.setTimer (false);
			Vector3 nodePos = (Vector3)path [index];
			if (index == 0) {
				GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (nodePos.x, nodePos.y, nodePos.z);
				index++;
			} else {
				if (GetComponent<RectTransform> ().anchoredPosition3D != nodePos) {
					GetComponent<RectTransform> ().anchoredPosition3D = Vector3.MoveTowards (GetComponent<RectTransform> ().anchoredPosition3D, nodePos, Time.deltaTime * speed);
				} else
					index++;
			}
		} else if (startMove && index >= path.Count) {
			endMove = true;
		}
	}

	public static void setEnd(){
		endMove = true;
	}
	public static void setBlueNodeArray(Transform[,] array){
		blueNodeArray = array;
	}

	public static void setRedNodeArray(Transform[,] array){
		redNodeArray = array;
	}

	public static void setBonusCharacterArray(){
		bonusCharacterPos = new Vector3[blueEdgeRespond.bonusNode.Count];
		for (int i = 0; i < blueEdgeRespond.bonusNode.Count; i++) {
			int size = aiMode_init.nodeNo + 4;
			int rowIndex = (int)(blueEdgeRespond.bonusNode[i]) / size;
			int colIndex = (int)(blueEdgeRespond.bonusNode[i]) % size;
			bonusCharacterPos[i] = blueNodeArray [rowIndex, colIndex].GetComponent<RectTransform>().anchoredPosition;
		}
	}

	public static void winAnimation(ArrayList p){
		path = p;
		startMove = true;
	}

	public static void reset(){
		endMove = false;
		startMove = false;
		index = 0;
	}
}

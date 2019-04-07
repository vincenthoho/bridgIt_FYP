using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class multi_init : MonoBehaviour {
	public GameObject redNodePrefab;
	public GameObject redEdgePrefab, redEdgePrefab_ver;
	public GameObject blueNodePrefab;
	public GameObject blueEdgePrefab, blueEdgePrefab_ver;
	public GameObject blueEdgeParent_tmp, redEdgeParent_tmp;
	public GameObject ch;
	public GameObject en;
	public GameObject blueWinText;
	public GameObject redWinText;
	public GameObject spentTimeText;
	public GameObject tmp_turnText;
	public GameObject bridgeText_tmp, remainBridgeText_tmp;
	public GameObject homeButton;
	public GameObject instructionButton;
	public GameObject replayButton;
	public GameObject UIbar;
	public Canvas canvas;
	public GameObject UIpanel;
	public AudioClip winClaps;
	public AudioClip failSE;

	private float verticalGap;
	private float horizonalGap;
	Camera cam;
	public Transform[,] redEdgeRowArray = new Transform[5,5];
	public Transform[,] redEdgeColArray = new Transform[4,6];
	public Transform[,] blueEdgeRowArray = new Transform[6,4];
	public Transform[,] blueEdgeColArray = new Transform[5,5];
	public Transform[,] blueNodeArray = new Transform[6, 7];
	public Transform[,] redNodeArray = new Transform[7, 6];

	public static int level = 1;
	public static int score = 0;
	public static int totalLength = 3;
	public static int maxRow = 2;
	public static int maxCol = 3;
	public static int retries = 3;
	public static int blueBridges = 10;
	public static int redBridges = 10;
	public static int maxBridges = 0;
	public static bool limitBridge = false;
	public static GameObject turntext, retrytext, bridgeText, remainBridgeText, blueEdgeParent, redEdgeParent;

	private static ArrayList aiMoves = new ArrayList();
	
	// Use this for initialization
	void Start () {
		cam = GetComponent<Camera> ();
		RectTransform CanvasRect = canvas.GetComponent<RectTransform> ();
		blueEdgeParent = blueEdgeParent_tmp;
		redEdgeParent = redEdgeParent_tmp;
		turntext = tmp_turnText;
		remainBridgeText = remainBridgeText_tmp;
		bridgeText = bridgeText_tmp;

		retries = 3;

		//set UI bar to the screen top
		float x = UIbar.transform.GetComponent<RectTransform> ().anchoredPosition.x;
		float y = (CanvasRect.rect.height / 2) - 34.8f;
		UIbar.transform.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (x,y);

		redEdgeRespond.reset ();
		blueEdgeRespond.reset ();
		character.reset ();
		enemy.reset ();

		blueWinText.SetActive(false);
		redWinText.SetActive (false);
		UIpanel.SetActive (false);

		//set usable bridges
		if (limitBridge) {
			blueBridges = maxBridges;
			redBridges = maxBridges;
			bridgeText.SetActive (true);
			remainBridgeText.SetActive (true);
			remainBridgeText.GetComponent<Text> ().text = maxBridges.ToString ();
		}else
			bridgeText.SetActive (false);

		int nodeNo = level-1;
		if (level > 2) 
			nodeNo = 1;

		totalLength = 3+nodeNo+1;
		maxRow = 2+nodeNo+1;
		maxCol = 3+nodeNo+1;

		verticalGap = 200f;
		horizonalGap = 120f;

		int i,j;

		//create red and blue edges implicitly
		redEdgeRowArray [0, 0] = redEdgePrefab.transform;
		blueEdgeRowArray [0, 0] = blueEdgePrefab.transform;

		Vector3 redEdgePos = redEdgePrefab.GetComponent<RectTransform> ().anchoredPosition3D;
		redEdgePrefab.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (redEdgePos.x - (nodeNo * 0.5f * verticalGap), redEdgePos.y + (nodeNo * 0.5f * horizonalGap), redEdgePos.z);
		redEdgePos = redEdgePrefab.GetComponent<RectTransform> ().anchoredPosition3D;
		Vector3 redEdgePos_ver = redEdgePrefab_ver.GetComponent<RectTransform> ().anchoredPosition3D;
		redEdgePrefab_ver.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3(redEdgePos_ver.x - (nodeNo * 0.5f * verticalGap), redEdgePos_ver.y + (nodeNo * 0.5f * horizonalGap), 0);
		redEdgePos_ver = redEdgePrefab_ver.GetComponent<RectTransform> ().anchoredPosition3D;
		Vector3 blueEdgePos = blueEdgePrefab.GetComponent<RectTransform> ().anchoredPosition3D;
		blueEdgePrefab.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (blueEdgePos.x - (nodeNo * 0.5f * verticalGap), blueEdgePos.y + (nodeNo * 0.5f * horizonalGap), blueEdgePos.z);
		blueEdgePos = blueEdgePrefab.GetComponent<RectTransform> ().anchoredPosition3D;
		Vector3 blueEdgePos_ver = blueEdgePrefab_ver.GetComponent<RectTransform> ().anchoredPosition3D;
		blueEdgePrefab_ver.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (blueEdgePos_ver.x - (nodeNo * 0.5f * verticalGap), blueEdgePos_ver.y + (nodeNo * 0.5f * horizonalGap), 0);
		blueEdgePos_ver = blueEdgePrefab_ver.GetComponent<RectTransform> ().anchoredPosition3D;

		for (i=0; i<3+(nodeNo+1); i++) {
			for (j=0; j<3+(nodeNo+1); j++) {
				//if (!(j == 0 && i == 0)) {
					//Vector3 newRedPos = new Vector3 (redEdgePos.x + j * 90, redEdgePos.y - i * 50, redEdgePos.z);
					Vector3 newRedPos = new Vector3 (redEdgePos.x + j * verticalGap, redEdgePos.y - i * horizonalGap, redEdgePos.z);
					//redEdgeRowArray [i, j] = Instantiate (redEdgePrefab, new Vector2 (redEdgePrefab.transform.position.x + j * 12, redEdgePrefab.position.y - i * 10), Quaternion.identity) as Transform;
					GameObject newRedEdge = Instantiate(redEdgePrefab, newRedPos, redEdgePrefab.transform.rotation);
					newRedEdge.transform.SetParent (redEdgePrefab.transform.parent, false);
					redEdgeRowArray[i,j] = newRedEdge.transform.GetChild(0).transform;
					newRedEdge.GetComponent<redEdgeRespond> ().setMode (0);
				//}
				Vector3 newBluePos = new Vector3 (blueEdgePos_ver.x + j * verticalGap, blueEdgePos_ver.y - i * horizonalGap, 0);
				//blueEdgeColArray[i,j] = Instantiate (blueEdgePrefab, new Vector2 (-24 + j * 12, 20 - i * 10), Quaternion.Euler(0,0,90)) as Transform;
				GameObject newBlueEdge = Instantiate(blueEdgePrefab_ver, newBluePos, Quaternion.Euler(0,0,90));
				newBlueEdge.transform.SetParent (blueEdgePrefab.transform.parent, false);
				blueEdgeColArray[i,j] = newBlueEdge.transform.GetChild(0).transform;
				newBlueEdge.GetComponent<blueEdgeRespond> ().setMode (0);
				
				//blueEdgeColArray[i,j].GetComponent<Renderer>().enabled = false;
				//redEdgeRowArray[i,j].GetComponent<Renderer>().enabled = false;
				changeAlpha(0, blueEdgeColArray[i,j]);
				changeAlpha(0, redEdgeRowArray [i, j]);

				contactBlueEdge(blueEdgeColArray[i,j],i,j,"Col");
				contactRedEdge (redEdgeRowArray [i, j], i, j, "Row");
			}
		}

		for (i=0; i<2+(nodeNo+1); i++) {
			for (j=0; j<4+(nodeNo+1); j++) {
				//redEdgeColArray [i, j] = Instantiate (redEdgePrefab, new Vector2 (-30 + j * 12, 15 - i * 10), Quaternion.Euler(0,0,90)) as Transform;
				GameObject newRedEdge = Instantiate(redEdgePrefab_ver, new Vector3(redEdgePos_ver.x + j * verticalGap, redEdgePos_ver.y - i * horizonalGap, 0), Quaternion.Euler(0,0,90));
				newRedEdge.transform.SetParent (redEdgePrefab.transform.parent, false);
				redEdgeColArray[i,j] = newRedEdge.transform.GetChild(0).transform;
				newRedEdge.GetComponent<redEdgeRespond> ().setMode (0);
				//redEdgeColArray [i, j].GetComponent<Renderer>().enabled = false;
				//if (!(j==0 && i == 0)){
					//blueEdgeRowArray [j,i] = Instantiate (blueEdgePrefab, new Vector2 (blueEdgePrefab.position.x + i * 12, blueEdgePrefab.position.y - j * 10), Quaternion.identity) as Transform;
					Vector3 newBluePos = new Vector3(blueEdgePos.x + i * verticalGap, blueEdgePos.y - j * horizonalGap, blueEdgePos.z);
					GameObject newBlueEdge = Instantiate (blueEdgePrefab, newBluePos, Quaternion.identity);
					newBlueEdge.transform.SetParent (blueEdgePrefab.transform.parent, false);
					blueEdgeRowArray [j, i] = newBlueEdge.transform.GetChild(0).transform;
					newBlueEdge.GetComponent<blueEdgeRespond> ().setMode (0);
				//}
				//blueEdgeRowArray [j,i].GetComponent<Renderer>().enabled = false;
				if (j == 0 || j == 4 + nodeNo) {
					changeAlpha (255, blueEdgeRowArray [j, i]);
					changeAlpha (255, redEdgeColArray [i, j]);
				} else {
					changeAlpha (0, blueEdgeRowArray [j, i]);
					changeAlpha (0, redEdgeColArray [i, j]);
				}
				
				contactBlueEdge(blueEdgeRowArray[j,i],j,i,"Row");
				contactRedEdge (redEdgeColArray [i, j], i, j, "Col");
			}
		}

		Vector3 redNodePos = redNodePrefab.GetComponent<RectTransform> ().anchoredPosition3D;
		redNodePrefab.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (redNodePos.x - (nodeNo * 0.5f * verticalGap), redNodePos.y + (nodeNo * 0.5f * horizonalGap), redNodePos.z);
		redNodePos = redNodePrefab.GetComponent<RectTransform> ().anchoredPosition3D;
		redNodePrefab.transform.SetAsLastSibling ();
		redNodeArray [0, 0] = redNodePrefab.transform;
		Vector3 blueNodePos = blueNodePrefab.GetComponent<RectTransform> ().anchoredPosition3D;
		blueNodePrefab.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (blueNodePos.x - (nodeNo * 0.5f * verticalGap), blueNodePos.y + (nodeNo * 0.5f * horizonalGap), blueNodePos.z);
		blueNodePos = blueNodePrefab.GetComponent<RectTransform> ().anchoredPosition3D;
		blueNodePrefab.transform.SetAsLastSibling ();
		blueNodeArray [0, 0] = blueNodePrefab.transform;

		//Debug.Log ("Original red node pos: " + redNodePrefab.transform.position.x + ", " + redNodePrefab.transform.position.y + ", " + redNodePrefab.transform.position.z);
		//Debug.Log ("Original red node rect pos: " + redNodePos.x + ", " + redNodePos.y + ", " + redNodePos.z);
		//create red nodes
		for (i = 0; i <= 2+(nodeNo+1); i++) {
			for (j = 0; j <=3+(nodeNo+1); j++) {
				if (!(j == 0 && i == 0)) {
					//Instantiate (redNodePrefab, new Vector2 (redNodePrefab.position.x + j * 12, redNodePrefab.position.y - i * 10), Quaternion.identity);
					Vector3 newPos = new Vector3 (redNodePos.x + j * verticalGap, redNodePos.y - i * horizonalGap, redNodePos.z);
					//Debug.Log ("New node " + i + "," + j + ": " + newPos.x + ", " + newPos.y + ", " + newPos.z);
					GameObject newRedNode = Instantiate (redNodePrefab, newPos, redNodePrefab.transform.rotation);
					newRedNode.transform.SetParent (redNodePrefab.transform.parent, false);
					newRedNode.GetComponent<RectTransform> ().anchoredPosition = newPos;
					redNodeArray [i, j] = newRedNode.transform;
				}
			}
		}
		//create blue nodes
		for (i = 0; i <= 3+(nodeNo+1);i++)
		{
			for (j = 0; j <=2+(nodeNo+1); j++)
			{
				if (!(j==0 && i == 0)){
					//Instantiate(blueNodePrefab,new Vector2(blueNodePrefab.transform.position.x + j*12,blueNodePrefab.transform.position.y - i*10),Quaternion.identity);
					Vector3 newPos = new Vector3 (blueNodePos.x + j * verticalGap, blueNodePos.y - (i * horizonalGap), blueNodePos.z);
					//Debug.Log ("New node " + i + "," + j + ": " + newPos.x + ", " + newPos.y + ", " + newPos.z);
					GameObject newBlueNode = Instantiate (blueNodePrefab, newPos, blueNodePrefab.transform.rotation);
					newBlueNode.transform.SetParent (blueNodePrefab.transform.parent, false);
					newBlueNode.GetComponent<RectTransform> ().anchoredPosition = newPos;
					blueNodeArray [i, j] = newBlueNode.transform;
				}	
			}
		}

		blueEdgeRespond.setNodeArray (blueNodeArray);
		redEdgeRespond.setNodeArray (redNodeArray);

		//Set all blue edge and red edge's parent
		GameObject[] tmp_BlueEdge = GameObject.FindGameObjectsWithTag("BlueEdge");
		GameObject[] tmp_RedEdge = GameObject.FindGameObjectsWithTag ("RedEdge");

		foreach (GameObject g in tmp_BlueEdge)
			g.transform.SetParent (blueEdgeParent.transform);
		foreach (GameObject g in tmp_RedEdge)
			g.transform.SetParent (redEdgeParent.transform);

		ch.transform.SetAsLastSibling ();
		en.transform.SetAsLastSibling ();

		blueEdgeRespond.raiseTurn ();
	}

	private void changeAlpha(int value, Transform go){
		Color temp = go.GetComponent<Image> ().color;
		temp.a = value;
		go.GetComponent<Image> ().color = temp;
	}

	public static void setLimitBridges(int bridgesNo){
		if (bridgesNo == 0)
			return;

		limitBridge = true;
		maxBridges = bridgesNo;
	}

	public static void setLimitMoves(){
	}


	public static void useBlueBridge(){
		blueBridges--;
		remainBridgeText.GetComponent<Text> ().text = "" + blueBridges;
		if (blueBridges == 0)
			remainBridgeText.GetComponent<Text> ().color = new Color32(0xED, 0x1C, 0x24, 0xFF); //red
		else
			remainBridgeText.GetComponent<Text> ().color = new Color32(0x82, 0x82, 0x82, 0xFF); //grey
	}

	public static void removeBlueBridge(){
		blueBridges++;
		remainBridgeText.GetComponent<Text> ().text = "" + blueBridges;
		if (blueBridges != 0)
			remainBridgeText.GetComponent<Text> ().color = new Color32(0x82, 0x82, 0x82, 0xFF); //grey
	}

	public static void useRedBridge(){
		redBridges--;
		remainBridgeText.GetComponent<Text> ().text = "" + redBridges;
		if (blueBridges == 0)
			remainBridgeText.GetComponent<Text> ().color = new Color32(0xED, 0x1C, 0x24, 0xFF); //red
		else
			remainBridgeText.GetComponent<Text> ().color = new Color32(0x82, 0x82, 0x82, 0xFF); //grey
	}

	public static void removeRedBridge(){
		redBridges++;
		remainBridgeText.GetComponent<Text> ().text = "" + redBridges;
		if (blueBridges != 0)
			remainBridgeText.GetComponent<Text> ().color = new Color32(0x82, 0x82, 0x82, 0xFF); //grey
	}

	public static void switchTurn(string s){
		switch (s) {
			case "ai":
				turntext.GetComponent<Text> ().text = "Red's turn...";
				turntext.GetComponent<Text> ().color = new Color32 (0xED, 0x1C, 0x24, 0xFF); //ED1C24FF red
				bridgeText.GetComponent<Text> ().color = new Color32 (0xED, 0x1C, 0x24, 0xFF); //ED1C24FF red
				remainBridgeText.GetComponent<Text>().color = new Color32 (0xED, 0x1C, 0x24, 0xFF); //ED1C24FF red
				remainBridgeText.GetComponent<Text>().text = redBridges.ToString();
				blueEdgeParent.transform.SetSiblingIndex (0);
				redEdgeParent.transform.SetSiblingIndex (1);
				break;
			case "player":
				turntext.GetComponent<Text> ().text = "Blue's turn..."; 
				turntext.GetComponent<Text> ().color = new Color32(0x00, 0xA2, 0xE8, 0xFF); //00A2E8FF blue
				bridgeText.GetComponent<Text> ().color = new Color32(0x00, 0xA2, 0xE8, 0xFF); //00A2E8FF blue
				remainBridgeText.GetComponent<Text>().color = new Color32(0x00, 0xA2, 0xE8, 0xFF); //00A2E8FF blue
				remainBridgeText.GetComponent<Text>().text = blueBridges.ToString();
				blueEdgeParent.transform.SetSiblingIndex (1);
				redEdgeParent.transform.SetSiblingIndex (0);
				break;
		}
	}

	private void contactBlueEdge(Transform edge, int row, int col, string role){
		Debug.Log ("Contact edge " + row + ", " + col);
		blueEdgeRespond temp = (blueEdgeRespond)edge.GetComponent (typeof(blueEdgeRespond));
		temp.setRole (role);
		temp.setRow (row);
		temp.setCol (col);
	}

	private void contactRedEdge(Transform edge, int row, int col, string role){
		Debug.Log ("Contact edge " + row + ", " + col);
		redEdgeRespond temp = (redEdgeRespond)edge.GetComponent (typeof(redEdgeRespond));
		temp.setRole (role);
		temp.setRow (row);
		temp.setCol (col);
	}

	public void disableButtons(){
		homeButton.GetComponent<Button> ().interactable = false;
		instructionButton.GetComponent<Button> ().interactable = false;
	}

	public void endgame(string winner){
		if (character.endMove || enemy.endMove) {
			UIpanel.SetActive (true);
		
			if (winner.Equals ("blue")) {
				AudioSource.PlayClipAtPoint (winClaps, Camera.main.transform.position);
				blueWinText.SetActive (true);
				spentTimeText.SetActive (true);
				spentTimeText.GetComponent<Text> ().text = "Time used: " + timer.getTime() + " seconds";
				replayButton.SetActive (true);
			} else if (winner.Equals ("red")) {
				AudioSource.PlayClipAtPoint (winClaps, Camera.main.transform.position);
				redWinText.SetActive (true);
				spentTimeText.SetActive (true);
				spentTimeText.GetComponent<Text> ().text = "Time used: " + timer.getTime() + " seconds";
				replayButton.SetActive (true);
			}
		}
	}

	public void reset(){
		level = 1;
		score = 0;
		retries = 3;
		limitBridge = false;
	}
}

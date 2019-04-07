using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Globalization;
using UnityEngine.SceneManagement;

public class blueEdgeRespond : MonoBehaviour {
	private static bool myTurn;
	private static bool[,] blockedRow = new bool[6,4]; //default as false
	private static bool[,] blockedCol = new bool[5,5];
	private static bool[,] visitRow = new bool[6,4];
	private static bool[,] visitCol = new bool[5,5];
	public static ArrayList blueMoves = new ArrayList();
	public static ArrayList checkmatePath = new ArrayList ();
	private static Transform[,] blueNodeArray;
	public static ArrayList bonusNode = new ArrayList();
    public static ArrayList erasedNode = new ArrayList();
	public static int stepCount;
	public static int moveCount;
	public static int collectedBonusNode = 0;//total no. of bonus node connected at last
	public static int cmX; public static int cmY;
	public static bool checkmate = false;
	public static bool checkmateCol = false;
	public static bool skipLeft = false;
	public static bool skipUp = false;
	//private static bool clicked = false;
	private int mode;
	private int row;
	private int col;
	private string role;
	private static bool checkWin = false;

	public GameObject ch;
	public GameObject warningText;
	public AudioClip pressSE;
	
	// Use this for initialization
	void Awake () {
		row = -1;
		col = -1;
		role = "";
		stepCount = 0;
		moveCount = 0;
		//clicked = false;
		blueMoves  = new ArrayList();
		/*
		for (int i = 0; i < 4; i++)
			visitRow [0, i] = true;
		for (int i = 0; i < 4; i++)
			visitRow [aiMode_init.maxCol, i] = true;
			*/
	}

	public static bool[,] getVisitRow(){
		return visitRow;
	}

	public static bool[,] getVisitCol(){
		return visitCol;
	}

	public static void setNodeArray(Transform[,] array){
		blueNodeArray = array;
	}

	private static void changeAlpha(int value, Transform go){
		Color temp = go.GetComponent<Image> ().color;
		temp.a = value;
		go.GetComponent<Image> ().color = temp;
	}

	public void ClickEvent(){
		this.transform.GetChild (0).transform.GetComponent<blueEdgeRespond> ().OnMouseDownEvent ();
	}

	public static void redo(){
        if (blueMoves.Count - 1 >= 0)
        {
            Edge e = (Edge)blueMoves[blueMoves.Count - 1];
            //Debug.Log("<color=yellow>Redo Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
            if (e.getTransform().GetComponent<Image>().color.a == 255)
            {
                Debug.Log("<color=yellow>Redo Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
                changeAlpha(0, e.getTransform());
                if (e.getType() == "Row")
                {
                    shannon.setResistance(aiMode_init.maxCol * aiMode_init.maxCol + (aiMode_init.maxCol - 1) * (e.getRow() - 1) + e.getCol(), 1);
                    visitRow[e.getRow(), e.getCol()] = false;
                    stepCount--;
                    if (e.getRow() != 0 && e.getRow() != 5)
                        aiRespond.unblockEdge(e.getRow() - 1, e.getCol() + 1, "Col");
                    if (aiMode_init.nodesErased)
                    {
                        foreach (int erasedNodeNo in erasedNode)
                        {
                            if (e.getRow() == erasedNodeNo / (aiMode_init.nodeNo + 4)+1)
                            {
                                if (e.getCol() == erasedNodeNo % (aiMode_init.nodeNo + 4)-1)
                                {
                                    blueMoves.Remove(e);
                                    if ((visitRow[e.getRow(), e.getCol()] == false && visitRow[e.getRow(), e.getCol()+1] == false))
                                    {
                                        return;
                                    }
                                    redo();
                                }
                                else if (e.getCol() == erasedNodeNo % (aiMode_init.nodeNo + 4))
                                {
                                    blueMoves.Remove(e);
                                    if ((visitRow[e.getRow(), e.getCol()] == false && visitRow[e.getRow(), e.getCol()-1] == false))
                                    {
                                        return;
                                    }
                                    redo();
                                }
                            }
                        }
                    }
                }
                else
                {
                    shannon.setResistance(e.getRow() * aiMode_init.maxCol + e.getCol(), 1);
                    visitCol[e.getRow(), e.getCol()] = false;
                    stepCount--;
                    if (e.getRow() != 0 && e.getRow() != 5)
                        aiRespond.unblockEdge(e.getRow(), e.getCol(), "Row");
                    if (aiMode_init.nodesErased)
                    {
                        foreach (int erasedNodeNo in erasedNode)
                        {
                            if (e.getCol() == erasedNodeNo % (aiMode_init.nodeNo + 4))
                            {
                                if (e.getRow() == erasedNodeNo / (aiMode_init.nodeNo + 4))
                                {
                                    Debug.Log("<color=red>Redo 1 Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
                                    blueMoves.Remove(e);
                                    if ((visitCol[e.getRow(), e.getCol()] == false && visitCol[e.getRow() + 1, e.getCol()] == false)) {
                                        Debug.Log("<color=yellow>Redo 1 Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
                                        return;
                                    }
                                    redo();
                                }
                                else if (e.getRow() == erasedNodeNo / (aiMode_init.nodeNo + 4) + 1) {
                                    Debug.Log("<color=red>Redo 2 Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
                                    blueMoves.Remove(e);
                                    if ((visitCol[e.getRow(), e.getCol()] == false && visitCol[e.getRow() - 1, e.getCol()] == false))
                                    {
                                        Debug.Log("<color=yellow>Redo 1 Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
                                        return;
                                    }
                                    redo();
                                }
                            }
                        }
                    }
                }
                if (aiMode_init.limitMoves)
                    moveCount++;
                if (aiMode_init.limitBridges)
                    aiMode_init.removeBlueBridge();
                aiRespond.redo();
            }
            else
            {
                changeAlpha(255, e.getTransform());
                if (e.getType() == "Row")
                {
                    shannon.setResistance(aiMode_init.maxCol * aiMode_init.maxCol + (aiMode_init.maxCol - 1) * (e.getRow() - 1) + e.getCol(), 0);
                    visitRow[e.getRow(), e.getCol()] = true;
                    stepCount++;
                    if (e.getRow() != 0 && e.getRow() != 5)
                        aiRespond.blockEdge(e.getRow() - 1, e.getCol() + 1, "Col");
                    if (aiMode_init.nodesErased)
                    {
                        foreach (int erasedNodeNo in erasedNode)
                        {
                            if (e.getRow() == erasedNodeNo / (aiMode_init.nodeNo + 4) + 1)
                            {
                                if (e.getCol() == erasedNodeNo % (aiMode_init.nodeNo + 4) - 1)
                                {
                                    blueMoves.Remove(e);
                                    if ((visitRow[e.getRow(), e.getCol()] == true && visitRow[e.getRow(), e.getCol() + 1] == true))
                                    {
                                        return;
                                    }
                                    redo();
                                }
                                else if (e.getCol() == erasedNodeNo % (aiMode_init.nodeNo + 4))
                                {
                                    blueMoves.Remove(e);
                                    if ((visitRow[e.getRow(), e.getCol()] == true && visitRow[e.getRow(), e.getCol() - 1] == true))
                                    {
                                        return;
                                    }
                                    redo();
                                }
                            }
                        }
                    }
                }
                else
                {
                    shannon.setResistance(e.getRow() * aiMode_init.maxCol + e.getCol(), 0);
                    visitCol[e.getRow(), e.getCol()] = true;
                    stepCount++;
                    if (e.getRow() != 0 && e.getRow() != 5)
                        aiRespond.blockEdge(e.getRow(), e.getCol(), "Row");
                    if (aiMode_init.nodesErased)
                    {
                        foreach (int erasedNodeNo in erasedNode)
                        {
                            if (e.getCol() == erasedNodeNo % (aiMode_init.nodeNo + 4))
                            {
                                if (e.getRow() == erasedNodeNo / (aiMode_init.nodeNo + 4))
                                {
                                    Debug.Log("<color=red>Redo 1 Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
                                    blueMoves.Remove(e);
                                    if ((visitCol[e.getRow(), e.getCol()] == true && visitCol[e.getRow() + 1, e.getCol()] == true))
                                    {
                                        Debug.Log("<color=yellow>Redo 1 Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
                                        return;
                                    }
                                    redo();
                                }
                                else if (e.getRow() == erasedNodeNo / (aiMode_init.nodeNo + 4) + 1)
                                {
                                    Debug.Log("<color=red>Redo 2 Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
                                    blueMoves.Remove(e);
                                    if ((visitCol[e.getRow(), e.getCol()] == true && visitCol[e.getRow() - 1, e.getCol()] == true))
                                    {
                                        Debug.Log("<color=yellow>Redo 1 Row:" + e.getRow() + ", Col:" + e.getCol() + "</color>");
                                        return;
                                    }
                                    redo();
                                }
                            }
                        }
                    }
                }
                if (aiMode_init.limitBridges)
                    aiMode_init.useBlueBridge();
            }
            blueMoves.Remove(e);
        }
	}

    public void putEdge(int row, int col, string role, Transform edge){
        Debug.Log("<color=yellow>putEdge row: " + row + " col: " + col + "</color>");
        if (role == "Row"){
            if (blockedRow[row, col] == false){

                if (aiMode_init.nodesErased)
                {
                    foreach (int erasedNodeNo in erasedNode)
                    {

                        if (row == erasedNodeNo / (aiMode_init.nodeNo + 4) + 1)
                        {
                            if (col == erasedNodeNo % (aiMode_init.nodeNo + 4) - 1)
                            {
                                if (col + 1 > aiMode_init.nodeNo + 2 || blockedRow[row, col + 1] == true || (visitCol[row - 1, col + 1] == true && visitCol[row, col + 1] == true))
                                    return;
                            }
                            else if (col == erasedNodeNo % (aiMode_init.nodeNo + 4))
                            {
                                if (col - 1 < 0 || blockedRow[row, col-1] == true || (visitCol[row - 1, col] == true && visitCol[row, col] == true))
                                    return;
                            }
                        }
                    }
                }

                visitRow[row, col] = true;  //as a visit
                stepCount++;
                if (aiMode_init.limitMoves){
                    moveCount++;
                    aiMode_init.updateMoves();
                }
                changeAlpha(255, edge);//display edge
                int size = aiMode_init.maxCol;
                Debug.Log("set resistance at " + (size * size + (size - 1) * (row - 1) + col));
                shannon.setResistance(size * size + (size - 1) * (row - 1) + col, 0);
                blueMoves.Add(new Edge(row, col, role, edge));
                checkmatePath = checkCheckmate();

                //block red side edge
                if (row != 0 && row != 5){
                    redEdgeRespond.blockEdge(row - 1, col + 1, "Col");
                    aiRespond.blockEdge(row - 1, col + 1, "Col");
                }

                if (SceneManager.GetActiveScene().name == "scene_ai" && aiMode_init.limitBridges)
                    aiMode_init.useBlueBridge();
                else if (SceneManager.GetActiveScene().name == "scene_multi" && multi_init.limitBridge)
                    multi_init.useBlueBridge();

                if (aiMode_init.nodesErased)
                {
                    foreach (int erasedNodeNo in erasedNode)
                    {

                        if (row == erasedNodeNo / (aiMode_init.nodeNo + 4)+1)
                        {
                            if (col == erasedNodeNo % (aiMode_init.nodeNo + 4) - 1)
                            {
                                if (visitRow[row, col + 1] != true)
                                {
                                    putEdge(row, col + 1, this.role, aiMode_init.blueEdgeRowArray[row, col + 1]);
                                    return;
                                }
                            }
                            else if (col == erasedNodeNo % (aiMode_init.nodeNo + 4))
                            {
                                if (visitRow[row, col - 1] != true)
                                {
                                    putEdge(row, col - 1, this.role, aiMode_init.blueEdgeRowArray[row, col - 1]);
                                    return;
                                }
                            }
                        }
                    }
                }

                //Switch turn according to the mode -> switch to AI or Another Player
                if (SceneManager.GetActiveScene().name == "scene_multi"){
                    myTurn = false;
                    multi_init.switchTurn("ai");
                    if (!win())
                        redEdgeRespond.raiseTurn();
                }
                else if (SceneManager.GetActiveScene().name == "scene_ai")
                    StartCoroutine(changeToAI(row, col, role));

                //Check win
                if (win())
                    checkWin = true;
            }
        }
        else{
            if (blockedCol[row, col] == false){
                if (aiMode_init.nodesErased)
                {
                    foreach (int erasedNodeNo in erasedNode)
                    {

                        if (col == erasedNodeNo % (aiMode_init.nodeNo + 4))
                        {
                            if (row == erasedNodeNo / (aiMode_init.nodeNo + 4))
                            {
                                if (blockedCol[row + 1, col] == true || (visitRow[row+1, col-1] == true && visitRow[row+1, col] == true))
                                    return;
                            }
                            else if (row == erasedNodeNo / (aiMode_init.nodeNo + 4) + 1)
                            {
                                if (blockedCol[row - 1, col] == true || (visitRow[row, col - 1] == true && visitRow[row, col] == true))
                                    return;
                            }
                        }
                    }
                }

                visitCol[row, col] = true;  //as a visit
                stepCount++;
                if (SceneManager.GetActiveScene().name == "scene_ai" && aiMode_init.limitMoves){
                    moveCount++;
                    aiMode_init.updateMoves();
                }
                changeAlpha(255, edge);
                Debug.Log("set resistance at " + (row * aiMode_init.maxCol + col));
                shannon.setResistance(row * aiMode_init.maxCol + col, 0);
                blueMoves.Add(new Edge(row, col, role, edge));
                checkmatePath = checkCheckmate();

                //block red side edge
                redEdgeRespond.blockEdge(row, col, "Row");
                aiRespond.blockEdge(row, col, "Row");

                if (SceneManager.GetActiveScene().name == "scene_ai" && aiMode_init.limitBridges)
                    aiMode_init.useBlueBridge();
                else if (SceneManager.GetActiveScene().name == "scene_multi" && multi_init.limitBridge)
                    multi_init.useBlueBridge();

                if (aiMode_init.nodesErased){
                    foreach (int erasedNodeNo in erasedNode){
                        if (col == erasedNodeNo % (aiMode_init.nodeNo + 4)){
                            if (row == erasedNodeNo / (aiMode_init.nodeNo + 4))
                            {
                                if (visitCol[row + 1, col] != true)
                                {
                                    putEdge(row + 1, col, role, aiMode_init.blueEdgeColArray[row + 1, col]); return;
                                }
                            }
                            else if (row == erasedNodeNo / (aiMode_init.nodeNo + 4) + 1)
                            {
                                if (visitCol[row - 1, col] != true)
                                {
                                    putEdge(row - 1, col, role, aiMode_init.blueEdgeColArray[row - 1, col]);
                                    return;
                                }
                            }
                        }
                    }
                }

                //Switch turn according to the mode -> switch to AI or Another Player
                if (SceneManager.GetActiveScene().name == "scene_multi"){
                    myTurn = false;
                    multi_init.switchTurn("ai");
                    if (!win())
                        redEdgeRespond.raiseTurn();
                }
                else if (SceneManager.GetActiveScene().name == "scene_ai")
                    StartCoroutine(changeToAI(row, col, role));

                //Check win
                if (win())
                    checkWin = true;
            }
        }
    }

    public void removeEdge(int row, int col, string role, Transform edge){
        Edge e;
        if (this.role == "Row")
        {
            e = new Edge(row, col, "Row", edge);
            if (row != 0 && row != aiMode_init.totalLength)
            {
                shannon.setResistance(aiMode_init.maxCol * aiMode_init.maxCol + (aiMode_init.maxCol - 1) * (row - 1) + col, 1);
                visitRow[row, col] = false;
                stepCount--;
                if (row != 0 && row != 5){
                    aiRespond.unblockEdge(row - 1, col + 1, "Col");
                }
                changeAlpha(0, edge);
                if (aiMode_init.nodesErased){
                    foreach (int erasedNodeNo in erasedNode){
                        if (row == erasedNodeNo / (aiMode_init.nodeNo + 4)+1){
                            if (col == erasedNodeNo % (aiMode_init.nodeNo + 4)-1){
                                if (visitRow[row, col+1] == true)
                                    removeEdge(row, col+1, role, aiMode_init.blueEdgeRowArray[row, col+1]);
                            }
                            else if (col == erasedNodeNo % (aiMode_init.nodeNo + 4))
                                if (visitRow[row, col-1] == true)
                                    removeEdge(row, col-1, role, aiMode_init.blueEdgeRowArray[row, col-1]);
                        }
                    }
                }
            }
        }
        else{
            shannon.setResistance(row * aiMode_init.maxCol + col, 1);
            e = new Edge(row, col, "Col", edge);
            visitCol[row, col] = false;
            stepCount--;
            aiRespond.unblockEdge(row, col, "Row");
            changeAlpha(0, edge);
            if (aiMode_init.nodesErased) {
                foreach (int erasedNodeNo in erasedNode){
                    if (col == erasedNodeNo % (aiMode_init.nodeNo + 4)){
                        if (row == erasedNodeNo / (aiMode_init.nodeNo + 4)){
                            if (visitCol[row + 1, col] == true)
                                removeEdge(row + 1, col, role, aiMode_init.blueEdgeColArray[row + 1, col]);
                        }else if (row == erasedNodeNo / (aiMode_init.nodeNo + 4) + 1)
                            if (visitCol[row - 1, col] == true)
                                removeEdge(row - 1, col, role, aiMode_init.blueEdgeColArray[row - 1, col]);
                    }
                }
            }
        }
        blueMoves.Remove(e);
        blueMoves.Add(e);
        if (SceneManager.GetActiveScene().name == "scene_multi")
            multi_init.removeBlueBridge();
        else if (SceneManager.GetActiveScene().name == "scene_ai")
            aiMode_init.removeBlueBridge();
    }
	
	public void OnMouseDownEvent(){
		
		Debug.Log (row + " " + col + " clicked blue");
		//If empty space is clicked
		if (myTurn && (this.GetComponent<Image> ().color.a == 0)) {
			//if no more bridge left
			if ((SceneManager.GetActiveScene().name == "scene_ai" && aiMode_init.limitBridges && aiMode_init.blueBridges == 0) || 
				(SceneManager.GetActiveScene().name == "scene_multi" && multi_init.limitBridge && multi_init.blueBridges == 0))
				StartCoroutine (showWarning ());
			else {
				AudioSource.PlayClipAtPoint (pressSE, Camera.main.transform.position);
                putEdge(row, col, this.role, this.transform);
            }
		}

		//If existing bridge is clicked
		if (myTurn && (this.GetComponent<Image> ().color.a == 255) && (aiMode_init.limitBridges || multi_init.limitBridge)) {
            //remove bridge
            removeEdge(row, col, this.role, this.transform);
		}
	}

	private IEnumerator showWarning(){
		warningText.SetActive (true);
		yield return new WaitForSeconds (1.5f);
		warningText.SetActive (false);
	}

	private IEnumerator changeToAI(int row, int col, string type){
		int second = Random.Range (1, 3);
		Debug.Log ("Waiting for " + second);
		myTurn = false;
		//clicked = false;
		aiMode_init.switchTurn ("ai");
		Debug.Log ("<color=green>switchTurn(ai)</color>");

		yield return null;
		yield return new WaitForSeconds(1);

		if (!win ()) {
			Debug.Log ("<color=green>log before raise turn</color>");
			Debug.Log ("<color=green>not win</color>");
			Debug.Log ("<color=green>raiseTurn</color>");
			aiRespond.raiseTurn (row, col, type);
		}else{
			checkWin = true;
		}
	}

	public ArrayList getCheckMate(){
		checkmate = findCheckmatePath ();
		if (checkmate) {
			ArrayList tmp = new ArrayList ();
			tmp.Add (cmX);tmp.Add (cmY);
			return tmp;
		}
		return null;
	}

	//Check if the player almost win
	//if True, block the user first before calculate with Shannon Heuristic
	public ArrayList checkCheckmate(){
		ArrayList pathList = new ArrayList ();
		checkmate = false;

		if (stepCount >= aiMode_init.totalLength-1) {
			//find start edge from the top
			for (int i = 0; i < aiMode_init.totalLength; i++){
				if(visitCol[0, i] ==  true){
					Coordination cord = new Coordination(0,i);
					/*
					pathList.Add(cord);
					checkmate = setPathArray (cord, pathList, aiMode_init.totalLength, aiMode_init.totalLength - 1, true);
					if (checkmate)
						return pathList;
					*/
					pathList.Add (cord);
					checkmate = findCheckmatePath (cord, pathList, aiMode_init.totalLength, aiMode_init.totalLength, false, new Coordination(-1,-1));
					if (checkmate)
						return pathList;
				}
			}

			//find start edge from the buttom
			for (int i = 0; i < aiMode_init.totalLength; i++){
				if(visitCol[aiMode_init.maxRow, i] ==  true){
					Coordination cord = new Coordination(aiMode_init.maxRow,i);
					/*
					pathList.Add(cord);
					checkmate = setPathArray (cord, pathList, aiMode_init.totalLength, 1, true);
					if (checkmate)
						return pathList;
					*/
					pathList.Add (cord);
					checkmate = findCheckmatePath (cord, pathList, aiMode_init.totalLength, 0, false, new Coordination(-1, -1));
					if (checkmate)
						return pathList;
				}
			}
		}
		return pathList;
	}

	private bool checkCoorEqual(ArrayList p, int x, int y){
		foreach (Coordination c in p) {
			if (c.getX () == x && c.getY () == y)
				return true;
		}
		return false;
	}

	public static int getCheckmateX(){
		if (checkmatePath.Count >= 1) {
			/*
			Vector3 cm = (Vector3)checkmatePath [blueEdgeRespond.checkmatePath.Count - 1];
			cm = new Vector3 (cm.x, cm.y-19.5f, cm.z);
			for (int i = 0; i < 6; i++) {
				for (int j = 0; j < 7; j++) {
					if (blueNodeArray [i, j] != null && blueNodeArray [i, j].GetComponent<RectTransform> ().anchoredPosition3D.Equals(cm))
						return i;
				}
			}
			*/
			Coordination c = (Coordination)checkmatePath [checkmatePath.Count - 1];
			//if (c.getX () == aiMode_init.totalLength)
			//	return aiMode_init.maxRow;
			//else
				return c.getX ();
			//return -1;
		} else
			return -1;
	}

	public static int getCheckmateY(){
		if (checkmatePath.Count >= 1) {
			/*
			Vector3 cm = (Vector3)checkmatePath [blueEdgeRespond.checkmatePath.Count - 1];
			cm = new Vector3 (cm.x, cm.y-19.5f, cm.z);
			for (int i = 0; i < 6; i++) {
				for (int j = 0; j < 7; j++) {
					if (blueNodeArray [i, j] != null && blueNodeArray [i, j].GetComponent<RectTransform> ().anchoredPosition3D.Equals(cm))
						return j;
				}
			}
			return -1;
			*/
			Coordination c = (Coordination)checkmatePath [checkmatePath.Count - 1];
			foreach(Coordination co in checkmatePath)
				Debug.Log(co.getX() +", "+ co.getY());
			return c.getY ();
		} else
			return -1;
	}

	void Update(){
		//if (checkWin) {
		if (checkWin && character.endMove) {
			timer.setTimer (false);
			Camera.main.SendMessage ("endgame", "blue");
			checkWin = false;
		}
	}

	/*
	private void changeToAIwithoutLag(int row, int col, string type){
		Debug.Log ("<color=green>Turn myTurn false</color>");
		myTurn = false;
		Debug.Log ("<color=green>switchTurn(ai)</color>");
		aiMode_init.switchTurn ("ai");
		if (!win ()) {
			Debug.Log ("<color=green>not win</color>");
			redEdgeRespond.raiseTurn ();
			aiRespond.raiseTurn (row, col, type, false);
		}
		Debug.Log ("<color=green>switchTurn(player)</color>");
		aiMode_init.switchTurn ("player");
	}
	*/

	public static void raiseTurn(){
		blueEdgeRespond.myTurn = true;
	}

	public void setRow(int row){
		this.row = row;
	}

	public void setCol(int col){
		this.col = col;
	}

	public void setRole(string role){
		this.role = role;
	}

	public void setMode(int mode){
		this.mode = mode;
	}

	public static void blockEdge(int row, int col, string role){
		if (role == "Row")
			blueEdgeRespond.blockedRow [row, col] = true;
		else
			blueEdgeRespond.blockedCol [row, col] = true;
	}

	public static void unblockEdge(int row, int col, string role){
		if (role == "Row")
			blueEdgeRespond.blockedRow [row, col] = false;
		else
			blueEdgeRespond.blockedCol [row, col] = false;
	}


	private bool win(){
		Debug.Log ("<color=green>check win</color>");
		bool won = false;
		ArrayList foundRowList;
		int length = 0;
		if (SceneManager.GetActiveScene ().name == "scene_multi")
			length = multi_init.totalLength;
		else if (SceneManager.GetActiveScene ().name == "scene_ai")
			length = aiMode_init.totalLength;
		
		if (stepCount >= length) {
			//find start edge
			for (int i = 0; i < length; i++){
				if(visitCol[0,i] ==  true){
					foundRowList = new ArrayList ();
					Coordination cord = new Coordination(0,i);
					foundRowList.Add(cord);
					//won = findNextEdge(0,i,1,foundRowList);
					won = setPathArray(cord, foundRowList, length, length, false);
					if (won) {
						//StartCoroutine(winAnimation (foundRowList));
						//ch.SetActive(true);
						Camera.main.SendMessage("disableButtons");

						ArrayList path = new ArrayList ();

						//set first path
						Coordination coor = (Coordination)foundRowList[0];
						addPath (coor.getX (), coor.getY (), path);
						addPath (coor.getX ()+1, coor.getY (), path);
						Coordination c = new Coordination (coor.getX () + 1, coor.getY ());
						setPathArray (c, path, length, length, false);
						if(aiMode_init.mustConnect)
							checkConnectBonusNode (path);
						character.winAnimation(path);

						break;
					}
				}
			}
		}
		Debug.Log ("<color=green>" + won + "</color>");
		return won;
	}

	//check if bonus node connected
	private void checkConnectBonusNode(ArrayList path){
		foreach (Vector3 o in path) {
			Vector3 bn = new Vector3 (o.x, o.y-19.5f, o.z);
			foreach(int nodeIndex in bonusNode){
				int size = aiMode_init.nodeNo + 4;
				int rowIndex = nodeIndex / size;
				int colIndex = nodeIndex % size;
				Vector3 np = blueNodeArray [rowIndex, colIndex].GetComponent<RectTransform>().anchoredPosition;
				if (np.Equals (bn))
					collectedBonusNode++;
			}
		}
	}

	//Function to add path for setPathArray()
	private void addPath(int x, int y, ArrayList p){
		Vector3 pos = blueNodeArray [x, y].GetComponent<RectTransform> ().anchoredPosition3D;
		p.Add (new Vector3 (pos.x, pos.y + 19.5f, pos.z));
		Debug.Log ("<color=blue> Added: x," + x + ", " + y + "</color>");		
	}

	//Find linked path start from c and fille the path to arraylist "path" 
	//bool checkCM -> check checkmate
	private bool setPathArray(Coordination c, ArrayList path ,int length, int end, bool checkCM){
		Debug.Log ("<color=yellow> Called with coor: x," + c.getX() + ", " + c.getY() + "</color>");

		//if path found
		bool pathFound = false;

		//if path finding finished
		if (c.getX () == end) {
			if (checkCM) {
				if(end == 1)
					return blockedCol[c.getX()-1,c.getY()] != true;
				else
					return blockedCol[c.getX(),c.getY()] != true;
			}else
				return true;
		}

		//find path upward
		if (!pathFound && c.getX () - 1 >= 0) {
			if (visitCol [c.getX () - 1, c.getY ()] == true) {
				Vector3 pos = blueNodeArray [c.getX()-1, c.getY()].GetComponent<RectTransform> ().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX () - 1, c.getY (), path);
					Coordination coor = new Coordination (c.getX () - 1, c.getY ());
					pathFound = setPathArray(coor, path, length, end, checkCM);
				}
			}
		}

		//find path downward
		if (!pathFound && c.getX () + 1 <= (length)) {
			if (visitCol [c.getX (), c.getY ()] == true) {
				Vector3 pos = blueNodeArray [c.getX()+1, c.getY()].GetComponent<RectTransform> ().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX () + 1, c.getY (), path);
					Coordination coor = new Coordination (c.getX () + 1, c.getY ());
					pathFound = setPathArray(coor, path, length, end, checkCM);
				}
			}
		}

		//find path at left
		if(!pathFound && c.getY() - 1 >= 0){
			if(visitRow[c.getX(), c.getY()-1] == true){
				Vector3 pos = blueNodeArray[c.getX(), c.getY()-1].GetComponent<RectTransform>().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX (), c.getY () - 1, path);
					Coordination coor = new Coordination (c.getX (), c.getY ()-1);
					pathFound = setPathArray(coor, path, length, end, checkCM);
				}
			}
		}

		//find path at right
		if(!pathFound && c.getY() + 1 <= (length-1)){
			if(visitRow[c.getX(), c.getY()] == true){
				Vector3 pos = blueNodeArray[c.getX(), c.getY()+1].GetComponent<RectTransform>().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX (), c.getY () + 1, path);
					Coordination coor = new Coordination (c.getX (), c.getY ()+1);
					pathFound = setPathArray(coor, path, length, end, checkCM);
				}
			}
		}

		if (!pathFound) {
			Vector3 pos = blueNodeArray[c.getX(), c.getY()].GetComponent<RectTransform>().anchoredPosition3D;
			path.Remove (new Vector3 (pos.x, pos.y + 19.5f, pos.z));
			Debug.Log ("<color=red>Remove Coor: x," + c.getX() + ", " + c.getY() + "</color>");
		}

		return pathFound;
	}

	private bool findCheckmatePath(){
		for (int i = 0; i < 6; i++) {
			for (int j = 0; j < 4; j++) {
				if (visitRow [i, j] == true) {
					//check left
					if (j+2 < aiMode_init.maxRow+2 && j+1 < aiMode_init.maxRow+2 && visitRow [i, j + 2] && !blockedRow [i, j + 1]) {
						cmX = i; cmY = j + 1;
						checkmateCol = true;
						return true;
					}

					//check right
					if (j-2 >= 0 && j-1 >= 0 && visitRow [i, j - 2] && !blockedRow[i , j -1]){
						cmX = i; cmY = j - 1;
						checkmateCol = true;
						return true;
					}

					//check up-right
					if (i-2 >= 0 && j+1 < aiMode_init.maxCol && j+1 < aiMode_init.maxCol && visitCol [i - 2, j + 1] && !blockedCol [i - 1, j + 1]) {
						cmX = i - 1; cmY = j + 1;
						checkmateCol = false;
						return true;
					}

					//check down-right
					if (i+1 < aiMode_init.maxCol && j +1 < aiMode_init.maxCol && visitCol [i + 1, j + 1] && !blockedCol [i, j + 1]) {
						cmX = i; cmY = j + 1;
						checkmateCol = false;
						return true;
					}

					//check up-left
					if (i-2 >= 0 && j-1 >= 0 && i - 1 >= 0 && j+1 < aiMode_init.maxCol && visitCol [i - 2, j - 1] && !blockedCol [i - 1, j + 1]) {
						cmX = i - 1; cmY = j + 1;
						checkmateCol = false;
						return true;
					}

					//check down-left
					if (i+1 < aiMode_init.maxCol && j-1 >= 0 && visitCol [i + 1, j - 1] && !blockedCol [i, j - 1]) {
						cmX = i; cmY = j - 1;
						checkmateCol = false;
						return true;
					}
				}
			}
		}

		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				if (visitCol[i, j] == true) {
					if (i - 1 >= 0 && !blockedCol [i - 1, j]) {
						if ((i - 1 >= 0 && j - 1 >= 0 && visitRow [i - 1, j - 1]) || (i - 1 >= 0 && visitRow [i - 1, j]) || (i - 2 >= 0 && visitCol [i - 2, j])) {
							cmX = i - 1; cmY = j;
							checkmateCol = false;
							return true;
						}
					}

					if (i + 1 < aiMode_init.maxCol && !blockedCol [i + 1, j]) {
						if ((i + 1 < aiMode_init.maxRow && j - 1 >= 0 && visitRow [i + 1, j - 1]) || (i + 1 < aiMode_init.maxRow && visitRow [i + 1, j]) || (i + 1 < aiMode_init.maxCol && visitCol [i + 1, j])) {
							cmX = i + 1; cmY = j;
							checkmateCol = false;
							return true;
						}
					}

					if (j - 1 > 0 && !blockedRow [i, j - 1]) {
						if ((i - 1 >= 0 && j - 1 >= 0 && visitCol [i - 1, j - 1]) || (j - 1 >= 0 && visitCol [i, j - 1]) || (j - 2 >= 0 && visitRow [i, j - 2])) {
							cmX = i; cmY = j - 1;
							checkmateCol = true;
							return true;
						}
					}

					if (i + 1 < aiMode_init.maxRow && j - 1 >= 0 && !blockedRow [i + 1, j - 1]) {
						if ((j - 1 >= 0 && visitCol [i, j - 1]) || (i + 1 < aiMode_init.maxCol && j - 1 > 0 && visitCol [i + 1, j - 1]) || (i + 1 < aiMode_init.maxRow && j - 2 >= 0 && visitRow [i + 1, j - 2])) {
							cmX = i + 1; cmY = j - 1;
							checkmateCol = true;
							return true;
						}
					}

					if (!blockedRow [i, j]) {
						if ((i - 1 >= 0 && j + 1 < aiMode_init.maxCol && visitCol [i - 1, j + 1]) || (j + 1 < aiMode_init.maxCol && visitCol [i, j + 1]) || (j + 1 < aiMode_init.maxRow + 2 && visitRow [i, j + 1])) {
							cmX = i; cmY = j;
							checkmateCol = true;
							return true;
						}
					}

					if (i + 1 < aiMode_init.maxRow && !blockedRow [i + 1, j]) {
						if ((j + 1 < aiMode_init.maxCol && visitCol [i, j + 1]) || (i + 1 < aiMode_init.maxCol && j + 1 < aiMode_init.maxCol && visitCol [i + 1, j + 1]) || (i + 1 < aiMode_init.maxRow && j + 1 < aiMode_init.maxRow + 2 && visitRow [i + 1, j + 1])) {
							cmX = i + 1; cmY = j;
							checkmateCol = true;
							return true;
						}
					}
				}
			}
		}

		return false;
	}

	//find path almost connected
	//c -> starting coordinate | path -> arraylist to record checked path | length -> board size | skipped -> check if skipped once already
	private bool findCheckmatePath(Coordination c, ArrayList path, int length, int end, bool skipped, Coordination targetCoor){
		//is the path found
		bool pathFound = false;
		bool preSkipped = skipped;

		//if path finding finished, return blank targetCoor
		if (c.getX () == end) {
			Debug.Log ("<color=red>Found end</color>");
			path.Add (targetCoor);
			return true;
		}

		//find path upward
		if (!pathFound && c.getX () - 1 >= 0) {
			Coordination coor = new Coordination (c.getX () - 1, c.getY ());
			if (visitCol [c.getX () - 1, c.getY ()] == true) { 
				if (!checkCoorEqual(path, coor.getX(), coor.getY()) && targetCoor.Equals(coor) == false) {
					Debug.Log ("<color=orange>Check Coor: x," + coor.getX() + ", " + coor.getY() + "</color>");
					path.Add (coor);
					pathFound = findCheckmatePath (coor, path, length, end, skipped, targetCoor);
				}
			}
		}

		//find path downward
		if (!pathFound && c.getX () + 1 <= (length)) {
			Coordination coor = new Coordination (c.getX () + 1, c.getY ());
			if (visitCol [c.getX (), c.getY ()] == true) {
				if (!checkCoorEqual(path, coor.getX(), coor.getY()) && targetCoor.Equals(coor) == false) {
					Debug.Log ("<color=orange>Check Coor: x," + coor.getX() + ", " + coor.getY() + "</color>");
					path.Add (coor);
					pathFound = findCheckmatePath (coor, path, length, end, skipped, targetCoor);
				}
			}
		}

		Debug.Log (pathFound);
		//find path at left
		if(pathFound == false && c.getY() - 1 >= 0){
			Coordination coor = new Coordination (c.getX (), c.getY ()-1);
			if(visitRow[c.getX(), c.getY()-1] == true){
				if (!checkCoorEqual(path, coor.getX(), coor.getY()) && targetCoor.Equals(coor) == false) {
					Debug.Log ("<color=orange>Check Coor: x," + coor.getX() + ", " + coor.getY() + "</color>");
					path.Add (coor);
					pathFound = findCheckmatePath (coor, path, length, end, skipped, targetCoor);
				}
			}
		}

		//find path at right
		if(!pathFound && c.getY() + 1 <= (length-1)){
			Coordination coor = new Coordination (c.getX (), c.getY ()+1);
			if(visitRow[c.getX(), c.getY()] == true){
				if (!checkCoorEqual(path, coor.getX(), coor.getY()) && targetCoor.Equals(coor) == false) {
					Debug.Log ("<color=orange>Check Coor: x," + coor.getX() + ", " + coor.getY() + "</color>");
					path.Add (coor);
					pathFound = findCheckmatePath (coor, path, length, end, skipped, targetCoor);
				}
			}
		}

		//Skip upward
		if (!pathFound && c.getX () - 1 >= 0) {
			Coordination coor = new Coordination (c.getX () - 1, c.getY ());
			if (visitCol [c.getX () - 1, c.getY ()] != true) { 
				if (!skipped && !checkCoorEqual (path, coor.getX (), coor.getY ()) && !blockedCol [c.getX () - 1, c.getY ()] && targetCoor.Equals(coor) == false) {
					Debug.Log ("<color=orange>Skip to Coor: x," + coor.getX () + ", " + coor.getY () + "</color>");
					skipped = true;
					checkmateCol = false;
					skipLeft = false;
					skipUp = true;
					targetCoor = coor;
					pathFound = findCheckmatePath (coor, path, length, end, skipped, targetCoor);
					if (!pathFound)
						skipped = false;
				}
			}
		}

		//Skip downward
		if (!pathFound && c.getX () + 1 <= (length)) {
			Coordination coor = new Coordination (c.getX () + 1, c.getY ());
			if (visitCol [c.getX (), c.getY ()] != true) {
				if (!skipped && !checkCoorEqual(path, coor.getX(), coor.getY()) && !blockedCol[c.getX(), c.getY()]  && targetCoor.Equals(coor) == false) {
					Debug.Log ("<color=orange>Skip to Coor: x," + coor.getX() + ", " + coor.getY() + "</color>");
					skipped = true;
					checkmateCol = false;
					skipLeft = false;
					skipUp = false;
					targetCoor = coor;
					pathFound = findCheckmatePath (coor, path, length, end, skipped, targetCoor);
					if (!pathFound)
						skipped = false;
				}
			}
		}

		//Skip left
		if(pathFound == false && c.getY() - 1 >= 0){
			Coordination coor = new Coordination (c.getX (), c.getY ()-1);
			if(visitRow[c.getX(), c.getY()-1] != true){
				if (!skipped && !checkCoorEqual(path, coor.getX(), coor.getY()) && !blockedRow[c.getX(), c.getY()-1] && targetCoor.Equals(coor) == false) {
					Debug.Log (pathFound);
					Debug.Log ("<color=orange>Skip to Coor: x," + coor.getX() + ", " + coor.getY() + "</color>");
					skipped = true;
					checkmateCol = true;
					skipLeft = true;
					skipUp = false;
					targetCoor = coor;
					pathFound = findCheckmatePath (coor, path, length, end, skipped, targetCoor);
					if (!pathFound)
						skipped = false;
				}
			}
		}

		//Skip right
		if(!pathFound && c.getY() + 1 <= (length-1)){
			Coordination coor = new Coordination (c.getX (), c.getY ()+1);
			if(visitRow[c.getX(), c.getY()] != true){
				if (!skipped && !checkCoorEqual(path, coor.getX(), coor.getY()) && !blockedRow[c.getX(), c.getY()]  && targetCoor.Equals(coor) == false) {
					Debug.Log ("<color=orange>Skip to Coor: x," + coor.getX() + ", " + coor.getY() + "</color>");
					skipped = true;
					checkmateCol = true;
					skipLeft = false;
					skipUp = false;
					targetCoor = coor;
					pathFound = findCheckmatePath (coor, path, length, end, skipped, targetCoor);
					if (!pathFound)
						skipped = false;
				}
			}
		}

		//if path not connected yet, return a null coordination
		if (!pathFound) {
			Debug.Log ("<color=red>Remove Coor: x," + c.getX () + ", " + c.getY () + "</color>");
			//path.Remove (c);
			//skipped = preSkipped;
			skipped = false;
			return false;
		} else
			return true;

		skipped = false;
		return false;
	}

	//find path from top to down
	private bool findNextEdge(int edgeRow, int edgeCol, int rowReached, ArrayList foundCol){
		bool found = false;
		Coordination cord;

		int length = aiMode_init.totalLength;
		int maxCol = aiMode_init.maxCol;

		if (rowReached == length) //reached
			found = true;
		else {
			cord = new Coordination(edgeRow+1, edgeCol);
			if (edgeRow+1 <= maxCol && visitCol[edgeRow+1,edgeCol] == true && !visit(cord,foundCol)){
				foundCol.Add(cord);
				found = findNextEdge(edgeRow+1,edgeCol,rowReached+1,foundCol);
				if(!found)//delete the new element added in arraylist
					foundCol.RemoveAt(foundCol.Count-1);
			}

			//find downward left row
			if (edgeCol!=0 && !found){
				if (edgeRow+1 <= maxCol && edgeCol-1 >= 0 && visitRow[edgeRow+1,edgeCol-1] == true)
						found = findNextVerticalEdge(edgeRow+1,edgeCol-1,rowReached,foundCol);
			}

			//find upward left row
			if(!found && edgeCol !=0){
				if (edgeCol-1 >= 0 && visitRow[edgeRow,edgeCol-1] == true)
					found = findNextVerticalEdge(edgeRow, edgeCol-1, rowReached-1,foundCol);
			}

			//downward right row
			if(edgeCol != 4 && !found){
				if (edgeRow+1 <= maxCol && visitRow[edgeRow+1,edgeCol] == true)
						found = findNextVerticalEdge (edgeRow+1,edgeCol,rowReached,foundCol);
				}
			}

			//upward right
			if (!found && edgeCol != 4) {
				if (visitRow[edgeRow,edgeCol] == true)
					found = findNextVerticalEdge(edgeRow,edgeCol,rowReached-1,foundCol);
			}

			//backward col
			if (!found && edgeRow-1 >= 0){
				cord = new Coordination(edgeRow-1,edgeCol);
				if (edgeRow-1 >= 0 && visitCol[edgeRow-1,edgeCol] == true && !visit(cord,foundCol)){
					foundCol.Add(cord);
					found = findNextEdge(edgeRow-1,edgeCol,rowReached-1,foundCol);
					if(!found)
						foundCol.RemoveAt(foundCol.Count-1);
				}
			}

		return found;
	}

	private bool findNextVerticalEdge(int edgeRow, int edgeCol, int rowReached, ArrayList foundCol){
		bool found = false;
		Coordination cord;

		//left downward col
		if (edgeRow < 5) {
			cord = new Coordination (edgeRow, edgeCol);
			if (visitCol [edgeRow, edgeCol] == true && !visit (cord, foundCol)) {
				foundCol.Add (cord);
				found = findNextEdge (edgeRow, edgeCol, rowReached + 1, foundCol);
				if (!found)
					foundCol.RemoveAt (foundCol.Count - 1);
			}
		}

		//left upward col
		if (edgeRow - 1 >= 0) {
			cord = new Coordination (edgeRow-1, edgeCol);
			if (!found && edgeRow-1 >= 0 && visitCol[edgeRow-1,edgeCol] == true && !visit (cord,foundCol)){
				foundCol.Add(cord);
				found = findNextEdge(edgeRow-1,edgeCol, rowReached, foundCol);
				if(!found)
					foundCol.RemoveAt(foundCol.Count-1);
			}
		}

		//right downward col
		if (edgeRow < 5) {
			cord = new Coordination (edgeRow, edgeCol + 1);
			if (edgeCol <= aiMode_init.maxRow && visitCol [edgeRow, edgeCol + 1] == true && !visit (cord, foundCol)) {
				foundCol.Add (cord);
				found = findNextEdge (edgeRow, edgeCol + 1, rowReached + 1, foundCol);
				if (!found)
					foundCol.RemoveAt (foundCol.Count - 1);
			}
		}

		//right upward col
		if (edgeRow - 1 >= 0) {
			cord = new Coordination(edgeRow-1, edgeCol+1);
			if(!found && edgeRow-1 >= 0 && edgeCol+1 <= aiMode_init.maxRow && visitCol[edgeRow-1,edgeCol+1] == true && ! visit (cord,foundCol)){
				foundCol.Add(cord);
				found = findNextEdge(edgeRow-1,edgeCol+1, rowReached,foundCol);
				if (!found)
					foundCol.RemoveAt (foundCol.Count-1);
			}
		}

		int r = edgeRow;
		int c = edgeCol;
		int stepper = 1;

		while (!found && c+stepper <= 3) {
			if (visitRow [r, c+stepper] == true) {
				if (r<5){
					cord = new Coordination(r, c+stepper);
					if (c+stepper+1 <= aiMode_init.maxRow && visitCol [r, c + stepper + 1] == true && !visit (cord,foundCol)){	//next downward right
						foundCol.Add(cord);
						found = findNextEdge (r, c+ stepper + 1, rowReached + 1,foundCol);
						if(!found)
							foundCol.RemoveAt(foundCol.Count-1);
					}
				}

				//next upward right
				if(r-1>=0){
					cord = new Coordination(r-1, c+stepper + 1);
					if (r-1 >= 0 && c+stepper+1 <= aiMode_init.maxRow && !found && visitCol [r - 1, c + stepper + 1] == true && !visit (cord,foundCol)){ 
						foundCol.Add(cord);
						found = findNextEdge(r-1, c+stepper+1 ,rowReached,foundCol);
						if(!found)
							foundCol.RemoveAt(foundCol.Count-1);
					}
				}
			} else //if no next horizontal edge, no need further search
				break;	
			stepper++;
		}

		stepper = 1; //intialize
		
		//same
		while (!found && c-stepper>=0) {
			if (c-stepper >= 0 && visitRow[r, c - stepper] == true) {
				if (r<5){
					cord = new Coordination(r, c-stepper);
					if (c-stepper >= 0 && visitCol[r, c-stepper] == true && !visit (cord,foundCol)){	//left downward col
						foundCol.Add(cord);
						found = findNextEdge (r, c - stepper,rowReached + 1,foundCol);	
						if(!found)
							foundCol.RemoveAt(foundCol.Count-1);
					}
				}

				if (r-1>=0){
					cord = new Coordination(r-1, c-stepper);
					if (visitCol[r-1, c-stepper] == true && !visit (cord,foundCol)){
						foundCol.Add(cord);
						found = findNextEdge(r-1,c-stepper,rowReached,foundCol);
						if(!found)
							foundCol.RemoveAt(foundCol.Count-1);
					}
				}
			} else
				break;
			stepper++;
		}

		return found;
	}

	private bool visit(Coordination cord, ArrayList list){
		for (int i = 0; i < list.Count; i++) {
			Coordination temp = list[i] as Coordination;
			if(cord.Equals(temp))
				return true;
		}
		
		return false;
	}
	
	private class Coordination{
		private int x;
		private int y;
		
		public Coordination(int a, int b){
			x = a;
			y = b;
		}
		
		public bool Equals(Coordination other){
			if (this.x == other.getX () && this.y == other.getY ())
				return true;
			else
				return false;
		}
		
		public int getX(){
			return x;
		}
		
		public int getY(){
			return y;
		}
	}

	private class Edge{
		private int row;
		private int col;
		private string type;
		private Transform trans;

		public Edge(int r, int c, string t, Transform tr){
			row = r;
			col = c;
			type = t;
			trans = tr;
		}

		public int getRow(){
			return row;
		}

		public int getCol(){
			return col;
		}

		public string getType(){
			return type;
		}

		public Transform getTransform(){
			return trans;
		}
	}

	public static void reset(){
		//reset visitCol
		for (int i = 0; i < 5; i++)
			for (int j = 0; j < 5; j++) {
				visitCol [i, j] = false;
				blockedCol[i,j] = false;
			}
		
		//reset visitRow
		for (int i = 0; i< 6; i++)
			for (int j = 0; j <4; j++) {
				visitRow [i, j] = false;
				blockedRow [i, j] = false;
			}

		myTurn = false;
		checkWin = false;
		checkmate = false;
		checkmateCol = false;
		skipLeft = false;
		skipUp = false;
		//clicked = false;
		checkmatePath = new ArrayList ();
		stepCount = 0;
		collectedBonusNode = 0;
	}
}//end class
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class redEdgeRespond : MonoBehaviour {
	private static bool myTurn;
	private static bool[,] blockedRow = new bool[5,5]; //default as false
	private static bool[,] blockedCol = new bool[4,6];
	private static bool[,] visitRow = new bool[5,5];
	private static bool[,] visitCol = new bool[4,6];
	private static Transform[,] redNodeArray;
	private static int stepCount;
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
	}

	public static void setNodeArray(Transform[,] array){
		redNodeArray = array;
	}

	public void ClickEvent(){
		this.transform.GetChild (0).transform.GetComponent<redEdgeRespond> ().OnMouseDownEvent ();
	}

	void Update(){
		if (checkWin && character.endMove) {
			timer.setTimer (false);
			Camera.main.SendMessage ("endgame", "red");
			checkWin = false;
		}
	}

	public void OnMouseDownEvent(){
		Debug.Log (row + " " + col + " clicked red");
		//if click on invicible bridge
		if (myTurn && (this.GetComponent<Image>().color.a == 0)) {
			//if no more bridge left
			if (multi_init.limitBridge && multi_init.redBridges <= 0)
				StartCoroutine (showWarning ());//show warning message
			else {
				AudioSource.PlayClipAtPoint (pressSE, Camera.main.transform.position);
				if (this.role == "Row") {
					if (blockedRow [row, col] == false) {
						visitRow [row, col] = true;	//as a visit
						changeAlpha (255, this.transform);//display edge
						stepCount++;
					
						//block blue side edge
						blueEdgeRespond.blockEdge (row, col, "Col");

						//Deduct usable bridges when it limit usage of bridges
						if (multi_init.limitBridge)
							multi_init.useRedBridge ();

						//stop my turn
						myTurn = false;

						if (win ())
							checkWin = true;
						else {
							multi_init.switchTurn ("player");
							blueEdgeRespond.raiseTurn ();
						}
					}
				} else {
					if (blockedCol [row, col] == false) {
						visitCol [row, col] = true;	//as a visit
						changeAlpha (255, this.transform);
						stepCount++;

						//BLOCK BLUE SIDE HERE
						if (col != 0 && col != 5)
							blueEdgeRespond.blockEdge (row + 1, col - 1, "Row");

						//Deduct usable bridges when it limit usage of bridges
						if (multi_init.limitBridge)
							multi_init.useRedBridge ();

						//stop my turn
						myTurn = false;

						if (win ())
							checkWin = true;
						else {
							multi_init.switchTurn ("player");
							blueEdgeRespond.raiseTurn ();
						}
					}
				}
			}
		}

		//If non-invisible bridge is clicked
		if (myTurn && (this.GetComponent<Image> ().color.a == 255) && multi_init.limitBridge) {
			if (this.role == "Row") {
				if (row != 0 && row != multi_init.totalLength) {
					visitRow [row, col] = false;
					stepCount--;
					if (row != 0 && row != 5) {
						blueEdgeRespond.unblockEdge (row - 1, col + 1, "Col");
					}
					changeAlpha (0, this.transform);
				}
			} else {
				visitCol [row, col] = false;
				stepCount--;
				blueEdgeRespond.unblockEdge (row, col, "Row");
				changeAlpha (0, this.transform);
			}
			multi_init.removeRedBridge ();
		}
	}

	private void changeAlpha(int value, Transform go){
		Color temp = go.GetComponent<Image> ().color;
		temp.a = value;
		go.GetComponent<Image> ().color = temp;
	}

	private IEnumerator showWarning(){
		warningText.SetActive (true);
		yield return new WaitForSeconds (1.5f);
		warningText.SetActive (false);
	}

	public static void raiseTurn(){
		redEdgeRespond.myTurn = true;
	}

	public void setRow(int arow){
		this.row = arow;
	}
	
	public void setCol(int acol){
		this.col = acol;
	}
	
	public void setRole(string arole){
		this.role = arole;
	}

	public void setMode(int mode){
		this.mode = mode;
	}

	public static void blockEdge(int row, int col, string role){
		if (role == "Row")
			redEdgeRespond.blockedRow [row, col] = true;
		else
			redEdgeRespond.blockedCol [row, col] = true;
	}

	public static void unblockEdge(int row, int col, string role){
		if (role == "Row")
			redEdgeRespond.blockedRow [row, col] = false;
		else
			redEdgeRespond.blockedCol [row, col] = false;
	}

	//determine whether "I" get win
	private bool win(){
		bool won = false;
		ArrayList foundRowList;
		
		if (stepCount >= multi_init.totalLength) {
			//find start edge
			for (int i = 0; i < multi_init.totalLength; i++){
				if(visitRow[i,0] ==  true){
					foundRowList = new ArrayList ();
					Coordination cord = new Coordination(i,0);
					foundRowList.Add(cord);
					//won = findNextEdge(i,0,1,foundRowList);
					won = setPathArray(cord, foundRowList);
					if (won) {
						Camera.main.SendMessage("disableButtons");

						ArrayList path = new ArrayList ();
						//set first path
						Coordination coor = (Coordination)foundRowList[0];
						addPath (coor.getX (), coor.getY (), path);
						addPath (coor.getX (), coor.getY ()+1, path);
						Coordination c = new Coordination (coor.getX (), coor.getY ()+1);
						setPathArray (c, path);

						character.winAnimation(path);
						break;
					}
				}
			}
		}
		
		return won;
	}

	private void addPath(int x, int y, ArrayList p){
		Vector3 pos = redNodeArray [x, y].GetComponent<RectTransform> ().anchoredPosition3D;
		p.Add (new Vector3 (pos.x, pos.y + 19.5f, pos.z));
		Debug.Log ("<color=blue> Added: x," + x + ", " + y + "</color>");		
	}

	private bool setPathArray(Coordination c, ArrayList path){
		Debug.Log ("<color=yellow> Called with coor: x," + c.getX() + ", " + c.getY() + "</color>");

		//if path found
		bool pathFound = false;

		//path finding finished
		if (c.getY () == multi_init.totalLength)
			return true;

		//find path upward
		if (!pathFound && c.getX () - 1 >= 0) {
			if (visitCol [c.getX () - 1, c.getY ()] == true) {
				Vector3 pos = redNodeArray [c.getX()-1, c.getY()].GetComponent<RectTransform> ().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX () - 1, c.getY (), path);
					Coordination coor = new Coordination (c.getX () - 1, c.getY ());
					pathFound = setPathArray(coor, path);
				}
			}
		}

		//find path downward
		if (!pathFound && c.getX () + 1 <= (multi_init.totalLength-1)) {
			if (visitCol [c.getX (), c.getY ()] == true) {
				Vector3 pos = redNodeArray [c.getX()+1, c.getY()].GetComponent<RectTransform> ().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX () + 1, c.getY (), path);
					Coordination coor = new Coordination (c.getX () + 1, c.getY ());
					pathFound = setPathArray(coor, path);
				}
			}
		}

		//find path at left
		if(!pathFound && c.getY() - 1 >= 0){
			if(visitRow[c.getX(), c.getY()-1] == true){
				Vector3 pos = redNodeArray[c.getX(), c.getY()-1].GetComponent<RectTransform>().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX (), c.getY () - 1, path);
					Coordination coor = new Coordination (c.getX (), c.getY ()-1);
					pathFound = setPathArray(coor, path);
				}
			}
		}

		//find path at right
		if(!pathFound && c.getY() + 1 <= (multi_init.totalLength)){
			if(visitRow[c.getX(), c.getY()] == true){
				Vector3 pos = redNodeArray[c.getX(), c.getY()+1].GetComponent<RectTransform>().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX (), c.getY () + 1, path);
					Coordination coor = new Coordination (c.getX (), c.getY ()+1);
					pathFound = setPathArray(coor, path);
				}
			}
		}

		if (!pathFound) {
			Vector3 pos = redNodeArray[c.getX(), c.getY()].GetComponent<RectTransform>().anchoredPosition3D;
			path.Remove (new Vector3 (pos.x, pos.y + 19.5f, pos.z));
			Debug.Log ("<color=red>Remove Coor: x," + c.getX() + ", " + c.getY() + "</color>");
		}

		return pathFound;
	}

	//pass a horizontal edge coordination into this method
	private bool findNextEdge(int edgeRow, int edgeCol, int colReached, ArrayList foundRow){
		bool found = false;
		Coordination cord;

		if (colReached == multi_init.totalLength) //reached
			found = true;
		else {
				//check straigh next horizontal edge
				cord = new Coordination(edgeRow, edgeCol+1);
			if (edgeCol+1 <= multi_init.maxCol && visitRow[edgeRow,edgeCol+1] == true && !visit (cord,foundRow)){
					foundRow.Add(cord);
					found = findNextEdge(edgeRow,edgeCol+1,colReached+1,foundRow);
					if(!found)//delete the new element added in arraylist
						foundRow.RemoveAt(foundRow.Count-1);
				}

				//check downward vertical edge
				if (!found && edgeCol+1 <= multi_init.maxCol && edgeRow!=4 && visitCol[edgeRow,edgeCol+1] == true){
					found = findNextHorizontalEdge(edgeRow,edgeCol+1,colReached,foundRow);
				}
				
				//check backward downward
				if(!found && edgeRow!=4 && visitCol[edgeRow,edgeCol] == true){
					found = findNextHorizontalEdge(edgeRow,edgeCol,colReached-1,foundRow);
				}
				
				//check upward vertical edge
				if (!found && edgeRow !=0 && edgeCol+1 <= multi_init.maxCol && visitCol[edgeRow-1,edgeCol+1] == true){
					found = findNextHorizontalEdge(edgeRow-1,edgeCol+1,colReached,foundRow);
				}

				//check backward upward
				if(!found && edgeRow != 0 && visitCol[edgeRow-1,edgeCol] == true)
					found = findNextHorizontalEdge(edgeRow-1,edgeCol,colReached-1,foundRow);

				if (!found && edgeCol-1 >= 0){//check backward row
					cord = new Coordination(edgeRow,edgeCol-1);
					if (visitRow[edgeRow,edgeCol-1] == true && !visit(cord,foundRow)){
						foundRow.Add(cord);
						found = findNextEdge(edgeRow,edgeCol-1,colReached-1,foundRow);
						if(!found)
							foundRow.RemoveAt(foundRow.Count-1);
					}
				}
			}
		return found;
	}

	private bool findNextHorizontalEdge(int edgeRow, int edgeCol, int colReached, ArrayList foundRow){
		bool found = false;
		Coordination cord;

		//find upper edge
		if (edgeCol < 5) {
			cord = new Coordination (edgeRow, edgeCol);
			if (visitRow [edgeRow, edgeCol] == true && !visit (cord, foundRow)) {
				foundRow.Add (cord);
				found = findNextEdge (edgeRow, edgeCol, colReached + 1, foundRow);
				if (!found)
					foundRow.RemoveAt (foundRow.Count - 1);
			}
		}

		//find upper edge (backward)
		if (edgeCol - 1 >= 0) {
			cord = new Coordination (edgeRow, edgeCol - 1);
			if (!found && visitRow [edgeRow, edgeCol - 1] == true && !visit (cord, foundRow)) {
				foundRow.Add (cord);
				found = findNextEdge (edgeRow, edgeCol - 1, colReached, foundRow);
				if (!found)
					foundRow.RemoveAt (foundRow.Count - 1);
			}
		}

		//find lower edge
		if (edgeCol < 5) {
			cord = new Coordination (edgeRow + 1, edgeCol);
			if (!found && visitRow [edgeRow + 1, edgeCol] == true && !visit (cord, foundRow)) {
				foundRow.Add (cord);
				found = findNextEdge (edgeRow + 1, edgeCol, colReached + 1, foundRow);
				if (!found)
					foundRow.RemoveAt (foundRow.Count - 1);
			}
		}

		//find loweredge (backward)
		if (edgeCol - 1 >= 0) {
			cord = new Coordination (edgeRow + 1, edgeCol - 1);
			if (!found && visitRow [edgeRow + 1, edgeCol - 1] == true && !visit (cord, foundRow)) {
				foundRow.Add (cord);
				found = findNextEdge (edgeRow + 1, edgeCol - 1, colReached, foundRow);
				if (!found)
					foundRow.RemoveAt (foundRow.Count - 1);
			}
		}
		
		int r = edgeRow;
		int c = edgeCol;
		int stepper = 1;

		//find next horizontal edge in other row, both backward or forward direction
		while (!found && r+stepper <= 3) {
			if (visitCol [r + stepper, c] == true) {
				if (c<5){
					cord = new Coordination(r+stepper+1, c);
					if (visitRow [r + stepper + 1, c] == true && !visit (cord,foundRow)){	//forward
						foundRow.Add(cord);
						found = findNextEdge (r + stepper + 1, c, colReached + 1,foundRow);
						if(!found)
							foundRow.RemoveAt(foundRow.Count-1);
					}
				}

				if (c-1>=0){
					cord = new Coordination(r+stepper+1, c-1);
					if (!found && visitRow [r + stepper + 1, c - 1] == true && !visit (cord,foundRow)){ //not in foundArrayList
						foundRow.Add(cord);
						found = findNextEdge(r+stepper+1, c-1 ,colReached,foundRow);
						if(!found)
							foundRow.RemoveAt(foundRow.Count-1);
					}
				}
			} else //if no next horizontal edge, no need further search
				break;	
			stepper++;
		}
		
		stepper = 1; //intialize

		//same
		while (!found && r-stepper>=0) {
			if (visitCol [r - stepper, c] == true) {
				cord = new Coordination(r-stepper, c);
				if (visitRow [r - stepper, c] == true && !visit (cord,foundRow)){
					foundRow.Add(cord);
					found = findNextEdge (r - stepper, c, colReached + 1,foundRow);
					if(!found)
						foundRow.RemoveAt(foundRow.Count-1);
				}

				cord = new Coordination(r-stepper, c-1);
				if (visitRow[r-stepper, c-1] == true && !visit (cord,foundRow)){
					foundRow.Add(cord);
					found = findNextEdge(r-stepper,c-1,colReached,foundRow);
					if(!found)
						foundRow.RemoveAt(foundRow.Count-1);
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

	public static void reset(){
		//reset visitrow
		for (int i = 0; i < 5; i++)
			for (int j = 0; j < 5; j++) {
				visitRow [i, j] = false;
				blockedRow[i,j] =false;
			}
		//reset visitcol
		for (int i = 0; i< 4; i++)
			for (int j = 0; j <6; j++) {
				visitCol [i, j] = false;
				blockedCol[i,j] =false;
			}

		//as red is the first one
		myTurn = false;
		checkWin = false;
		stepCount = 0;
	}
}//end class
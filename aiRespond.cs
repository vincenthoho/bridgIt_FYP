using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Globalization;
public class aiRespond : MonoBehaviour {
	
	private static bool[,] blockedRow = new bool[5,5]; //default as false
	private static bool[,] blockedCol = new bool[4,6];
	private static bool[,] visitRow = new bool[5,5];
	private static bool[,] visitCol = new bool[4,6];
	private static Transform[,] redEdgeRowArray;
	private static Transform[,] redEdgeColArray;
	private static Transform[,] redNodeArray;
	private static int stepCount;
	private static int maxBridges = 0;
	private static int bridges = 10;
	private static ArrayList allMyMoves = new ArrayList();
	private static ArrayList allBlueMoves = new ArrayList();
	public static ArrayList aiMoves = new ArrayList();
    public static ArrayList erasedNode = new ArrayList();
    private static bool checkWin = false;
	public static bool checkmate = false;
	public static bool checkmateCol = false;
	public static bool skipLeft = false;
	public static bool skipUp = false;

	void Awake () {
		stepCount = 0;
		checkWin = false;
		aiMoves = new ArrayList ();
		bridges = maxBridges;
	}

	public static void setBrisgesNo(int no){
		maxBridges = no;
	}

	private static void changeAlpha(int value, Transform go){
		Color temp = go.GetComponent<Image> ().color;
		temp.a = value;
		go.GetComponent<Image> ().color = temp;
	}

	public static void setRowArray(Transform[,] array){
		redEdgeRowArray = array;
	}

	public static void setColArray(Transform[,] array){
		redEdgeColArray = array;
	}

	public static void setNodeArray(Transform[,] array){
		redNodeArray = array;
	}

	//AI responds to the player's move
	private static void move(int lastRow, int lastCol, string lastRole){
		if (stepCount == 0 && aiMode_init.aiFirst) {
			Debug.Log ("<color=yellow>changed to ai turn</color>");
			//place vertical edge
			int randomRow = (int)Random.Range (1, aiMode_init.maxRow - 1);
			int randomCol = (int)Random.Range (1, aiMode_init.maxCol - 1);
			//redEdgeColArray [randomRow, randomCol].GetComponent<Renderer>().enabled = true;
			checkPath();
			blueEdgeRespond.raiseTurn ();
			/*
			Debug.Log ("random contact on " + randomRow + ", " + randomCol);
			changeAlpha (255, redEdgeColArray [randomRow, randomCol]);
			int size = aiMode_init.maxCol; 
			shannon.setResistance (size*size+(size-1)*randomRow+(randomCol-1), 0);
			visitCol [randomRow, randomCol] = true;
			blueEdgeRespond.blockEdge (randomRow + 1, randomCol - 1, "Row");
			blueEdgeRespond.raiseTurn ();
			allMyMoves.Add (new LastMove (randomRow, randomCol, "Col"));
			*/
			bridges--;
			stepCount++;
		} else {
			if (!aiMode_init.limitBridges || (aiMode_init.limitBridges && bridges > 0)) {
				Debug.Log ("<color=blue>changed to ai turn</color>");
				allBlueMoves.Add (new LastMove (lastRow, lastCol, lastRole));
				//blockLastBlue ();
				checkPath();
				bridges--;

				if (win ()) {
					checkWin = true;
				} else {
					blueEdgeRespond.raiseTurn ();
					aiMode_init.switchTurn ("player");
				}
			} else {
				removeBridge ();
				move (lastRow, lastCol, lastRole);
			}
		}
	}

	private static void removeBridge(){
		int min = 10;
		int count = 0;
		int row = 0;
		LastMove l = (LastMove)aiMoves[0];

		//Pick one edge from the shortest connected route
		for (int i = 0; i <= (aiMode_init.totalLength-1); i++) {
			for (int j = 0; j <= (aiMode_init.totalLength-1); j++) {
				Debug.Log("<color=pink>check " + i + ", " + j + "</color>");
				if (visitRow [i, j] == true) {
					count++;
				}
			}
			if (count < 5 && count < min){
				min = count;
				row = i;
			}
			count = 0;
		}

		foreach (LastMove lm in aiMoves) {
			if (lm.getRow () == row)
				l = lm;
		}

		//Remove choosed edge
		if (l.getRole ().Equals ("Col")) {
			changeAlpha (0, redEdgeColArray [l.getRow (), l.getCol ()]);
			visitCol [l.getRow (), l.getCol ()] = false;
			if (l.getCol () != 0 && l.getCol () != 5)
				blueEdgeRespond.unblockEdge (l.getRow () + 1, l.getCol () - 1, "Row");
			stepCount--;
		} else {
			changeAlpha (0, redEdgeRowArray [l.getRow (), l.getCol ()]);
			visitRow [l.getRow (), l.getCol ()] = false;
			blueEdgeRespond.unblockEdge (l.getRow (), l.getCol (), "Col");
			stepCount--;
		}
		aiMoves.Remove (l);
		//aiMoves.Add (l);
		bridges++;
	}

	void Update(){
		//if (checkWin) {
		if (checkWin && enemy.endMove) {
			timer.setTimer (false);
			Camera.main.SendMessage ("endgame", "red");
			checkWin = false;
		}
	}

	public static void blockLastBlue(){
		Debug.Log ("<color=red>enter blockLastBlue</color>");
		LastMove temp = new LastMove (0, 0, "Row");
		if(allBlueMoves.Count > 0)
			temp = (LastMove)allBlueMoves [allBlueMoves.Count - 1];
		bool blocked = true;
		Debug.Log ("<color=yellow>start find match path</color>");
		if (temp.getRole ().Equals ("Row")) {
			if (temp.getRow () == 0 || temp.getRow () == 5)
				//placeAtEmptyRow ();
				expand ();
			else{
				//block a lighten path by extending the row (block it from connecting up to down)
				if (allBlueMoves.Contains (new LastMove (temp.getRow () - 1, temp.getCol (), "Col")))
					blocked = !placeRowEdge(temp.getRow(),temp.getCol()+1);
				if (blocked && allBlueMoves.Contains (new LastMove (temp.getRow () - 1, temp.getCol () + 1, "Col"))) 
					blocked = !placeRowEdge(temp.getRow(),temp.getCol());
				if (blocked && allBlueMoves.Contains (new LastMove (temp.getRow (), temp.getCol (), "Col"))) 
					blocked = !placeRowEdge(temp.getRow()-1,temp.getCol()+1);
				if (blocked && allBlueMoves.Contains (new LastMove (temp.getRow (), temp.getCol () + 1, "Col"))) 
					blocked = !placeRowEdge (temp.getRow () - 1, temp.getCol ());

				//block the possibility of 2 rows connected as lighten by placing a row between them
				if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()+1,temp.getCol()-1,"Row")))
					blocked = !placeRowEdge(temp.getRow(),temp.getCol());
				if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()+1,temp.getCol()+1,"Row")))
					blocked = !placeRowEdge(temp.getRow(),temp.getCol()+1);
				if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()-1,temp.getCol()+1,"Row")))
					blocked = !placeRowEdge(temp.getRow()-1,temp.getCol()+1);
				if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()-1,temp.getCol()-1,"Row")))
					blocked = !placeRowEdge(temp.getRow()-1,temp.getCol());

				//prevent long "L" that is a | _ and then connect it to |__
				if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()+1,temp.getCol(),"Col")))
					blocked = !placeRowEdge(temp.getRow(),temp.getCol());
				if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()-2,temp.getCol(),"Col")))
					blocked = !placeRowEdge(temp.getRow()-1,temp.getCol());
				if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()+1,temp.getCol()+1,"Col")))
					blocked = !placeRowEdge(temp.getRow(),temp.getCol()+1);
				if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()-2,temp.getCol()+1,"Col")))
					blocked = !placeRowEdge(temp.getRow()-1,temp.getCol()+1);

				//block a long lighten: if the case is |___ , block the rightMost row, so that no further extension to downward 
				if (blocked){
					int counter = 1;
					while(allBlueMoves.Contains(new LastMove(temp.getRow(),temp.getCol()-counter,"Row")) && counter <= 3){
						if(allBlueMoves.Contains(new LastMove(temp.getRow()-1,temp.getCol()-counter,"Col"))){
							blocked = !placeRowEdge(temp.getRow(),temp.getCol()+1);
							break;
						}
						if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow(),temp.getCol()-counter,"Col"))){
							blocked = !placeRowEdge(temp.getRow()-1,temp.getCol()+1);
						}
						counter++;
					}
					if(blocked){
						counter = 1;
						while(allBlueMoves.Contains(new LastMove(temp.getRow(),temp.getCol()+counter,"Row")) && counter <= 3){
							if(allBlueMoves.Contains (new LastMove(temp.getRow ()-1,temp.getCol()+1+counter,"Col"))){
								blocked = !placeRowEdge(temp.getRow(),temp.getCol());
								break;
							}
							if(blocked && allBlueMoves.Contains (new LastMove(temp.getRow(),temp.getCol()+1+counter,"Col"))){
								blocked = !placeRowEdge(temp.getRow()-1,temp.getCol());
							}
							counter++;
						}
					}
				}

				if (blocked)	//expand the length of the path
					placeAtEmptyRow ();
					//expand ();
			}
		} else {	//if role == "Col"
			//prevent a lighten connection
			if (allBlueMoves.Contains(new LastMove(temp.getRow()-1,temp.getCol()-1,"Col")))
				blocked = !placeColEdge(temp.getRow()-1,temp.getCol());
			if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()-1,temp.getCol()+1,"Col")))
				blocked = !placeColEdge(temp.getRow()-1,temp.getCol()+1);
			if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()+1,temp.getCol()-1,"Col")))
				blocked = !placeColEdge(temp.getRow(),temp.getCol());
			if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()+1,temp.getCol()+1,"Col")))
				blocked = !placeColEdge(temp.getRow(),temp.getCol()+1);

			//prevent a "L" connection
			if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow(),temp.getCol()-2,"Row")))
				blocked = !placeColEdge(temp.getRow()-1,temp.getCol());
			if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow(),temp.getCol()+1,"Row")))
				blocked = !placeColEdge(temp.getRow()-1,temp.getCol()+1);
			if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()-1,temp.getCol()-2,"Row")))
				blocked = !placeColEdge(temp.getRow(),temp.getCol());
			if (blocked && allBlueMoves.Contains(new LastMove(temp.getRow()+1,temp.getCol()+1,"Row")))
				blocked = !placeColEdge(temp.getRow(),temp.getCol()+1);


			if(blocked){
				if (temp.getRow() == 1 || temp.getRow() == 4){
					blocked = !placeRowEdge(temp.getRow()-1,temp.getCol());
				}else{
					blocked = !placeRowEdge(temp.getRow()+1,temp.getCol ());
					if(blocked && temp.getRow() != 4){
						blocked = !placeRowEdge(temp.getRow()+1,temp.getCol());
					}
				}

				//block the extension from up to down
				if(blocked){
					int nextRow = 0;
					int start = 0;
					int end = 0;
					while(!allBlueMoves.Contains(new LastMove(nextRow, temp.getCol(),"Col"))){
						start = nextRow;
						nextRow++;
					}
					end = start;
					while(allBlueMoves.Contains(new LastMove(nextRow, temp.getCol(),"Col"))){
						end = nextRow;
						nextRow++;
					}

					if (end != 4){
						blocked = !placeRowEdge(end+1,temp.getCol());
						if(blocked && start != 0)
							blocked = !placeRowEdge(start-1,temp.getCol());
					}else
						blocked = !placeRowEdge(start-1, temp.getCol());
				}
			}

			if(blocked){
				placeAtEmptyRow ();
				//expand ();
			}
		}
	}

	private static bool checkCoorEqual(ArrayList p, int x, int y){
		foreach (Coordination c in p) {
			if (c.getX () == x && c.getY () == y)
				return true;
		}
		return false;
	}

	//Check if AI almost win
	//if True, block the user first before calculate with Shannon Heuristic
	public static ArrayList checkCheckmate(){
		ArrayList pathList = new ArrayList ();
		checkmate = false;

		if (stepCount >= aiMode_init.totalLength-1) {
			//find start edge from left
			for (int i = 0; i < aiMode_init.totalLength; i++){
				if(visitRow[i, 0] ==  true){
					Coordination cord = new Coordination(i, 0);
					pathList.Add (cord);
					checkmate = findCheckmatePath (cord, pathList, aiMode_init.totalLength, aiMode_init.totalLength, false, new Coordination(-1,-1));
					if (checkmate)
						return pathList;
				}
			}

			//find start edge from right
			for (int i = 0; i < aiMode_init.totalLength; i++){
				if(visitRow[i, aiMode_init.maxRow] ==  true){
					Coordination cord = new Coordination(i, aiMode_init.maxRow);
					pathList.Add (cord);
					checkmate = findCheckmatePath (cord, pathList, aiMode_init.totalLength, 0, false, new Coordination(-1, -1));
					if (checkmate)
						return pathList;
				}
			}
		}
		return pathList;
	}

	/*
	*/

	//find path almost connected
	//c -> starting coordinate | path -> arraylist to record checked path | length -> board size | skipped -> check if skipped once already
	private static bool findCheckmatePath(Coordination c, ArrayList path, int length, int end, bool skipped, Coordination targetCoor){
		//is the path found
		bool pathFound = false;
		bool preSkipped = skipped;

		//if path finding finished, return blank targetCoor
		if (c.getY () == end) {
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
		if (!pathFound && c.getX () + 1 < (length)) {
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
		if(!pathFound && c.getY() + 1 <= (length)){
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
		if (!pathFound && c.getX () + 1 < (length)) {
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
		if(!pathFound && c.getY() + 1 <= (length)){
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

	//catch winning strategy that cannot be stopped currently -> return caught or not
	public static bool catchWinningStrategy(){
		bool[,] blueCol = blueEdgeRespond.getVisitCol ();

		//check from top
		for (int i = 0; i < aiMode_init.maxCol; i++) {
			if (blueCol [0, i]) {
				if (i+1<aiMode_init.maxCol && ((blueCol [2+aiMode_init.nodeNo, i + 1] && blueCol [3+aiMode_init.nodeNo, i + 1]) || (blueCol [1+aiMode_init.nodeNo, i + 1] && blueCol [2+aiMode_init.nodeNo, i + 1]))) {
					Debug.Log ("check winning strategy");
					if (!visitCol [0, i + 1] && !blockedCol[0, i+1]) {
						placeColEdge (0, i + 1);
						return true;
					}
				}
				if (i - 1 >= 0 && ((blueCol [2+aiMode_init.nodeNo, i - 1] && blueCol [3+aiMode_init.nodeNo, i - 1]) || (blueCol [1+aiMode_init.nodeNo, i - 1] && blueCol [2+aiMode_init.nodeNo, i - 1]))) {
					Debug.Log ("check winning strategy");
					if (!visitCol [0, i] && !blockedCol[0,i]) {
						placeColEdge (0, i);
						return true;
					}
				}
			}
		}

		//check from buttom
		for (int i = 0; i < aiMode_init.maxCol; i++) {
			if (blueCol [aiMode_init.maxCol-1, i]) {
				if (i+1<aiMode_init.maxCol && ((blueCol [0, i + 1] && blueCol [1, i + 1]) || (blueCol [1, i + 1] && blueCol [2, i + 1]))) {
					Debug.Log ("check winning strategy");
					if (!visitCol [aiMode_init.maxRow - 1, i + 1] && !blockedCol[aiMode_init.maxRow - 1, i + 1]) {
						placeColEdge (aiMode_init.maxRow - 1, i + 1);
						return true;
					}
				}
				if (i - 1 >= 0 && ((blueCol [0, i - 1] && blueCol [1, i - 1]) || (blueCol [1, i - 1] && blueCol [2, i - 1]))) {
					Debug.Log ("check winning strategy");
					if (!visitCol [aiMode_init.maxRow - 1, i] && !blockedCol[aiMode_init.maxRow - 1, i + 1]) {
						placeColEdge (aiMode_init.maxRow - 1, i);
						return true;
					}
				}
			}
		}
		return false;
	}

	//check path with Shannon Heuristic
	public static void checkPath(){
		int size = aiMode_init.maxCol;

		//If AI checkmates, connect it to win first
		ArrayList checkmatePath = checkCheckmate ();
		if (checkmate) {
			Debug.Log ("<color=red>checkmate</color>");
			Coordination c = new Coordination (0, 0);
			if(checkmatePath.Count >= 1)
				c = (Coordination)checkmatePath[checkmatePath.Count -1];
			int cmX = c.getX();
			int cmY = c.getY ();
			if (checkmateCol) {
				if (skipLeft)
					placeRowEdge (cmX, cmY);
				else
					placeRowEdge (cmX, cmY - 1);
			} else {
				if (skipUp)
					placeColEdge (cmX, cmY);
				else
					placeColEdge (cmX - 1, cmY);
			}			
			return;
		}

		//If player checkmates, block player first
		if (blueEdgeRespond.checkmate) {
			Debug.Log ("<color=white>checkmate</color>");
			int cmX = blueEdgeRespond.getCheckmateX ();
			int cmY = blueEdgeRespond.getCheckmateY ();
            bool putSuccess = true;
			if (blueEdgeRespond.checkmateCol) {
				if (blueEdgeRespond.skipLeft)
                    putSuccess = placeColEdge(cmX - 1, cmY + 1);
				else
                    putSuccess = placeColEdge(cmX - 1, cmY);
			} else {
				if (blueEdgeRespond.skipUp)
                    putSuccess = placeRowEdge(cmX, cmY);
				else
                    putSuccess = placeRowEdge(cmX - 1, cmY);
			}			
            //if(putSuccess)
			    return;
		}

		bool t = catchWinningStrategy ();
		Debug.Log ("caught: " + t);
		if (t) {
			return;
		}

		int maxI = shannon.getMaxCurrent (size);
		if (maxI < size * size) {
			int i = maxI / size;
			int j = maxI % size;
			if ((i == 0 && j == 0) || (visitRow [i, j] == true || blockedRow [i, j] == true))
				blockLastBlue ();
			else
				placeRowEdge (i, j);
		} else {
			maxI = maxI - (size * size);
			int i = maxI / (size - 1);
			int j = maxI % (size - 1);
			if(visitCol[i, j] == true || blockedCol[i, j] == true)
				blockLastBlue();
			else
				placeColEdge (i, j+1);
		}
	}

	public static void redo(){
		LastMove lm = (LastMove)aiMoves [aiMoves.Count - 1];
		int size = aiMode_init.maxCol;
		if (lm.getRole ().Equals ("Col")) {
			shannon.setResistance (size*size+(size-1)*lm.getRow()+(lm.getCol()-1), 1);
			changeAlpha (0, redEdgeColArray [lm.getRow (), lm.getCol ()]);
			visitCol [lm.getRow (), lm.getCol ()] = false;
			if (lm.getCol () != 0 && lm.getCol () != 5)
				blueEdgeRespond.unblockEdge (lm.getRow () + 1, lm.getCol () - 1, "Row");
			stepCount--;
		} else {
			shannon.setResistance (lm.getRow()*size+lm.getCol(), 1);
			changeAlpha (0, redEdgeRowArray [lm.getRow (), lm.getCol ()]);
			visitRow [lm.getRow (), lm.getCol ()] = false;
            blueEdgeRespond.unblockEdge(lm.getRow(), lm.getCol(), "Col");
            stepCount--;
            if (aiMode_init.nodesErased)
            {
                foreach (int n in erasedNode)
                {
                    if (lm.getRow() == n / (aiMode_init.nodeNo + 3) + 1)
                    {
                        if (lm.getCol() == n % (aiMode_init.nodeNo + 3))
                        {
                            aiMoves.Remove(lm);
                            if (visitRow[lm.getRow(), lm.getCol() + 1] == false)
                                return;
                            else
                                redo();
                        }
                        else if (lm.getCol() == n % (aiMode_init.nodeNo + 3) + 1)
                        {
                            aiMoves.Remove(lm);
                            if (visitRow[lm.getRow(), lm.getCol() - 1] == false)
                                return;
                            else
                                redo();
                        }
                    }
                }
            }
		}
		bridges++;
		aiMoves.Remove (lm);
	}

	private static bool placeColEdge(int row, int col){
		Debug.Log ("Place col edge at " + row + ", " + col);

		if (row < 0 || col < 0 || row >= 4 || col >= 6 || visitCol [row, col] == true || blockedCol [row, col] == true || redEdgeColArray [row, col] == null)
			return false;
		else {
			int size = aiMode_init.maxCol;
            if (aiMode_init.nodesErased){
                foreach (int n in erasedNode){
                    if (col == (n % (aiMode_init.nodeNo + 3)) + 1){
                        if (row == n / (aiMode_init.nodeNo + 3)){
                            if (blockedCol[row+1, col] == true || (visitRow[row + 1, col - 1] == true && visitRow[row + 1, col] == true))
                                return false;
                        }
                        else if (row == (n / (aiMode_init.nodeNo + 3)) + 1){
                            if (blockedCol[row-1, col] == true || (visitRow[row, col - 1] == true && visitRow[row, col] == true))
                                return false;
                        }
                    }
                }
            }
            //shannon.setResistance (size*size+(size-1)*row+(col-1), -1);
            shannon.setResistance (size*size+(size-1)*row+(col-1), (float)decimal.Parse("1E-08", NumberStyles.Float));
			//shannon.getMaxCurrent (size);
			changeAlpha(255, redEdgeColArray[row,col]);
			visitCol[row,col] = true;

            if (aiMode_init.nodesErased)
            {
                foreach (int n in erasedNode)
                {
                    if (col == (n % (aiMode_init.nodeNo + 3)) + 1)
                    {
                        if (row == n / (aiMode_init.nodeNo + 3))
                        {
                            placeColEdge(row + 1, col);
                        }
                        else if (row == (n / (aiMode_init.nodeNo + 3)) + 1)
                        {
                            placeColEdge(row - 1, col);
                        }
                    }
                }
            }

            if (col!=0 && col != 5)
				blueEdgeRespond.blockEdge(row+1,col-1,"Row");

			stepCount++;
			aiMoves.Add (new LastMove (row, col, "Col"));
			return true;
		}
	}

	private static bool placeRowEdge(int row, int col){
		Debug.Log ("Place row edge at " + row + ", " + col);

		if (row < 0 || col < 0 || visitRow [row, col] == true || blockedRow [row, col] == true || redEdgeRowArray [row, col] == null)
			return false;
		else {
			int size = aiMode_init.maxCol;
            if (aiMode_init.nodesErased){
                foreach (int n in erasedNode){
                    if (row == n / (aiMode_init.nodeNo + 3) + 1){
                        if (col == n % (aiMode_init.nodeNo + 3)){
                            if (blockedRow[row, col+1] == true || (visitCol[row - 1, col + 1] == true && visitCol[row, col + 1] == true))
                                return false;
                        }
                        else if (col == n % (aiMode_init.nodeNo + 3) + 1){
                            if (blockedRow[row, col-1] == true || (visitCol[row - 1, col] == true && visitCol[row, col] == true))
                                return false;
                        }
                    }
                }
            }
            //shannon.setResistance (size*size+(size-1)*row+(col-1), -1);
            shannon.setResistance (row*size+col, (float)decimal.Parse("1E-08", NumberStyles.Float));
			//shannon.getMaxCurrent (size);
			changeAlpha(255, redEdgeRowArray [row, col]);
			visitRow [row, col] = true;

            if (aiMode_init.nodesErased)
            {
                foreach (int n in erasedNode)
                {
                    if (row == n / (aiMode_init.nodeNo + 3)+1)
                    {
                        if (col == n % (aiMode_init.nodeNo + 3))
                        {
                            placeRowEdge(row, col + 1);
                        }
                        else if (col == n % (aiMode_init.nodeNo + 3) + 1)
                        {
                            placeRowEdge(row, col - 1);
                        }
                    }
                }
            }

            blueEdgeRespond.blockEdge (row, col, "Col");
			stepCount++;
			aiMoves.Add (new LastMove (row, col, "Row"));
			return true;
		}
	}

	//"randomly" expand, but expand the row, which has been placed most edges
	private static void expand(){
		bool blocked = true;
		int row = -1,max = 0,count = 0;
		for (int i = 0; i <= (aiMode_init.totalLength-1); i++) {
			for (int j = 0; j <= (aiMode_init.totalLength-1); j++) {
				Debug.Log("<color=purple>check " + i + ", " + j + "</color>");
				if (visitRow [i, j] == true)
					count++;
				if (blockedRow [i, j] == true)
					count++;
			}
			if (count < 5 && count > max){
				max = count;
				row = i;
			}
			count = 0;
		}

		if (row != -1) {
			int temp = 0;
			do {
				Debug.Log("<color=orange>place at " + row + ", " + temp + "</color>");
				blocked = !placeRowEdge (row, temp);
				temp++;

				if(blocked && temp > aiMode_init.maxRow){
					bool place = placeAtEmptyRow();
					if(place)
						placeAtEmptyCol();
					break;
				}

			} while(blocked);
		} else {
			//int temp = 0;
			bool place = placeAtEmptyRow();
			if (place)
				placeAtEmptyCol ();
			/*
			do {
				blocked = !placeRowEdge (Random.Range (0, 5), Random.Range (0, 5));
			} while(blocked);
			*/
		}
	}

	//Find empty row to place
	private static bool placeAtEmptyRow(){
		bool blocked = true;
		for (int i = 0; i < aiMode_init.maxCol; i++) {
			for (int j = 0; j < aiMode_init.maxRow+1; j++) {
				blocked = !placeRowEdge (i, j);
				if (!blocked) 
					return false;
			}
		}
		return true;
	}

	//find empty col to place
	private static bool placeAtEmptyCol(){
		bool blocked = true;
		for (int i = 0; i < aiMode_init.maxRow; i++) {
			for (int j = 1; j < aiMode_init.maxCol; j++) {
				blocked = !placeColEdge (i, j);
				if (!blocked)
					return false;
			}
		}
		return true;
	}

	public static void blockEdge(int row, int col, string role){
		if (role == "Row")
			blockedRow [row, col] = true;
		else
			blockedCol [row, col] = true;
	}

	public static void unblockEdge(int row, int col, string role){
		if (role == "Row")
			blockedRow [row, col] = false;
		else
			blockedCol [row, col] = false;
	}

	public static void raiseTurn(int row, int col, string role){	
		move(row, col, role);
	}

	//determine whether "I" get win
	private static bool win(){
		bool won = false;
		ArrayList foundRowList = new ArrayList ();
		
		if (stepCount >= aiMode_init.totalLength) {
			Debug.Log ("<color=red>Check path</color>");
			//find start edge
			for (int i = 0; i < aiMode_init.totalLength; i++){
				Debug.Log ("<color=red>Find path at  visitRow[" + i + ",0]</color>");
				if(visitRow[i,0] ==  true){
					foundRowList = new ArrayList ();
					Coordination cord = new Coordination(i,0);
					foundRowList.Add(cord);
					//won = findNextEdge(i,0,1,foundRowList);
					won = setPathArray (cord, aiMode_init.totalLength, foundRowList);
					if (won) {
						Camera.main.SendMessage("disableButtons");

						ArrayList path = new ArrayList ();
						//set first path
						Coordination coor = (Coordination)foundRowList[0];
						addPath (coor.getX (), coor.getY (), path);
						addPath (coor.getX (), coor.getY ()+1, path);
						Coordination c = new Coordination (coor.getX (), coor.getY ()+1);
						setPathArray (c, aiMode_init.totalLength, path);

						//ArrayList path = setPathArray (foundRowList);
						enemy.winAnimation (path);
						break;
					}
				}
			}
		}
		Debug.Log ("<color=yellow>" + won + "</color>");
		return won;
	}

	private static void addPath(int x, int y, ArrayList p){
		Vector3 pos = redNodeArray [x, y].GetComponent<RectTransform> ().anchoredPosition3D;
		p.Add (new Vector3 (pos.x, pos.y + 19.5f, pos.z));
		Debug.Log ("<color=blue> Added: x," + x + ", " + y + "</color>");		
	}

	//Check if a path is connected from "c" to "end" and fill the ArrayList to record the path if yes
	private static bool setPathArray(Coordination c, int end, ArrayList path){
		Debug.Log ("<color=yellow> Called with coor: x," + c.getX() + ", " + c.getY() + "</color>");

		//if path found
		bool pathFound = false;

		//path finding finished
		if (c.getY () == end)
			return true;

		//find path upward
		if (!pathFound && c.getX () - 1 >= 0) {
			if (visitCol [c.getX () - 1, c.getY ()] == true) {
				Vector3 pos = redNodeArray [c.getX()-1, c.getY()].GetComponent<RectTransform> ().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX () - 1, c.getY (), path);
					Coordination coor = new Coordination (c.getX () - 1, c.getY ());
					pathFound = setPathArray(coor, aiMode_init.totalLength, path);
				}
			}
		}

		//find path downward
		if (!pathFound && c.getX () + 1 <= (aiMode_init.totalLength-1)) {
			if (visitCol [c.getX (), c.getY ()] == true) {
				Vector3 pos = redNodeArray [c.getX()+1, c.getY()].GetComponent<RectTransform> ().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX () + 1, c.getY (), path);
					Coordination coor = new Coordination (c.getX () + 1, c.getY ());
					pathFound = setPathArray(coor, aiMode_init.totalLength, path);
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
					pathFound = setPathArray(coor, aiMode_init.totalLength, path);
				}
			}
		}

		//find path at right
		if(!pathFound && c.getY() + 1 <= (aiMode_init.totalLength)){
			if(visitRow[c.getX(), c.getY()] == true){
				Vector3 pos = redNodeArray[c.getX(), c.getY()+1].GetComponent<RectTransform>().anchoredPosition3D;
				if (!path.Contains (new Vector3 (pos.x, pos.y + 19.5f, pos.z))) {
					addPath (c.getX (), c.getY () + 1, path);
					Coordination coor = new Coordination (c.getX (), c.getY ()+1);
					pathFound = setPathArray(coor, aiMode_init.totalLength, path);
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
	private static bool findNextEdge(int edgeRow, int edgeCol, int colReached, ArrayList foundRow){
		bool found = false;
		Coordination cord;
		
		if (colReached == aiMode_init.totalLength) //reached
			found = true;
		else {
			//check straigh next horizontal edge
			cord = new Coordination(edgeRow, edgeCol+1);
			if (visitRow[edgeRow,edgeCol+1] == true && !visit (cord,foundRow)){
				foundRow.Add(cord);
				found = findNextEdge(edgeRow,edgeCol+1,colReached+1,foundRow);
				if(!found)//delete the new element added in arraylist
					foundRow.RemoveAt(foundRow.Count-1);
			}
			
			//check downward vertical edge
			if (!found && edgeRow!=4 && visitCol[edgeRow,edgeCol+1] == true){
				found = findNextHorizontalEdge(edgeRow,edgeCol+1,colReached,foundRow);
			}
			
			//check backward downward
			if(!found && edgeRow!=4 && visitCol[edgeRow,edgeCol] == true){
				found = findNextHorizontalEdge(edgeRow,edgeCol,colReached-1,foundRow);
			}
			
			//check upward vertical edge
			if (!found && edgeRow !=0 && visitCol[edgeRow-1,edgeCol+1] == true){
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
	
	private static bool findNextHorizontalEdge(int edgeRow, int edgeCol, int colReached, ArrayList foundRow){
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
	
	private static bool visit(Coordination cord, ArrayList list){
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
			blockedRow[i,j] = false;
		}
		//reset visitcol
		for (int i = 0; i< 4; i++)
		for (int j = 0; j <6; j++) {
			visitCol [i, j] = false;
			blockedCol[i,j] =false;
		}
		stepCount = 0;
		checkWin = false;
		checkmate = false;
		checkmateCol = false;
		skipLeft = false;
		skipUp = false;
		allMyMoves.Clear ();
		allBlueMoves.Clear ();
		shannon.reset ();
	}

	private class LastMove{
		private int row;
		private int col;
		private string role;

		public LastMove(int myRow, int myCol, string myRole){
			row = myRow;
			col = myCol;
			role = myRole;
		}

		public int getRow(){
			return row;
		}

		public int getCol(){
			return col;
		}

		public string getRole(){
			return role;
		}

		public override bool Equals(System.Object obj){
			return ((LastMove)obj).getRow () == row && ((LastMove)obj).getCol () == col && ((LastMove)obj).getRole ().Equals (role);
		}
	}
}
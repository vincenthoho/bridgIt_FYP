using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class levelModifier : MonoBehaviour {
	private static string[] multi_limitBridge = {"No", "7", "10", "13"};
	private static int index_limitBridge = 0;

	public GameObject construct_limitBridge;

	public void callToLevel(int lv){
		aiMode_init.reset ();
		aiMode_init.setLevel (lv);
		aiMode_init.aiFirst = menu_init.aiFirst;
		if (menu_init.limitMoveMode)
			callToLevel_limitMove (lv);

		switch (lv) {
		case 1:
			setBoardSize (0);
			break;
		case 2:
			setBoardSize (1);
			break;
		case 3:
			setBoardSize (0);
			setLimitBridge (7);
			break;
		case 4:
			setBoardSize (1);
			setLimitBridge (10);
			break;
		case 5:
			setBoardSize (1);
			setLimitBridge (13);
			setMustConnect (4);
			break;
		case 6:
			setBoardSize (1);
			setLimitBridge (10);
			setMustConnect (3);
			break;
        case 7:
            setBoardSize(1);
            setLimitBridge(10);
            setEraseNodes(1);
            break;
        case 8:
            setBoardSize(1);
            setLimitBridge(10);
            setEraseNodes(2);
            break;
        }
	}

	public void callToLevel_limitMove(int lv){
		switch (lv) {
		case 1:
			setLimitMoves (9);
			break;
		case 2:
			setLimitMoves (11);
			break;
		case 3:
			setLimitMoves (9);
			break;
		case 4:
			setLimitMoves (11);
			break;
		case 5:
			setLimitMoves (15);
			break;
		case 6:
			setLimitMoves (12);
			break;
        case 7:
            setLimitMoves(11);
            break;
        case 8:
            setLimitMoves(15);
            break;
        }
	}

	public void goToNextLevel(){
		int tmp_level = aiMode_init.level+1;
		//aiMode_init.reset ();
		//aiMode_init.setLevel (tmp_level);
		Application.LoadLevel ("scene_ai");
		callToLevel (tmp_level);
	}

	public void replayLevel(){
		int tmp_level = aiMode_init.level;
		//aiMode_init.reset ();
		//aiMode_init.setLevel (tmp_level);
		Application.LoadLevel ("scene_ai");
		callToLevel (tmp_level);
	}

	public void setLimitMoves(int move){
		aiMode_init.setLimitMoves (move);
	}
	
	public void setBoardSize(int size){
		aiMode_init.setBoardSize (size);
	}

	public void setLimitBridge(int bridgesNo){
		aiMode_init.setLimitBridge (bridgesNo);
	}

	public void setMustConnect(int mustNodesNo){
		aiMode_init.setMustConnect (mustNodesNo);
	}

    public void setEraseNodes(int eraseNodesNo){
        aiMode_init.setEraseNodes(eraseNodesNo, "blue", blueEdgeRespond.erasedNode);
        aiMode_init.setEraseNodes(eraseNodesNo, "red", aiRespond.erasedNode);
    }

	public void construct_left(){
		if (index_limitBridge - 1 >= 0) {
			index_limitBridge--;
			construct_limitBridge.GetComponent<Text> ().text = getMulti_limitBridge();
		}
	}

	public void construct_right(){
		if (index_limitBridge + 1 < multi_limitBridge.Length) {
			index_limitBridge++;
			construct_limitBridge.GetComponent<Text> ().text = getMulti_limitBridge();
		}
	}

	public static string getMulti_limitBridge(){
		return multi_limitBridge [index_limitBridge];
	}

	public static void reset(){
		index_limitBridge = 0;
	}
}

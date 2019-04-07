using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class buttonHandler : MonoBehaviour {
	private int page = 0;
	public GameObject backBtn, forwardBtn;
	public Sprite[] instructionPage = new Sprite[2];

	public static void setAiFirst(bool first){
		aiMode_init.aiFirst = first;
	}

	public void changeSceneTo(string sceneName){
		Application.LoadLevel (sceneName);
		if (sceneName == "scene_menu") 
			menu_init.aiFirst = aiMode_init.aiFirst;
	}

	public void goToLevel_single(int level){
		Application.LoadLevel ("scene_ai");
		aiMode_init.level = level;
		aiMode_init.aiFirst = menu_init.aiFirst;
	}

	public void goToLevel_multi(int level){
		Application.LoadLevel ("scene_multi");
		multi_init.level = level;
		if (!levelModifier.getMulti_limitBridge ().Equals ("No")) {
			multi_init.setLimitBridges (int.Parse (levelModifier.getMulti_limitBridge ()));
			levelModifier.reset ();
		}
	}

	public void openPanel(GameObject panel){
		panel.SetActive (true);
		timer.setTimer (false);
	}

	public void closePanel(GameObject panel){
		panel.SetActive (false);
		timer.setTimer (true);
	}

	public void hidePanel(GameObject panel){
		panel.SetActive (false);
	}

	public void hideButton(GameObject btn){
		btn.SetActive (false);
	}

	public void switchButton(GameObject anotherButton){
		anotherButton.SetActive (true);
	}

	public void pageForward(GameObject panel){
		if (page+1 < instructionPage.Length)
			page++;
		panel.GetComponent<Image> ().sprite = instructionPage [page];
		backBtn.SetActive (true);
		if (page >= instructionPage.Length-1)
			forwardBtn.SetActive (false);
	}

	public void pageBack(GameObject panel){
		if (page-1 >= 0)
			page--;
		panel.GetComponent<Image> ().sprite = instructionPage [page];
		forwardBtn.SetActive (true);
		if (page <= 0)
			backBtn.SetActive (false);
	}

	public void toogleFirst(){
		menu_init.aiFirst = this.GetComponent<Toggle>().isOn;
	}

	public void redo(){
		aiMode_init.redo ();
	}
}

using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour 
{
	public GameObject Loading;

	static public MenuManager Instance { get; private set; }

	void Start()
	{
		Instance = this;

		Screen.autorotateToPortrait	= true;
		Screen.autorotateToPortraitUpsideDown = true;
		Screen.autorotateToLandscapeLeft = false;
		Screen.autorotateToLandscapeRight = false;

		/*
		if (PersistentData.Instance().Data != null)
		{
			InitStruct initStruct = PersistentData.Instance().Data as InitStruct;

			//GameObject.Find("CBSwap").GetComponentInChildren<MenuCheckBox>().State = initStruct.propSwap;
			//GameObject.Find("CBTopFall").GetComponentInChildren<MenuCheckBox>().State = initStruct.propTopFall;
		}*/
	}

	public void LoadLevel(int index)
	{
		StartCoroutine (LoadLevelCor(index));
	}

	IEnumerator LoadLevelCor(int index)
	{
		GameObject go = (GameObject)GameObject.Instantiate(Loading);
		go.SetActive(true);

		yield return null;
		
		InitStruct init = new InitStruct(index, 
		                                 false, true
		                                 //GameObject.Find("CBSwap").GetComponentInChildren<MenuCheckBox>().State,
		                               //  GameObject.Find("CBTopFall").GetComponentInChildren<MenuCheckBox>().State
		                                 );
		
		PersistentData.Instance().Data = init;

		string sceneName = "puzzle";
		if(index == -1)
			sceneName = "levelsMap";

		Application.LoadLevel(sceneName);
	}
}

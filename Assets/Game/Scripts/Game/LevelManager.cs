using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
	public GameObject Loading;
	static public LevelManager Instance { get; private set; }

	[SerializeField]
	private List<LevelButton> LevelButtons;

	[SerializeField]
	private GameObject MapPanel;

	private bool _swap = false;
	private bool _topFall = true;

	void Start () 
	{
		//PlayerPrefs.DeleteAll();

		Instance = this;

		// read level settings testing data
		if (PersistentData.Instance().Data != null)
		{
			InitStruct initStruct = PersistentData.Instance().Data as InitStruct;
			
			_swap = initStruct.propSwap;
			_topFall = initStruct.propTopFall;
		}

		// read finished levels
		int levelsCount = 1;
		if (!PlayerPrefs.HasKey (Constants.LevelsFinished)) 
		{
			PlayerPrefs.SetInt (Constants.LevelsFinished, levelsCount);
			PlayerPrefs.Save();
		} 
		else 
		{
			levelsCount = PlayerPrefs.GetInt (Constants.LevelsFinished);
		}

		// setup level buttons
		for (int i = 0; i < LevelButtons.Count; ++i)
		{
			if(i < levelsCount)
			{
				if(i == levelsCount -1)
					LevelButtons[i].ActivateButton();
				else
					LevelButtons[i].FinishButton();
			}
			else
			{
				LevelButtons[i].LockButton();
			}
		}

		// set scroll position to active button
		Vector3 mapPos = MapPanel.transform.position;

		float newY = mapPos.y - LevelButtons[levelsCount -1].transform.position.y;
		if(newY < mapPos.y && newY > -mapPos.y)
			MapPanel.transform.position = new Vector3(mapPos.x, newY, mapPos.z);
		else if(newY < -mapPos.y)
			MapPanel.transform.position = new Vector3(mapPos.x, -mapPos.y, mapPos.z);
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

		string sceneName = "puzzle";
		if(index == -1)
		{
			sceneName = "menu";
		}
		else
		{
			InitStruct init = new InitStruct(index, _swap, _topFall);
			PersistentData.Instance().Data = init;
		}

		Application.LoadLevel(sceneName);
	}
}

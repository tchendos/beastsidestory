using UnityEngine;
using System.Collections;

public class OpenSector : MonoBehaviour 
{
	public int OpenSectorAtLevel;

	[SerializeField]
	private GameObject _fog;

	void Start () 
	{
		int levelCount = 1;
		if(PlayerPrefs.HasKey(Constants.LevelsFinished))
			levelCount = PlayerPrefs.GetInt (Constants.LevelsFinished);

		_fog.SetActive(levelCount < OpenSectorAtLevel);
	}


}

using UnityEngine;
using System.Collections;

public class MenuButton : Button 
{
	public int levelIndex;

	override protected void Clicked()
	{
		MenuManager.Instance.LoadLevel(levelIndex);
	}
}

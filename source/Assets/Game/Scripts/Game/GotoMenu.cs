using UnityEngine;
using System.Collections;

public class GotoMenu : Button 
{
	public GameObject Loading;

	override protected void Clicked()
	{
		StartCoroutine(LoadMenu());
	}

	IEnumerator LoadMenu()
	{
		GameObject go = (GameObject)GameObject.Instantiate(Loading);
		go.SetActive(true);
		
		yield return null;
		
		Application.LoadLevel("menu");
	}
}

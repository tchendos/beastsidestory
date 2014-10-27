using UnityEngine;
using System;

public class PersistentData:MonoBehaviour
{
	public object Data;

	private static PersistentData _instance;
	
	public static PersistentData Instance()
	{
		if (_instance == null)
		{
			_instance = GameObject.FindObjectOfType(typeof(PersistentData)) as PersistentData;
			if (_instance == null)
			{
				GameObject container = new GameObject();
				container.name = "PersistentDataContainer";
				_instance = container.AddComponent(typeof(PersistentData)) as PersistentData;
				DontDestroyOnLoad(container);
			}
		}
		return _instance;
	}
}

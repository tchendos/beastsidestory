using UnityEngine;
using System.Collections;

public class SortingLayer : MonoBehaviour
{
	[SerializeField]
	private bool _defaultSet = false;
	[SerializeField]
	private int _defaultSortingLayerId = 0;
	[SerializeField]
	private int _defaultSortingOrder = 0;

	public bool IsDefaultSet { get { return _defaultSet; } }
	public int DefaultSortingLayerId { get { return _defaultSortingLayerId; } private set { _defaultSortingLayerId = value; } }
	public int DefaultSortingOrder { get { return _defaultSortingOrder; } private set { _defaultSortingOrder = value; } }

	public static void ForceLayerID(GameObject go, string layerName)
	{
		if (go.renderer != null)
			go.renderer.sortingLayerName = layerName;
		for(int i = 0; i < go.transform.childCount; i++)
			ForceLayerID(go.transform.GetChild(i).gameObject, layerName);
	}

	public void SetDefaults(int layerId, int sortingOrder)
	{
		SetDefaults(layerId, sortingOrder, true, true);
	}
	
	private void SetDefaults(int layerId, int sortingOrder, bool setLayer, bool setOrder)
	{
		_defaultSet = true;
		if (setLayer)
			DefaultSortingLayerId = layerId;
		if (setOrder)
			DefaultSortingOrder = sortingOrder;
		
		Renderer renderer = gameObject.GetComponent<Renderer>();
		if (renderer)
		{
			if (setLayer && renderer.sortingLayerID != layerId) 
				renderer.sortingLayerID = layerId;
			if (setOrder && renderer.sortingOrder != sortingOrder) 
				renderer.sortingOrder = sortingOrder;
		}
	}
}

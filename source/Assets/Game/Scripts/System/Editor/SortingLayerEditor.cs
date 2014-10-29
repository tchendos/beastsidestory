using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[ExecuteInEditMode]
[CustomEditor(typeof(SortingLayer))]
public class SortingLayerEditor : UnityEditor.Editor
{
	private List<string> _layerNames = new List<string>();
	private List<int> _layerIds = new List<int>();
	private UnityEngine.GameObject _rootGameObj;
	private int _oldSortingOrder = 0;
	private int _oldSortingLayerIdx = 0;
	private SortingLayer _target;

	public override void OnInspectorGUI()
	{
		Renderer renderer = _rootGameObj.renderer;
		
		if (renderer)
		{
			_oldSortingLayerIdx = _layerIds.FindIndex(x => x == renderer.sortingLayerID);
			_oldSortingOrder = renderer.sortingOrder;
		}
		else
			EditorGUILayout.LabelField("No renderer found on this gameObject.");
		
		if (_layerNames.Count == 0)
		{
			EditorGUILayout.LabelField("Can't get renderer information!");
			return;
		}
		
		int newlayerIdx = EditorGUILayout.Popup("Sorting Layer ", _oldSortingLayerIdx, _layerNames.ToArray());
		if (renderer && newlayerIdx != _oldSortingLayerIdx) 
		{
			Undo.RecordObject(renderer, "Edit Sorting Layer ID");
			renderer.sortingLayerName = _layerNames[newlayerIdx];
			EditorUtility.SetDirty(renderer);
		}
		_oldSortingLayerIdx = newlayerIdx;
		
		int newSortingLayerOrder = EditorGUILayout.IntField("Sorting Layer Order", _oldSortingOrder);
		if (renderer && newSortingLayerOrder != _oldSortingOrder) 
		{
			Undo.RecordObject(renderer, "Edit Sorting Order");
			renderer.sortingOrder = newSortingLayerOrder;
			EditorUtility.SetDirty(renderer);
		}
		_oldSortingOrder = newSortingLayerOrder;
		
		if (newlayerIdx >= 0)
			_target.SetDefaults(_layerIds[newlayerIdx], newSortingLayerOrder);
	}

	private void OnEnable()
	{
		_target = target as SortingLayer;
		_rootGameObj = _target.gameObject;
		
		Renderer renderer = _rootGameObj.renderer;
		if (!renderer)
		{
			Renderer[] renderers = _rootGameObj.GetComponentsInChildren<Renderer>(true);
			if (renderers.Length > 0)
				renderer = renderers[0];
		}
		
		Renderer tmpRenderer = null;
		if (!renderer)
		{
			tmpRenderer = _rootGameObj.AddComponent<SpriteRenderer>();
			renderer = tmpRenderer;
		}
		
		int origLayerId = renderer.sortingLayerID;
		
		for (int i = 0; i < 100; ++i)
		{
			renderer.sortingLayerID = i;
			string name = renderer.sortingLayerName;
			if (i == 0 && name == "") // this is default
				name = "Default";
			if (name != "")
			{
				_layerNames.Add(name);
				_layerIds.Add(renderer.sortingLayerID);
			}
		}
		
		renderer.sortingLayerID = origLayerId;
		
		if (_target.IsDefaultSet)
		{
			_oldSortingLayerIdx = _layerIds.FindIndex(x => x == _target.DefaultSortingLayerId);
			_oldSortingOrder = _target.DefaultSortingOrder;
		}
		else
		{
			_oldSortingLayerIdx = _layerIds.FindIndex(x => x == renderer.sortingLayerID);
			_oldSortingOrder = _rootGameObj.renderer ? _rootGameObj.renderer.sortingOrder : renderer.sortingOrder - 1;
		}
		
		if (tmpRenderer)
			DestroyImmediate(tmpRenderer);
	}
}

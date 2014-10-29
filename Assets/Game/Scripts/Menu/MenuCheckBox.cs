using UnityEngine;
using System.Collections;

public class MenuCheckBox : MonoBehaviour 
{
	GameObject _cb;
	bool _state;

	public bool State 
	{
		get
		{
			return _state;
		}
		set
		{
			if (_state != value)
			{
				_state = value;
				if (_cb != null)
					_cb.SetActive(_state);
			}
		}

	}
	// Use this for initialization
	void Start () 
	{
		_cb = transform.FindChild("CheckBox").gameObject;
		if (_cb != null)
			_state = _cb.activeSelf;
	}
	
	public void OnMouseUp()
	{
		State = !State;
	}
}

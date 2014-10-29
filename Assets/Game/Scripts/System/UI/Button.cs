using UnityEngine;
using System.Collections;

public abstract class Button : MonoBehaviour 
{
	Collider2D _collider;
	bool _mouseDown;

	void Awake()
	{
		_collider = GetComponent<Collider2D>();
	}

	bool CheckMouse()
	{
		Vector2 wPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		return _collider.OverlapPoint(wPos);
	}
	void OnMouseDown()
	{
		if (CheckMouse())
		{
			_mouseDown = true;
		}
		else _mouseDown = false;
	}
	void OnMouseUp()
	{
		if (_mouseDown)
		{
			if (CheckMouse())
			{
				Clicked ();
			}
		}
	}
	abstract protected void Clicked();
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Controller:MonoBehaviour
{
	const float TAP_TIMEOUT = 0.8f;
	const float TAP_DISTANCE_THRESHOLD_SQR = 20 * 20;
	Camera _camera;
	float _distance;

	bool _state;

	bool _dragging;
	Vector3 _dragStartWorld;
	Vector2i _dragStartGrid;
	float _tapTimeOut;

	public void SetView(Camera camera, float distance) { _camera = camera; _distance = distance; }
	public Camera GetCamera() { return _camera; }

	public void SetInputState(bool state) 
	{
		_state = state;
		if (!_state)
			_dragging = false;
	}

	Vector3 ScreenToWorld(Vector3 position)
	{
		position.z = _distance;
		return _camera.ScreenToWorldPoint(position);
	}

	Vector2i WorldToGrid(Vector3 position)
	{
		return new Vector2i((int)((position.x - Constants.FIELD_RECT.lt.x) / Constants.FIELD_GRID_X),
		                    Constants.FIELD_SIZE_Y - 1 - (int)((position.y - Constants.FIELD_RECT.lt.y) / Constants.FIELD_GRID_Y));
	}

	void Update()
	{
		if (_camera == null || !_state)
			return;

		/*if (Input.multiTouchEnabled)
		{
			if (Input.touchCount > 0)
			{
			}
		}
		else*/
		if (Input.GetMouseButtonDown(0))
		{
			_dragStartWorld = ScreenToWorld(Input.mousePosition);
			if (_dragStartWorld.x >= Constants.FIELD_RECT.lt.x && _dragStartWorld.y >= Constants.FIELD_RECT.lt.y &&
			    _dragStartWorld.x < Constants.FIELD_RECT.rb.x && _dragStartWorld.y < Constants.FIELD_RECT.rb.y)
			{
				_tapTimeOut = Time.time + TAP_TIMEOUT;
				_dragStartGrid = WorldToGrid(_dragStartWorld);
				_dragging = true;
			}
			else _dragging = false;
			//Debug.Log("GetMouseButtonDown: " + Input.mousePosition + " / " + _dragStartWorld + " / " + _dragStartGrid + " / " + _dragging);
		}
		else if (Input.GetMouseButton(0) && _dragging)
		{
			Vector3 delta = ScreenToWorld(Input.mousePosition) - _dragStartWorld;
			if (Mathf.Abs(delta.x) > (Constants.FIELD_GRID_X / 2) ||
			    Mathf.Abs(delta.y) > (Constants.FIELD_GRID_Y / 2))
			{
				if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
				{
					Vector2i newPos;
					newPos.x = delta.x < 0 ? (_dragStartGrid.x - 1) : (_dragStartGrid.x + 1);
					newPos.y =_dragStartGrid.y;
					if (Grid.Valid(newPos))
					{
						_dragging = false;
						StartCoroutine(Puzzle.Swipe(newPos, _dragStartGrid));
					}
				}
				else 
				{
					Vector2i newPos;
					newPos.x = _dragStartGrid.x;
					newPos.y = delta.y < 0 ? (_dragStartGrid.y + 1) : (_dragStartGrid.y - 1);
					if (Grid.Valid(newPos))
					{
						_dragging = false;
						StartCoroutine(Puzzle.Swipe(newPos, _dragStartGrid));
					}
				}
			}
		}

		if (Input.GetMouseButtonUp(0))
		{
			if (_dragging && _tapTimeOut > Time.time)
			{
				Vector3 tapDelta = _dragStartWorld - ScreenToWorld(Input.mousePosition);
				if (tapDelta.sqrMagnitude < TAP_DISTANCE_THRESHOLD_SQR)
				{
					StartCoroutine(Puzzle.Tap(_dragStartGrid));
				}
			}
		}

		if (Input.GetMouseButtonUp(1))
		{
			Vector3 posWorld = ScreenToWorld(Input.mousePosition);
			if (posWorld.x >= Constants.FIELD_RECT.lt.x && posWorld.y >= Constants.FIELD_RECT.lt.y &&
			    posWorld.x < Constants.FIELD_RECT.rb.x && posWorld.y < Constants.FIELD_RECT.rb.y)
			{
				Vector2i posGrid = WorldToGrid(posWorld);

				//StartCoroutine(Puzzle.DestroyGem(posGrid));
				StartCoroutine(Puzzle.ChangeGem(posGrid, GameManager.Instance.LevelData.GetCrossGemType()));
			}
		}

		if (!Input.GetMouseButton(0))
			_dragging = false;
	}
}

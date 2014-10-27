using System;
using UnityEngine;

[ExecuteInEditMode]
public class DoubleSlider:MonoBehaviour
{
	public float ChangeWait = 1.0f;
	public float ChangeSpeed = 0.5f;

	public SpriteSlider TopSlider;
	public SpriteSlider BottomSlider;

	bool _diff = true;

	float _value;
	float _min;
	float _max;

	float _lastChangeTime;

	public float sliderValue
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				if (value > _max)
				{
					_diff = true;
					_max = value;
					if (BottomSlider != null)
						BottomSlider.sliderValue = value;
				}
				if (value < _min)
				{
					_diff = true;
					_min = value;
					if (TopSlider != null)
						TopSlider.sliderValue = value;
				}
				_value = value;
				_lastChangeTime = Time.time;
			}
		}
	}

	void Awake()
	{
		sliderValue = 1.0f;
		Instant();
	}

	public void Instant()
	{
		_diff = false;
		_min = _max = _value;
		if (TopSlider != null)
			TopSlider.sliderValue = _value;
		if (BottomSlider != null)
			BottomSlider.sliderValue = _value;
	}

	void Update()
	{
		if (_diff && Time.time > _lastChangeTime + ChangeWait)
		{
			bool diff = false;
			if (_min != _value)
			{
				float delta = _value - _min;
				if (delta > Time.deltaTime * ChangeSpeed)
				{
					_min += Time.deltaTime * ChangeSpeed; diff = true;
				}
				else _min = _value;
				if (TopSlider != null)
					TopSlider.sliderValue = _min;
			}
			if (_max != _value)
			{
				float delta = _max - _value;
				if (delta > Time.deltaTime * ChangeSpeed)
				{
					_max -= Time.deltaTime * ChangeSpeed; diff = true;
				}
				else _max = _value;
				if (BottomSlider != null)
					BottomSlider.sliderValue = _max;
			}
			_diff = diff;
		}
	}
}

using UnityEngine;

namespace HUD
{
	public class Health : MonoBehaviour
	{
		int _current;

		bool _updateVisual;

		DoubleSlider _slider;
		ShadowLabel _label;

		LevelData _levelData;

		void Awake () 
		{
			_levelData = GameManager.Instance.LevelData;

			_current = _levelData.GetHealth();

			_slider = GameManager.Instance.Definitions.HudHealthBar.GetComponentInChildren<DoubleSlider>();
			_slider.sliderValue = 1.0f;
			_label = GameManager.Instance.Definitions.HudHealthLabel.GetComponentInChildren<ShadowLabel>();
			_label.text = _levelData.GetHealth().ToString() + "/" + _levelData.GetHealth().ToString();

			_updateVisual = true;
		}

		public int Get()
		{
			return _current;
		}

		public void Add(int health)
		{
			_current += health;
			if (_current < 0)
				_current = 0;
			else if (_current > _levelData.GetHealth())
				_current = _levelData.GetHealth();

			Set(_current);
		}
		
		public Vector3 GetBarCenter()
		{
			return _slider.transform.position;
		}

		public void Set(int value)
		{
			_current = value;

			if (_updateVisual)
			{
				float newValue = (float)value / _levelData.GetHealth();
				bool change = _slider.sliderValue != newValue;

				_slider.sliderValue = newValue;
				_label.text = value.ToString() + "/" + _levelData.GetHealth().ToString();

				if (change)
					StartCoroutine(Effects.Shake(GameManager.Instance.Definitions.HudHeart, GameManager.Instance.Definitions.ShakeTime));
			}
		}

		public void SetUpdateVisual(bool state)
		{
			_updateVisual = state;
			if (state)
				Set(_current);
		}
	}
}

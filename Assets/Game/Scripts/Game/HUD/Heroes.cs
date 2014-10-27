using UnityEngine;
using System.Collections.Generic;

namespace HUD
{
	public class Heroes : MonoBehaviour
	{
		const int MAXHEROES = 3;

		struct Hero
		{
			public int _value;
			public bool _isActive;

			public GameObject _active;
			public GameObject _inActive;
			public GameObject _hero;

			public DoubleSlider _slider;
			public ShadowLabel _label;
		};

		Hero[] _heroes;

		bool _updateVisual;
		
		private LevelData _levelData;

		void Awake () 
		{
			_levelData = GameManager.Instance.LevelData;
			_updateVisual = true;

			int count = _levelData.GetHeroCount();
			DebugUtils.Assert(count <= MAXHEROES);
			_heroes = new Hero[count];

			for (int i = 0; i < count; i++)
			{
				GameObject go = GameManager.Instance.Definitions.HeroHUDPrefabs[i];
				_heroes[i]._slider = go.GetComponentInChildren<DoubleSlider>();
				_heroes[i]._label = go.GetComponentInChildren<ShadowLabel>();
				_heroes[i]._active = go.transform.FindChild("Active").gameObject;
				_heroes[i]._inActive = go.transform.FindChild("InActive").gameObject;
				_heroes[i]._hero = _heroes[i]._inActive.transform.FindChild("Hero").gameObject;

				Set(i, 0);
				SetState(i, true);
			}
			for (int i = count; i < MAXHEROES; i++)
			{
				GameObject go = GameManager.Instance.Definitions.HeroHUDPrefabs[i];
				go.SetActive(false);
			}
		}

		public void SetUpdateVisual(bool state)
		{
			_updateVisual = state;
			if (state)
			{
				for (int i = 0; i < _levelData.GetHeroCount(); i++)
				{
					Set(i, _heroes[i]._value);
				}
			}
		}

		public Vector3 GetPosition(int index)
		{
			return _heroes[index]._hero.transform.position;
		}
		
		public void Set(int index, int value)
		{
			_heroes[index]._value = value;
			if (_updateVisual)
			{
				float newValue = ((float)value) / _levelData.GetHeroInfo(index)._pointsToSpawn;
				bool change = _heroes[index]._slider.sliderValue != newValue;
				_heroes[index]._slider.sliderValue = newValue;
				_heroes[index]._label.text = value.ToString();
				if (change && _heroes[index]._isActive)
					StartCoroutine(Effects.Shake(GameManager.Instance.Definitions.HeroHUDPrefabs[index], GameManager.Instance.Definitions.ShakeTime));
			}
		}
		
		public void SetState(int index, bool state)
		{
			_heroes[index]._isActive = state;

			_heroes[index]._inActive.SetActive(state);
			_heroes[index]._active.SetActive(!state);
			_heroes[index]._slider.Instant();
		}
	}
}

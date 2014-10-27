using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HUD
{
	public class Manager : MonoBehaviour
	{
		public Health Health { get; private set; }
		public Coins Coins { get; private set; }
		public Heroes Heroes { get; private set; }
		public Waves Waves { get; private set; }
		public Overlay Overlay { get; private set; }

		ShadowLabel _multiplierLabel;
		
		bool _updateVisual;

		void Start () 
		{
			Health = gameObject.AddComponent<Health>();
			Coins = gameObject.AddComponent<Coins>();
			Heroes = gameObject.AddComponent<Heroes>();
			Waves = gameObject.AddComponent<Waves>();
			Overlay = gameObject.AddComponent<Overlay>();

			_updateVisual = true;
		}

		public void SetUpdateVisual(bool state)
		{
			if (_updateVisual != state)
			{
				_updateVisual = state;

				Health.SetUpdateVisual(state);
				Coins.SetUpdateVisual(state);
				Heroes.SetUpdateVisual(state);
			}
		}
	}
}

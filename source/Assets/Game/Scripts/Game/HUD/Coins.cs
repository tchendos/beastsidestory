using UnityEngine;

namespace HUD
{
	public class Coins : MonoBehaviour
	{
		int _current;
		bool _updateVisual;

		ShadowLabel _label;

		void Awake () 
		{
			_current = 0;
			_label = GameManager.Instance.Definitions.HudCoinsLabel.GetComponentInChildren<ShadowLabel>();
			_label.text = "000";

			_updateVisual = true;
		}

		public void Add(int coins)
		{
			_current += coins;

			Set(_current);
		}
		
		public void Set(int coins)
		{
			_current = coins;

			if (_updateVisual)
				_label.text = coins.ToString("D3");
		}
		
		public Vector3 GetCenter()
		{
			return _label.transform.position;
		}
		
		public void SetUpdateVisual(bool state)
		{
			_updateVisual = state;
			if (state)
				Set(_current);
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HUD
{
	public class Overlay : MonoBehaviour
	{
		ShadowLabel _multiplierLabel;
		
		GameObject _overlayWin;
		GameObject _overlayLose;

		void Awake () 
		{
			_overlayWin = GameManager.Instance.Definitions.HudOverlayWin;
			_overlayWin.SetActive(false);

			_overlayLose = GameManager.Instance.Definitions.HudOverlayLose;
			_overlayLose.SetActive(false);
		}

		public void Show(bool win)
		{
			if (win)
			{
				_overlayWin.SetActive(true);
			}
			else
			{
				_overlayLose.SetActive(true);
			}
		}

		public IEnumerator ShowMultiplier(int multiplier)
		{
			GameManager.Instance.Definitions.HudMultiplier.SetActive(true);
			if (_multiplierLabel == null)
				_multiplierLabel = GameManager.Instance.Definitions.HudMultiplier.GetComponentInChildren<ShadowLabel>();
			_multiplierLabel.text = multiplier.ToString() + "x";

			float time = GameManager.Instance.Definitions.MultiplierTime;
			Effects.TweenColor(GameManager.Instance.Definitions.HudMultiplier, time, new Color(1, 1, 1, 0), Tweener.Method.EaseIn);
			ScaleTween tweenScale = ScaleTween.Begin(GameManager.Instance.Definitions.HudMultiplier, time, Vector3.one * 4);
			tweenScale.method = Tweener.Method.EaseIn;
			yield return new WaitForSeconds(time);

			Effects.TweenColor(GameManager.Instance.Definitions.HudMultiplier, 0, Color.white, Tweener.Method.EaseIn);
			ScaleTween.Begin(GameManager.Instance.Definitions.HudMultiplier, 0, Vector3.one);
			GameManager.Instance.Definitions.HudMultiplier.SetActive(false);
		}
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Effects:MonoBehaviour
{
	static public IEnumerator Sparkle(float time, GameObject particleDanger, Vector3 position)
	{
		GameObject p2 = (GameObject)Instantiate(particleDanger);
		p2.transform.parent = GameManager.Instance.Definitions.FieldRoot.transform;
		p2.transform.position = position;
		p2.transform.localScale = new Vector3(1,1,1);
		p2.SetActive(true);
		
		yield return new WaitForSeconds(time);
		
		GameObject.Destroy(p2);
	}

	static public IEnumerator Pulse(float timeFly, float timeFlyWait, float timeHit, 
	                                GameObject particleBegin, GameObject particleHit, GameObject particleDanger, 
	                                Vector3 startPosition, Vector3 destPosition, bool fly)
	{
		GameObject p = (GameObject)Instantiate(particleHit);
		p.transform.parent = GameManager.Instance.Definitions.FieldRoot.transform;
		p.transform.position = startPosition;
		p.transform.localScale = new Vector3(1,1,1);
		p.SetActive(true);

		GameObject p1 = null;
		if (particleBegin != null)
		{
			p1 = (GameObject)Instantiate(particleBegin);
			p1.transform.position = startPosition;
			p1.transform.localScale = new Vector3(1,1,1);
			p1.SetActive(true);
		}

		PositionTween tweenPosition;
		if (fly)
		{
			Vector3 destPositionLocal = p.transform.worldToLocalMatrix.MultiplyVector(destPosition);
			
			tweenPosition = PositionTween.Begin(p, timeFly, destPositionLocal);
			tweenPosition.method = Tweener.Method.EaseIn;
		}

		yield return new WaitForSeconds(timeFlyWait);

		GameObject p2 = (GameObject)Instantiate(particleDanger);
		p2.transform.parent = GameManager.Instance.Definitions.FieldRoot.transform;
		p2.transform.position = destPosition;
		p2.transform.localScale = new Vector3(1,1,1);
		p2.SetActive(true);
		
		yield return new WaitForSeconds(timeHit);

		if (p1 != null)
		{
			GameObject.Destroy(p1);
		}
		GameObject.Destroy(p);
		GameObject.Destroy(p2);
	}

	public static void TweenColor(GameObject go, float duration, Color color, Tweener.Method method)
	{
		if (go.activeSelf && go.renderer != null)
		{
			ColorTween tweenColor = ColorTween.Begin(go, duration, color);
			tweenColor.method = method;
			if (duration == 0)
				tweenColor.color = color;
		}
		
		for(int i = 0; i < go.transform.childCount; i++)
			TweenColor(go.transform.GetChild(i).gameObject, duration, color, method);
	}

	public static void ChangeMaterial(GameObject go, Material material)
	{
		if (go.activeSelf && go.renderer != null)
		{
			go.renderer.material = material;
		}
		
		for(int i = 0; i < go.transform.childCount; i++)
			ChangeMaterial(go.transform.GetChild(i).gameObject, material);
	}

	public static IEnumerator Shake(GameObject go, float time)
	{
		float pTime = time / 4;
		Vector3 position = go.transform.localPosition;
		PositionTween tweenPosition = PositionTween.Begin(go, pTime, position + new Vector3(-10, 0, 0));
		tweenPosition.method = Tweener.Method.EaseOut;
		tweenPosition.steeperCurves = true;

		yield return new WaitForSeconds(pTime);

		tweenPosition = PositionTween.Begin(go, pTime, position + new Vector3(10, 0, 0));
		tweenPosition.method = Tweener.Method.Linear;

		yield return new WaitForSeconds(pTime * 2);
		
		tweenPosition = PositionTween.Begin(go, pTime, position);
		tweenPosition.method = Tweener.Method.Linear;

		yield return new WaitForSeconds(pTime);
	}
}

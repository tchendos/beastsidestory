using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Gem : MonoBehaviour 
{
	public const string SortLayer1 = "Default";
	public const string SortLayer2 = "FieldMiddle";
	public const string SortLayer3 = "FieldTop";
	public const string SortLayer4 = "HUDTop";

	private LevelData _levelData;
	
	private ShadowLabel _labelL;
	private ShadowLabel _labelR;
	private GameObject _glowEnemy;
	private GameObject _glowHero;

	private Vector2i _gridPos;
	private int _type;
	private int _data;
	private bool _isTop;
	private string _layerName;

	private bool _dead;

	private GameObject _visualPrefab;

	public Vector2i GetGridPosition() { return _gridPos; }
	public int GetGemType() { return _type; }
	public LevelData.GemEffectType GetEffectType() { return GameManager.Instance.LevelData.GetGemTypeInfo(_type)._effectType; }

	public void SetData(int data) { _data = data; }
	public int GetData() { return _data; }

	public bool IsDead() { return _dead; }
	public void Die() { _dead = true; }
	
	public void Init(Vector2i gridPos)
	{
		_levelData = GameManager.Instance.LevelData;

		_labelR = transform.Find("LabelR").GetComponent<ShadowLabel>();
		_labelL = transform.Find("LabelL").GetComponent<ShadowLabel>();
		_glowEnemy = transform.Find("GlowEnemy").gameObject;
		_glowHero = transform.Find("GlowHero").gameObject;
		DebugUtils.Assert(_labelR != null && _labelL != null && _glowEnemy != null && _glowHero != null);

		SetupPosition(gridPos, 0);

		GenerateGemType(GameManager.Instance.LevelData.GetStartGemIndices());

		DisableFadeOut();
	}

	public void SetupPosition(Vector2i gridPos, float time)
	{
		Vector3 position = Grid.CalculatePosition(gridPos);

		TweenPosition(time, position, Tweener.Method.EaseIn, false);

		_gridPos = gridPos;
	}
	
	public void GenerateGemType(HashSet<int> set)
	{
		int rnd = UnityEngine.Random.Range (0, set.Count);
		foreach(int index in set)
		{
			if (rnd == 0)
			{
				SetGemType(index);
				break;
			}
			rnd--;
		}
	}
	
	public void SetGemType(int typeIndex)
	{
		DisableFadeOut();

		// remove rest
		if (_visualPrefab != null)
			_levelData.ReturnGemPrefab(_type, _visualPrefab);

		_type = typeIndex;

		// instantiate rest
		_visualPrefab = _levelData.LendGemPrefab(_type);
		Vector3 scale = _visualPrefab.transform.localScale;
		_visualPrefab.SetActive(true);
		_visualPrefab.transform.parent = transform;
		_visualPrefab.transform.localPosition = new Vector3(0,0,0);
		_visualPrefab.transform.localRotation = new Quaternion();
		_visualPrefab.transform.localScale = scale;

		_dead = false;
		_isTop = false;

		_labelR.gameObject.SetActive(false);
		_labelL.gameObject.SetActive(false);
		_glowEnemy.SetActive(false);
		_glowHero.SetActive(false);
	}

	public IEnumerator ChangeGemType(int typeIndex, GameObject prefab)
	{
		float changeTime = GameManager.Instance.Definitions.ChangeGemWaitTime;

		if (prefab != null)
		{
			StartCoroutine(Effects.Sparkle(GameManager.Instance.Definitions.ChangeGemEffectTime, prefab, transform.position));
			yield return new WaitForSeconds(changeTime);
		}

		SetGemType(typeIndex);

		if (prefab != null)
			yield return new WaitForSeconds(changeTime);
	}

	public void SetOnTop()
	{
		_isTop = true;
		UpdateDepth();
	}

	public void ForceDepth(string layerName)
	{
		if (_layerName != layerName)
		{
			_layerName = layerName;
			SortingLayer.ForceLayerID(gameObject, layerName);
		}
	}
	
	public void UpdateDepth()
	{
		if (_isTop)
			ForceDepth(SortLayer2);
		else ForceDepth(SortLayer1);
	}
	
	public void EnableLabelR(Color color)
	{
		_labelR.gameObject.SetActive(true);
		_labelR.color = color;
	}
	
	public void EnableLabelL(Color color)
	{
		_labelL.gameObject.SetActive(true);
		_labelL.color = color;
	}
	
	public void SetLabelR(string text)
	{
		_labelR.text = text;
	}
	
	public void SetLabelL(string text)
	{
		_labelL.text = text;
	}
	public void SetGlowActive(bool state, bool hero)
	{
		if (hero)
			_glowHero.SetActive(state);
		else _glowEnemy.SetActive(state);
	}

	IEnumerator ReturnHero(float time)
	{
		int heroIndex = GetData();
		ForceDepth(Gem.SortLayer4);
		
		Color color = new Color(1, 1, 1, 0);
		TweenScale(time, Vector3.one * 2.0f, Tweener.Method.EaseIn);
		TweenPosition(time, GameManager.Instance.HUD.Heroes.GetPosition(heroIndex), Tweener.Method.EaseInOut, false);
		
		yield return new WaitForSeconds((time / 5) * 4);
			
		TweenColor(time / 5, color, Tweener.Method.EaseIn);

		GameManager.Instance.HUD.Heroes.SetState(heroIndex, true);
	}

	public void SetupFadeOut(float time)
	{
		if (GetEffectType() == LevelData.GemEffectType.ET_HERO)
		{
			StartCoroutine(ReturnHero(time));
		}
		else
		{
			TweenColor(time, new Color(1, 1, 1, 0), Tweener.Method.EaseIn);
			TweenScale(time, Vector3.one * 4, Tweener.Method.EaseIn);
			ForceDepth(SortLayer3);
		}
	}
	
	private void DisableFadeOut()
	{
		TweenColor(0, Color.white, Tweener.Method.EaseIn);
		TweenScale(0, Vector3.one, Tweener.Method.EaseIn);
		UpdateDepth();
	}
	
	public void TweenPosition(float duration, Vector3 pos, Tweener.Method method, bool steeperCurves)
	{
		PositionTween tweenPosition = PositionTween.Begin(gameObject, duration, pos);
		tweenPosition.method = method;
		tweenPosition.steeperCurves = steeperCurves;
	}
	public void TweenScale(float duration, Vector3 scale, Tweener.Method method)
	{
		ScaleTween tweenScale = ScaleTween.Begin(gameObject, duration, scale);
		tweenScale.method = method;
	}
	public void TweenColor(float duration, Color color, Tweener.Method method)
	{
		Effects.TweenColor(gameObject, duration, color, method);
	}
}

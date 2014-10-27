using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class HeroManager:MonoBehaviour
{
	private List<SpawnInfo> _spawnInfoList;
	
	struct HeroInfo
	{
		public Figure _script;

		public bool _spawned;
		public int _spawnPoints;
	}
	private HeroInfo[] _heroes;
	
	private LevelData _levelData;

	private bool _updateVisual;

	void Awake()
	{
		_updateVisual = true;

		_levelData = GameManager.Instance.LevelData;

		_spawnInfoList = new List<SpawnInfo>();
		_heroes = new HeroInfo[_levelData.GetHeroCount()];
	}

	public bool GetUpdateVisual()
	{
		return _updateVisual;
	}
	
	public void SetUpdateVisual(bool state)
	{
		if (_updateVisual != state)
		{
			_updateVisual = state;
			if (_updateVisual)
			{
				Figure figure;
				for(int j = 0; j < _levelData.GetHeroCount(); j++)
				{
					figure = _heroes[j]._script;
					if (figure)
						figure.ActionPoints = figure.ActionPoints;
				}
			}
		}
	}
	
	public Figure GetAtPosition(Vector2i pos)
	{
		Gem gem = GameManager.Instance.Grid.Get (pos);
		if (gem.GetEffectType() == LevelData.GemEffectType.ET_HERO)
		{
			return _heroes[gem.GetData()]._script;
		}
		return null;
	}
	
	public Vector3 GetPosition(int index)
	{
		if (_heroes[index]._script)
			return _heroes[index]._script.transform.position;
		else return GameManager.Instance.HUD.Heroes.GetPosition(index);
	}

	public bool IsAttack(Vector2i newPos, Vector2i lastPos)
	{
		Gem newGem = GameManager.Instance.Grid.Get(newPos);
		LevelData.GemEffectType newEffect = newGem.GetEffectType();
		Gem oldGem = GameManager.Instance.Grid.Get(lastPos);
		LevelData.GemEffectType oldEffect = oldGem.GetEffectType();
		return (newEffect == LevelData.GemEffectType.ET_ENEMY && oldEffect == LevelData.GemEffectType.ET_HERO ||
		        newEffect == LevelData.GemEffectType.ET_HERO && oldEffect == LevelData.GemEffectType.ET_ENEMY);
	}

	public IEnumerator Attack(Vector2i newPos, Vector2i lastPos)
	{
		Gem newGem = GameManager.Instance.Grid.Get(newPos);
		LevelData.GemEffectType newEffect = newGem.GetEffectType();
		Gem oldGem = GameManager.Instance.Grid.Get(lastPos);

		Gem hero = newEffect == LevelData.GemEffectType.ET_ENEMY ? oldGem : newGem;
		Gem enemy = newEffect == LevelData.GemEffectType.ET_HERO ? oldGem : newGem;
		yield return StartCoroutine(Attack(hero.gameObject.GetComponent<Figure>(), enemy.gameObject.GetComponent<Figure>(), Grid.DetermineDirection(newPos, lastPos)));
	}

	public void CollectGem(Vector2i gridPos, int index, int multiplier)
	{
		Debug.Log("Collected " + index + " x " + multiplier + " @ " + gridPos);
		if (_heroes[index]._script)
		{
			_heroes[index]._script.ActionPoints += multiplier;
		}
		else
		{
			_heroes[index]._spawnPoints += multiplier;

			if (!_heroes[index]._spawned)
			{
				GameManager.Instance.HUD.Heroes.Set(index, _heroes[index]._spawnPoints);
				if (_heroes[index]._spawnPoints >= _levelData.GetHeroInfo(index)._pointsToSpawn)
					AddSpawn(gridPos, index);
			}
		}
	}
	
	public void Charge()
	{
		for(int j = 0; j < _levelData.GetHeroCount(); j++)
		{
			if (_heroes[j]._script)
			{
				_heroes[j]._script.NextTurn();
			}
		}
	}

	public Figure FindNearest(Vector2i gridPos)
	{
		float minDist = float.MaxValue;
		int minIndex = -1;
		for(int j = 0; j < _levelData.GetHeroCount(); j++)
		{
			if (_heroes[j]._script)
			{
				Gem hd =_heroes[j]._script.GetGemScript();
				Vector2i dPos = hd.GetGridPosition() - gridPos;

				float dist = Mathf.Sqrt(dPos.x*dPos.x + dPos.y*dPos.y);
				if (dist < minDist)
				{
					minDist = dist; minIndex = j;
				}
			}
		}
		if (minIndex == -1)
			return null;
		else return _heroes[minIndex]._script;
	}

	public IEnumerator Attack(Figure attacker, Figure attackee, Directions dir)
	{
		int attackerPoints = attacker.ActionPoints;
		int attackeePoints = attackee.ActionPoints;
		Debug.Log("attack attacker(" + attackerPoints + ") / attackee(" + attackeePoints + ")");
		int damage = Math.Min (Math.Min(attackerPoints, attackeePoints), _levelData.GetMaxAttack());

		yield return StartCoroutine(attacker.Attack(damage, attackee, GameManager.Instance.Definitions.ParticleAttack));

		attacker.ActionPoints -= damage;
		attackee.ActionPoints -= damage;
	}

	public IEnumerator Spawn()
	{
		foreach(SpawnInfo info in _spawnInfoList)
		{
			// Check whether the gem slot is not already occupied by other hero or enemy
			Vector2i gridPos = GameManager.Instance.Grid.FindValidPosition(info._gridPos, LevelData.GetBasicGemTypes(), -1);
			
			Gem gem = GameManager.Instance.Grid.Get(gridPos);
			
			Figure figure = gem.GetComponent<Figure>();
			_heroes[info._heroIndex]._script = figure;
			
			figure.ActionPoints = _heroes[info._heroIndex]._spawnPoints + _levelData.GetHeroInfo(info._heroIndex)._actionPoints;

			GameManager.Instance.HUD.Heroes.SetState(info._heroIndex, false);
			
			yield return StartCoroutine(figure.Spawn(_levelData.GetHeroInfo(info._heroIndex)._gemType, 
			                                         _levelData.GetHeroInfo(info._heroIndex)._power, 
			                                         true,
			                                         info._heroIndex,
			                                         GameManager.Instance.Definitions.SpawnTime));
			_heroes[info._heroIndex]._script.ActionPoints = figure.ActionPoints; //karel
		}
		
		_spawnInfoList.Clear();
	}
	
	public void Remove(Figure figure)
	{
		Debug.Log("Remove figure: hero " + figure.IsHero() + ", index " + figure.GetGemScript().GetData());
		DebugUtils.Assert(figure.IsHero());
		DebugUtils.Assert(figure.GetHashCode() == _heroes[figure.GetGemScript().GetData()]._script.GetHashCode());
		_heroes[figure.GetGemScript().GetData()]._script = null;
		_heroes[figure.GetGemScript().GetData()]._spawned = false;
		_heroes[figure.GetGemScript().GetData()]._spawnPoints = 0;
	}
	
	struct SpawnInfo
	{
		public Vector2i _gridPos;
		public int _heroIndex;
	}
	
	void AddSpawn(Vector2i gridPos, int heroIndex)
	{
		_heroes[heroIndex]._spawned = true;
		_heroes[heroIndex]._spawnPoints = 0;
		_heroes[heroIndex]._script = null;

		// spawn hero
		SpawnInfo spawnInfo;
		spawnInfo._gridPos = gridPos;
		spawnInfo._heroIndex = heroIndex;
		_spawnInfoList.Add(spawnInfo);
	}
	
	public int GetHeroesCount() { return _levelData.GetHeroCount(); }
	public Figure GetHeroAtIndex(int index) { return _heroes[index]._script; }
}

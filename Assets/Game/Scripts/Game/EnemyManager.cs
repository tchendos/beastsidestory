using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyManager:MonoBehaviour
{
	struct EnemyInfo
	{
		public Figure _script;

		public int _enemyType;
	}
	private EnemyInfo[] _enemies;
	private int _enemyCounter;

	private int _nextWave;
	private int _enemiesToFetch;
	private int _lastWaveMoves;

	private LevelData _levelData;

	private bool _updateVisual;

	void Awake()
	{
		_updateVisual = true;

		_levelData = GameManager.Instance.LevelData;

		_enemies = new EnemyInfo[_levelData.GetWaveEnemyCount()];
		_enemyCounter = 0;
		
		_nextWave = -1;
		_enemiesToFetch = 0;
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
				for(int j = 0; j < _enemyCounter; j++)
				{
					figure = _enemies[j]._script;
					if (figure)
						figure.ActionPoints = figure.ActionPoints;
				}
			}
		}
	}
	
	public Figure GetAtPosition(Vector2i pos)
	{
		Gem gem = GameManager.Instance.Grid.Get (pos);
		if (gem.GetEffectType() == LevelData.GemEffectType.ET_ENEMY)
		{
			return _enemies[gem.GetData()]._script;
		}
		return null;
	}

	public IEnumerator Move()
	{
		bool onceMore;
		int processCounter = 0;
		bool[] processed = new bool[_levelData.GetWaveEnemyCount()];
		for(int i = 0; i < _levelData.GetWaveEnemyCount(); i++)
			processed[i] = false;
		
		do
		{
			onceMore = false; processCounter = 0;
			for(int i = 0; i < _enemyCounter; i++)
			{
				if (_enemies[i]._script != null && !_enemies[i]._script.GetGemScript().IsDead()  && !processed[i])
				{
					Figure figure = _enemies[i]._script;
					Gem ed = figure.GetGemScript();
					Figure hFigure = GameManager.Instance.HeroManager.FindNearest(ed.GetGridPosition());
					Gem hd = null;
					Directions attackDir = Directions.None;
					if (hFigure != null)
					{
						hd = hFigure.GetGemScript();
						attackDir = GetAttackDir(hd.GetGridPosition(), ed.GetGridPosition());
						if (attackDir != Directions.None)
						{
							/// attack adjacent hero
							yield return StartCoroutine(GameManager.Instance.HeroManager.Attack(figure, hFigure, attackDir));
							Debug.Log ("Attack: " + i + "," + ed.IsDead() + "," + hd.IsDead());
							yield return StartCoroutine(Puzzle.CollapseField(attackDir, null));
						}
					}
					if (attackDir == Directions.None)
					{
						if (figure.PowerCharge == 0 && figure.IsPowerAttackAvailable())
						{
							yield return StartCoroutine(figure.PowerAttack());
						}
						else if (hd != null)
						{
							Vector2i last = ed.GetGridPosition();
							yield return StartCoroutine(figure.Move(hd));
							if (ed.GetGridPosition() == last)
							{
								onceMore = true;
								continue;
							}
						}
						else
						{
							yield return StartCoroutine(figure.PlayerAttack());
						}
					}

					processCounter++;
					processed[i] = true;
				}
			}
		} while (onceMore && processCounter > 0);
	}

	public void Charge()
	{
		for(int i = 0; i < _enemyCounter; i++)
		{
			if (_enemies[i]._script != null)
			{
				_enemies[i]._script.NextTurn();
			}
		}
	}

	public bool AllDead()
	{
		for(int i = 0; i < _enemyCounter; i++)
		{
			if (_enemies[i]._script)
				return false;
		}
		return _enemiesToFetch == 0  && (_nextWave + 1) >= _levelData.GetWaveCount();
	}
	
	public IEnumerator FetchWave()
	{
		Vector2i gridPos;

		CheckWave();
		
		while(_enemiesToFetch > 0)
		{
			_enemiesToFetch--;
			CheckWave();

			gridPos = GameManager.Instance.Grid.FindValidPosition(Grid.RandomGridPosition(), LevelData.GetBasicGemTypes(), -1);

			int enemyType = _levelData.GetWaveInfo(_nextWave)._enemyType;
			Gem gem = GameManager.Instance.Grid.Get(gridPos);
			Figure figure = gem.gameObject.GetComponent<Figure>();
			_enemies[_enemyCounter]._script = figure;
			figure.ActionPoints = _levelData.GetEnemyInfo(enemyType)._actionPoints;
			_enemies[_enemyCounter]._enemyType = enemyType;

			GameManager.Instance.HUD.Waves.DisableEnemy(_enemyCounter);

			yield return StartCoroutine(figure.Spawn(_levelData.GetEnemyInfo(enemyType)._gemType, 
			                                         _levelData.GetEnemyInfo(enemyType)._power,
			                                         false,
			                                         _enemyCounter,
			                                         GameManager.Instance.Definitions.SpawnTime));
			
			_enemyCounter++;
		}
	}

	void CheckWave()
	{
		if (_enemiesToFetch == 0 && 
		    (_nextWave + 1) < _levelData.GetWaveCount() &&
		    (GameManager.Instance.HUD.Waves.GetMoveCount() - _lastWaveMoves) >= _levelData.GetWaveInfo(_nextWave + 1)._moves)
		{
			_nextWave++;
			LevelData.WaveInfo waveInfo = _levelData.GetWaveInfo(_nextWave);
			_enemiesToFetch = waveInfo._count;
			_lastWaveMoves += waveInfo._moves;
		}
	}
	
	Directions GetAttackDir(Vector2i gridPos1, Vector2i gridPos2)
	{
		Vector2i dPos = gridPos1 - gridPos2;
		if (dPos.x == 0 && Math.Abs(dPos.y) == 1 || dPos.y == 0 && Math.Abs(dPos.x) == 1)
		{
			return Grid.DetermineDirection(gridPos1, gridPos2);
		}

		return Directions.None;
	}

	public void Remove(Figure figure)
	{
		Debug.Log("Remove figure: hero " + figure.IsHero() + ", index " + figure.GetGemScript().GetData());
		DebugUtils.Assert(!figure.IsHero());
		DebugUtils.Assert(figure.GetHashCode() == _enemies[figure.GetGemScript().GetData()]._script.GetHashCode());
		_enemies[figure.GetGemScript().GetData()]._script = null;
	}

	public int GetEnemiesCount() { return _levelData.GetWaveEnemyCount(); }
	public Figure GetEnemyAtIndex(int index) { return _enemies[index]._script; }
}

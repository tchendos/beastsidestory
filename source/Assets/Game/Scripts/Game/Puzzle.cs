using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Puzzle
{
	// Use this for initialization
	static public void Init() 
	{
		GameManager.Instance.Controller.SetView(GameManager.Instance.Definitions.MainCamera, 1000.0f);

		GameManager.Instance.Grid.Generate();

		CollapseFieldInstant(Directions.Down);

		SetInputState(true);
	}

	static public IEnumerator Tap(Vector2i pos)
	{
		SetInputState(false);

		Figure hero = GameManager.Instance.HeroManager.GetAtPosition(pos);
		if (hero != null)
		{
			if (hero.IsPowerAttackAvailable())
			{
				yield return StartCoroutine(hero.PowerAttack());

				yield return StartCoroutine(ProcessMove(Directions.Down, null));
			}
			else
			{
				float pulseTime = GameManager.Instance.Definitions.ShakeTime;
				yield return StartCoroutine(hero.Shake(pulseTime));
			}
		}

		SetInputState(true);
	}

	static public IEnumerator Swipe(Vector2i newPos, Vector2i lastPos)
	{
		SetInputState(false);

		if (GameManager.Instance.HeroManager.IsAttack(newPos, lastPos))
		{
			yield return StartCoroutine(GameManager.Instance.HeroManager.Attack(newPos, lastPos));

			yield return StartCoroutine(ProcessMove(Grid.DetermineDirection(newPos, lastPos), null));
		}
		else 
		{
			if (GameManager.Instance.Grid.Get(newPos).GetEffectType() == LevelData.GemEffectType.ET_HERO ||
			    GameManager.Instance.Grid.Get(lastPos).GetEffectType() == LevelData.GemEffectType.ET_HERO ||
			    GameManager.Instance.Grid.IsValidSwap(newPos, lastPos))
			{
				yield return StartCoroutine(GameManager.Instance.Grid.SwapGems(newPos, lastPos));

				yield return StartCoroutine(ProcessMove(Grid.DetermineDirection(newPos, lastPos), lastPos));

			}
			else yield return StartCoroutine(GameManager.Instance.Grid.SwapGemsFail(newPos, lastPos));
		}

		SetInputState(true);
	}

	static public IEnumerator DestroyGem(Vector2i posGrid)
	{
		SetInputState(false);
		GameManager.Instance.Grid.Get(posGrid).Die();
		yield return StartCoroutine(CollapseField(Directions.Down, null));
		SetInputState(true);
	}
	
	static public IEnumerator ChangeGem(Vector2i posGrid, int gemTypeIndex)
	{
		SetInputState(false);
		yield return StartCoroutine(GameManager.Instance.Grid.Get(posGrid).ChangeGemType(gemTypeIndex, null));
		yield return StartCoroutine(CollapseField(Directions.Down, null));
		SetInputState(true);
	}
	
	static IEnumerator ProcessMove(Directions dir, Vector2i? lastPos)
	{
		GameManager.Instance.HUD.Waves.AddMove();

		yield return StartCoroutine(CollapseField(dir, lastPos));

		yield return StartCoroutine(GameManager.Instance.EnemyManager.Move());
		
		if (GameManager.Instance.HUD.Health.Get() > 0)
		{
			yield return StartCoroutine(GameManager.Instance.EnemyManager.FetchWave());

			GameManager.Instance.EnemyManager.Charge();
			GameManager.Instance.HeroManager.Charge();

			GameManager.Instance.Grid.Log();

			if (GameManager.Instance.EnemyManager.AllDead())
			{
				SetInputState(false);
				GameManager.Instance.HUD.Overlay.Show(true);

				// increase LevelsFinished if player won last opened level
				if(PlayerPrefs.HasKey(Constants.LevelsFinished))
				{
					int levelCount = PlayerPrefs.GetInt(Constants.LevelsFinished);
					if(GameManager.Instance.InitStruct.levelIndex == (levelCount-1))
					{
						PlayerPrefs.SetInt(Constants.LevelsFinished, levelCount+1);
						PlayerPrefs.Save();
					}
				}
			}
		}
		else
		{
			SetInputState(false);
			GameManager.Instance.HUD.Overlay.Show(false);
		}
	}

	static void CollapseFieldInstant(Directions dir)
	{
		bool[,] matched;
		
		while (true)
		{
			matched = GameManager.Instance.Grid.CheckMatches();
			if (matched != null)
			{
				Vector2i pos;
				for(pos.x = 0; pos.x < Constants.FIELD_SIZE_X; pos.x++)
				{
					for(pos.y = 0; pos.y < Constants.FIELD_SIZE_Y; pos.y++)
					{
						if (matched[pos.x, pos.y])
						{
							Gem gem = GameManager.Instance.Grid.Get(pos);
							if (GameManager.Instance.LevelData.GetGemTypeInfo(gem.GetGemType())._effectType != LevelData.GemEffectType.ET_HERO)
							    gem.Die();
						}
					}
				}
				GameManager.Instance.Grid.Collapse(matched, 0, dir);
			}
			else break;
		}
		GameManager.Instance.Grid.Log();
	}

	static public IEnumerator CollapseField(Directions dir, Vector2i? lastPos)
	{
		float posTime = GameManager.Instance.Definitions.CollapseFieldTime;
		bool[,] matched;

		while (true)
		{
			matched = GameManager.Instance.Grid.CheckMatches();
			if (matched != null)
			{
				yield return StartCoroutine(Collect(matched, dir, lastPos));
				lastPos = null;
				
				int maxCtr = GameManager.Instance.Grid.Collapse(matched, posTime, dir);

				//yield return new WaitForSeconds(GameManager.Instance.Definitions.CollectStatsTime);

				float waitTime = posTime * (maxCtr - 1);// - GameManager.Instance.Definitions.CollectStatsTime;
				if (waitTime > 0)
					yield return new WaitForSeconds(waitTime);
				
				yield return StartCoroutine(GameManager.Instance.HeroManager.Spawn());

				GameManager.Instance.Grid.Log();
			}
			else 
			{
				yield break;
			}
		}
	}

	static bool IsMatch(Vector2i pos, int[,] matchMap, int gemType)
	{
		return Grid.Valid(pos) && matchMap[pos.x, pos.y] == gemType;
	}
	static int DetectSpecialGem(ref int[,] matchMap, Vector2i pos, bool clear)
	{
		if (matchMap[pos.x, pos.y] >= 0)
		{
			int gemType = matchMap[pos.x, pos.y];
			int wingL = IsMatch(new Vector2i(pos.x - 1, pos.y), matchMap, gemType) ? IsMatch(new Vector2i(pos.x - 2, pos.y), matchMap, gemType) ? 2 : 1 : 0;
			int wingR = IsMatch(new Vector2i(pos.x + 1, pos.y), matchMap, gemType) ? IsMatch(new Vector2i(pos.x + 2, pos.y), matchMap, gemType) ? 2 : 1 : 0;
			int wingU = IsMatch(new Vector2i(pos.x, pos.y - 1), matchMap, gemType) ? IsMatch(new Vector2i(pos.x, pos.y - 2), matchMap, gemType) ? 2 : 1 : 0;
			int wingD = IsMatch(new Vector2i(pos.x, pos.y + 1), matchMap, gemType) ? IsMatch(new Vector2i(pos.x, pos.y + 2), matchMap, gemType) ? 2 : 1 : 0;

			if (clear)
			{
				for (int x = pos.x - wingL; x < pos.x + wingR + 1; x++)
					matchMap[x, pos.y] = -1;
				for (int y = pos.y - wingU; y < pos.y + wingD + 1; y++)
					matchMap[pos.x, y] = -1;
			}

			int count = (wingL == 2 ? 1 : 0) + (wingR == 2 ? 1 : 0) + (wingU == 2 ? 1 : 0) + (wingD == 2 ? 1 : 0);
			if (count == 3)
			{
				return GameManager.Instance.LevelData.GetBoostGemType();
			}
			else if (count == 2)
			{
				if (wingL == 2 && wingR == 2 || wingU == 2 && wingD == 2)
					return GameManager.Instance.LevelData.GetBoostGemType();

				return GameManager.Instance.LevelData.GetCrossGemType();
			}
			else
			{
				if (count == 1 && (wingL + wingR + wingD + wingU) == 4)
					return GameManager.Instance.LevelData.GetCrossGemType();
				if ((wingL + wingR) == 3 || (wingU + wingD) == 3)
					return GameManager.Instance.LevelData.GetMultiplyGemType();
			}
		}
		return -1;
	}

	static void CreateSpecialGem(ref int[,] matchMap, ref bool[,] matched, Vector2i pos)
	{
		int matchData = matchMap[pos.x, pos.y];

		// change special gem pos, so it does not overwrite hero
		Vector2i sPos = pos;
		Gem gem = GameManager.Instance.Grid.Get(pos);
		if (gem.GetEffectType() == LevelData.GemEffectType.ET_HERO)
		{
			if (pos.x > 0 && matchMap[pos.x - 1, pos.y] == matchData)
				sPos.x--;
			else if (pos.x < (Constants.FIELD_SIZE_X - 1) && matchMap[pos.x + 1, pos.y] == matchData)
				sPos.x++;
			else if (pos.y > 0 && matchMap[pos.x, pos.y - 1] == matchData)
				sPos.y--;
			else if (pos.y < (Constants.FIELD_SIZE_Y - 1) && matchMap[pos.x, pos.y + 1] == matchData)
				sPos.y++;
		}

		int newGemType = DetectSpecialGem(ref matchMap, pos, true);
		if (newGemType != -1)
		{
			matched[sPos.x, sPos.y] = false;
			StartCoroutine(GameManager.Instance.Grid.Get(sPos).ChangeGemType(newGemType + matchData, GameManager.Instance.Definitions.ParticleSpecialGem));
		}
	}

	static Vector2i[] directionVectors = new Vector2i[4] { new Vector2i(0, 1), new Vector2i(0, -1), new Vector2i(1, 0), new Vector2i(-1, 0) };
	
	static void CreateSpecialGems(ref bool[,] matched, Directions dir, Vector2i? lastPos)
	{
		int[,] matchMap = new int[Constants.FIELD_SIZE_X,Constants.FIELD_SIZE_Y];
		int[,] specialGems = new int[Constants.FIELD_SIZE_X,Constants.FIELD_SIZE_Y];
		Vector2i mapPos;
		int mapData;
		for(mapPos.x = 0; mapPos.x < Constants.FIELD_SIZE_X; mapPos.x++)
		{
			for(mapPos.y = 0; mapPos.y < Constants.FIELD_SIZE_Y; mapPos.y++)
			{
				if (matched[mapPos.x, mapPos.y])
				{
					LevelData.GemTypeInfo gti = GameManager.Instance.LevelData.GetGemTypeInfo(GameManager.Instance.Grid.Get(mapPos).GetGemType());
					mapData = gti._colored ? gti._effectData : -1;
				}
				else mapData = -1;
				matchMap[mapPos.x,mapPos.y] = mapData;
				specialGems[mapPos.x,mapPos.y] = -1;
			}
		}
		
		if (lastPos != null)
		{
			Vector2i pos = lastPos.Value;
			CreateSpecialGem(ref matchMap, ref matched, pos);
			
			pos = lastPos.Value + directionVectors[(int)dir];
			DebugUtils.Assert(Grid.Valid(pos));
			CreateSpecialGem(ref matchMap, ref matched, pos);
		}

		int start = -1, startType = -1, best = -1, bestGem = -1;
		for(mapPos.x = 0; mapPos.x < Constants.FIELD_SIZE_X; mapPos.x++)
		{
			for(mapPos.y = 0; mapPos.y < Constants.FIELD_SIZE_Y; mapPos.y++)
			{
				if (start == -1)
				{
					if (matchMap[mapPos.x,mapPos.y] != -1)
					{
						start = mapPos.y; startType = matchMap[mapPos.x,mapPos.y]; bestGem = -1; best = mapPos.y;

					}
				}
				if (start != -1)
				{
					if (matchMap[mapPos.x,mapPos.y] == startType)
					{
						int gem = DetectSpecialGem(ref matchMap, mapPos, false);
						if (gem > bestGem)
						{
							bestGem = gem; best = mapPos.y;
						}
					}
					else
					{
						start = -1; specialGems[mapPos.x,best] = bestGem; mapPos.y--;
					}
				}
			}
			if (start != -1)
			{
				specialGems[mapPos.x,best] = bestGem; start = -1;
			}
		}

		for(mapPos.y = 0; mapPos.y < Constants.FIELD_SIZE_Y; mapPos.y++)
		{
			for(mapPos.x = 0; mapPos.x < Constants.FIELD_SIZE_X; mapPos.x++)
			{
				if (start == -1)
				{
					if (matchMap[mapPos.x,mapPos.y] != -1)
					{
						start = mapPos.x; startType = matchMap[mapPos.x,mapPos.y]; bestGem = -1;
						
					}
				}
				if (start != -1)
				{
					if (matchMap[mapPos.x,mapPos.y] == startType)
					{
						int gem = specialGems[mapPos.x,mapPos.y];
						if (gem > bestGem)
						{
							bestGem = gem; best = mapPos.x;
						}
					}
					else
					{
						start = -1; 
						CreateSpecialGem(ref matchMap, ref matched, new Vector2i(best, mapPos.y));
						mapPos.x--;
					}
				}
			}
			if (start != -1)
			{
				CreateSpecialGem(ref matchMap, ref matched, new Vector2i(best, mapPos.y));
				start = -1;
			}
		}
	}

	static void ApplyCross(List<Vector2i> crossList, bool[,] matched, Vector2i pos)
	{
		Gem gem = GameManager.Instance.Grid.Get(pos);
		LevelData.GemTypeInfo ti = GameManager.Instance.LevelData.GetGemTypeInfo(gem.GetGemType());
		if (ti._effectType == LevelData.GemEffectType.ET_ENEMY)
		{
			if (!gem.IsDead())
			{
				Figure figure = GameManager.Instance.EnemyManager.GetEnemyAtIndex(gem.GetData());
				figure.ActionPoints -= GameManager.Instance.LevelData.GetCrossDamage();
				if (figure.GetGemScript().IsDead())
					matched[pos.x, pos.y] = true;
				StartCoroutine(Effects.Sparkle(GameManager.Instance.Definitions.CrossEnemyTime, 
				                               GameManager.Instance.Definitions.ParticleCrossEnemy, 
				                               figure.transform.position));
			}
		}
		else if (ti._effectType != LevelData.GemEffectType.ET_HERO)
		{
			if (ti._effectType == LevelData.GemEffectType.ET_GEM_CROSS && !crossList.Contains(pos))
				crossList.Add(pos);
			matched[pos.x, pos.y] = true;
		}
	}

	static bool _ignoreSpecialGems = false;

	static IEnumerator CollectCross(bool[,] matched)
	{
		Vector2i pos;
		List<Vector2i> crossList = GameManager.Instance.Grid.DetectMatchedGems(matched, LevelData.GemEffectType.ET_GEM_CROSS);
		if (crossList != null)
		{
			_ignoreSpecialGems = true;
			for(int i = 0; i < crossList.Count; i++)
			{
				pos = crossList[i];
				for(pos.x = 0; pos.x < Constants.FIELD_SIZE_X; pos.x++)
					ApplyCross(crossList, matched, pos);
				
				pos = crossList[i];
				for(pos.y = 0; pos.y < Constants.FIELD_SIZE_Y; pos.y++)
					ApplyCross(crossList, matched, pos);
				
				StartCoroutine(Effects.Sparkle(GameManager.Instance.Definitions.CrossTime, 
				                               GameManager.Instance.Definitions.ParticleCross, 
				                               GameManager.Instance.Grid.Get(crossList[i]).transform.position));
				yield return new WaitForSeconds(GameManager.Instance.Definitions.CrossTime / 2);
			}
			//yield return new WaitForSeconds(GameManager.Instance.Definitions.CrossTime / 2);
		}
	}

	static IEnumerator CollectBoost(bool[,] matched, int multiplier)
	{
		List<Vector2i> boostList = GameManager.Instance.Grid.DetectMatchedGems(matched, LevelData.GemEffectType.ET_GEM_BOOST);
		if (boostList != null)
		{
			Vector3 gemPos;
			for(int j = 0; j < boostList.Count; j++)
			{
				gemPos = GameManager.Instance.Grid.Get (boostList[j]).transform.position;
				for(int i = 0; i < GameManager.Instance.LevelData.GetHeroCount(); i++)
				{
					GameManager.Instance.HeroManager.CollectGem(boostList[j], i, multiplier * GameManager.Instance.LevelData.GetBoostPoints());
					
					StartCoroutine(Effects.Pulse(GameManager.Instance.Definitions.BoostFlyTime, 
					                             GameManager.Instance.Definitions.BoostFlyWaitTime, 
					                             GameManager.Instance.Definitions.BoostHitTime,
					                             null, 
					                             GameManager.Instance.Definitions.ParticleBoostFly, 
					                             GameManager.Instance.Definitions.ParticleBoost,
					                             gemPos,
					                             GameManager.Instance.HeroManager.GetPosition(i),
					                             true));
				}
				yield return new WaitForSeconds(GameManager.Instance.Definitions.BoostFlyTime);
			}
		}
	}

	static IEnumerator Collect(bool[,] matched, Directions dir, Vector2i? lastPos)
	{
		GameManager.Instance.HUD.SetUpdateVisual(false);
		GameManager.Instance.EnemyManager.SetUpdateVisual(false);
		GameManager.Instance.HeroManager.SetUpdateVisual(false);
		
		Vector2i pos;
		_ignoreSpecialGems = false;
		yield return StartCoroutine(CollectCross(matched));

		int multiplier = 1;
		List<Vector2i> multiplyList = GameManager.Instance.Grid.DetectMatchedGems(matched, LevelData.GemEffectType.ET_GEM_MULTIPLY);
		if (multiplyList != null)
		{
			for (int i = 0; i < multiplyList.Count; i++)
				multiplier *= 2;
			yield return StartCoroutine(GameManager.Instance.HUD.Overlay.ShowMultiplier(multiplier));
		}

		yield return StartCoroutine(CollectBoost(matched, multiplier));

		if (!_ignoreSpecialGems)
			CreateSpecialGems(ref matched, dir, lastPos);

		for(pos.x = 0; pos.x < Constants.FIELD_SIZE_X; pos.x++)
		{
			for(pos.y = 0; pos.y < Constants.FIELD_SIZE_Y; pos.y++)
			{
				if (matched[pos.x, pos.y])
				{
					Collect(pos, multiplier);
				}
			}
		}
		
		yield return new WaitForSeconds(GameManager.Instance.Definitions.CollectStatsTime);

		GameManager.Instance.HUD.SetUpdateVisual(true);
		GameManager.Instance.EnemyManager.SetUpdateVisual(true);
		GameManager.Instance.HeroManager.SetUpdateVisual(true);
		
		float fadeOutTime = GameManager.Instance.Definitions.MatchFadeTime - GameManager.Instance.Definitions.CollectStatsTime;
		yield return new WaitForSeconds(fadeOutTime);
	}
	
	static void Collect(Vector2i gridPos, int multiplier)
	{
		Gem gem = GameManager.Instance.Grid.Get(gridPos);
		int type =  gem.GetGemType();
		int data = GameManager.Instance.LevelData.GetGemTypeInfo(type)._effectData;
		Vector3 destPosition = Vector3.zero;
		LevelData.GemEffectType et = GameManager.Instance.LevelData.GetGemTypeInfo(type)._effectType;
		switch(et)
		{
		case LevelData.GemEffectType.ET_GEM_HEALTH:
			destPosition = GameManager.Instance.HUD.Health.GetBarCenter();
			GameManager.Instance.HUD.Health.Add(data * multiplier);
			gem.Die();
			break;
		case LevelData.GemEffectType.ET_GEM_COIN:
			destPosition = GameManager.Instance.HUD.Coins.GetCenter();
			GameManager.Instance.HUD.Coins.Add(data * multiplier);
			gem.Die();
			break;
		case LevelData.GemEffectType.ET_GEM_NORMAL:
		case LevelData.GemEffectType.ET_GEM_MULTIPLY:
		case LevelData.GemEffectType.ET_GEM_CROSS:
		case LevelData.GemEffectType.ET_GEM_BOOST:
			destPosition = GameManager.Instance.HeroManager.GetPosition(data);
			GameManager.Instance.HeroManager.CollectGem(gridPos, data, multiplier);
			gem.Die();
			break;
		case LevelData.GemEffectType.ET_HERO:
			destPosition = GameManager.Instance.HeroManager.GetPosition(data);
			GameManager.Instance.HeroManager.CollectGem(gridPos, data, multiplier);
			break;
		case LevelData.GemEffectType.ET_ENEMY:
			destPosition = GameManager.Instance.HUD.Waves.GetEnemyPosition(data);
			break;
		case LevelData.GemEffectType.ET_EMPTY:
			gem.Die();
			break;
		default:
			DebugUtils.Assert(false);
			break;
		}

		if (gem.IsDead())
		{
			float fadeOutTime = GameManager.Instance.Definitions.MatchFadeTime;

			gem.SetupFadeOut(fadeOutTime);
			StartCoroutine(Effects.Pulse(GameManager.Instance.Definitions.MatchFlyTime, 
			                             GameManager.Instance.Definitions.MatchFlyWaitTime, 
			                             GameManager.Instance.Definitions.MatchHitTime, null,
							              GameManager.Instance.Definitions.ParticleCollectFly, 
							              GameManager.Instance.Definitions.ParticleCollect,
							              gem.transform.position,
							              destPosition,
							              true));
		}
	}

	static void SetInputState(bool state)
	{
		GameManager.Instance.Controller.SetInputState(state);
	}

	static Coroutine StartCoroutine(IEnumerator routine)
	{
		return GameManager.Instance.StartCoroutine(routine);
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Figure : MonoBehaviour 
{
	Gem _gem;
	int _actionPoints;
	int _powerCharge;
	bool _hero;
	bool _spawned;
	int _hudIndex;
	LevelData.Power _power;

	public int ActionPoints
	{
		get { return _actionPoints; }
		set
		{
			_actionPoints = value;
			//if (GameManager.Instance.HeroManager.GetUpdateVisual())
			{
				int clamped = Math.Max(_actionPoints, 0);
				_gem.SetLabelR(clamped.ToString());
				if (_hero)
					GameManager.Instance.HUD.Heroes.Set(_hudIndex, clamped);
			}
			if (_actionPoints <= 0)
			{
				Debug.Log("Dead: " + _gem.GetGridPosition() + " / hero: " + _hero + " / " + _hudIndex + " / " + _gem.GetData());
				_gem.Die();
				if (_hero)
				{
					GameManager.Instance.HeroManager.Remove(this);
				}
				else GameManager.Instance.EnemyManager.Remove(this);
			}
		}
	}
	public int PowerCharge
	{
		get { return _powerCharge; }
		set
		{
			_powerCharge =  value;
			_gem.SetLabelL(_powerCharge.ToString());
		}
	}
	public bool IsHero() { return _hero; }

	void Awake()
	{
		_gem = gameObject.GetComponent<Gem>();
	}

	public Gem GetGemScript() { return _gem; }

	public void NextTurn()
	{
		if (_power._type == LevelData.Power.Types.NONE)
			return;

		if (_spawned)
			_spawned = false;
		else if (PowerCharge > 0)
		{
			PowerCharge--;
			if (PowerCharge == 0)
				_gem.SetGlowActive(true, _hero);
		}
	}

	public IEnumerator Scale(float time)
	{
		_gem.TweenScale(time, new Vector3(1.5f, 1.5f, 1.5f), Tweener.Method.EaseOut);
		
		yield return new WaitForSeconds(time);
		
		_gem.TweenScale(time, Vector3.one, Tweener.Method.EaseOut);
	}
	
	public IEnumerator Shake(float time)
	{
		yield return StartCoroutine(Effects.Shake(_gem.gameObject, time));
	}

	public IEnumerator Spawn(int gemType, LevelData.Power power, bool hero, int hudIndex, float time)
	{
		_hudIndex = hudIndex;
		_hero = hero;
		_power = power;
		PowerCharge = _power._turns;
		_spawned = true;

		_gem.SetData(hudIndex);
		_gem.SetGemType(gemType);
		_gem.SetOnTop();
		_gem.EnableLabelR(_hero ? GameManager.Instance.Definitions.HeroLifeColor :
		                 GameManager.Instance.Definitions.EnemyLifeColor);
		if (_power._type != LevelData.Power.Types.NONE)
			_gem.EnableLabelL(Color.white);

		_gem.ForceDepth(Gem.SortLayer4);

		Color color = new Color(1, 1, 1, 0);
		_gem.TweenColor(0, color, Tweener.Method.EaseIn);
		_gem.TweenScale(0, Vector3.one * 2.0f, Tweener.Method.EaseIn);
		_gem.TweenPosition(0, _hero ? GameManager.Instance.HUD.Heroes.GetPosition(_hudIndex) :
		                   GameManager.Instance.HUD.Waves.GetEnemyPosition(_hudIndex), Tweener.Method.EaseInOut, false);

		color.a = 1;
		_gem.TweenColor(time / 5, color, Tweener.Method.EaseIn);
		_gem.TweenScale(time, Vector3.one, Tweener.Method.EaseIn);
		_gem.TweenPosition(time, Grid.CalculatePosition(_gem.GetGridPosition()), Tweener.Method.EaseInOut, false);
		
		yield return new WaitForSeconds(time);
		
		_gem.TweenColor(0, color, Tweener.Method.EaseIn);
		_gem.TweenScale(0, Vector3.one, Tweener.Method.EaseIn);
		_gem.TweenPosition(0, Grid.CalculatePosition(_gem.GetGridPosition()), Tweener.Method.EaseInOut, false);
		_gem.UpdateDepth();
	}
	
	public IEnumerator Attack(int damage, Figure attackee, GameObject particlePrefab)
	{
		float moveTime = GameManager.Instance.Definitions.AttackMoveTime;
		float particleTime = GameManager.Instance.Definitions.AttackParticleTime;
		int attacks = Math.Min(3, damage / 5 + 1);
		int attackStrength = damage / attacks + 1;
		int attackerPointsC = ActionPoints;
		int attackeePointsC = attackee.ActionPoints;
		
		Vector3 originalPos = Grid.CalculatePosition(_gem.GetGridPosition());
		Vector3 attackeePos = Grid.CalculatePosition(attackee.GetGemScript().GetGridPosition());
		Vector3 position = (originalPos + attackeePos) / 2;
		
		GameObject p = (GameObject)Instantiate(particlePrefab);
		p.transform.parent = GameManager.Instance.Definitions.FieldRoot.transform;
		p.transform.localPosition = attackeePos;
		p.transform.localScale = new Vector3(1,1,1);

		_gem.ForceDepth(Gem.SortLayer3);

		for (int i = 0; i < attacks; i++)
		{
			_gem.TweenPosition(moveTime, position, Tweener.Method.EaseIn, false);
			
			yield return new WaitForSeconds(moveTime);
			
			p.SetActive(true);
			
			yield return new WaitForSeconds(particleTime);
			
			if (i == attacks - 1)
			{
				attackerPointsC = ActionPoints - damage;
				attackeePointsC = attackee.ActionPoints - damage;
			}
			else
			{
				attackerPointsC -= attackStrength;
				attackeePointsC -= attackStrength;
			}
			if (attackerPointsC < 0)
				attackerPointsC = 0;
			_gem.SetLabelR(attackerPointsC.ToString());
			if (attackeePointsC < 0)
				attackeePointsC = 0;
			attackee.GetGemScript().SetLabelR(attackeePointsC.ToString());
			
			_gem.TweenPosition(moveTime, originalPos, Tweener.Method.EaseIn, false);
			
			yield return new WaitForSeconds(moveTime);
			
			p.SetActive(false);
		}
		
		_gem.UpdateDepth();
		
		GameObject.Destroy(p);
	}

	public bool IsPowerAttackAvailable()
	{
		if (PowerCharge != 0)
			return false;

		switch(_power._type)
		{
		case LevelData.Power.Types.ABSORB_GEMS:
			Vector2i gemPos = GameManager.Instance.Grid.FindNearest(_gem.GetGridPosition(), _power._et);
			return (gemPos.x != -1);
		case LevelData.Power.Types.COLLECT_GEMS:
		case LevelData.Power.Types.DAMAGE:
		case LevelData.Power.Types.TRANSFORM_RANDOM_GEMS:
			return true;
		case LevelData.Power.Types.DAMAGE_FIGURES_SELF:
		case LevelData.Power.Types.DAMAGE_FIGURES_TARGET:
		{
			int count = _hero ? GameManager.Instance.EnemyManager.GetEnemiesCount():GameManager.Instance.HeroManager.GetHeroesCount();
			for(int i = 0; i < count; i++)
			{
				Figure figure = _hero ? GameManager.Instance.EnemyManager.GetEnemyAtIndex(i) : GameManager.Instance.HeroManager.GetHeroAtIndex(i);
				if (figure && !figure.GetGemScript().IsDead())
					return true;
			}
			return false;
		}
		}

		return false;
	}

	public IEnumerator PowerAttack()
	{
		float pulseTime = GameManager.Instance.Definitions.PulseTime;
		ResetPowerCharge();
		yield return StartCoroutine(Scale(pulseTime));

		switch(_power._type)
		{
		case LevelData.Power.Types.DAMAGE:
			StartCoroutine(Effects.Pulse(GameManager.Instance.Definitions.PulseFlyTime, 
			                                          GameManager.Instance.Definitions.PulseFlyWaitTime, 
			                                          GameManager.Instance.Definitions.PulseHitTime, null,
                                                       GameManager.Instance.Definitions.ParticlePowerDamageFly, 
                                                       GameManager.Instance.Definitions.ParticlePowerDamage, 
                                                       transform.position, 
			                                          GameManager.Instance.HUD.Health.GetBarCenter(), true));

			yield return new WaitForSeconds(GameManager.Instance.Definitions.PulseFlyWaitTime);

			GameManager.Instance.HUD.Health.Add(-((int)(ActionPoints * _power._f + 0.5f) + _power._i));

			yield return new WaitForSeconds(GameManager.Instance.Definitions.PulseHitTime);
			break;

		case LevelData.Power.Types.DAMAGE_FIGURES_SELF:
		case LevelData.Power.Types.DAMAGE_FIGURES_TARGET:
		{
			bool anyDead = false;
			int count = _hero ? GameManager.Instance.EnemyManager.GetEnemiesCount():GameManager.Instance.HeroManager.GetHeroesCount();
			for(int i = 0; i < count; i++)
			{
				Figure figure = _hero ? GameManager.Instance.EnemyManager.GetEnemyAtIndex(i) : GameManager.Instance.HeroManager.GetHeroAtIndex(i);
				if (figure && !figure.GetGemScript().IsDead())
				{
					yield return StartCoroutine(Effects.Pulse(GameManager.Instance.Definitions.PulseFlyTime, 
					                                          GameManager.Instance.Definitions.PulseFlyWaitTime, 
					                                          GameManager.Instance.Definitions.PulseHitTime, null,
                                                               GameManager.Instance.Definitions.ParticlePowerDamageFigureFly, 
                                                               GameManager.Instance.Definitions.ParticlePowerDamageFigure, 
                                                               transform.position, 
                                                               figure.transform.position, true));
					int points = _power._type == LevelData.Power.Types.DAMAGE_FIGURES_SELF ? ActionPoints : figure.ActionPoints;
					figure.ActionPoints -= (int)(points * _power._f + 0.5f) + _power._i;
					anyDead |= figure.GetGemScript().IsDead();
				}
			}
			if (anyDead)
				yield return StartCoroutine(Puzzle.CollapseField(Directions.Down, null));
			break;
		}
		case LevelData.Power.Types.ABSORB_GEMS:
		{
			bool any = false;
			for(int i = 0; i < _power._i; i++)
			{
				Vector2i gemPos = GameManager.Instance.Grid.FindNearest(_gem.GetGridPosition(), _power._et);
				if (gemPos.x == -1)
					break;

				StartCoroutine(Effects.Pulse(GameManager.Instance.Definitions.PulseFlyTime, 
				                             GameManager.Instance.Definitions.PulseFlyWaitTime, 
				                             GameManager.Instance.Definitions.PulseHitTime, 
				                             GameManager.Instance.Definitions.ParticlePowerAbsorbHealthGem,
				                             GameManager.Instance.Definitions.ParticlePowerAbsorbHealthFly, 
											 GameManager.Instance.Definitions.ParticlePowerAbsorbHealth, 
				                             GameManager.Instance.Grid.Get(gemPos).transform.position,
				                             transform.position,
				                             true));
				yield return new WaitForSeconds(pulseTime);

				Gem gem = GameManager.Instance.Grid.Get(gemPos);
				gem.SetupFadeOut(pulseTime);
				yield return StartCoroutine(Scale(pulseTime));
				ActionPoints += _power._i2;

				gem.SetGemType(GameManager.Instance.LevelData.GetEmptyGemType());
				gem.TweenColor(0, new Color(0,0,0,0), Tweener.Method.EaseIn);
				gem.Die();
				any = true;
			}
			if (any)
				yield return StartCoroutine(Puzzle.CollapseField(Directions.Down, null));
			break;
		}
		case LevelData.Power.Types.COLLECT_GEMS:
		{
			Vector2i gridPos;
			for(int j = 0; j < 4; j++)
			{
				for(int i = 1; i <= _power._i; i++)
				{
					gridPos = _gem.GetGridPosition() + Vector2i.Directions[j] * i;
					if (Grid.Valid(gridPos))
					{
						Gem gem = GameManager.Instance.Grid.Get(gridPos);
						LevelData.GemEffectType et = GameManager.Instance.LevelData.GetGemTypeInfo(gem.GetGemType())._effectType;
						if (_hero && et == LevelData.GemEffectType.ET_ENEMY ||
						    !_hero && et == LevelData.GemEffectType.ET_HERO)
						{
							Figure figure = _hero ? GameManager.Instance.EnemyManager.GetAtPosition(gridPos) : 
								GameManager.Instance.HeroManager.GetAtPosition(gridPos);
							figure.ActionPoints -= (int)((float)ActionPoints * _power._f) + _power._i2;
							StartCoroutine(Effects.Sparkle(pulseTime, GameManager.Instance.Definitions.ParticlePowerCollectGemEnemy, gem.transform.position));
						}
						else if (et != LevelData.GemEffectType.ET_HERO)
						{
							gem.Die();
							StartCoroutine(Effects.Sparkle(pulseTime, GameManager.Instance.Definitions.ParticlePowerCollectGem, gem.transform.position));
						}
					}
				}
			}
			yield return new WaitForSeconds(pulseTime);

			yield return StartCoroutine(Puzzle.CollapseField(Directions.Down, null));
			break;
		}
		case LevelData.Power.Types.TRANSFORM_RANDOM_GEMS:
		{
			Vector2i pos;
			Coroutine c = null;
			LevelData.GemTypeInfo gti = GameManager.Instance.LevelData.GetGemTypeInfo(_power._i2);
			int excludeColor = gti._colored ? gti._effectData : -1;
			for(int j = 0; j < _power._i; j++)
			{
				pos = GameManager.Instance.Grid.FindValidPosition(Grid.RandomGridPosition(), LevelData.GetBasicGemTypes(), excludeColor);

				c = StartCoroutine(GameManager.Instance.Grid.Get(pos).ChangeGemType(_power._i2, GameManager.Instance.Definitions.ParticlePowerTransformGem));
				yield return new WaitForSeconds(GameManager.Instance.Definitions.TransformGemWaitTime);
			}
			yield return c;
			yield return StartCoroutine(Puzzle.CollapseField(Directions.Down, null));
			break;
		}
		}
	}

	public IEnumerator PlayerAttack()
	{
		float pulseTime = GameManager.Instance.Definitions.PulseTime;
		yield return StartCoroutine(Scale(pulseTime));
		StartCoroutine(Effects.Pulse(GameManager.Instance.Definitions.PulseFlyTime, 
		                                          GameManager.Instance.Definitions.PulseFlyWaitTime, 
		                                          GameManager.Instance.Definitions.PulseHitTime,
		                                          null,
		                                          GameManager.Instance.Definitions.ParticleAttackPlayerFly,
		                                          GameManager.Instance.Definitions.ParticleAttackPlayer,
		                                          transform.position,
		                             GameManager.Instance.HUD.Health.GetBarCenter(), false));

		yield return new WaitForSeconds(GameManager.Instance.Definitions.PulseFlyWaitTime);

		GameManager.Instance.HUD.Health.Add(-ActionPoints);

		yield return new WaitForSeconds(GameManager.Instance.Definitions.EnemyToPlayerWaitTime);
	}
	
	public IEnumerator Move(Gem hd)
	{
		Vector2i dPos = hd.GetGridPosition() - _gem.GetGridPosition();
		if (Math.Abs(dPos.x) > Math.Abs(dPos.y))
		{
			dPos.x = Math.Sign(dPos.x); dPos.y = 0;
		}
		else
		{
			dPos.y = Math.Sign(dPos.y); dPos.x = 0;
		}
		
		float posTime = GameManager.Instance.Definitions.EnemyMoveTime;
		
		Vector2i lastPos = _gem.GetGridPosition();
		Vector2i newPos = _gem.GetGridPosition() + dPos;
		Gem sd = GameManager.Instance.Grid.Get(newPos);
		if (sd.GetEffectType() == LevelData.GemEffectType.ET_ENEMY)
			yield break;

		GameManager.Instance.Grid.Set(_gem, newPos, posTime);
		GameManager.Instance.Grid.Set(sd, lastPos, posTime);
		
		// make sure there will be no collapsing after swapping the gems
		// (by changing type of the gem being swapped and checking again)
		// BRUTE FORCE
		while (GameManager.Instance.Grid.CheckMatches() != null)
			sd.GenerateGemType(GameManager.Instance.LevelData.GetBasicGemIndices());
		
		yield return new WaitForSeconds(posTime);
		
		_gem.SetupPosition(newPos, 0);
		sd.SetupPosition(lastPos, 0);
	}

	private void ResetPowerCharge()
	{
		PowerCharge = _power._turns + 1;
		_gem.SetGlowActive(false, _hero);
	}
}

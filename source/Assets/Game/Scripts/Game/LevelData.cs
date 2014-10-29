using UnityEngine;
using System;
using System.Collections.Generic;

public class LevelData:MonoBehaviour
{
	public enum GemEffectType
	{
		ET_GEM_HEALTH,
		ET_GEM_COIN,
		ET_GEM_NORMAL,
		ET_GEM_MULTIPLY,
		ET_GEM_CROSS,
		ET_GEM_BOOST,
		ET_HERO,
		ET_ENEMY,
		ET_EMPTY,

		ET_COUNT
	}
	public struct GemTypeInfo
	{
		public GameObject _prefab;
		public GemEffectType _effectType;
		public int _effectData;
		public bool _colored;
	}

	static HashSet<LevelData.GemEffectType> _basicGemTypes = new HashSet<LevelData.GemEffectType>() { LevelData.GemEffectType.ET_GEM_COIN, LevelData.GemEffectType.ET_GEM_HEALTH, LevelData.GemEffectType.ET_GEM_NORMAL };
	static HashSet<int> _basicGemIndices;
	static HashSet<int> _startGemIndices;

	private int _commonGemTypeCount;
	private int _gemTypeCount;
	private GemTypeInfo[] _gemTypeInfo;

	private int _multiplyGemType;
	private int _crossGemType;
	private int _boostGemType;
	private int _emptyGemType;

	private int _crossDamage;

	public struct Power
	{
		public enum Types
		{
			NONE,
			
			DAMAGE,                // deals damage to opponent's shared health (can only damage player - enemies does not have shared health); damage done is calculated by self.actionPoints * _f + i
			DAMAGE_FIGURES_TARGET, // damages all opponent's figures; damage done is calculated by target.actionPoints * _f + _i
			DAMAGE_FIGURES_SELF,   // damages all opponent's figures; damage done is calculated by self.actionPoints * _f + _i
			ABSORB_GEMS,           // _i defines number of gems inhaled; _i2 defines number of actionPoints added for each gem; _et defines the type of the EffectType of the gems to be absorbed

			COLLECT_GEMS,          // collect gems horizontally and vertically to the distance of _i; hit enemies with self.actionPoints * _f + _i2
			TRANSFORM_RANDOM_GEMS, // transforms _i random gems to type _i2
		}
		
		public int _turns;
		public Types _type;
		public float _f;
		public int _i;
		public int _i2;
		public GemEffectType _et;
	}

	public struct HeroInfo
	{
		public GameObject _prefab;
		public int _gemType;

		public int _actionPoints;

		public Power _power;

		public int _pointsToSpawn;
	}
	private int _heroCount;
	private HeroInfo[] _heroInfo;

	public struct EnemyInfo
	{
		public GameObject _prefab;
		public int _gemType;

		public int _actionPoints;
		
		public Power _power;
	}
	private int _enemyCount;
	private EnemyInfo[] _enemyInfo;

	public struct WaveInfo
	{
		public int _enemyType;
		public int _count;
		public int _moves;
	}
	private int _waveCount;
	private WaveInfo[] _waveInfo;
	private int _totalMoves;
	private int _waveEnemyCount;

	private int _health;
	private int _maxAttack;
	private int _boostPoints;
	
	public void initLevel(int levelIndex)
	{
		switch(levelIndex)
		{
		case 0:
			initLevel1(); break;
		case 1:
			initLevel2(); break;
		case 2:
			initLevel3(); break;
		case 3:
			initLevel4(); break;
		case 4:
			initLevel5(); break;
		case 5:
			initLevel6(); break;
		default:
			initLevelCendos(); break;
		}
		
		_totalMoves = 0;
		_waveEnemyCount = 0;
		for (int i = 0; i < _waveCount; i++)
		{
			_waveEnemyCount += _waveInfo[i]._count;
			_totalMoves += _waveInfo[i]._moves;
		}
	}

	public void initLevelBase(int heroCount)
	{
		_basicGemIndices = new HashSet<int>();
		_startGemIndices = new HashSet<int>();
		_heroCount = heroCount;
		_enemyCount = 4;
		_health = 200;
		_maxAttack = 200;
		_boostPoints = 5;
		_crossDamage = 15;

		_commonGemTypeCount = _heroCount + 2;
		_gemTypeCount = _commonGemTypeCount + _heroCount * 4 + _enemyCount + 1;
		_gemTypeInfo = new GemTypeInfo[_gemTypeCount];
		int i = 0;
		for (i = 0; i < _heroCount; i++)
		{
			_gemTypeInfo[i]._prefab = GameManager.Instance.Definitions.GemNormalPrefabs[i];
			_gemTypeInfo[i]._effectType = GemEffectType.ET_GEM_NORMAL;
			_gemTypeInfo[i]._effectData = i;
			_gemTypeInfo[i]._colored = true;
			_basicGemIndices.Add(i); _startGemIndices.Add(i);
		}
		_gemTypeInfo[i]._prefab = GameManager.Instance.Definitions.GemHealthPrefab;
		_gemTypeInfo[i]._effectType = GemEffectType.ET_GEM_HEALTH;
		_gemTypeInfo[i]._effectData = 10; // health points per gem
		_gemTypeInfo[i]._colored = false;
		_basicGemIndices.Add(i);
		i++;
		_gemTypeInfo[i]._prefab = GameManager.Instance.Definitions.GemCoinPrefab;
		_gemTypeInfo[i]._effectType = GemEffectType.ET_GEM_COIN;
		_gemTypeInfo[i]._effectData = 1; // coins per coin collected :]
		_gemTypeInfo[i]._colored = false;
		_basicGemIndices.Add(i); _startGemIndices.Add(i);
		i++;
		_multiplyGemType = i;
		for (int j = 0; j < _heroCount; j++, i++)
		{
			_gemTypeInfo[i]._prefab = GameManager.Instance.Definitions.GemMultiplyPrefabs[j];
			_gemTypeInfo[i]._effectType = GemEffectType.ET_GEM_MULTIPLY;
			_gemTypeInfo[i]._effectData = j;
			_gemTypeInfo[i]._colored = true;
		}
		_crossGemType = i;
		for (int j = 0; j < _heroCount; j++, i++)
		{
			_gemTypeInfo[i]._prefab = GameManager.Instance.Definitions.GemCrossPrefabs[j];
			_gemTypeInfo[i]._effectType = GemEffectType.ET_GEM_CROSS;
			_gemTypeInfo[i]._effectData = j;
			_gemTypeInfo[i]._colored = true;
		}
		_boostGemType = i;
		for (int j = 0; j < _heroCount; j++, i++)
		{
			_gemTypeInfo[i]._prefab = GameManager.Instance.Definitions.GemBoostPrefabs[j];
			_gemTypeInfo[i]._effectType = GemEffectType.ET_GEM_BOOST;
			_gemTypeInfo[i]._effectData = j;
			_gemTypeInfo[i]._colored = true;
		}
		int heroGemType = i;
		for (int j = 0; j < _heroCount; j++, i++)
		{
			_gemTypeInfo[i]._prefab = GameManager.Instance.Definitions.HeroPrefabs[j];
			_gemTypeInfo[i]._effectType = GemEffectType.ET_HERO;
			_gemTypeInfo[i]._effectData = j;
			_gemTypeInfo[i]._colored = true;
		}
		int enemyGemType = i;
		for (int j = 0; j < _enemyCount; j++, i++)
		{
			_gemTypeInfo[i]._prefab = GameManager.Instance.Definitions.EnemyPrefabs[j];
			_gemTypeInfo[i]._effectType = GemEffectType.ET_ENEMY;
			_gemTypeInfo[i]._effectData = j;
			_gemTypeInfo[i]._colored = false;
		}
		_emptyGemType = i;
		_gemTypeInfo[i]._prefab = GameManager.Instance.Definitions.GemCoinPrefab;
		_gemTypeInfo[i]._effectType = GemEffectType.ET_EMPTY;
		_gemTypeInfo[i]._effectData = -2;
		_gemTypeInfo[i]._colored = false;
		i++;

		_heroInfo = new HeroInfo[_heroCount];
		for (int j = 0; j < _heroCount; j++)
		{
			_heroInfo[j]._prefab = GameManager.Instance.Definitions.HeroHUDPrefabs[j];
			_heroInfo[j]._gemType = heroGemType + j;
		}
		_heroInfo[0]._pointsToSpawn = 7; //amount of gems to spawn hero
		_heroInfo[0]._actionPoints = 5; //initial amount of health for spawned hero
		_heroInfo[0]._power._type = Power.Types.COLLECT_GEMS;
		_heroInfo[0]._power._turns = 3;
		_heroInfo[0]._power._i = 3;
		_heroInfo[0]._power._f = 0;
		_heroInfo[0]._power._i2 = 5;
		_heroInfo[1]._pointsToSpawn = 13; //amount of gems to spawn hero
		_heroInfo[1]._actionPoints = 8; //initial amount of health for spawned hero
		_heroInfo[1]._power._type = Power.Types.TRANSFORM_RANDOM_GEMS;
		_heroInfo[1]._power._turns = 4;
		_heroInfo[1]._power._i = 9;
		_heroInfo[1]._power._i2 = 1;
		if (heroCount>2)
		{
			_heroInfo[2]._pointsToSpawn = 19; //amount of gems to spawn hero
			_heroInfo[2]._actionPoints = 30; //initial amount of health for spawned hero
			_heroInfo[2]._power._type = Power.Types.DAMAGE_FIGURES_TARGET;
			_heroInfo[2]._power._turns = 5;
			_heroInfo[2]._power._f = 0.25f;
			_heroInfo[2]._power._i = 0;
		}
		if (heroCount>3)
		{
			_heroInfo[3]._pointsToSpawn = 20; //amount of gems to spawn hero
			_heroInfo[3]._actionPoints = 35; //initial amount of health for spawned hero
		}
		
		_enemyInfo = new EnemyInfo[_enemyCount];
		for (int j = 0; j < _enemyCount; j++)
		{
			_enemyInfo[j]._prefab = GameManager.Instance.Definitions.EnemyHUDPrefabs[j];
			_enemyInfo[j]._gemType = enemyGemType + j;
		}
		
		_enemyInfo[0]._actionPoints = 30;
		_enemyInfo[0]._power._turns = 0;
		_enemyInfo[0]._power._type = Power.Types.NONE;

		_enemyInfo[1]._actionPoints = 35;
		_enemyInfo[1]._power._turns = 3;
		_enemyInfo[1]._power._type = Power.Types.ABSORB_GEMS;
		_enemyInfo[1]._power._f = 0;
		_enemyInfo[1]._power._i = 3;
		_enemyInfo[1]._power._i2 = 3;
		_enemyInfo[1]._power._et = GemEffectType.ET_GEM_HEALTH;

		_enemyInfo[2]._actionPoints = 30;
		_enemyInfo[2]._power._turns = 2;
		_enemyInfo[2]._power._type = Power.Types.DAMAGE;
		_enemyInfo[2]._power._f = 1;
		_enemyInfo[2]._power._i = 0;

		_enemyInfo[3]._actionPoints = 60;
		_enemyInfo[3]._power._turns = 4;
		_enemyInfo[3]._power._type = Power.Types.DAMAGE_FIGURES_SELF;
		_enemyInfo[3]._power._f = 0.20f;
		_enemyInfo[3]._power._i = 0;
	}
	
	public void initLevel1()
	{
		initLevelBase(3);
		
		_waveCount = 2;
		_waveInfo = new WaveInfo[_waveCount];
		
		//waves
		_waveInfo[0]._enemyType = 0;
		_waveInfo[0]._count = 1;
		_waveInfo[0]._moves = 5;
		
		_waveInfo[1]._enemyType = 0;
		_waveInfo[1]._count = 2;
		_waveInfo[1]._moves = 2;
		
	}
	
	public void initLevel2()
	{
		initLevelBase(3);
		
		_waveCount = 3;
		_waveInfo = new WaveInfo[_waveCount];
		
		//waves
		_waveInfo[0]._enemyType = 0;
		_waveInfo[0]._count = 1;
		_waveInfo[0]._moves = 5;
		
		_waveInfo[1]._enemyType = 0;
		_waveInfo[1]._count = 2;
		_waveInfo[1]._moves = 5;
		
		_waveInfo[2]._enemyType = 1;
		_waveInfo[2]._count = 1;
		_waveInfo[2]._moves = 5;
		
	}
	
	public void initLevel3()
	{
		initLevelBase(3);
		
		_waveCount = 3;
		_waveInfo = new WaveInfo[_waveCount];
		
		//waves
		_waveInfo[0]._enemyType = 1;
		_waveInfo[0]._count = 2;
		_waveInfo[0]._moves = 6;
		
		_waveInfo[1]._enemyType = 0;
		_waveInfo[1]._count = 2;
		_waveInfo[1]._moves = 5;
		
		_waveInfo[2]._enemyType = 3;
		_waveInfo[2]._count = 1;
		_waveInfo[2]._moves = 6;
		
	}

	public void initLevel4()
	{
		initLevelBase(3);
		
		_waveCount = 4;
		_waveInfo = new WaveInfo[_waveCount];
		
		//waves
		_waveInfo[0]._enemyType = 0;
		_waveInfo[0]._count = 1;
		_waveInfo[0]._moves = 3;
		
		_waveInfo[1]._enemyType = 1;
		_waveInfo[1]._count = 1;
		_waveInfo[1]._moves = 9;
		
		_waveInfo[2]._enemyType = 3;
		_waveInfo[2]._count = 1;
		_waveInfo[2]._moves = 8;
		
		_waveInfo[3]._enemyType = 2;
		_waveInfo[3]._count = 3;
		_waveInfo[3]._moves = 6;
	}	
	
	public void initLevel5()
	{
		initLevelBase(3);
		
		_waveCount = 4;
		_waveInfo = new WaveInfo[_waveCount];
		
		//waves
		_waveInfo[0]._enemyType = 0;
		_waveInfo[0]._count = 2;
		_waveInfo[0]._moves = 6;
		
		_waveInfo[1]._enemyType = 3;
		_waveInfo[1]._count = 1;
		_waveInfo[1]._moves = 6;
		
		_waveInfo[2]._enemyType = 2;
		_waveInfo[2]._count = 2;
		_waveInfo[2]._moves = 8;
		
		_waveInfo[3]._enemyType = 0;
		_waveInfo[3]._count = 4;
		_waveInfo[3]._moves = 8;
		
	}	
	
	public void initLevel6()
	{
		initLevelBase(3);
		
		_waveCount = 3;
		_waveInfo = new WaveInfo[_waveCount];
		
		//waves
		_waveInfo[0]._enemyType = 1;
		_waveInfo[0]._count = 2;
		_waveInfo[0]._moves = 10;
		
		_waveInfo[1]._enemyType = 3;
		_waveInfo[1]._count = 2;
		_waveInfo[1]._moves = 8;
		
		_waveInfo[2]._enemyType = 2;
		_waveInfo[2]._count = 3;
		_waveInfo[2]._moves = 9;
	}
	
	public void initLevelCendos()
	{
		initLevelBase(3);
		
		_health = 250;

		_waveCount = 3;
		_waveInfo = new WaveInfo[_waveCount];
		
		//waves
		_waveInfo[0]._enemyType = 0;
		_waveInfo[0]._count = 3;
		_waveInfo[0]._moves = 1;
		
		_waveInfo[1]._enemyType = 3;
		_waveInfo[1]._count = 1;
		_waveInfo[1]._moves = 20;
		
		_waveInfo[2]._enemyType = 2;
		_waveInfo[2]._count = 3;
		_waveInfo[2]._moves = 30;
		
		_heroInfo[0]._pointsToSpawn = 10; //amount of gems to spawn hero
		_heroInfo[0]._actionPoints = 30; //initial amount of health for spawned hero
		/*_heroInfo[0]._power._type = Power.Types.ABSORB_GEMS;
		_heroInfo[0]._power._turns = 1;
		_heroInfo[0]._power._f = 0;
		_heroInfo[0]._power._i = 3;
		_heroInfo[0]._power._i2 = 3;
		_heroInfo[0]._power._et = GemEffectType.ET_GEM_HEALTH;*/

		_heroInfo[1]._pointsToSpawn = 2; //amount of gems to spawn hero
		_heroInfo[1]._actionPoints = 2; //initial amount of health for spawned hero
		if (_heroCount>2)
		{
			_heroInfo[2]._pointsToSpawn = 2; //amount of gems to spawn hero
			_heroInfo[2]._actionPoints = 2; //initial amount of health for spawned hero
		}
		if (_heroCount>3)
		{
			_heroInfo[3]._pointsToSpawn = 2; //amount of gems to spawn hero
			_heroInfo[3]._actionPoints = 2; //initial amount of health for spawned hero
		}
	}
	
	static public HashSet<LevelData.GemEffectType> GetBasicGemTypes() { return _basicGemTypes; }
	public HashSet<int> GetBasicGemIndices() { return _basicGemIndices; }
	public HashSet<int> GetStartGemIndices() { return _startGemIndices; }

	public int GetHealth() { return _health; }
	public int GetMaxAttack() { return _maxAttack; }
	public int GetBoostPoints() { return _boostPoints; }
	public int GetCrossDamage() { return _crossDamage; }

	public GemTypeInfo GetGemTypeInfo(int type) { return _gemTypeInfo[type]; }
	public int GetCommonGemTypeCount() { return _commonGemTypeCount; }
	public int GetGemTypeCount() { return _gemTypeCount; }
	public int GetMultiplyGemType() { return _multiplyGemType; }
	public int GetCrossGemType() { return _crossGemType; }
	public int GetBoostGemType() { return _boostGemType; }
	public int GetEmptyGemType() { return _emptyGemType; }

	public HeroInfo GetHeroInfo(int idx) { return _heroInfo[idx]; }
	public int GetHeroCount() { return _heroCount; }

	public EnemyInfo GetEnemyInfo(int idx) { return _enemyInfo[idx]; }
	public int GetEnemyCount() { return _enemyCount; }

	public WaveInfo GetWaveInfo(int idx) { return _waveInfo[idx]; }
	public int GetWaveCount() { return _waveCount; }
	public int GetWaveEnemyCount() { return _waveEnemyCount; }
	
	public int GetTotalMoves() { return _totalMoves; }

	Dictionary<int,List<GameObject>> _gemPrefabs = new Dictionary<int,List<GameObject>>();
	public GameObject LendGemPrefab(int type)
	{
		List<GameObject> goList;
		if (_gemPrefabs.TryGetValue(type, out goList))
		{
			if (goList.Count > 0)
			{
				int index = goList.Count - 1;
				GameObject go = goList[index];
				goList.RemoveAt(index);
				return go;
			}
		}
		return GameObject.Instantiate(_gemTypeInfo[type]._prefab) as GameObject;
	}

	public void ReturnGemPrefab(int type, GameObject go)
	{
		go.SetActive(false);
		go.transform.parent = null;

		List<GameObject> goList;
		if (_gemPrefabs.TryGetValue(type, out goList))
		{
			goList.Add(go);
		}
		else
		{
			_gemPrefabs.Add(type, new List<GameObject>() { go });
		}
	}
}

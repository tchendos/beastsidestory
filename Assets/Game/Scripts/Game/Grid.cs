using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Grid:MonoBehaviour
{
	struct GridItem
	{
		public Gem _script;
	}
	
	private GridItem[,] _grid;

	private LevelData _levelData;

	static public Vector3 CalculatePosition(Vector2i gridPos)
	{
		return new Vector3(Constants.FIELD_POS_X + gridPos.x * Constants.FIELD_GRID_X, 
		                   Constants.FIELD_POS_Y - gridPos.y * Constants.FIELD_GRID_Y, 0);
	}
	
	static public Directions DetermineDirection(Vector2i newPos, Vector2i lastPos)
	{
		Directions dir;

		if (newPos.x == lastPos.x)
			dir = newPos.y > lastPos.y ? Directions.Down : Directions.Up;
		else if (newPos.y == lastPos.y)
			dir = newPos.x > lastPos.x ? Directions.Right : Directions.Left;
		else 
		{
			DebugUtils.Assert(false);
			dir = Directions.None;
		}
		return dir;
	}

	public static Vector2i RandomGridPosition()
	{
		return new Vector2i(UnityEngine.Random.Range(0, Constants.FIELD_SIZE_X), UnityEngine.Random.Range(0, Constants.FIELD_SIZE_Y));
	}

	// todo: potentially, method can hang - for example when 2x2 heroes / enemies are in the corner and the starting position is there as well
	// workaround: make radius scale
	public Vector2i FindValidPosition(Vector2i sPos, HashSet<LevelData.GemEffectType> validTypes, int excludeColor)
	{
		Vector2i gridPos = sPos;
		int i = 0;
		while(true)
		{
			if (Grid.Valid(gridPos))
			{
				LevelData.GemTypeInfo gti = GameManager.Instance.LevelData.GetGemTypeInfo(Get(gridPos).GetGemType());
				if (validTypes.Contains(gti._effectType) && (!gti._colored || gti._effectData != excludeColor))
					return gridPos;
			}
			int span = 1, mod = 3;
			if (i > 9)
			{
				span = 2; mod = 5;
			}
			gridPos.x = sPos.x - span + i % mod;
			gridPos.y = sPos.y - span + i / mod;
			i++;
		}
	}
	
	void Awake()
	{
		_levelData = GameManager.Instance.LevelData;

		_grid = new GridItem[Constants.FIELD_SIZE_X, Constants.FIELD_SIZE_Y];
	}

	public Gem Get(Vector2i gridPos)
	{
		return _grid[gridPos.x,gridPos.y]._script;
	}
	public void Set(Gem gem, Vector2i gridPos, float time)
	{
		_grid[gridPos.x, gridPos.y]._script = gem;
		gem.SetupPosition(gridPos, time);
	}
	static public bool Valid(Vector2i pos)
	{
		return (pos.x >= 0 && pos.x < Constants.FIELD_SIZE_X &&
		        pos.y >= 0 && pos.y < Constants.FIELD_SIZE_Y);
	}

	public Vector2i FindNearest(Vector2i start, LevelData.GemEffectType type)
	{
		Vector2i gridPos;
		Vector2i nearestPos = new Vector2i(-1, -1);
		int nearestDist = int.MaxValue;
		for(gridPos.x = 0; gridPos.x < Constants.FIELD_SIZE_X; gridPos.x++)
		{
			for(gridPos.y = 0; gridPos.y < Constants.FIELD_SIZE_Y; gridPos.y++)
			{
				if (!_grid[gridPos.x, gridPos.y]._script.IsDead() && _grid[gridPos.x, gridPos.y]._script.GetEffectType() == type)
				{
					Vector2i dPos = gridPos - start;
					int dist = dPos.MagnitudeSqr();
					if (nearestDist > dist)
					{
						nearestDist = dist;
						nearestPos = gridPos;
						if (dist == 1)
							return gridPos;
					}
				}
			}
		}
		return nearestPos;
	}
		

	public void Generate()
	{
		GameObject prefab = GameManager.Instance.Definitions.GemBasePrefab;
		prefab.SetActive(true);

		GameObject gem;
		Gem gemScript;
		Vector2i gridPos;
		for(gridPos.x = 0; gridPos.x < Constants.FIELD_SIZE_X; gridPos.x++)
		{
			for(gridPos.y = 0; gridPos.y < Constants.FIELD_SIZE_Y; gridPos.y++)
			{
				gem = GameObject.Instantiate(prefab) as GameObject;
				gem.name = "diamondInst" + gridPos.x + "_" + gridPos.y;
				gem.transform.parent = GameManager.Instance.Definitions.FieldRoot.transform;

				gemScript = gem.GetComponent<Gem>();
				gemScript.Init(gridPos);
				_grid[gridPos.x, gridPos.y]._script = gemScript;
			}
		}

		prefab.SetActive(false);
	}

	public bool IsValidSwap(Vector2i newPos, Vector2i lastPos)
	{
		if (GameManager.Instance.InitStruct.propSwap)
			return true;

		Gem newGem = _grid[newPos.x, newPos.y]._script;
		Gem oldGem = _grid[lastPos.x, lastPos.y]._script;
		
		_grid[newPos.x, newPos.y]._script = oldGem;
		_grid[lastPos.x, lastPos.y]._script = newGem;
		bool[,] matched = CheckMatches();
		_grid[newPos.x, newPos.y]._script = newGem;
		_grid[lastPos.x, lastPos.y]._script = oldGem;

		return matched != null;
	}

	public IEnumerator SwapGems(Vector2i newPos, Vector2i lastPos)
	{
		float posTime = GameManager.Instance.Definitions.SwapGemsTime;
		
		Gem newGem = _grid[newPos.x, newPos.y]._script;
		Gem oldGem = _grid[lastPos.x, lastPos.y]._script;

		_grid[newPos.x, newPos.y]._script = oldGem;
		_grid[newPos.x, newPos.y]._script.SetupPosition(newPos, posTime);
		_grid[lastPos.x, lastPos.y]._script = newGem;
		_grid[lastPos.x, lastPos.y]._script.SetupPosition(lastPos, posTime);
		
		yield return new WaitForSeconds(posTime);
		
		_grid[newPos.x, newPos.y]._script.SetupPosition(newPos, 0);
		_grid[lastPos.x, lastPos.y]._script.SetupPosition(lastPos, 0);
	}
	
	public IEnumerator SwapGemsFail(Vector2i newPos, Vector2i lastPos)
	{
		float posTime = GameManager.Instance.Definitions.SwapGemsFailTime;
		
		_grid[newPos.x, newPos.y]._script.SetupPosition(lastPos, posTime);
		_grid[lastPos.x, lastPos.y]._script.SetupPosition(newPos, posTime);
		
		yield return new WaitForSeconds(posTime);
		
		_grid[newPos.x, newPos.y]._script.SetupPosition(newPos, posTime);
		_grid[lastPos.x, lastPos.y]._script.SetupPosition(lastPos, posTime);
		
		yield return new WaitForSeconds(posTime);
		
		_grid[newPos.x, newPos.y]._script.SetupPosition(newPos, 0);
		_grid[lastPos.x, lastPos.y]._script.SetupPosition(lastPos, 0);
	}

	public bool[,] CheckMatches()
	{
		bool[,] matches = null;
		Vector2i gridPos;
		for(gridPos.x = 0; gridPos.x < Constants.FIELD_SIZE_X; gridPos.x++)
		{
			for(gridPos.y = 0; gridPos.y < Constants.FIELD_SIZE_Y; gridPos.y++)
			{
				Gem gem = _grid[gridPos.x, gridPos.y]._script;
				int type = gem.GetGemType();
				if (gem.IsDead())
					SetMatch(ref matches, gridPos, true);
				if ((gridPos.x + 2) < Constants.FIELD_SIZE_X &&
				    CheckMatch(type, _grid[gridPos.x + 1, gridPos.y]._script.GetGemType()) &&
				    CheckMatch(type, _grid[gridPos.x + 2, gridPos.y]._script.GetGemType()))
				{
					SetMatch(ref matches, gridPos, _levelData.GetGemTypeInfo(type)._effectType != LevelData.GemEffectType.ET_HERO);
					SetMatch(ref matches, gridPos + new Vector2i(1,0), _levelData.GetGemTypeInfo(_grid[gridPos.x + 1, gridPos.y]._script.GetGemType())._effectType != LevelData.GemEffectType.ET_HERO);
					SetMatch(ref matches, gridPos + new Vector2i(2,0), _levelData.GetGemTypeInfo(_grid[gridPos.x + 2, gridPos.y]._script.GetGemType())._effectType != LevelData.GemEffectType.ET_HERO);
				}
				if ((gridPos.y + 2) < Constants.FIELD_SIZE_Y &&
				    CheckMatch(type, _grid[gridPos.x, gridPos.y + 1]._script.GetGemType()) &&
				    CheckMatch(type, _grid[gridPos.x, gridPos.y + 2]._script.GetGemType()))
				{
					SetMatch(ref matches, gridPos, _levelData.GetGemTypeInfo(type)._effectType != LevelData.GemEffectType.ET_HERO);
					SetMatch(ref matches, gridPos + new Vector2i(0,1), _levelData.GetGemTypeInfo(_grid[gridPos.x, gridPos.y + 1]._script.GetGemType())._effectType != LevelData.GemEffectType.ET_HERO);
					SetMatch(ref matches, gridPos + new Vector2i(0,2), _levelData.GetGemTypeInfo(_grid[gridPos.x, gridPos.y + 2]._script.GetGemType())._effectType != LevelData.GemEffectType.ET_HERO);
				}
			}
		}
		
		return matches;
	}

	private void SetMatch(ref bool[,] matches, Vector2i gridPos, bool state)
	{
		if (matches == null)
			matches = new bool[Constants.FIELD_SIZE_X,Constants.FIELD_SIZE_Y];
		
		matches[gridPos.x, gridPos.y] = true;//state;
	}

	public List<Vector2i> DetectMatchedGems(bool[,] matched, LevelData.GemEffectType effectType)
	{
		List<Vector2i> list = null;
		Vector2i gridPos;
		for(gridPos.x = 0; gridPos.x < Constants.FIELD_SIZE_X; gridPos.x++)
		{
			for(gridPos.y = 0; gridPos.y < Constants.FIELD_SIZE_Y; gridPos.y++)
			{
				if (matched[gridPos.x, gridPos.y])
				{
					Gem gem = _grid[gridPos.x, gridPos.y]._script;
					LevelData.GemTypeInfo ti = _levelData.GetGemTypeInfo(gem.GetGemType());
					if (ti._effectType == effectType)
					{
						if (list == null)
							list = new List<Vector2i>();
						list.Add(gridPos);
					}
				}
			}
		}
		return list;
	}

	public int Collapse(bool[,] matched, float posTime, Directions dir)
	{
		if (GameManager.Instance.InitStruct.propTopFall)
			dir = Directions.Down;

		int x, y, xx, yy, ctr, maxCtr = 0;
		switch(dir)
		{
		case Directions.Down:
			for(x = 0; x < Constants.FIELD_SIZE_X; x++)
			{
				ctr = 1;
				for(y = Constants.FIELD_SIZE_Y - 1; y >= 0; y--)
				{
					Gem gem = _grid[x, y]._script;
					if (gem.IsDead())
					{
						for(yy = y; yy > 0; yy--)
						{
							_grid[x, yy]._script = _grid[x, yy - 1]._script;
							_grid[x, yy]._script.SetupPosition(new Vector2i(x, yy), posTime * ctr);
						}
						_grid[x, 0]._script = gem;
						_grid[x, 0]._script.SetupPosition(new Vector2i(x, -1 * ctr), 0);
						_grid[x, 0]._script.SetupPosition(new Vector2i(x, 0), posTime * ctr);
						_grid[x, 0]._script.GenerateGemType(GameManager.Instance.LevelData.GetBasicGemIndices());
						y++;
						ctr++;
					}
				}
				if (ctr > maxCtr)
					maxCtr = ctr;
			}
			break;
		case Directions.Up:
			for(x = 0; x < Constants.FIELD_SIZE_X; x++)
			{
				ctr = 1;
				for(y = 0; y < Constants.FIELD_SIZE_Y; y++)
				{
					Gem gem = _grid[x, y]._script;
					if (gem.IsDead())
					{
						for(yy = y; yy < Constants.FIELD_SIZE_Y - 1; yy++)
						{
							_grid[x, yy]._script = _grid[x, yy + 1]._script;
							_grid[x, yy]._script.SetupPosition(new Vector2i(x, yy), posTime * ctr);
						}
						_grid[x, Constants.FIELD_SIZE_Y - 1]._script = gem;
						_grid[x, Constants.FIELD_SIZE_Y - 1]._script.SetupPosition(new Vector2i(x, Constants.FIELD_SIZE_Y - 1 + ctr), 0);
						_grid[x, Constants.FIELD_SIZE_Y - 1]._script.SetupPosition(new Vector2i(x, Constants.FIELD_SIZE_Y - 1), posTime * ctr);
						_grid[x, Constants.FIELD_SIZE_Y - 1]._script.GenerateGemType(GameManager.Instance.LevelData.GetBasicGemIndices());
						y--;
						ctr++;
					}
				}
				if (ctr > maxCtr)
					maxCtr = ctr;
			}
			break;
		case Directions.Right:
			for(y = 0; y < Constants.FIELD_SIZE_Y; y++)
			{
				ctr = 1;
				for(x = Constants.FIELD_SIZE_X - 1; x >= 0; x--)
				{
					Gem gem = _grid[x, y]._script;
					if (gem.IsDead())
					{
						for(xx = x; xx > 0; xx--)
						{
							_grid[xx, y]._script = _grid[xx - 1, y]._script;
							_grid[xx, y]._script.SetupPosition(new Vector2i(xx, y), posTime * ctr);
						}
						_grid[0, y]._script = gem;
						_grid[0, y]._script.SetupPosition(new Vector2i(-1 * ctr, y), 0);
						_grid[0, y]._script.SetupPosition(new Vector2i(0, y), posTime * ctr);
						_grid[0, y]._script.GenerateGemType(GameManager.Instance.LevelData.GetBasicGemIndices());
						x++;
						ctr++;
					}
				}
				if (ctr > maxCtr)
					maxCtr = ctr;
			}
			break;
		case Directions.Left:
			for(y = 0; y < Constants.FIELD_SIZE_Y; y++)
			{
				ctr = 1;
				for(x = 0; x < Constants.FIELD_SIZE_X; x++)
				{
					Gem gem = _grid[x, y]._script;
					if (gem.IsDead())
					{
						for(xx = x; xx < Constants.FIELD_SIZE_X - 1; xx++)
						{
							_grid[xx, y]._script = _grid[xx + 1, y]._script;
							_grid[xx, y]._script.SetupPosition(new Vector2i(xx, y), posTime * ctr);
							matched[xx, y] = matched[xx + 1, y];
						}
						_grid[Constants.FIELD_SIZE_X - 1, y]._script = gem;
						_grid[Constants.FIELD_SIZE_X - 1, y]._script.SetupPosition(new Vector2i(Constants.FIELD_SIZE_X - 1 + ctr, y), 0);
						_grid[Constants.FIELD_SIZE_X - 1, y]._script.SetupPosition(new Vector2i(Constants.FIELD_SIZE_X - 1, y), posTime * ctr);
						_grid[Constants.FIELD_SIZE_X - 1, y]._script.GenerateGemType(GameManager.Instance.LevelData.GetBasicGemIndices());
						matched[Constants.FIELD_SIZE_X - 1, y] = false;
						x--;
						ctr++;
					}
				}
				if (ctr > maxCtr)
					maxCtr = ctr;
			}
			break;
		default:
			DebugUtils.Assert(false); 
			break;
		}

		return maxCtr;
	}
	
	public struct MoveCombination
	{
		public MoveCombination(Gem d1, Gem d2, Gem d3) 
		{
			this.d1 = d1;
			this.d2 = d2;
			this.d3 = d3;
		}
		
		public Gem d1;
		public Gem d2;
		public Gem d3;
	}
	
	public List<MoveCombination> CheckMoves()
	{
		int actualType = -1;
		Gem actualScript = null;
		Gem nextScript = null;
		List<MoveCombination> availableMoves = new List<MoveCombination>();
		
		//--- just for debug - to see if its works ok
		//--- remove FOR cycle
		Vector2i pos;
		for(pos.x = 0; pos.x < Constants.FIELD_SIZE_X; pos.x++)
		{
			for(pos.y = 0; pos.y < Constants.FIELD_SIZE_Y; pos.y++)
			{
				actualScript = GetGemScriptOnPosition(pos);
				SpriteRenderer r = renderer as SpriteRenderer;
				r.color = Color.white;
			}
		}
		//---
		
		// check all vertical combinations
		for(pos.x = 0; pos.x < Constants.FIELD_SIZE_X; pos.x++)
		{
			for(pos.y = 0; pos.y < Constants.FIELD_SIZE_Y; pos.y++)
			{
				actualScript = GetGemScriptOnPosition(pos);
				actualType = actualScript.GetGemType();
				
				nextScript = GetGemScriptOnPosition(pos + new Vector2i(0,1));
				
				if(nextScript && actualType == nextScript.GetGemType()) 
				{
					if(addMove(actualScript, nextScript, pos + new Vector2i(0,3), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(1,2), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(-1,2), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(0,-2), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(1,-1), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(-1,-1), availableMoves)) continue;
				}
				
				nextScript = GetGemScriptOnPosition(pos + new Vector2i(0,2));
				
				if(nextScript && actualType == nextScript.GetGemType()) 
				{
					if(addMove(actualScript, nextScript, pos + new Vector2i(-1,1), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(1,1), availableMoves)) continue;
				}
			}
		}
		
		// check all horizontal combinations
		for(pos.y = 0; pos.y < Constants.FIELD_SIZE_Y; pos.y++)
		{
			for(pos.x = 0; pos.x < Constants.FIELD_SIZE_X; pos.x++)
			{
				actualScript = GetGemScriptOnPosition(pos);
				actualType = actualScript.GetGemType();
				
				nextScript = GetGemScriptOnPosition(pos + new Vector2i(1,0));
				
				if(nextScript && actualType == nextScript.GetGemType()) 
				{
					if(addMove(actualScript, nextScript, pos + new Vector2i(3,0), 	 availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(2,-1), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(2,1), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(-2,0), 	 availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(-1,-1), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(-1,1), availableMoves)) continue;
				}
				
				nextScript = GetGemScriptOnPosition(pos + new Vector2i(2,0));
				
				if(nextScript && actualType == nextScript.GetGemType()) 
				{
					if(addMove(actualScript, nextScript, pos + new Vector2i(1,1), availableMoves)) continue;
					if(addMove(actualScript, nextScript, pos + new Vector2i(1,-1), availableMoves)) continue;
				}
			}
		}
		
		return availableMoves;
	}

	private bool addMove(Gem actualScript, Gem nextScript, Vector2i thirdScript, List<MoveCombination> availableMoves)
	{
		Gem d = CheckGemTypeOnPosition(thirdScript, actualScript.GetGemType());
		if(d) 
		{
			SpriteRenderer r = renderer as SpriteRenderer;
			r.color = new Color(1.0f, 1.0f, 1.0f, 0.1f);
			availableMoves.Add(new MoveCombination(actualScript, nextScript, d));
			return true;
		}
		return false;
	}
	
	private Gem GetGemScriptOnPosition(Vector2i gridPos) 
	{
		if (Valid(gridPos))
			return _grid[gridPos.x, gridPos.y]._script; 
		else return null;
	}
	
	private Gem CheckGemTypeOnPosition(Vector2i gridPos, int type)
	{
		Gem d = GetGemScriptOnPosition(gridPos);
		
		if(d && CheckMatch(d.GetGemType(), type)) 
			return d;
		
		return null;
	}

	private bool CheckMatch(int type1, int type2)
	{
		LevelData.GemTypeInfo ti1 = _levelData.GetGemTypeInfo(type1);
		LevelData.GemTypeInfo ti2 = _levelData.GetGemTypeInfo(type2);
		
		if (ti1._colored && ti2._colored && ti1._effectData == ti2._effectData)
			return true;
		if (ti1._effectType == LevelData.GemEffectType.ET_GEM_HEALTH && ti2._effectType == LevelData.GemEffectType.ET_GEM_HEALTH)
			return true;
		if (ti1._effectType == LevelData.GemEffectType.ET_GEM_COIN && ti2._effectType == LevelData.GemEffectType.ET_GEM_COIN)
			return true;

		return false;
	}

	string[] effectStr = new string[(int)LevelData.GemEffectType.ET_COUNT] { "H", "C", "G", "M", "X", "B", "@", "#", "."};

	public void Log()
	{
		if (Application.isEditor)
		{
			string str = "";
			for(int y = 0; y < Constants.FIELD_SIZE_Y; y++)
			{
				for(int x = 0; x < Constants.FIELD_SIZE_X; x++)
				{
					LevelData.GemTypeInfo gi = GameManager.Instance.LevelData.GetGemTypeInfo(_grid[x,y]._script.GetGemType());
					int data = gi._effectData;
					if (data > 9)
						data = 0;
					str += effectStr[(int)gi._effectType] + data.ToString() + " ";
				}
				str += "\n";
			}
			Debug.Log(str);
		}
	}
}

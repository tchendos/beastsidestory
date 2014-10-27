using UnityEngine;
using System.Collections;

public class GameManager:MonoBehaviour
{
	static GameManager _instance;

	public Grid Grid { get; private set; }
	public HeroManager HeroManager { get; private set; }
	public EnemyManager EnemyManager { get; private set; }
	public HUD.Manager HUD { get; private set; }
	public Controller Controller { get; private set; }
	public Definitions Definitions { get; private set; }
	public LevelData LevelData { get; private set; }
	public Effects Effects { get; private set; }

	public InitStruct InitStruct { get; private set; }

	void Awake()
	{
		_instance = this;

		InitStruct = PersistentData.Instance().Data as InitStruct;
		if (InitStruct == null)
		{
			InitStruct = new InitStruct(-1, true, true);
		}

		Definitions = FindOrAddComponent<Definitions>();

		Constants.Init();

		LevelData = FindOrAddComponent<LevelData>();
		LevelData.initLevel(InitStruct.levelIndex);

		Grid = FindOrAddComponent<Grid>();
		EnemyManager = FindOrAddComponent<EnemyManager>();
		HeroManager = FindOrAddComponent<HeroManager>();
		HUD = FindOrAddComponent<HUD.Manager>();
		Controller = FindOrAddComponent<Controller>();
		Effects = FindOrAddComponent<Effects>();
		Puzzle.Init();
	}

	private T FindOrAddComponent<T>() where T : Component
	{
		T cmp = gameObject.GetComponent<T>();
		if (cmp == null)
			cmp = gameObject.AddComponent<T>();
		return cmp;
	}

	public static GameManager Instance
	{
		get
		{
			return _instance;
		}
	}
}

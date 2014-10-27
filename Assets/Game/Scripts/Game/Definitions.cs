using UnityEngine;
using System.Collections.Generic;

public class Definitions:MonoBehaviour
{
	public Camera MainCamera;

	public GameObject GemBasePrefab;

	public GameObject GemHealthPrefab;
	public GameObject GemCoinPrefab;
	public GameObject[] GemNormalPrefabs;
	public GameObject[] GemMultiplyPrefabs;
	public GameObject[] GemCrossPrefabs;
	public GameObject[] GemBoostPrefabs;
	public GameObject[] HeroPrefabs;
	public GameObject[] HeroHUDPrefabs;
	public GameObject[] EnemyPrefabs;
	public GameObject[] EnemyHUDPrefabs;

	public GameObject FieldRoot;

	public GameObject HudHeart;
	public GameObject HudHealthBar;
	public GameObject HudHealthLabel;
	public GameObject HudCoins;
	public GameObject HudCoinsLabel;
	public GameObject HudOverlayWin;
	public GameObject HudOverlayLose;
	public GameObject HudWave;
	public GameObject HudWaveBar;
	public GameObject HudMultiplier;

	public GameObject ParticleAttack;

	public GameObject ParticleAttackPlayerFly;
	public GameObject ParticleAttackPlayer;

	public GameObject ParticlePowerDamageFly;
	public GameObject ParticlePowerDamage;

	public GameObject ParticlePowerDamageFigureFly;
	public GameObject ParticlePowerDamageFigure;
	
	public GameObject ParticlePowerAbsorbHealthGem;
	public GameObject ParticlePowerAbsorbHealthFly;
	public GameObject ParticlePowerAbsorbHealth;

	public GameObject ParticlePowerCollectGem;
	public GameObject ParticlePowerCollectGemEnemy;

	public GameObject ParticlePowerTransformGem;

	public GameObject ParticleCollectFly;
	public GameObject ParticleCollect;
	
	public GameObject ParticleBoostFly;
	public GameObject ParticleBoost;
	
	public GameObject ParticleCross;
	public GameObject ParticleCrossEnemy;

	public GameObject ParticleSpecialGem;

	public Material DisabledEnemyMaterial;
	public Color DisabledEnemyColor = new Color(1, 1, 1, 0.7f);

	public Color HeroLifeColor = new Color(0.78f, 1.0f, 0.625f, 1.0f);
	public Color EnemyLifeColor = new Color(0.76f, 0.14f, 0.0f, 1.0f);

	public float AttackMoveTime = 0.2f;
	public float AttackParticleTime = 0.1f;
	public float SpawnTime = 0.8f;
	public float PulseTime = 0.4f;
	public float PulseFlyTime = 0.4f;
	public float PulseFlyWaitTime = 0.2f;
	public float PulseHitTime = 0.8f;
	public float EnemyToPlayerWaitTime = 0.5f;
	public float BoostFlyTime = 0.3f;
	public float BoostFlyWaitTime = 0.1f;
	public float BoostHitTime = 0.8f;
	public float MatchFlyTime = 0.3f;
	public float MatchFlyWaitTime = 0.1f;
	public float MatchHitTime = 0.8f;
	public float ChangeGemEffectTime = 2.0f;
	public float ChangeGemWaitTime = 0.2f;
	public float ShakeTime = 0.2f;
	public float EnemyMoveTime = 0.3f;
	public float SwapGemsTime = 0.1f;
	public float SwapGemsFailTime = 0.1f;
	public float CollapseFieldTime = 0.2f;
	public float MatchFadeTime = 0.3f;
	public float MultiplierTime = 0.5f;
	public float CrossTime = 0.5f;
	public float CrossEnemyTime = 0.8f;
	public float TransformGemWaitTime = 0.1f;
	public float CollectStatsTime = 0.3f;

	public int FieldCenterX = 0;
	public int FieldCenterY = 0;
	public int FieldSizeX = 8;
	public int FieldSizeY = 8;
	public int FieldGridX = 62;
	public int FieldGridY = 62;
}

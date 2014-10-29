using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace HUD
{
	public class Waves : MonoBehaviour
	{
		int _moveCount;

		SpriteSlider _waveBar;

		struct Wave
		{
			public GameObject _main;
			public Transform _portrait;
			public GameObject _enemy;
		}
		Wave[] _waves;

		private LevelData _levelData;

		void Awake () 
		{
			_levelData = GameManager.Instance.LevelData;

			_moveCount = 0;

			int count = _levelData.GetWaveCount();
			
			_waveBar = GameManager.Instance.Definitions.HudWaveBar.GetComponentInChildren<SpriteSlider>();
			SpriteRenderer r = _waveBar.renderer as SpriteRenderer;
			Bounds waveBarBounds = r.bounds;
			_waveBar.sliderValue = 0;

			int actualMoves = 0;
			GameObject waveTemplate = GameManager.Instance.Definitions.HudWave;
			Vector3 pos = waveTemplate.transform.position;
			Vector3 scale = waveTemplate.transform.localScale;
			waveTemplate.SetActive(false);
			_waves = new Wave[count];
			for(int i = 0; i < count; i++)
			{
				GameObject go = (GameObject)Instantiate(waveTemplate);
				_waves[i]._main = go;
				
				go.SetActive(true);
				go.name = "Wave" + i;
				go.transform.parent = waveTemplate.transform.parent;

				LevelData.WaveInfo wi = _levelData.GetWaveInfo(i);
				actualMoves += wi._moves;
				go.transform.position = new Vector3(-waveBarBounds.extents.x + waveBarBounds.extents.x * 2 * actualMoves / _levelData.GetTotalMoves(), pos.y, pos.z);
				go.transform.localScale = scale;

				ShadowLabel label = go.GetComponentInChildren<ShadowLabel>();
				/*string str = "";
				for(int j = 0; j < wi._count; j++)
					str += "!";
				label.text = str;*/
				label.text = "x" + wi._count.ToString();

				_waves[i]._portrait = go.transform.FindChild("Portrait");
				GameObject goEnemy = GameObject.Instantiate(_levelData.GetEnemyInfo(wi._enemyType)._prefab) as GameObject;
				goEnemy.transform.parent = _waves[i]._portrait;
				goEnemy.transform.localPosition = Vector3.zero;
				goEnemy.transform.localRotation = Quaternion.identity;
				SortingLayer.ForceLayerID(goEnemy, "HUDBack");

				_waves[i]._enemy = goEnemy;
			}
		}

		public void AddMove()
		{
			_moveCount++;
			SetMoveSlider((float)_moveCount / _levelData.GetTotalMoves());
		}
		public int GetMoveCount()
		{
			return _moveCount;
		}

		public void DisableEnemy(int index)
		{
			int waveIndex = GetEnemyWaveIndex(index);
			Effects.TweenColor(_waves[waveIndex]._enemy, 0, GameManager.Instance.Definitions.DisabledEnemyColor, Tweener.Method.Linear);
			Effects.ChangeMaterial(_waves[waveIndex]._enemy, GameManager.Instance.Definitions.DisabledEnemyMaterial);
		}

		public Vector3 GetEnemyPosition(int index)
		{
			int waveIndex = GetEnemyWaveIndex(index);
			return _waves[waveIndex]._portrait.position;
		}

		void SetMoveSlider(float factor)
		{
			_waveBar.sliderValue = factor;
		}

		int GetEnemyWaveIndex(int index)
		{
			int count = _levelData.GetWaveCount();
			for(int i = 0; i < count; i++)
			{
				int wcount = _levelData.GetWaveInfo(i)._count;
				if (index < wcount)
					return i;

				index -= wcount;
			}
			DebugUtils.Assert(false);
			return -1;
		}
	}
}

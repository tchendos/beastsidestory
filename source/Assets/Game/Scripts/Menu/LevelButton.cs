using UnityEngine;
using System.Collections;

public class LevelButton : MonoBehaviour
{
	[SerializeField]
	private GameObject _activeButton;

	[SerializeField]
	private GameObject _finishedButton;

	[SerializeField]
	private GameObject _lockedButton;


	public void ActivateButton()
	{
		_activeButton.SetActive(true);
		_finishedButton.SetActive(false);
		_lockedButton.SetActive(false);
	}

	public void LockButton()
	{
		_activeButton.SetActive(false);
		_finishedButton.SetActive(false);
		_lockedButton.SetActive(true);
	}

	public void FinishButton()
	{
		_activeButton.SetActive(false);
		_finishedButton.SetActive(true);
		_lockedButton.SetActive(false);
	}
}

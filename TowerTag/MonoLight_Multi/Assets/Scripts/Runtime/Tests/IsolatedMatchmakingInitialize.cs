using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManagement;

public class IsolatedMatchmakingInitialize : MonoBehaviour
{
    private void Awake()
	{
		PlayerPrefs.SetInt(TowerTag.PlayerPrefKeys.Tutorial, 1);

		ConnectionManager.Instance._onConnectedToMasterDelegate += () => GameManager.Instance.StartMatchmaking();
	}
}

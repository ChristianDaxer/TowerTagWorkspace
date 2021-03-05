using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsolatedLeaderboardInitialize : MonoBehaviour
{
    private void Awake()
    {
        ConnectionManager.Instance._onConnectedToMasterDelegate += () => GameManager.Instance.StartOnlyLeaderboards();
    }
}

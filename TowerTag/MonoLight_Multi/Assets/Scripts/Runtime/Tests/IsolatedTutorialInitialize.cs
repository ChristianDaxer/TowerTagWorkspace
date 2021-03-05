using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManagement;

public class IsolatedTutorialInitialize : MonoBehaviour
{
    private void Awake()
    {
        ConnectionManager.Instance._onConnectedToMasterDelegate += () => GameManager.Instance.StartTutorial(true);
        
    }
}

using System.Collections;
using GameManagement;
using TowerTagSOES;
using UnityEngine;

public class PrefabFactoryLobby : MonoBehaviour {
    [SerializeField] private GameObject _vrPlayer;
    [SerializeField] private GameObject _cameraRig;
    [SerializeField] private GameObject _fpsPlayer;

    private void Awake()
    {
        StartCoroutine(CreateClient());
    }

    private IEnumerator CreateClient()
    {
        if(!GameInitialization.Initialized)
            yield return new WaitUntil(() => GameInitialization.Initialized);
        if (SharedControllerType.VR) {
            InstantiateWrapper.InstantiateWithMessage(TowerTagSettings.Home ? _vrPlayer : _cameraRig, transform);
            yield break;
        }

        if (SharedControllerType.NormalFPS) {
            if (TowerTagSettings.Home)
                InstantiateWrapper.InstantiateWithMessage(_fpsPlayer, transform);
        }
        ConfigurationManager.Configuration.TeamID = -1;
    }
}
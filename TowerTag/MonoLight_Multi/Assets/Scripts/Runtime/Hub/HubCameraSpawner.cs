using TowerTagSOES;
using UnityEngine;

public class HubCameraSpawner : MonoBehaviour {
    [SerializeField] private GameObject _timelineCamera;
    [SerializeField] private GameObject _staticCamera;

    private void Awake() {
        if(SharedControllerType.IsAdmin || SharedControllerType.Spectator)
           InstantiateWrapper.InstantiateWithMessage(TowerTagSettings.Hologate ? _timelineCamera : _staticCamera);
    }
}

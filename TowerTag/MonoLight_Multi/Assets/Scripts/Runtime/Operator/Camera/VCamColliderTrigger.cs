using Cinemachine;
using OperatorCamera;
using TowerTagSOES;
using UnityEngine;
using static OperatorCamera.CameraManager.CameraMode;
using Random = UnityEngine.Random;

/// <summary>
/// Script for example the tunnel to trigger the hot spot cam
/// </summary>
public class VCamColliderTrigger : MonoBehaviour {

    [SerializeField]
    private CinemachineVirtualCamera[] _cameras;
    private CinemachineVirtualCamera _activeCamera;
    private bool _cameraActive;

    private CameraManager _camManager;
    //The count of player in the collider
    private int _playerInCollider;

    private void OnEnable() {
        GameManager.Instance.MatchHasFinishedLoading += OnMatchFinishedLoading;
    }

    private void OnDisable() {
        GameManager.Instance.MatchHasFinishedLoading -= OnMatchFinishedLoading;
    }

    private void OnMatchFinishedLoading(IMatch match) {
        match.RoundStartingAt += OnRoundStartingAt;
    }

    private void OnRoundStartingAt(IMatch match, int time) {
        if(_cameraActive)
            DeactivateCamera();
    }

    private void Start()
    {
        //no usage if not operator
        if (!SharedControllerType.IsAdmin) {
            Destroy(gameObject);
            return;
        }

        _camManager = AdminController.Instance.CamManager;
        _cameras = GetComponentsInChildren<CinemachineVirtualCamera>();
        foreach (CinemachineVirtualCamera cam in _cameras) { cam.gameObject.SetActive(false); }
    }

    private void OnTriggerEnter(Collider other)
    {
        _playerInCollider++;
        //When 2 or more players are active in the direct mode => activate tunnel camera
        if (_playerInCollider >= 2 && !_cameraActive
                                   && _camManager.CurrentCameraMode == Automatic) {
            ActivateCamera();
        }
    }

    private void ActivateCamera() {
        int cameraIndex = Random.Range(0, _cameras.Length);
        _activeCamera = _cameras[cameraIndex];
        _activeCamera.gameObject.SetActive(true);
        _cameraActive = true;
        _camManager.TimeSinceLastCut = 0;
    }

    private void OnTriggerExit(Collider other)
    {
        _playerInCollider--;
        //When 1 or less players are active in the direct mode => deactivate tunnel camera
        if (_playerInCollider <= 1 && _cameraActive) {
            DeactivateCamera();
        }
    }

    private void DeactivateCamera() {
        _activeCamera.gameObject.SetActive(false);
        _activeCamera = null;
        _cameraActive = false;
    }
}

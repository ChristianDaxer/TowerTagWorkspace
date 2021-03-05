using TowerTagSOES;
using UnityEngine;

public class MenuVRComponent : MonoBehaviour {
    [SerializeField] private GameObject _container;
    [SerializeField] private GameObject _offboardingInstructions;

    private void Awake() {
        if(TowerTagSettings.Home) {
            Destroy(this);
        }
    }

    private void OnEnable() {
        ConnectionManager.Instance.ErrorOccured += OnErrorOccurred;
    }

    private void OnDisable() {
        if (ConnectionManager.Instance != null)
            ConnectionManager.Instance.ErrorOccured -= OnErrorOccurred;
    }

    private void Start() {
        _container.SetActive(SharedControllerType.VR || SharedControllerType.NormalFPS);
        _offboardingInstructions.SetActive(TTSceneManager.Instance.ShowOffboardingInstructions);
    }

    private void OnErrorOccurred(ConnectionManager connectionManager, MessagesAndErrors.ErrorCode errorCode) {
        _offboardingInstructions.SetActive(true);
    }
}
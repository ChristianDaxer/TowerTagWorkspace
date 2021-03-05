using TowerTagSOES;
using UnityEngine;


public class IngameUIController : MonoBehaviour {
    [SerializeField] private GameObject _bugReportParent;
    [SerializeField] private GameObject _backToMainParent;
    [SerializeField] private GameObject _soundMenuParent;
    [SerializeField] private GameObject _operatorCanvas;

    private bool IngameOverlayCanvasActive => _backToMainParent.activeSelf;

    private void Start() {
        if (TowerTagSettings.Hologate)
            return;

        TTSceneManager.Instance.PillarOffsetSceneLoaded += OnPillarOffsetSceneLoaded;
        TTSceneManager.Instance.HubSceneLoaded += OnHubSceneLoaded;
        TTSceneManager.Instance.ConnectSceneLoaded += OnConnectSceneLoaded;
        TTSceneManager.Instance.OffboardingSceneLoaded += OnOffboardingSceneLoaded;
        TTSceneManager.Instance.CommendationSceneLoaded += OnCommendationSceneLoaded;
        GameManager.Instance.MatchHasFinishedLoading += OnMatchHasFinishedLoading;
        GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;

        
        PlayerInput _rightXRController = (PlayerInput)PlayerInputBase.rightHand;
        PlayerInput _leftXRController = (PlayerInput)PlayerInputBase.leftHand;

        if (_rightXRController != null)
            _rightXRController.OnMenuDown += MenuButtonClicked;

        if (_leftXRController != null)
            _leftXRController.OnMenuDown += MenuButtonClicked;

    }

    private void MenuButtonClicked(PlayerInputBase fromAction)
    {
        ToggleIngameOverlayCanvas(!_soundMenuParent.activeSelf);
    }

    private void ToggleIngameOverlayCanvas(bool state) {
        _soundMenuParent.CheckForNull()?.SetActive(state);
        _backToMainParent.CheckForNull()?.SetActive(state);
    }

    private void OnDestroy() {
        if (TowerTagSettings.Hologate)
            return;

        if (TTSceneManager.Instance != null) {
            TTSceneManager.Instance.PillarOffsetSceneLoaded -= OnPillarOffsetSceneLoaded;
            TTSceneManager.Instance.HubSceneLoaded -= OnHubSceneLoaded;
            TTSceneManager.Instance.ConnectSceneLoaded -= OnConnectSceneLoaded;
            TTSceneManager.Instance.OffboardingSceneLoaded -= OnOffboardingSceneLoaded;
            TTSceneManager.Instance.CommendationSceneLoaded -= OnCommendationSceneLoaded;
        }

        GameManager.Instance.MatchHasFinishedLoading -= OnMatchHasFinishedLoading;
        GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;
        
        PlayerInput _rightXRController = (PlayerInput)PlayerInputBase.rightHand;
        PlayerInput _leftXRController = (PlayerInput)PlayerInputBase.leftHand;

        if (_rightXRController != null)
            _rightXRController.OnMenuDown -= MenuButtonClicked;

        if (_leftXRController != null)
            _leftXRController.OnMenuDown -= MenuButtonClicked;
    }

    private void Update() {
        AbortMatchVotingController.Tick();
        if(TowerTagSettings.Home) return;
        if (Input.GetKeyDown(KeyCode.Escape) && !SharedControllerType.IsAdmin) {
            ToggleIngameOverlayCanvas(!IngameOverlayCanvasActive);
        }
    }

    private void LateUpdate() {
        if(TowerTagSettings.Home) return;
        if (!SharedControllerType.IsAdmin || !Input.GetKeyDown(KeyCode.Escape)) return;
        if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Configure) {
            // We are in the Hub Scene
            AdminController.Instance.OnCloseButton();
        }
    }

    private void OnOffboardingSceneLoaded() {
        _bugReportParent.CheckForNull()?.SetActive(true);
        if (!TowerTagSettings.Home) {
            _soundMenuParent.CheckForNull()?.SetActive(false);
            _backToMainParent.CheckForNull()?.SetActive(false);
            _operatorCanvas.CheckForNull()?.SetActive(true);
        }
        else {
            _soundMenuParent.CheckForNull()?.SetActive(false);
            _backToMainParent.CheckForNull()?.SetActive(false);
            _operatorCanvas.CheckForNull()?.SetActive(true);
        }
    }

    private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
        _operatorCanvas.CheckForNull()?.SetActive(false);
    }

    private void OnPillarOffsetSceneLoaded() {
        _bugReportParent.CheckForNull()?.SetActive(true);
        _soundMenuParent.CheckForNull()?.SetActive(false);
        _backToMainParent.CheckForNull()?.SetActive(true);
        _operatorCanvas.CheckForNull()?.SetActive(false);
    }

    private void OnConnectSceneLoaded() {
        _bugReportParent.CheckForNull()?.SetActive(false);
        _soundMenuParent.CheckForNull()?.SetActive(false);
        _backToMainParent.CheckForNull()?.SetActive(false);
        _operatorCanvas.CheckForNull()?.SetActive(false);
    }

    private void OnCommendationSceneLoaded() {
        _bugReportParent.CheckForNull()?.SetActive(true);
        if (!SharedControllerType.IsAdmin && !TowerTagSettings.Home) {
            _soundMenuParent.CheckForNull()?.SetActive(true);
            _backToMainParent.CheckForNull()?.SetActive(true);
            _operatorCanvas.CheckForNull()?.SetActive(false);
        }
        else {
            _soundMenuParent.CheckForNull()?.SetActive(false);
            _backToMainParent.CheckForNull()?.SetActive(false);
            _operatorCanvas.CheckForNull()?.SetActive(true);
        }
    }

    private void OnHubSceneLoaded() {
        _bugReportParent.CheckForNull()?.SetActive(true);
        if (!SharedControllerType.IsAdmin && !TowerTagSettings.Home && !TowerTagSettings.BasicMode) {
            _soundMenuParent.CheckForNull()?.SetActive(true);
            _backToMainParent.CheckForNull()?.SetActive(true);
            _operatorCanvas.CheckForNull()?.SetActive(false);
        }
        else {
            _soundMenuParent.CheckForNull()?.SetActive(false);
            _backToMainParent.CheckForNull()?.SetActive(false);
            _operatorCanvas.CheckForNull()?.SetActive(true);
        }
    }

    private void OnMatchHasFinishedLoading(IMatch match) {
        _bugReportParent.CheckForNull()?.SetActive(true);
        if (!TowerTagSettings.Home) {
            _soundMenuParent.CheckForNull()?.SetActive(true);
            _backToMainParent.CheckForNull()?.SetActive(false);
            _operatorCanvas.CheckForNull()?.SetActive(false);
        }
        else {
            _soundMenuParent.CheckForNull()?.SetActive(false);
            _backToMainParent.CheckForNull()?.SetActive(false);
            _operatorCanvas.CheckForNull()?.SetActive(false);
        }
    }
}
using System.Linq;
using Photon.Pun;
using TowerTagSOES;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

public class BackToMainMenuUIController : MonoBehaviour {
    [FormerlySerializedAs("enabledControllerTypes")] [SerializeField, Tooltip("The Controller Types which are enabled to get back to the main menu")]
    private ControllerType[] _enabledControllerTypes;

    [FormerlySerializedAs("mainMenuButton")] [SerializeField, Tooltip("A press of this button loads the main menu")]
    private KeyCode _mainMenuButton = KeyCode.Escape;


    [FormerlySerializedAs("mainMenuUiOverlay")]
    [Space, Header("UI Elements")]
    [SerializeField, Tooltip("Drag the canvas object which holds the UI elements to go back to the main menu here")]
    private Canvas _mainMenuUiOverlay;

    [FormerlySerializedAs("LoadMainMenuUiOverlayCanvasPrefab")]
    [SerializeField,
     Tooltip(
         "Drag the canvas prefab which holds the UI elements to go back to the main menu here. It is used if the object field itself is not set")]
    private Canvas _loadMainMenuUiOverlayCanvasPrefab;

    [SerializeField, Tooltip("Message queue for overlay pop up messages")]
    private MessageQueue _overlayMessageQueue;

    private bool _queryPending;

    private void Start() {
        if (!_mainMenuUiOverlay) {
            _mainMenuUiOverlay = InstantiateWrapper.InstantiateWithMessage(_loadMainMenuUiOverlayCanvasPrefab.gameObject).GetComponent<Canvas>();
        }
    }

    private void Update() {
        if (Input.GetKeyDown(_mainMenuButton)) {
            LoadMainMenuIfValid();
        }
    }

    public void OnCloseButton() {
        if (!_queryPending && !SharedControllerType.PillarOffsetController) {
            _overlayMessageQueue.AddYesNoMessage(
                "This will disconnect you and abort any running match.",
                "Are You Sure?",
                () => { _queryPending = true; },
                () => { _queryPending = false; },
                "OK",
                LoadMainMenu,
                "CANCEL");
        }

        if (SharedControllerType.PillarOffsetController) {
            _overlayMessageQueue.AddYesNoMessage(
                "Do you want to save this pillar position?",
                "Save?",
                () => { _queryPending = true; },
                () => { _queryPending = false; },
                "YES",
                () => {
                    SharedControllerType.Singleton.Set(this, ControllerType.VR);
                    FindObjectOfType<MeasurePillarOffset>().SaveToDisk();
                    LoadMainMenu();
                },
                "NO",
                () => {
                    SharedControllerType.Singleton.Set(this, ControllerType.VR);
                    LoadMainMenu();
                });
        }

        _queryPending = true;
    }

    /// <summary>
    /// Check if all requirements are met to go back to the main menu before calling LoadMainMenu()
    /// </summary>
    public void LoadMainMenuIfValid() {
        // check if we are currently connected to see if it makes sense to disconnect and load the main menu again
        ControllerType controllerType = SharedControllerType.Singleton.Value;
        if (PhotonNetwork.IsConnected) {
            if (_enabledControllerTypes.Contains(controllerType)) {
                OnCloseButton();
            }
            else {
                Debug.Log(name + ":" + GetType().Name + " - " +
                          "User tried to disconnect and go back to the main menu, but has not an enabled ControllerType (ControllerType: " +
                          controllerType + ")");
            }
        }
        else {
            Debug.Log(name + ":" + GetType().Name + " - " +
                      "User tried to disconnect and go back to the main menu, but is currently not connected");
        }
    }

    /// <summary>
    /// Disconnect the player and load the main menu again.
    /// </summary>
    private void LoadMainMenu() {
        _queryPending = false;
        Debug.Log(name + ":" + GetType().Name + " - " + "");
        ConnectionManager.Instance.Disconnect();
        TTSceneManager.Instance.LoadConnectScene(true);
    }
}
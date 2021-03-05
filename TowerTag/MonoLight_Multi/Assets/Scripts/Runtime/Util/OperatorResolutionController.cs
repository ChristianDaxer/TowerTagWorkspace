using TowerTagSOES;
using UnityEngine;

public class OperatorResolutionController : MonoBehaviour {
    [SerializeField] private SharedControllerType _controllerType;

    private void OnEnable() {
        if (_controllerType != null) {
            _controllerType.ValueSet += OnControllerTypeSet;
            OnControllerTypeSet(this, _controllerType);
        }
    }

    private void OnDisable() {
        if (_controllerType != null) {
            _controllerType.ValueSet -= OnControllerTypeSet;
        }
    }

    private static void OnControllerTypeSet(object sender, ControllerType controllerType) {
        if (SharedControllerType.IsAdmin) {
            SetOperatorResolution();
        }
    }

    private static void SetOperatorResolution() {
        // todo fix bug with 16:10 monitors
        // Set full-screen resolution to avoid issue with black screen in menu scene
        if (Screen.fullScreen) {
            Resolution maximumResolution = Screen.resolutions[Screen.resolutions.Length - 1];
            Debug.Log("Setting operator resolution to " + maximumResolution);
            Screen.SetResolution(maximumResolution.width, maximumResolution.height, Screen.fullScreen);
        }
    }
}
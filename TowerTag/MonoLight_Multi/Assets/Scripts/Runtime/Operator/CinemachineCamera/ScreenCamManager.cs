using Commendations;
using UnityEngine;

public class ScreenCamManager : MonoBehaviour {

    [SerializeField] private Camera _screenShotCamera;
    // Start is called before the first frame update
    void Start() {
        var operatorUiController = FindObjectOfType<CommendationsUIController>();
        if (operatorUiController != null && _screenShotCamera != null) {
            operatorUiController.ScreenshotCamera = _screenShotCamera;
        }
    }
}

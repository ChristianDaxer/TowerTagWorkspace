using AmplifyBloom;
using UnityEngine;

[RequireComponent(typeof(ScreenshotCompanion))]
public class ScreenshotCompanionController : MonoBehaviour {
    private int _resolutionMultiplier;
    private ScreenshotCompanion _screenShotCompanion;
    private Configuration _config;
    private Camera _mainCamera;

    private void OnEnable() {
        _mainCamera = null;
        _screenShotCompanion = GetComponent<ScreenshotCompanion>();
        if (_screenShotCompanion != null)
            RegisterListener();
    }

    private void OnDisable() {
        if (_screenShotCompanion != null)
            DeRegisterListener();
    }

    private void RegisterListener() {
        _screenShotCompanion.ScreenShotCompanionPrepareScreenShot += OnPreparingScreenShot;
        _screenShotCompanion.ScreenShotCompanionTakeUpScreenShot += OnTakeUpScreenShot;
        TTSceneManager.Instance.HubSceneLoaded += OnHubSceneLoaded;
    }

    private void OnHubSceneLoaded() {
        SetupScreenShotCamera();
    }

    private void DeRegisterListener() {
        _screenShotCompanion.ScreenShotCompanionPrepareScreenShot -= OnPreparingScreenShot;
        _screenShotCompanion.ScreenShotCompanionTakeUpScreenShot -= OnTakeUpScreenShot;
        if (TTSceneManager.Instance != null)
            TTSceneManager.Instance.HubSceneLoaded -= OnHubSceneLoaded;
    }

    private void OnPreparingScreenShot(ScreenshotCompanion sender, Camera camera1) {
        if (camera1 == null)
            return;
        _screenShotCompanion.settings.renderSizeMultiplier = _config.ScreenShotResolution;
        if (_screenShotCompanion.settings.renderSizeMultiplier >= 4)
            camera1.gameObject.GetComponent<AmplifyBloomEffect>().MainThresholdSize = MainThresholdSizeEnum.Quarter;
        if (_screenShotCompanion.settings.renderSizeMultiplier >= 2 &&
            _screenShotCompanion.settings.renderSizeMultiplier < 4)
            camera1.gameObject.GetComponent<AmplifyBloomEffect>().MainThresholdSize = MainThresholdSizeEnum.Half;
        if (_screenShotCompanion.settings.renderSizeMultiplier <= 1)
            camera1.gameObject.GetComponent<AmplifyBloomEffect>().MainThresholdSize = MainThresholdSizeEnum.Full;
    }

    private static void OnTakeUpScreenShot(ScreenshotCompanion sender, Camera camera1) {
        if (camera1 == null)
            return;
        camera1.gameObject.GetComponent<AmplifyBloomEffect>().MainThresholdSize = MainThresholdSizeEnum.Full;
    }

    private void Start() {
        _config = ConfigurationManager.Configuration;
    }


    private void SetupScreenShotCamera() {
        var camera1 = FindObjectOfType<Camera>();
        if (camera1 == null)
            return;
        if (_mainCamera == null) {
            _screenShotCompanion.AddCamera(camera1.gameObject);
            _screenShotCompanion.list[0].hotkey = KeyCode.F9;
        }

        if (_mainCamera == camera1)
            return;
        _screenShotCompanion.list[0].cam = camera1.gameObject;
    }
}
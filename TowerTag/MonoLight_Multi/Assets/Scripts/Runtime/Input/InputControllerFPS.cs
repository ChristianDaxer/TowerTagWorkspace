using System;
using TMPro;
using UnityEngine;

public class InputControllerFPS : MonoBehaviour, IInputController {
    // events
    public event Action GripPressed;
    public event Action GripReleased;
    public event Action<GunController.GunControllerState.TriggerAction> TriggerPressed;
    public event Action TriggerReleased;
    public event Action TeleportTriggered;

    [SerializeField] private float _velocityToTeleport = 1f;
    [SerializeField] private float _calculateVelocityTimeout = 0.2f;
    [SerializeField] private GameObject _shotsPerSecondObject;
    [SerializeField] private TMP_Text _spsText;


    private float _lastVelocityCalculation;
    private Vector3 _lastPosition;
    private Vector3 _lastMovementVector;
    private bool _mouseButtonTriggered;
    private bool _spsAllowed;
    private float _timer;
    private int _firedShots;

    private void Start() {
        _lastPosition = transform.position;

        _spsAllowed = Debug.isDebugBuild;

        _shotsPerSecondObject.SetActive(_spsAllowed && ConfigurationManager.Configuration.ShowShotsPerSecond);

        ConfigurationManager.ConfigurationUpdated += OnConfigurationUpdated;
    }

    private void OnDestroy() {
        ConfigurationManager.ConfigurationUpdated -= OnConfigurationUpdated;
    }

    private void OnConfigurationUpdated() {
        _shotsPerSecondObject.SetActive(_spsAllowed && ConfigurationManager.Configuration.ShowShotsPerSecond);
    }

    private void Update() {
        if (Time.time - _lastVelocityCalculation > _calculateVelocityTimeout) {
            CalculateVelocity();
        }

        if (Input.GetMouseButtonDown(1))
            _mouseButtonTriggered = true;

        if (Input.GetMouseButtonDown(0))
            TriggerClicked();

        if (Input.GetMouseButtonDown(1))
            GripClicked();

        if (Input.GetMouseButtonUp(1))
            GripUnClicked();

        if (Input.GetMouseButtonUp(0))
            TriggerUnClicked();

        // Debug Log for development Reason
        if (_spsAllowed
            && _shotsPerSecondObject.activeInHierarchy
            && ConnectionManager.Instance.ConnectionManagerState == ConnectionManager.ConnectionState.ConnectedToGame) {
            _timer += Time.deltaTime;
            if (_timer < 1) {
                if (Input.GetMouseButtonDown(0)) _firedShots++;
            }
            else {
                var count = _firedShots.ToString().Length;
                _spsText.text = count > 1 ? _firedShots.ToString() : "0" + _firedShots.ToString();
                _timer = 0f;
                _firedShots = 0;
            }
        }
    }

    private void GripClicked() {
        GripPressed?.Invoke();
    }

    private void GripUnClicked() {
        GripReleased?.Invoke();
    }

    private void TriggerClicked() {
        TriggerPressed?.Invoke(GunController.GunControllerState.TriggerAction.DetectByRaycast);
    }

    private void TriggerUnClicked() {
        TriggerReleased?.Invoke();
    }

    private void CalculateVelocity() {
        _lastVelocityCalculation = Time.time;
        _lastMovementVector = Input.mousePosition - _lastPosition;
        _lastPosition = Input.mousePosition;

        if (_lastMovementVector.magnitude / _calculateVelocityTimeout >= _velocityToTeleport || _mouseButtonTriggered) {
            TeleportTriggered?.Invoke();
        }

        _mouseButtonTriggered = false;
    }
}
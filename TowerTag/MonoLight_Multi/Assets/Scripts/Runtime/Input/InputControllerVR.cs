using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;
//using Valve.VR;

public class InputControllerVR : MonoBehaviour, IInputController
{
    private static InputControllerVR _instance;

    public static InputControllerVR Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            try
            {
                var inputControllerVr = FindObjectOfType<InputControllerVR>();
                if (inputControllerVr == null)
                {
                    if (PlayerManager.Instance.GetOwnPlayer() == null || !SharedControllerType.VR)
                    {
                        Debug.LogError("cant find local vr player");
                        return null;
                    }
                }

                _instance = inputControllerVr;
                return _instance;
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }

    // events
    public event Action<GunController.GunControllerState.TriggerAction> TriggerPressed;
    public event Action GripPressed;
    public event Action TriggerReleased;
    public event Action GripReleased;
    public event Action TeleportTriggered;

    [SerializeField] private ApplyTransform _gunApplyTransform;
    [SerializeField] private float _velocityToTeleport = 1f;
    [SerializeField] private float _calculateVelocityTimeout = 0.2f;
    [SerializeField, Range(0, 1)] private float _oculusCv1TresholdTriggerPressed = 0.95f;
    [SerializeField, Range(0, 1)] private float _oculusCv1TresholdTriggerReleased = 0.95f;
    [SerializeField, Range(0, 1)] private float _oculusTresholdTriggerPressed = 0.7f;
    [SerializeField, Range(0, 1)] private float _oculusTresholdTriggerReleased = 0.7f;
    [SerializeField] private GameObject _shotsPerSecondObject;
    [SerializeField] private TMP_Text _spsText;

    // Value between 0 and 1 (0 means direction doesnt matter, 1 means you have to be exact (1 is impossible))
    [SerializeField] private float _directionAccuracyToTeleport = 0.5f;

    // private static SteamVR_Behaviour_Pose _preferredController;

    private static PlayerInputBase _preferredXRController;

    private float _timer;
    private int _firedShots;
    private bool _spsAllowed;

    private float _lastVelocityCalculation;
    private Vector3 _lastPosition;
    private Vector3 _lastMovementVector;
    private bool _oculusTouchTriggerPressed;

    public void SetControllerAsPreferred(bool toRightHand) => PlayerPrefs.SetString(PlayerPrefKeys.PreferredHand, toRightHand ? PlayerHand.Right.ToString().ToUpper() : PlayerHand.Left.ToString().ToUpper());

    public PlayerHand TargetHand =>
        PlayerPrefs.HasKey(PlayerPrefKeys.PreferredHand) ?
            (PlayerPrefs.GetString(PlayerPrefKeys.PreferredHand) == "RIGHT" ?
                PlayerHand.Right : 
                PlayerHand.Left) :
            PlayerHand.Right;

    private PlayerInput _cachedActiveController;
    public PlayerInputBase ActiveController {
        get {
            if (PlayerInputBase.GetInstance(TargetHand, out var input)) {
                PlayerInput inputBase = (PlayerInput)input;
                if (_cachedActiveController != inputBase) {

                    if (_cachedActiveController != null) {
                        _cachedActiveController.OnToggleUp -= OnGripUp;
                        _cachedActiveController.OnToggleDown -= OnGripDown;
                        _cachedActiveController.OnTriggerDown -= OnTriggerButtonAction;
                        _cachedActiveController.OnTriggerUp -= OnTriggerButtonAction;
                        _cachedActiveController.OnTriggerStateValue -= OnTriggerThresholdAction;
                    }

                    inputBase.OnToggleUp += OnGripUp;
                    inputBase.OnToggleDown += OnGripDown;
                    inputBase.OnTriggerDown += OnTriggerButtonAction;
                    inputBase.OnTriggerUp += OnTriggerButtonAction;
                    inputBase.OnTriggerStateValue += OnTriggerThresholdAction;

                    if (_gunApplyTransform != null)
                        _gunApplyTransform.Source = inputBase.transform;

                    _cachedActiveController = inputBase;

                }

                return inputBase;
            }

            return null;
        }
    }

    // public Quaternion ActiveControllerRotation => 

    private void Awake()
    {
        _instance = this;

        _spsAllowed = Debug.isDebugBuild;
        _shotsPerSecondObject.SetActive(_spsAllowed && ConfigurationManager.Configuration.ShowShotsPerSecond);

    }

    private void OnEnable()
    {
        if (_instance == null)
            _instance = this;

        SceneManager.sceneLoaded += NewSceneLoaded;
    }

    private void Start() => ConfigurationManager.ConfigurationUpdated += OnConfigurationUpdated;

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= NewSceneLoaded;
        ConfigurationManager.ConfigurationUpdated -= OnConfigurationUpdated;
    }

    private void OnConfigurationUpdated() => _shotsPerSecondObject.SetActive(_spsAllowed && ConfigurationManager.Configuration.ShowShotsPerSecond);

    private void OnDestroy()
    {
        if (_instance != this)
            return;
        _instance = null;
    }

    private void OnTriggerButtonAction(PlayerInputBase fromSource, bool newState)
    {
        if (newState) {
            TriggerPressed?.Invoke(GunController.GunControllerState.TriggerAction.DetectByRaycast);
            _firedShots++;
            return;
        }

        TriggerReleased?.Invoke();
    }

    private void OnTriggerThresholdAction(PlayerInputBase fromSource, float newAxis) {
        if (_oculusTouchTriggerPressed) {
            if (ControllerTypeDetector.CurrentConnectedHmdType == ControllerTypeDetector.ConnectedHmdType.Cv1
                ? newAxis <= _oculusCv1TresholdTriggerReleased
                : newAxis <= _oculusTresholdTriggerReleased) {
                TriggerReleased?.Invoke();
                _oculusTouchTriggerPressed = false;
            }
        }
        else {
            if (ControllerTypeDetector.CurrentConnectedHmdType == ControllerTypeDetector.ConnectedHmdType.Cv1
                ? newAxis >= _oculusCv1TresholdTriggerPressed
                : newAxis >= _oculusTresholdTriggerPressed) {
                TriggerPressed?.Invoke(GunController.GunControllerState.TriggerAction.DetectByRaycast);
                _oculusTouchTriggerPressed = true;
                _firedShots++;
            }
        }
    }

    private void OnGripDown(PlayerInputBase fromSource, bool newState) => GripPressed?.Invoke();
    private void OnGripUp(PlayerInputBase fromSource, bool newState) => GripReleased?.Invoke();

    private void NewSceneLoaded(Scene scene, LoadSceneMode mode) {

        PlayerInputBase input = ActiveController;

        if (input == null)
            return;

        input.gameObject.SetActive(false);
        input.gameObject.SetActive(true);
    }

    private void Update() {
        if (Time.time - _lastVelocityCalculation > _calculateVelocityTimeout)
            CalculateVelocity();

        if (_spsAllowed
            && _shotsPerSecondObject.activeInHierarchy
            && ConnectionManager.Instance.ConnectionManagerState == ConnectionManager.ConnectionState.ConnectedToGame) {
            _timer += Time.deltaTime;
            if (_timer >= 1) {
                var count = _firedShots.ToString().Length;
                _spsText.text = count > 1 ? _firedShots.ToString() : "0" + _firedShots.ToString();
                _timer = 0f;
                _firedShots = 0;
            }
        }
    }

    private void CalculateVelocity() {
        _lastVelocityCalculation = Time.time;

        Transform controllerTransform = ActiveController.transform;
        Vector3 controllerTransformPosition = controllerTransform.position;

        _lastMovementVector = controllerTransformPosition - _lastPosition;
        _lastPosition = controllerTransformPosition;

        float velocity = _lastMovementVector.magnitude / _calculateVelocityTimeout;
        float prjFactor = Vector3.Dot(controllerTransform.up, _lastMovementVector.normalized);

        if (velocity >= _velocityToTeleport && prjFactor >= _directionAccuracyToTeleport)
            TeleportTriggered?.Invoke();
    }
}
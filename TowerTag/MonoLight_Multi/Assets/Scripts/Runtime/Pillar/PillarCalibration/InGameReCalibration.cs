using System;
using Runtime.Pillar.PillarCalibration;
using UnityEngine;


public class InGameReCalibration : MonoBehaviour {

    [Header("SteamVR Input Actions")]
    [Tooltip("Minimum value of an axis at which the input is considered as valid input.")]
    [SerializeField]
    private float _trackPadMinAxisValueThreshold = .5f;

    [Header("Movement")] [SerializeField] private float _offsetStepWidthPosition = 0.005f;
    [SerializeField] private float _offsetStepWidthRotationAngle = 0.1f;

    [Header("Apply Offset To")] [SerializeField]
    private ApplyPillarOffset _applyOffset;

    private bool _gripState, _menuState = false;
    private Vector2 _thumbState = Vector2.zero;
    private void OnEnable() {

        PlayerInput _rightXRController = (PlayerInput)PlayerInputBase.rightHand;
        PlayerInput _leftXRController = (PlayerInput)PlayerInputBase.leftHand;

        if (_rightXRController != null) { 
            _rightXRController.OnMenuDown += OnMenuDown;
            _rightXRController.OnMenuUp += OnMenuUp;
            _rightXRController.OnMove += OnMove;
            _rightXRController.OnToggleDown += OnGripStateChanged;
            _rightXRController.OnToggleUp += OnGripStateChanged;
            _rightXRController.calibrationMode = true;
        }

        if (_leftXRController != null) { 
            _leftXRController.OnMenuDown += OnMenuDown;
            _leftXRController.OnMenuUp += OnMenuUp;
            _leftXRController.OnMove += OnMove;
            _leftXRController.OnToggleDown += OnGripStateChanged;
            _leftXRController.OnToggleUp += OnGripStateChanged;
            _leftXRController.calibrationMode = true;
        }
    }

    private void OnDisable()
    {
        PlayerInput _rightXRController = (PlayerInput)PlayerInputBase.rightHand;
        PlayerInput _leftXRController = (PlayerInput)PlayerInputBase.leftHand;

        if (_leftXRController != null) { 
            _leftXRController.calibrationMode = false;
            _leftXRController.OnMenuDown -= OnMenuDown;
            _leftXRController.OnMenuUp -= OnMenuUp;
            _leftXRController.OnMove -= OnMove;
            _leftXRController.OnToggleDown -= OnGripStateChanged;
            _leftXRController.OnToggleUp -= OnGripStateChanged;
        }

        if (_rightXRController != null) { 
            _rightXRController.calibrationMode = false;
            _rightXRController.OnMenuDown -= OnMenuDown;
            _rightXRController.OnMenuUp -= OnMenuUp;
            _rightXRController.OnMove -= OnMove;
            _rightXRController.OnToggleDown -= OnGripStateChanged;
            _rightXRController.OnToggleUp -= OnGripStateChanged;
        }
    }

    private void OnGripStateChanged(PlayerInputBase controller, bool input)
    {
        _gripState = input;
        SomeButtonClicked(controller);
    }

    private void OnMove(PlayerInputBase controller, Vector2 input)
    {
        _thumbState = input;
        SomeButtonClicked(controller);
    }
    private void OnMenuUp(PlayerInputBase controller)
    {
        _menuState = false;
        SomeButtonClicked(controller);
    }

    private void OnMenuDown(PlayerInputBase controller)
    {
        _menuState = true;
        SomeButtonClicked(controller);
    }



    /// <summary>
    /// steamVR action Event method
    /// </summary>
    /// <param name="fromAction"></param>
    /// <param name="fromSources"></param>
    /// <param name="newState"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void SomeButtonClicked( PlayerInputBase fromSources) {
        // Check Config for ingame Calibration Flag & check if Player is in Hub scene
        if (!ConfigurationManager.Configuration.IngamePillarOffset || !TTSceneManager.Instance.IsInHubScene)
            return;

        // IsInCalibrationMode method returns the the valid calibration mode depends on button combo
        // returns 'none' if no valid button are triggered
        PillarOffsetManager.PillarOffsetCalibrationMode calibrationMode =
            IsInCalibrationMode(_thumbState != Vector2.zero, _menuState, _gripState);

        switch (calibrationMode) {
            case PillarOffsetManager.PillarOffsetCalibrationMode.Rotation:
                // Start Rotation calibration
                ProcessPillarCalibration(calibrationMode);
                break;
            case PillarOffsetManager.PillarOffsetCalibrationMode.Position:
                // start position calibration
                ProcessPillarCalibration(calibrationMode);
                break;
            case PillarOffsetManager.PillarOffsetCalibrationMode.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// process one calibration step
    /// read values from Config, grab input, calculate new offset, write offset back to disk
    /// </summary>
    /// <param name="calibrationMode">current calibration mode</param>
    /// <exception cref="ArgumentOutOfRangeException">invalid calibration mode</exception>
    private void ProcessPillarCalibration(PillarOffsetManager.PillarOffsetCalibrationMode calibrationMode) {
        Vector2 trackPadInput = GrabInput(calibrationMode);

        switch (calibrationMode) {
            case PillarOffsetManager.PillarOffsetCalibrationMode.Position:

                if (!(Mathf.Abs(trackPadInput.x) > 0) && !(Mathf.Abs(trackPadInput.y) > 0)) return;

                // read old offset values
                Vector3 positionOffset = ConfigurationManager.Configuration.PillarPositionOffset;
                Quaternion oldRotationOffset = ApplyPillarOffset
                    .RotationAngleToRotationOffset(ConfigurationManager.Configuration.PillarRotationOffsetAngle);

                // calculate new offset values from old & input
                positionOffset += Quaternion.Inverse(oldRotationOffset) *
                                  new Vector3(-trackPadInput.x, 0, -trackPadInput.y) * _offsetStepWidthPosition;

                UpdateNewPillarOffsetToConfig(calibrationMode, new Vector2(positionOffset.x, positionOffset.z));
                break;
            case PillarOffsetManager.PillarOffsetCalibrationMode.Rotation:

                if (Mathf.Abs(trackPadInput.x) >= 0 && Mathf.Abs(trackPadInput.x) <= 0) return;
                float rotationOffset = ConfigurationManager.Configuration.PillarRotationOffsetAngle;

                rotationOffset += trackPadInput.x * _offsetStepWidthRotationAngle;

                UpdateNewPillarOffsetToConfig(calibrationMode, new Vector2(rotationOffset, 0));

                break;

            case PillarOffsetManager.PillarOffsetCalibrationMode.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(calibrationMode), calibrationMode, null);
        }

        // apply new offset
        _applyOffset.ApplyOffsetFromConfigurationFile();
    }

    /// <summary>
    /// Write offset to config file depends on current calibration mode
    /// </summary>
    /// <param name="calibrationMode">current calibration mode</param>
    /// <param name="offset">new offset</param>
    /// <exception cref="ArgumentOutOfRangeException">invalid calibration mode</exception>
    private void UpdateNewPillarOffsetToConfig(PillarOffsetManager.PillarOffsetCalibrationMode calibrationMode, Vector2 offset) {
        switch (calibrationMode) {
            case PillarOffsetManager.PillarOffsetCalibrationMode.Position:
                ConfigurationManager.Configuration.PillarPositionOffset = new Vector3(offset.x, 0, offset.y);
                ConfigurationManager.WriteConfigToFile();
                break;
            case PillarOffsetManager.PillarOffsetCalibrationMode.Rotation:
                ConfigurationManager.Configuration.PillarRotationOffsetAngle = offset.x;
                ConfigurationManager.WriteConfigToFile();
                break;
            case PillarOffsetManager.PillarOffsetCalibrationMode.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(calibrationMode), calibrationMode, null);
        }
    }

    /// <summary>
    /// grab SteamVR Controller Input with Unity's OpenVR Input Mapping
    /// </summary>
    /// <param name="calibrationMode">current calibration Mode</param>
    /// <returns>Returns Track-Pad Axis Value depends on current calibration Mode</returns>
    /// <exception cref="ArgumentOutOfRangeException">invalid calibration mode</exception>
    private Vector2 GrabInput(PillarOffsetManager.PillarOffsetCalibrationMode calibrationMode) {
        Vector2 inputSteps = Vector2.zero;

        switch (calibrationMode) {
            case PillarOffsetManager.PillarOffsetCalibrationMode.Position: {
                float horizontal = _thumbState.x;
                float vertical = _thumbState.y;

                inputSteps.x = Mathf.Abs(horizontal) > _trackPadMinAxisValueThreshold ? Mathf.Sign(horizontal) : 0;
                inputSteps.y = Mathf.Abs(vertical) > _trackPadMinAxisValueThreshold ? Mathf.Sign(vertical) : 0;
                break;
            }

            case PillarOffsetManager.PillarOffsetCalibrationMode.Rotation: {
                float horizontal = -_thumbState.x;
                inputSteps.x = (int) (Mathf.Abs(horizontal) > _trackPadMinAxisValueThreshold
                    ? Mathf.Sign(horizontal)
                    : 0);
                break;
            }

            case PillarOffsetManager.PillarOffsetCalibrationMode.None:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(calibrationMode), calibrationMode, null);
        }

        return inputSteps;
    }

    /// <summary>
    /// returns the the valid calibration mode depends on button combo
    /// returns 'none' if no valid button combo are triggered
    /// </summary>
    /// <param name="trackPadPressed">Track-Pad Button pressed Status</param>
    /// <param name="menuPressed">Menu Button pressed Status</param>
    /// <param name="gripPressed">Grip Button pressed Status</param>
    /// <returns></returns>
    private static PillarOffsetManager.PillarOffsetCalibrationMode IsInCalibrationMode(bool trackPadPressed, bool menuPressed,
        bool gripPressed) {
        if (trackPadPressed && menuPressed && !gripPressed)
            return PillarOffsetManager.PillarOffsetCalibrationMode.Position; // menu button + touchPad to calibrate position
        if (trackPadPressed && menuPressed)
            return PillarOffsetManager.PillarOffsetCalibrationMode.Rotation; // menu button + touchPad + grip Button to calibrate rotation
        return PillarOffsetManager.PillarOffsetCalibrationMode.None;
    }
}
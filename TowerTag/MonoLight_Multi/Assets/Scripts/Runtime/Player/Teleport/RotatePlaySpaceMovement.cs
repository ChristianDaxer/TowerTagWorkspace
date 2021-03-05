using System;
using System.Collections;
using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class RotatePlaySpaceMovement : MonoBehaviour {
    public delegate void RotateAction(object sender, PlaySpaceDirection turningPlateDirection);

    public event RotateAction RotationStarted;
    public event RotateAction Rotated;

    [SerializeField] private Transform _playerSubParent;
    [SerializeField] private Transform _chaperone;

    private bool _playSpaceRotate;
    private IPlayer _player;
    private Quaternion _localStartRotation;
    private Quaternion _chaperoneStartRotation;
    private Vector3 _localStartPosition;
    private float _currentRotationOffset;
    private TeleportAlgorithm _teleportAlgorithm;
    private float _timer;
    private bool _coroutineIsRunning;
    private PlaySpaceDirection _currentPlaySpaceDirection;
    private bool _invertPlaySpaceRotation;
    private bool _smallPlaySpaceAreaActive;

    public bool InvertPlaySpaceRotation => _invertPlaySpaceRotation;

    public PlaySpaceDirection CurrentPlaySpaceDirection => _currentPlaySpaceDirection;

    public IPlayer Player {
        set {
            UnregisterListener();
            _player = value;
            RegisterListener();
        }
    }

    public enum PlaySpaceDirection {
        Front,
        Back,
        Left,
        Right
    }

    private void Start() {
        /*if (ConfigurationManager.Configuration.SmallPlayArea)
        {
            _playerSubParent.localPosition += new Vector3(0, 0, -_positionOffset);
        }*/

        _localStartPosition = _playerSubParent.localPosition;
        _localStartRotation = _playerSubParent.localRotation;
        if (SharedControllerType.VR && _chaperone != null) _chaperoneStartRotation = _chaperone.localRotation;
        _currentRotationOffset = ConfigurationManager.Configuration.PillarRotationOffsetAngle;

        _invertPlaySpaceRotation = ConfigurationManager.Configuration.InvertSmallPlayArea;
        _smallPlaySpaceAreaActive = ConfigurationManager.Configuration.SmallPlayArea;
    }

    private void OnEnable() {
        RegisterListener();

        // Set default Direction
        _currentPlaySpaceDirection = PlaySpaceDirection.Back;
    }

    private void OnDisable() {
        UnregisterListener();
    }

    public void ResetRotatePlaySpaceMovement() {
        _playerSubParent.localPosition = _localStartPosition;
        _playerSubParent.localRotation = _localStartRotation;
        _currentPlaySpaceDirection = PlaySpaceDirection.Back;
        _currentRotationOffset = ConfigurationManager.Configuration.PillarRotationOffsetAngle;
        if (SharedControllerType.VR && _chaperone != null) _chaperone.localRotation = _chaperoneStartRotation;
        if (_player != null && _player.CurrentPillar != null)
            _player.CurrentPillar.PillarTurningPlateController.ResetTurningPlateDirection();
    }

    [ContextMenu("TestPlaySpaceRotationReset")]
    public void TestReset() {
        ResetRotatePlaySpaceMovement();
    }

    private void RegisterListener() {
        if (_player?.RotatePlayspaceHandler != null)
            _player.RotatePlayspaceHandler.RotatingPlaySpace += OnPlaySpaceRotateStarted;
        if (_player != null && _player.IsMe) _player.GunController.TeleportTriggered += OnTeleportTriggered;
        ConfigurationManager.ConfigurationUpdated += OnConfigurationUpdated;
        GameManager.Instance.MatchHasFinishedLoading += RegisterMatchEvents;
    }

    private void UnregisterListener() {
        if (_player?.RotatePlayspaceHandler != null)
            _player.RotatePlayspaceHandler.RotatingPlaySpace -= OnPlaySpaceRotateStarted;
        if (_player != null && _player.IsMe) _player.GunController.TeleportTriggered -= OnTeleportTriggered;
        ConfigurationManager.ConfigurationUpdated -= OnConfigurationUpdated;
        GameManager.Instance.MatchHasFinishedLoading -= RegisterMatchEvents;
        if (GameManager.Instance.CurrentMatch != null) {
            UnregisterMatchEvents(GameManager.Instance.CurrentMatch);
        }
    }

    private void RegisterMatchEvents(IMatch match) {
        match.Finished += OnMatchFinished;
        match.RoundStartingAt += OnRoundStartAt;
    }

    private void OnRoundStartAt(IMatch match, int time) {
        ResetRotatePlaySpaceMovement();
    }

    private void UnregisterMatchEvents(IMatch match) {
        match.Finished -= OnMatchFinished;
        match.RoundStartingAt -= OnRoundStartAt;
    }

    private void OnMatchFinished(IMatch match) {
        ResetRotatePlaySpaceMovement();
        UnregisterMatchEvents(match);
    }

    private void OnTeleportTriggered(IPlayer player, Pillar target) {
        //if (target.IsSpectatorPillar) ResetRotatePlaySpaceMovement();
    }

    private void OnConfigurationUpdated() {
        // check if SPA was toggled
        if (_smallPlaySpaceAreaActive != ConfigurationManager.Configuration.SmallPlayArea) {
            _smallPlaySpaceAreaActive = ConfigurationManager.Configuration.SmallPlayArea;

            if (!_smallPlaySpaceAreaActive) ResetRotatePlaySpaceMovement();
        }

        // get latest transforms in case of user custom play space offset
        _localStartPosition = _playerSubParent.localPosition;
        _localStartRotation = _playerSubParent.localRotation;
        if (SharedControllerType.VR && _chaperone != null) _chaperoneStartRotation = _chaperone.localRotation;

        _invertPlaySpaceRotation = ConfigurationManager.Configuration.InvertSmallPlayArea;
    }

    private void OnPlaySpaceRotateStarted(object sender, RotatePlaySpaceHook target) {
        _playSpaceRotate = true;
        Rotate(target);
    }

    private void Rotate(RotatePlaySpaceHook target) {
        // early return
        if (!_playSpaceRotate || _coroutineIsRunning) return;

        // Get rotation direction -> returns -1 or 1
        var rotationDirection =
            PillarTurningPlateController.GetRotationDirection((PillarTurningPlateController.TurningSlot) target.ID,
                _invertPlaySpaceRotation);

        _currentRotationOffset += rotationDirection * 90;

        var rotationOffset = Quaternion.AngleAxis(-_currentRotationOffset, Vector3.up);
        RotationStarted?.Invoke(this, _currentPlaySpaceDirection);
        StartCoroutine(LerpRotation(target, _playerSubParent, rotationOffset * _localStartPosition,
            rotationOffset * _localStartRotation, 8.5f));
    }

    private IEnumerator LerpRotation(RotatePlaySpaceHook target,
        Transform player, Vector3 des, Quaternion desRotation, float speed) {
        _coroutineIsRunning = true;
        var running = true;
        Vector3 startPos = player.localPosition;
        Quaternion startRot = player.localRotation;
        var fraction = 0f;
        while (running) {
            if (fraction < 1f) {
                fraction += Time.deltaTime * speed;
                player.localPosition = Vector3.Lerp(startPos, des, fraction);
                player.localRotation = Quaternion.Lerp(startRot, desRotation, fraction);
                if (SharedControllerType.VR && _chaperone != null) {
                    _chaperone.localRotation = Quaternion.Lerp(startRot,
                        Quaternion.AngleAxis(
                            -_currentRotationOffset + ConfigurationManager.Configuration.PillarRotationOffsetAngle,
                            Vector3.up), fraction);
                }

                yield return null;
            }
            else {
                running = false;
            }
        }

        PlaySpaceRotateFinished(target);
        _coroutineIsRunning = false;
    }

    private void PlaySpaceRotateFinished(RotatePlaySpaceHook target) {
        _playSpaceRotate = false;
        UpdateCurrentPlaySpaceDirection(_currentPlaySpaceDirection, target.SlotObject.TurningSlot);
        Rotated?.Invoke(this, _currentPlaySpaceDirection);
    }

    private void UpdateCurrentPlaySpaceDirection(PlaySpaceDirection oldTurningPlatePosition,
        PillarTurningPlateController.TurningSlot rotationDirection) {
        if (_currentPlaySpaceDirection != oldTurningPlatePosition) {
            Debug.LogError("Some error occured. Current play space directions does not match.");
        }

        switch (oldTurningPlatePosition) {
            case PlaySpaceDirection.Front:
                // Set new current plate position
                _currentPlaySpaceDirection = rotationDirection == PillarTurningPlateController.TurningSlot.Left
                    ? PlaySpaceDirection.Right
                    : PlaySpaceDirection.Left;
                break;
            case PlaySpaceDirection.Back:

                _currentPlaySpaceDirection = rotationDirection == PillarTurningPlateController.TurningSlot.Left
                    ? PlaySpaceDirection.Left
                    : PlaySpaceDirection.Right;
                break;
            case PlaySpaceDirection.Left:
                _currentPlaySpaceDirection = rotationDirection == PillarTurningPlateController.TurningSlot.Left
                    ? PlaySpaceDirection.Front
                    : PlaySpaceDirection.Back;
                break;
            case PlaySpaceDirection.Right:
                _currentPlaySpaceDirection = rotationDirection == PillarTurningPlateController.TurningSlot.Left
                    ? PlaySpaceDirection.Back
                    : PlaySpaceDirection.Front;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(oldTurningPlatePosition), oldTurningPlatePosition, null);
        }
    }
}
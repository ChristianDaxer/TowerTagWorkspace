using System;
using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class PillarTurningPlateController : MonoBehaviour
{
    public enum TurningSlot
    {
        Left = 0,
        Right = 1
    }

    [SerializeField] private RotatePlaySpaceHook[] _hooks;

    public RotatePlaySpaceHook[] Hooks => _hooks;

    private Pillar _pillar;
    private IPlayer _player;
    private RotatePlaySpaceMovement _playSpaceMovementHelper;
    private RotatePlaySpaceMovement.PlaySpaceDirection _currentPlatePosition;
    private Quaternion _turningPlateIdentity;
    private bool _turningPlateActive;
    private bool _smallPlayAreaActive;

    private void OnEnable()
    {
        // Early Returns

        if (!TowerTagSettings.Home || !SharedControllerType.IsPlayer)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!GameManager.Instance.IsInLoadMatchState)
        {
            gameObject.SetActive(false);
            return;
        }

#if !UNITY_EDITOR
        if (!SharedControllerType.VR)
        {
            gameObject.SetActive(false);
            return;
        }
#endif

        _pillar = GetComponentInParent<Pillar>();

        if (_pillar == null)
        {
            Debug.LogError("TurningPlateController: Can't find owner Pillar.");
            Destroy(gameObject);
            return;
        }

        _player = PlayerManager.Instance.GetOwnPlayer();

        if (_player == null)
        {
            Debug.LogError("TurningPlateController: Can't find owner Player.");
            return;
        }

        ConfigurationManager.ConfigurationUpdated += OnConfigurationUpdated;
        _smallPlayAreaActive = ConfigurationManager.Configuration.SmallPlayArea;
        _playSpaceMovementHelper = _player.GunController.RotatePlayspaceHelper;

        _playSpaceMovementHelper.Rotated += OnRotationFinished;
        _pillar.OwnerChanged += OnOwnerChanged;

        _currentPlatePosition = RotatePlaySpaceMovement.PlaySpaceDirection.Back;
        Init(_player.TeamID, _pillar.Owner == _player);

        foreach (var hook in _hooks)
        {
            hook.SlotObject.Init(_player);
        }
    }

    private void OnDisable()
    {
        ConfigurationManager.ConfigurationUpdated -= OnConfigurationUpdated;
        if (_playSpaceMovementHelper != null)
            _playSpaceMovementHelper.Rotated -= OnRotationFinished;
        if (_pillar != null)
            _pillar.OwnerChanged -= OnOwnerChanged;
    }

    private void OnConfigurationUpdated()
    {
        if (_smallPlayAreaActive == ConfigurationManager.Configuration.SmallPlayArea) return;
        _smallPlayAreaActive = ConfigurationManager.Configuration.SmallPlayArea;
        ToggleTurningPlateOnPillar(_smallPlayAreaActive);
    }

    private void Init(TeamID playerTeamID, bool pillarIsOwnerPillar)
    {
        if (playerTeamID == TeamID.Ice)
        {
            gameObject.transform.Rotate(0, 180, 0);
        }

        var localRotation = transform.localRotation;
        _turningPlateIdentity = new Quaternion(
            localRotation.x,
            localRotation.y,
            localRotation.z,
            localRotation.w);

        if (!pillarIsOwnerPillar || !_smallPlayAreaActive)
            ToggleTurningPlateOnPillar(false);
    }

    private void OnOwnerChanged(Pillar pillar, IPlayer previousOwner, IPlayer newOwner)
    {
        //if (pillar.IsSpectatorPillar) return;
        ToggleTurningPlateOnPillar(newOwner == _player && _smallPlayAreaActive);
    }

    private void OnRotationFinished(object sender, RotatePlaySpaceMovement.PlaySpaceDirection turningPlateDirection)
    {
        if (_turningPlateActive) UpdateTurningPlate(turningPlateDirection);
    }

    private void ToggleTurningPlateOnPillar(bool status)
    {
        _turningPlateActive = status;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(status);
        }

        if (_turningPlateActive) UpdateTurningPlate(_playSpaceMovementHelper.CurrentPlaySpaceDirection);
    }

    public static int GetRotationDirection(TurningSlot targetSlot, bool invertRotation = false)
    {
        if (invertRotation)
            return targetSlot == TurningSlot.Left ? -1 : 1;
        return targetSlot == TurningSlot.Right ? -1 : 1;
    }

    private void UpdateTurningPlate(RotatePlaySpaceMovement.PlaySpaceDirection newTurningPlatePosition)
    {
        if(_playSpaceMovementHelper != null)
            SetTurningPlate(newTurningPlatePosition, _playSpaceMovementHelper.InvertPlaySpaceRotation);
    }

    public void ResetTurningPlateDirection()
    {
        // Reset turning plate rotation
        //gameObject.transform.rotation = _turningPlateIdentity;

        _currentPlatePosition = RotatePlaySpaceMovement.PlaySpaceDirection.Back;
        UpdateTurningPlate(_currentPlatePosition);
    }

    private void SetTurningPlate(RotatePlaySpaceMovement.PlaySpaceDirection turningPlateDirection,
        bool invertRotation = false)
    {
        var direction = invertRotation ? -1 : 1;

        // Reset turning plate rotation
        gameObject.transform.rotation = _turningPlateIdentity;

        // Set new direction depends on play space movement rotation direction
        switch (turningPlateDirection)
        {
            case RotatePlaySpaceMovement.PlaySpaceDirection.Front:
                _currentPlatePosition = RotatePlaySpaceMovement.PlaySpaceDirection.Front;
                break;
            case RotatePlaySpaceMovement.PlaySpaceDirection.Back:
                transform.Rotate(0, 180, 0);
                _currentPlatePosition = RotatePlaySpaceMovement.PlaySpaceDirection.Back;
                break;
            case RotatePlaySpaceMovement.PlaySpaceDirection.Left:
                transform.Rotate(0, direction * 90, 0);
                _currentPlatePosition = RotatePlaySpaceMovement.PlaySpaceDirection.Left;
                break;
            case RotatePlaySpaceMovement.PlaySpaceDirection.Right:
                transform.Rotate(0, direction * -90, 0);
                _currentPlatePosition = RotatePlaySpaceMovement.PlaySpaceDirection.Right;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(turningPlateDirection), turningPlateDirection, null);
        }
    }
}
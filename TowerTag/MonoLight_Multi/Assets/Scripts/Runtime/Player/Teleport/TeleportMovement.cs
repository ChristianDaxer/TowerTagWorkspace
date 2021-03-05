using Photon.Pun;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Moves the player transform to a target <see cref="Pillar"/>.
/// </summary>
public class TeleportMovement : MonoBehaviour {
    #region serialized fields

    [SerializeField, Tooltip("Current Pillar of the local player as shared variable")]
    private SharedPillar _localPlayerPillar;

    [FormerlySerializedAs("minDistanceToTarget")] [SerializeField]
    private float _minDistanceToTarget;

    [FormerlySerializedAs("heightOffset")] [SerializeField]
    private float _heightOffset;

    [SerializeField] private Rigidbody _rigidBody;

    #endregion

    #region events

    public delegate void TeleportDelegate(int oldPillarID, int newPillarID);

    public event TeleportDelegate Teleporting;

    public delegate void TeleportStartDelegate(int newPillarID, Transform rootTransform);

    public event TeleportStartDelegate TeleportStarted;

    public delegate void TeleportFinishedDelegate(Transform rootTransform);

    public event TeleportFinishedDelegate Teleported;

    #endregion

    #region public properties

    private Transform ObjectToTeleport { get; set; }
    private Transform PrjTransform { get; set; }

    #endregion

    #region cached values

    private IPlayer _player;

    public IPlayer Player {
        set {
            UnregisterEventListeners();
            _player = value;
            RegisterEventListeners();
            if (_player != null && _player.CurrentPillar != null) Teleport(_player.CurrentPillar, 0);
        }
    }

    private bool _teleporting;
    private float _timer;
    private float _timeToTeleport;
    private readonly TeleportAlgorithm _teleportAlgorithm = new CurvedTeleportInverse();
    private Pillar _lastTarget;

    #endregion

    #region Init

    private void OnEnable() {
        RegisterEventListeners();
    }

    private void OnDisable() {
        UnregisterEventListeners();
    }

    private void RegisterEventListeners() {
        if (_player?.TeleportHandler != null) {
            _player.TeleportHandler.PlayerTeleporting += OnPlayerTeleporting;
        }
    }

    private void UnregisterEventListeners() {
        if (_player != null && _player.TeleportHandler != null) {
            _player.TeleportHandler.PlayerTeleporting -= OnPlayerTeleporting;
        }
    }

    #endregion

    private void OnPlayerTeleporting(TeleportHandler sender, Pillar origin, Pillar target, float timeToTeleport) {
        Teleport(target, timeToTeleport);
    }

    // visible for testing
    private void Teleport(Pillar target, float timeToTeleport) {
        if (target == null) {
            Debug.LogError("Teleporter.Teleport: Client " + PhotonNetwork.LocalPlayer.ActorNumber + ": Player(" +
                           _player.PlayerID +
                           ") can't teleport -> target is null!");
            return;
        }

        if (_teleporting) {
            Debug.LogWarning("Teleporter.teleport: Client " + PhotonNetwork.LocalPlayer.ActorNumber + ": Player(" +
                             _player.PlayerID +
                             ") -> Teleport has started already! (" + timeToTeleport + ")");
            OnTeleportFinished();
        }

        // reset old values
        _teleporting = false;


        if (ObjectToTeleport == null)
            ObjectToTeleport = _player.PlayerAvatar.TeleportTransform;

        if (ObjectToTeleport == null) {
            Debug.LogError("Teleporter.teleport: Client " + PhotonNetwork.LocalPlayer.ActorNumber + ": Player(" +
                           _player.PlayerID +
                           ") can't teleport -> TeleportObject is null!");
            return;
        }

        Teleporting?.Invoke(_lastTarget != null ? _lastTarget.ID : -1, target.ID);

        if (_lastTarget != null && target != null && _lastTarget.ID == target.ID) {
            Debug.LogWarning("Teleporter.teleport: Client " + PhotonNetwork.LocalPlayer.ActorNumber + ": Player(" +
                           _player.PlayerID + ") Teleport: from " + _lastTarget.ID + " to " +
                           target.ID);
        }

        _lastTarget = target;

        // if no time for Animation -> set Position immediately
        if (timeToTeleport <= 0f) {
            // set rotation on player to sync. Important, because position is synced in local coords
            if (PhotonNetwork.IsMasterClient)
                _player.SetRotationOnMaster(target.TeleportTransform.rotation);

            ObjectToTeleport.position = target.TeleportTransform.position;
            transform.localPosition = Vector3.up * _heightOffset;

            OnTeleportFinished();
            return;
        }

        // Init animated Teleport
        _teleporting = true;
        if (PrjTransform == null)
            PrjTransform = _player.PlayerAvatar.ProjectileSpawnTransform;

        _teleportAlgorithm.Init(ObjectToTeleport.position, target, _minDistanceToTarget, PrjTransform);

        if (_rigidBody != null) {
            _rigidBody.useGravity = false;
        }

        _timeToTeleport = timeToTeleport;
        _timer = 0f;

        TeleportStarted?.Invoke(target.ID, ObjectToTeleport);
    }

    private void Update() {
        if (!_teleporting)
            return;

        if (_timer < 1f) {
            _timer = Mathf.Clamp01(_timer + (Time.deltaTime / _timeToTeleport));

            ObjectToTeleport.position = _teleportAlgorithm.GetPositionAt(_timer);
        }
        else {
            transform.localPosition = Vector3.up * _heightOffset;
            if (_rigidBody != null) {
                _rigidBody.useGravity = true;
            }

            OnTeleportFinished();
        }
    }

    private void OnTeleportFinished() {
        _teleporting = false;

        Teleported?.Invoke(ObjectToTeleport);
    }
}
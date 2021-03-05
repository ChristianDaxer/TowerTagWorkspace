using System;
using Photon.Pun;
using TowerTag;

public class TeleportHandler {
    #region internal classes & structs

    // players Pillar & teleport data
    /// <summary>
    /// Internal struct to hold Infos for the current teleport.
    /// </summary>
    private struct TeleportInfo {
        /// <summary>
        /// ID of the Pillar we should teleport to.
        /// </summary>
        public int PillarID;

        /// <summary>
        /// TeleportType to calculate teleport duration on client (see enum in TeleportHelper class).
        /// </summary>
        public TeleportHelper.TeleportDurationType TeleportType;

        /// <summary>
        /// Serialize or deserialize internal state to given stream.
        /// </summary>
        /// <param name="stream">The stream we should write to (if it is a writing stream) or read from (if it is a reading stream).</param>
        public void Serialize(BitSerializer stream) {
            stream.Serialize(ref PillarID, BitCompressionConstants.MinPillarID, BitCompressionConstants.MaxPillarID);
            int teleportTypeCount = Enum.GetNames(typeof(TeleportHelper.TeleportDurationType)).Length;
            if (stream.IsWriting) {
                stream.WriteInt((int) TeleportType, 0, teleportTypeCount);
            }
            else {
                TeleportType = (TeleportHelper.TeleportDurationType) stream.ReadInt(0, teleportTypeCount);
            }
        }
    }

    #endregion

    #region Member variables & properties

    private IPhotonService _photonService;

    private IPhotonService PhotonService =>
        _photonService = _photonService ?? ServiceProvider.Get<IPhotonService>();

    /// <summary>
    /// Infos of the last Teleport which was requested and acknowledged by Master client
    /// (also used on client to prevent reset on automatic check (see CheckIfWeAreInSync() called in Players Update())).
    /// </summary>
    private TeleportInfo _lastTeleport;

    /// <summary>
    /// Should we sync teleport info from Master client to clients (used by Players Sync function to reduce Network bandwidth).
    /// If you want to force a sync, call TriggerSyncOnNextSerialization().
    /// </summary>
    public bool TeleportedSinceLastSync { get; private set; }

    /// <summary>
    /// The Pillar the player is occupying right now.
    /// </summary>
    private Pillar _currentPillar;

    public event Action<Pillar, Pillar> CurrentPillarChanged;
    /// <summary>
    /// Property to get the Pillar the player is occupying right now.
    /// returns null if no Pillar is set or the Pillar is not valid for the current scene (was set in another scene and not updated right now).
    /// </summary>
    public Pillar CurrentPillar {
        get {
            if (PillarManager.Instance.IsPillarValidInCurrentScene(_currentPillar)) {
                return _currentPillar;
            }

            return null;
        }
        private set {
            if(_currentPillar != value) {
                CurrentPillarChanged?.Invoke(_currentPillar, value);
                _currentPillar = value;
            }
        }
    }

    /// <summary>
    /// the owner of this TeleportHandler (the Player to teleport).
    /// </summary>
    private IPlayer _player;

    private GunController _gunController;

    public GunController GunController {
        set {
            if (_gunController != null)
                _gunController.TeleportTriggered -= RequestTeleport;
            _gunController = value;
            if (_gunController != null)
                _gunController.TeleportTriggered += RequestTeleport;
        }
    }

    #endregion

    #region Init & Cleanup

    public void Init(IPlayer owner) {
        _player = owner;
    }

    #endregion

    #region core for local predicted and Masterclient decision

    /// <summary>
    /// Triggers teleport on Master client (trigger local teleport event) and syncs TeleportInfo to clients only if
    /// teleport is Acknowledged by PillarManager.
    /// Attention: if the teleport was requested by a user (normal inGame Teleport) the sync should be forced
    /// even if the teleport was denied to reset the local Player who requested the teleport.
    /// (to force a sync call TriggerSyncOnNextSerialization()).
    /// </summary>
    /// <param name="playerPillarWhenTriggeredTeleportRequest">The Pillar the player was on when he send the teleport request (only used when userRequestedTeleport is true).</param>
    /// <param name="teleportTarget">The Pillar the player wants to teleport to.</param>
    /// <param name="teleportType">Type of teleport (see TeleportDurationType enum in teleportHelper class).</param>
    /// <param name="userRequestedTeleport">Was the teleport requested by a user?</param>
    /// <returns>True if the teleport was acknowledged, false when it was denied.</returns>
    public void TriggerTeleportOnMaster(Pillar playerPillarWhenTriggeredTeleportRequest, Pillar teleportTarget,
        TeleportHelper.TeleportDurationType teleportType, bool userRequestedTeleport) {
        if (teleportTarget == null) {
            Debug.LogError("Cannot trigger teleport: Target Pillar is null");
            return;
        }

        PillarManager pillarManager = PillarManager.Instance;

        // check if the user request is valid for the current Master client state
        if (userRequestedTeleport) {
            // is the requesting players gun disabled (so he should not trigger a teleport)
            if (_player.PlayerState.IsGunDisabled) {
                if(!_player.IsBot) Debug.LogWarning("Denied Teleport because it is userRequested & Gun is disabled");
                return;
            }

            // is the Master client in the same state (player is on same pillar) as the player when he triggered the teleport?
            if (playerPillarWhenTriggeredTeleportRequest != _player.CurrentPillar) {
                string pillarIDs = " clientPillarWhenTriggeredTeleport " +
                                   (playerPillarWhenTriggeredTeleportRequest != null
                                       ? playerPillarWhenTriggeredTeleportRequest.ID.ToString()
                                       : " - ");
                pillarIDs += ", currentPillarOnMaster: " +
                             (_player.CurrentPillar != null ? _player.CurrentPillar.ID.ToString() : " - ");
                Debug.LogWarning("Denied Teleport because it is userRequested " +
                                 "& the playerPillarWhenTriggeredTeleportRequest is not " +
                                 $"the Players current Pillar: {pillarIDs}");
                return;
            }

            if (!teleportTarget.CanTeleport(_player)) {
                Debug.LogWarning("Denied teleport, because the target pillar state does not allow it");
                return;
            }

            // check if targetPillar is reachable by Player (is true if playerPillar and targetPillar are neighbours)
            if (!pillarManager.IsPillarNeighbourOf(playerPillarWhenTriggeredTeleportRequest, teleportTarget)
            && !TTSceneManager.Instance.IsInHubScene) {
                Debug.LogWarning("Denied Teleport because it is userRequested " +
                                 "& the playerPillarWhenTriggeredTeleportRequest is no neighbour " +
                                 "of the Players current Pillar");
                return;
            }
        }

        // request teleport from PillarManager
        if (PillarManager.Instance.RequestTeleportOnMaster(_player, teleportTarget)) {
            Pillar origin = CurrentPillar;
            CurrentPillar = teleportTarget;

            // update TeleportInfo (to sync)
            _lastTeleport.PillarID = teleportTarget.ID;
            _lastTeleport.TeleportType = teleportType;

            // force sync if teleport was acknowledged
            TeleportedSinceLastSync = true;

            // calculate teleport duration to teleport local representation of player
            // (remote presentation if Master client is Admin or local representation
            // if Master client is host (Master client with local Player))
            float teleportDuration = TeleportHelper.CalculateTeleportDuration(_player, teleportTarget, teleportType);
            PlayerTeleporting?.Invoke(this, origin, CurrentPillar, teleportDuration);

            if (PhotonNetwork.IsMasterClient && GameManager.Instance.CurrentMatch != null
                                             && GameManager.Instance.CurrentMatch.IsActive
                                             && teleportType == TeleportHelper.TeleportDurationType.Teleport)
                GameManager.Instance.CurrentMatch.Stats.AddTeleport(_player);
            return;
        }

        Debug.LogWarning("TeleportHandler.TriggerTeleportOnMaster: Pillar manager denied Teleport!");
    }

    /// <summary>
    /// Try to trigger predicted teleport on a (non Master client) client. The resulting decision (to teleport or not)
    /// is made upon the local clients state and is just temporary,
    /// because the decision made by the Master client is synced and arrives later and overrides this local decision.
    /// This function tries to mimic the TriggerTeleportOnMaster function
    /// so the decision made is most likely the same as on Master client.
    /// If the predicted teleport is acknowledged locally the local teleport event is triggered.
    /// </summary>
    /// <param name="playerPillarWhenTriggeredTeleportRequest">The Pillar the player is on when he triggered the teleport request.</param>
    /// <param name="teleportTarget">Pillar the player wants to teleport to.</param>
    /// <returns>True if the teleport was acknowledged, false when it was denied.</returns>
    private void TriggerPredictedTeleportOnClient(Pillar playerPillarWhenTriggeredTeleportRequest,
        Pillar teleportTarget) {
        if (PhotonNetwork.IsMasterClient) return;
        if (teleportTarget == null) {
            Debug.LogError("Cannot trigger predicted teleport: Target Pillar is null");
            return;
        }

        PillarManager pillarManager = PillarManager.Instance;

        // is the requesting players gun disabled (so he should not trigger a teleport)
        if (_player.PlayerState.IsGunDisabled) {
            Debug.LogWarning(
                "TeleportHandler.TriggerPredictedTeleportOnClient: denied Teleport because it is userRequested & Gun is disabled");
            return;
        }

        // Do we have the same state as the player when he triggered the teleport (not really needed here with current call flow but anyway)?
        if (playerPillarWhenTriggeredTeleportRequest != _player.CurrentPillar) {
            Debug.LogWarning(
                "TeleportHandler.TriggerPredictedTeleportOnClient: denied Teleport because it is userRequested & the playerPillarWhenTriggeredTeleportRequest is not the Players current Pillar");
            return;
        }

        // check if targetPillar is reachable by Player (if playerPillar and targetPillar are neighbours)
        if (!pillarManager.IsPillarNeighbourOf(playerPillarWhenTriggeredTeleportRequest, teleportTarget)) {
            Debug.LogWarning(
                "TeleportHandler.TriggerPredictedTeleportOnClient: denied Teleport because it is userRequested & the playerPillarWhenTriggeredTeleportRequest is no neighbour of the Players current Pillar");
            return;
        }

        // request teleport from PillarManager
        if (PillarManager.Instance.RequestPredictedTeleportOnClient(_player, teleportTarget)) {
            Pillar origin = CurrentPillar;
            CurrentPillar = teleportTarget;

            const TeleportHelper.TeleportDurationType teleportType = TeleportHelper.TeleportDurationType.Teleport;
            // update TeleportInfo temporary to prevent automatic reset when CheckIfWeAreInSync() is called
            // gets overriden on next Master client sync
            _lastTeleport.PillarID = teleportTarget.ID;
            _lastTeleport.TeleportType = teleportType;

            float teleportDuration = TeleportHelper.CalculateTeleportDuration(_player, teleportTarget, teleportType);
            PlayerTeleporting?.Invoke(this, origin, CurrentPillar, teleportDuration);

            return;
        }

        Debug.LogWarning("TeleportHandler.TriggerPredictedTeleportOnClient: Pillar manager denied Teleport!");
    }

    #endregion

    #region Sync

    /// <summary>
    /// Force Sync of this Teleport handlers internal state on next PlayerSync.
    /// </summary>
    public void TriggerSyncOnNextSerialization() {
        TeleportedSinceLastSync = true;
    }

    /// <summary>
    /// Serialize and deserialize the internal state to the given stream.
    /// </summary>
    /// <param name="stream">Stream the internal state is written to or read from.</param>
    public void Serialize(BitSerializer stream) {
        // we are now in sync (only needed on Master to reduce NW bandwidth)
        TeleportedSinceLastSync = false;
        // serialize (read/write) TeleportInfo
        _lastTeleport.Serialize(stream);

        // if we have deserialized internal state on client, check if we should do something (trigger teleport)
        if (stream.IsReading)
            TriggerTeleportOnClient();
    }

    /// <summary>
    /// Check if we are up to date or if we should do something (trigger teleport).
    /// Is used to react on changes when we missed a sync event. Called from players Update (so we are always in sync).
    /// </summary>
    public void CheckIfWeAreInSync() {
        // we are not on the right Pillar -> Teleport
        if (CurrentPillar == null || CurrentPillar.ID != _lastTeleport.PillarID
                                  || !PillarManager.Instance.IsPillarIDValidInCurrentScene(_lastTeleport.PillarID)) {
            if (PhotonService.IsMasterClient) {
                Pillar pillar = PillarManager.Instance.FindSpectatorPillarForPlayer(_player);
                if (pillar != null) {
                    TeleportHelper.TeleportPlayerRequestedByGame(_player, pillar,
                        TeleportHelper.TeleportDurationType.Immediate);
                }
                else TeleportHelper.TeleportPlayerOnSpawnPillar(_player, TeleportHelper.TeleportDurationType.Immediate);
            }
            else {
                TriggerTeleportOnClient();
            }
        }
    }

    /// <summary>
    /// Trigger teleport event if the current players pillar is null or not equal to the synced Pillar we should be on.
    /// Also resets current players Pillar if it is not valid in this scene.
    /// </summary>
    private void TriggerTeleportOnClient() {
        // is our teleport target valid?
        if (!PillarManager.Instance.IsPillarIDValidInCurrentScene(_lastTeleport.PillarID)) {
            CurrentPillar = null;
            return;
        }

        if (CurrentPillar != null && CurrentPillar.ID == _lastTeleport.PillarID) {
            //Debug.Log("teleport handler.TriggerTeleportOnClient: " +
            //"don't need to teleport, we are already on the Pillar we want to teleport to!");
            return;
        }

        Pillar target = PillarManager.Instance.GetPillarByID(_lastTeleport.PillarID);
        if (target == null) {
            //Debug.LogError("Cannot target onto null pillar");
            return;
        }

        Pillar origin = CurrentPillar;
        CurrentPillar = target;
        // calculate teleport duration to teleport the local player
        float teleportDuration = TeleportHelper.CalculateTeleportDuration(_player, target, _lastTeleport.TeleportType);
        PlayerTeleporting?.Invoke(this, origin, target, teleportDuration);
    }

    #endregion

    #region local events

    /// <summary>
    /// Event delegate to trigger teleports on clients.
    /// </summary>
    /// <param name="sender">The teleport handler that emitted the event.</param>
    /// <param name="origin">The pillar the player teleports from. can be null.</param>
    /// <param name="target">Pillar to teleport to.</param>
    /// <param name="timeToTeleport">Duration of the teleport.</param>
    public delegate void TeleportDelegate(TeleportHandler sender, Pillar origin, Pillar target, float timeToTeleport);

    public event TeleportDelegate PlayerTeleporting;

    #endregion

    #region localToMasterEvents

    /// <summary>
    /// Event delegate to send teleport requests to Master client (triggered by local clients).
    /// </summary>
    /// <param name="pillarThePlayerIsCurrentlyOn">The Pillar the player is occupying when the teleport is requested.</param>
    /// <param name="targetPillar">The Pillar the player wants to teleport to.</param>
    /// <param name="timestampTheTeleportWasRequested">Timestamp the request is send/triggered.</param>
    public delegate void SendTeleportRequestToMasterDelegate(Pillar pillarThePlayerIsCurrentlyOn, Pillar targetPillar,
        int timestampTheTeleportWasRequested);

    public event SendTeleportRequestToMasterDelegate TeleportRequested;

    /// <summary>
    /// Throws an event to send a teleport request to the master client and triggers a predicted teleport on local client
    /// (which gets reset or acknowledged by Master client).
    /// </summary>
    private void RequestTeleport(IPlayer player, Pillar targetPillar) {
        // the Pillar the player is occupying right now
        Pillar currentClientPillar = _player.CurrentPillar;

        // send teleport request to Master client
        TeleportRequested?.Invoke(currentClientPillar, targetPillar, PhotonService.ServerTimestamp);

        // Trigger Local Teleport Prediction
        TriggerPredictedTeleportOnClient(currentClientPillar, targetPillar);
    }

    #endregion
}
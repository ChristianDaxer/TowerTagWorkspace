using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TowerTag;
using UnityEngine;


[Serializable]
public sealed class Pillar : Claimable {
    #region IChargable properties

    public override ChargeableType ChargeableType => ChargeableType.Pillar;

    [SerializeField]
    [ReadOnly]
    [Tooltip("PillarID: has to be unique. " +
             "Please use PillarConfigurationTool after you add/duplicate/removed Pillars from the scene")]
    private int _id = -1;

    public override int ID {
        get => _id;
        set => _id = value;
    }

    #endregion

    #region events

    public delegate void OwnerChangedCallback(Pillar pillar, IPlayer previousOwner, IPlayer newOwner);

    public event OwnerChangedCallback OwnerChanged;

    #endregion

    #region settings

    [SerializeField]
    [Tooltip("Is this Pillar used as standard Spawn Pillar? " +
             "This is used for spawning after scene load and in some GameModes. " +
             "A Number of GameModes use their own spawn rules and ignore this flag, " +
             "so please assure you use the right settings for the GameMode your scene will used with.")]
    private bool _isSpawnPillar;

    public bool IsSpawnPillar {
        get => _isSpawnPillar;
        set => _isSpawnPillar = value;
    }

    [SerializeField, Tooltip("Toggle whether orientation helper arrows shall be displayed when on this pillar")]
    private bool _showOrientationHelp;

    public bool ShowOrientationHelp {
        get => _showOrientationHelp;
        set => _showOrientationHelp = value;
    }

    [SerializeField]
    [Tooltip("This flag is used by some GameModes to handle this kind of Pillars special " +
             "(eg. capture the flag modes).")]
    private bool _isGoalPillar;

    public bool IsGoalPillar {
        get => _isGoalPillar;
        set {
            _isGoalPillar = value;
            RefreshModel();
        }
    }

    [SerializeField]
    [Tooltip("If this flag is enabled a Player can teleport to this Pillar independent " +
             "of the Players Team or the Pillars OwningTeam (you don't have to claim this Pillar to teleport to).")]
    private bool _allowTeleportWithoutTeamMatch;

    public bool AllowTeleportWithoutTeamMatch => _allowTeleportWithoutTeamMatch;

    [SerializeField] [Tooltip("If this flag is enabled, this Pillar can be used for Spectators.")]
    private bool _isSpectatorPillar;

    public bool IsSpectatorPillar {
        get => _isSpectatorPillar;
        set => _isSpectatorPillar = value;
    }

    [SerializeField] [Tooltip("If this flag is enabled, the player will always attempt to spanw on it.")]
    private bool _isDefaultPillar;
    public bool IsDefaultPillar {
        get => _isDefaultPillar;
        set => _isDefaultPillar = value;
    }

    #endregion

    [Header("Claim Properties")]
    [SerializeField]
    [Tooltip("The visuals on the top of the pillar indicate the claim status")]
    private PillarVisualsExtended _pillarVisuals;
    [SerializeField, Tooltip("Separate highlight to indicate goal tower status")]
    private GoalPillarIndicator _goalTowerIndicator;

    [SerializeField] private RopeGameAction ropeGameAction;

    public PillarVisualsExtended PillarVisuals => _pillarVisuals;

    [SerializeField]
    [Tooltip(
        "The time a Player needs to claim this Pillar if it is owned by another team. " +
        "Attention: this value will be overridden by BalancingConfig/GameModes!")]
    private float _timeToClaimIfOwned = 4f;

    public float TimeToClaimIfOwned {
        private get { return _timeToClaimIfOwned; }
        set { _timeToClaimIfOwned = value; }
    }

    [SerializeField]
    [Tooltip("The time a Player needs to claim this Pillar if it is neutral " +
             "(not owned by any team (owningTeam := -1)). " +
             "Attention: this value will be overridden by BalancingConfig/GameModes!")]
    private float _timeToClaimIfNotOwned = 4f;

    public float TimeToClaimIfNotOwned {
        private get { return _timeToClaimIfNotOwned; }
        set { _timeToClaimIfNotOwned = value; }
    }

    protected override float TimeToCharge =>
        OwningTeamID == TeamID.Neutral ? TimeToClaimIfNotOwned : TimeToClaimIfOwned;

    [Tooltip("This Transform is used for positioning and orienting a player when spawning/teleporting. " +
             "The z-axis of this Transform should be pointing in the direction the Player should look after a spawn.")]
    [SerializeField]
    private Transform _teleportTransform;

    public Transform TeleportTransform => _teleportTransform;

    [SerializeField] private PillarWalls _walls;

    [ReadOnly] [SerializeField] private IPlayer _occupyingPlayer;

    public IPlayer Owner {
        get => _occupyingPlayer;
        set {
            if (_occupyingPlayer == value)
                return;
            IPlayer previousPlayer = _occupyingPlayer;
            _occupyingPlayer = value;
            OwnerChanged?.Invoke(this, previousPlayer, _occupyingPlayer);
        }
    }

    public bool IsOccupied => _occupyingPlayer != null && _occupyingPlayer.GameObject != null;

    public TeamID OriginalOwnerTeamID { get; set; } = TeamID.Neutral;

    public bool GlassPillar {
        get => _glassPillar;
        set {
            _glassPillar = value;
            RefreshModel();
        }
    }

    [Header("Model")] [SerializeField] private bool _glassPillar;

    [SerializeField] private GameObject[] _solidPillarAssets;
    [SerializeField] private GameObject[] _goalPillarAssets;
    [SerializeField] private GameObject[] _nonGoalPillarAssets;
    [SerializeField] private PillarTurningPlateController _pillarTurningPlateController;
    public PillarTurningPlateController PillarTurningPlateController => _pillarTurningPlateController;

    private void OnValidate() {
        if (gameObject.scene.buildIndex == -1) return;
        RefreshModel();
        _pillarVisuals.OnValidate();
        _goalTowerIndicator.OnValidate();
    }

    private new void OnEnable() {
        base.OnEnable();
        PlayerAttached += OnPlayerAttached;
        OwnerChanged += OnOwnerChanged;
        PillarManager.Instance.RegisterPillar(this);
    }

    private void OnDestroy()
    {
        PillarManager.Instance.UnregisterPillar(ID);
    }

    private new void OnDisable() {
        base.OnDisable();
        PlayerAttached -= OnPlayerAttached;
        OwnerChanged -= OnOwnerChanged;
    }

    private void OnOwnerChanged(Pillar pillar, IPlayer previousOwner, IPlayer newOwner) {
        if (newOwner != null) {
            List<IPlayer> playersToDetach = AttachedPlayers.Where(p => p != newOwner).ToList();
            playersToDetach.ForEach(p => ropeGameAction.DisconnectRope(this, p));
        }
        ChargeableCollider.ForEach(coll => {
            if(coll.Collider != null)
                coll.Collider.enabled = newOwner == null;
        });
    }

    protected override void FinishChargingOnManager() {
        if (PhotonNetwork.IsMasterClient && GameManager.Instance.CurrentMatch != null
                                         && GameManager.Instance.CurrentMatch.IsActive) {
            AttachedPlayers.ForEach(chargingPlayer =>
                GameManager.Instance.CurrentMatch.Stats.AddClaim(chargingPlayer, this));
        }

        base.FinishChargingOnManager();
    }

    private void OnPlayerAttached(Chargeable chargeable, IPlayer player) {
        if (!PhotonNetwork.IsMasterClient) return;

        List<IPlayer> attachedOpponents = AttachedPlayers.Where(p => p.TeamID != player.TeamID).ToList();
        attachedOpponents.ForEach(p => ropeGameAction.DisconnectRope(this, p));
    }

    private void RefreshModel() {

        _solidPillarAssets?.ForEach(asset => {
            if (asset != null)
                asset.SetActive(!GlassPillar);
        });

        _nonGoalPillarAssets?.ForEach(asset => {
            if (asset != null)
                asset.SetActive(!_isGoalPillar && !GlassPillar);
        });

        _goalPillarAssets?.ForEach(asset => {
            if (asset != null)
                asset.SetActive(_isGoalPillar);
        });
    }

    public void Init() {
        ChargeFallbackTimeout = BalancingConfiguration.Singleton.ClaimRollbackTimeout;
        ChargeFallbackSpeed = BalancingConfiguration.Singleton.ClaimRollbackSpeed;

        if (_walls != null) {
            _walls.Init();
        }

        // remember the owning team at startup
        OriginalOwnerTeamID = OwningTeamID;
    }

    public override bool CanAttach(IPlayer player) {
        if (player.TeamID != CurrentCharge.teamID && CurrentCharge.teamID != TeamID.Neutral)
            return false;

        return CanTryToAttach(player);
    }

    public override bool CanTryToAttach(IPlayer player)
    {
        return IsClaimable && !IsOccupied && IsOnNeighbourPillar(player);
    }

    public override bool CanCharge(IPlayer player) {
        return CanAttach(player) && player.TeamID != OwningTeamID;
    }

    // is the given Player allowed to Teleport to this pillar?
    public bool CanTeleport(IPlayer player) {
        if (player == null) {
            Debug.LogError("Pillar.CanTeleport: Player is null! Returning false!");
            return false;
        }

        return CanTeleport(player.TeamID);
    }

    // is the given Player allowed to Teleport to this pillar?
    private bool CanTeleport(TeamID playerTeamId) {
        // Debug.LogFormat($"Player team ID: {playerTeamId}, Owning team ID: {OwningTeamID}");
        // Debug.LogFormat($"Is Occupied: {IsOccupied}, TeamID matches owning teamID: {playerTeamId == OwningTeamID}, Allow teleport without team match: {_allowTeleportWithoutTeamMatch}, Do all the players attached to the pillar match the team ID: {AttachedPlayers.All(player => player.TeamID == playerTeamId)}");
        return !IsOccupied && (playerTeamId == OwningTeamID || _allowTeleportWithoutTeamMatch) &&
               AttachedPlayers.All(player => player.TeamID == playerTeamId);
    }

    private bool IsOnNeighbourPillar(IPlayer player) {
        Pillar other = player.CurrentPillar;

        if (other == null) {
            Debug.LogError("Cannot check if player is on a neighbour pillar: player pillar is null");
            return false;
        }

        Pillar[] neighboursOfOther = PillarManager.Instance.GetNeighboursByPillarID(other.ID);
        if (neighboursOfOther == null) {
            Debug.LogError("Cannot check if player is on neighbour pillar: neighbour array is null");
            return false;
        }

        return neighboursOfOther.Any(pillar => pillar.ID == _id);
    }

    public void ResetOwningTeam() {
        OwningTeamID = OriginalOwnerTeamID;
    }

    #region SynchronisationFromMasterToClient

    public bool Serialize(BitSerializer stream) {
        // Serializing special member (which are properties or should trigger events)
        if (stream.IsReading) {
            // check PillarID
            int pID = stream.ReadInt(BitCompressionConstants.MinPillarID, BitCompressionConstants.MaxPillarID);
            if (pID != ID) {
                Debug.LogError("Pillar.Serialize: the stream called this function on the wrong Pillar!!!");
                return false;
            }

            // occupying player
            // if writing: write isOccupiedByPlayer to Stream (0/1) (which is set by the property isOccupied),
            //             if isOccupiedByPlayer is set write playerID to stream
            // if reading: read isOccupiedByPlayer, if isOccupiedByPlayer is set:
            //             read playerID and set corresponding Player to member
            bool isOccupiedTmp = stream.ReadBool();
            if (isOccupiedTmp) {
                int playerID = stream.ReadInt(BitCompressionConstants.MinPlayerID, BitCompressionConstants.MaxPlayerID);
                if (Owner == null || Owner.PlayerID != playerID)
                    Owner = PlayerManager.Instance.GetPlayer(playerID);
            }
            else {
                Owner = null;
            }

            // claim value
            var teamID =
                (TeamID) stream.ReadInt(BitCompressionConstants.MinTeamID, BitCompressionConstants.MaxTeamID);
            float chargeValue = stream.ReadFloat(0, 1, 0.001f);
            CurrentCharge = (teamID, chargeValue);

            // owning Team
            OwningTeamID =
                (TeamID) stream.ReadInt(BitCompressionConstants.MinTeamID, BitCompressionConstants.MaxTeamID);
            OriginalOwnerTeamID =
                (TeamID) stream.ReadInt(BitCompressionConstants.MinTeamID, BitCompressionConstants.MaxTeamID);
        }
        else {
            stream.WriteInt(_id, BitCompressionConstants.MinPillarID, BitCompressionConstants.MaxPillarID);

            // occupying player
            // if writing: write isOccupiedByPlayer to Stream (0/1) (which is set by the property isOccupied),
            //             if isOccupiedByPlayer is set write playerID to stream
            // if reading: read isOccupiedByPlayer, if isOccupiedByPlayer is set:
            //             read playerID and set corresponding Player to member
            stream.WriteBool(IsOccupied);
            if (IsOccupied) {
                stream.WriteInt(_occupyingPlayer.PlayerID, BitCompressionConstants.MinPlayerID,
                    BitCompressionConstants.MaxPlayerID);
            }

            // claim value
            stream.WriteInt((int) CurrentCharge.teamID, BitCompressionConstants.MinTeamID,
                BitCompressionConstants.MaxTeamID);
            stream.WriteFloat(CurrentCharge.value, 0, 1, 0.001f);

            // owning Team
            stream.WriteInt((int) OwningTeamID, BitCompressionConstants.MinTeamID, BitCompressionConstants.MaxTeamID);
            stream.WriteInt((int) OriginalOwnerTeamID, BitCompressionConstants.MinTeamID,
                BitCompressionConstants.MaxTeamID);
        }

        return true;
    }

    #endregion
}
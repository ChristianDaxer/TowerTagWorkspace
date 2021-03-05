using TowerTag;
using UnityEngine;

public class PlayerAvatar : MonoBehaviour {
    [SerializeField] private Transform _projectileSpawnTransform;
    [SerializeField] private GameObject _avatarPrefab;
    public Transform ProjectileSpawnTransform => _projectileSpawnTransform;
    public Transform TeleportTransform { get; private set; }
    public GameObject PlayerAvatarParent { get; private set; }
    public AvatarMovement AvatarMovement { get; private set; }
    public Targets Targets { get; private set; }

    private IPlayer Player { get; set; }
    private NameBadge _nameBandage;
    private GameObject _gunParent;
    private AvatarVisuals _avatarVisuals;
    private AvatarAnchor _avatarAnchor;
    private ParticleDamage _particleDamage;
    private AvatarHit _avatarHit;

    public void Init(IPlayer owner, Transform teleportTransform, AvatarMovement avatarMovement) {
        Player = owner;
        TeleportTransform = teleportTransform;
        AvatarMovement = avatarMovement;

        // instantiate avatar
        ITeam team = TeamManager.Singleton.Get(Player.TeamID);
        if (team == null) {
            Debug.LogError($"Cannot handle team change: No Team with teamID {Player.TeamID} found.");
            return;
        }

        PlayerAvatarParent = InstantiateWrapper.InstantiateWithMessage(_avatarPrefab, transform);
        foreach (Transform avatarTransform in PlayerAvatarParent.GetComponentsInChildren<Transform>()) {
            avatarTransform.gameObject.layer = avatarMovement.gameObject.layer;
        }

        // initialize damage detectors
        foreach (DamageDetectorBase damageDetectorBase in GetComponentsInChildren<DamageDetectorBase>()) {
            damageDetectorBase.Init(Player);
        }

        // avatar movement
        _avatarAnchor = GetComponentInChildren<AvatarAnchor>();
        if (_avatarAnchor != null) {
            AvatarMovement.HeadTargetTransform = _avatarAnchor.HeadTransform;
            AvatarMovement.BodyTargetTransform = _avatarAnchor.BodyTransform;

            if (Player.IsMe)
                AvatarMovement.alignToPlayer = true;
        }

        // avatar movement
        // avatar visuals
        _avatarVisuals = GetComponentInChildren<AvatarVisuals>();
        _particleDamage = GetComponentInChildren<ParticleDamage>();
        Targets = GetComponentInChildren<Targets>();
        if (_avatarVisuals != null) {
            _avatarVisuals.Init();
        }

        _gunParent = ProjectileSpawnTransform.parent.gameObject;

        //Just remote clients have a name bandage!
        if (!Player.IsMe) {
            _nameBandage = gameObject.GetComponentInChildren<NameBadge>();
            _avatarHit = AvatarMovement.gameObject.GetComponent<AvatarHit>();
            _avatarHit.HitAnimator = PlayerAvatarParent.GetComponent<Animator>();
            _avatarHit.Owner = owner;
        }

        RegisterListeners();
        TeamChanged(Player, Player.TeamID);
    }

    private void RegisterListeners() {
        if (Player != null) {
            Player.PlayerTeamChanged += TeamChanged;
            Player.ParticipatingStatusChanged += ToggleVisuals;
        }
    }

    private void UnregisterListeners() {
        if (Player != null) {
            Player.PlayerTeamChanged -= TeamChanged;
            Player.ParticipatingStatusChanged -= ToggleVisuals;
        }
    }

    private void OnDisable() {
        if (Player != null)
            UnregisterListeners();
    }

    private void TeamChanged(IPlayer player, TeamID teamID) {
        ITeam team = TeamManager.Singleton.Get(teamID);
        if (team == null) {
            Debug.LogError($"Cannot handle team change: No Team with teamID {teamID} found.");
            return;
        }

        // avatar visuals
        if (_avatarVisuals != null) {
            _avatarVisuals.SetTeamColor(teamID);
        }

        // Remote Player
        if (!Player.IsMe) {
            // health visuals
            var healthVisuals = GetComponent<RemoteHealthVisuals>();
            if (healthVisuals && _avatarVisuals) {
                healthVisuals.GhostVisuals = _avatarVisuals;
            }

            // Set Damage Particle Visualization
            if (_particleDamage != null) {
                _particleDamage.SetTeamColors(team);
            }
            else {
                Debug.LogError("PlayerAvatar.ChangeTeam: ParticleDamage not found!");
            }
        }
    }

    private void ToggleVisuals(IPlayer sender, bool active) {
        if(!sender.IsMe) {
            _avatarVisuals.ToggleRenderer(active);
            _gunParent.SetActive(active);
            _particleDamage.gameObject.SetActive(active);
            if (_nameBandage != null)
                _nameBandage.Badge.enabled = active;
        }
        else {
            Player.GunController.StateMachine.ChangeState(GunController.GunControllerStateMachine.State.Disabled);
        }
    }

    private void OnDestroy() {
        if (PlayerAvatarParent != null) {
            Destroy(PlayerAvatarParent);
        }
    }
}
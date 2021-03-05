using System.Linq;
using Photon.Pun;
using Photon.Voice.PUN;
using TowerTag;
using TowerTagSOES;
using UnityEngine;

[RequireComponent(typeof(IPlayer))]
public class PrefabFactory : MonoBehaviourPun
{
    [SerializeField] private GameObject _remoteClientPrefab;
    [SerializeField] private GameObject _localClientPrefabFps;
    [SerializeField] private GameObject _localClientPrefabVr;
    [SerializeField] private GameObject _localClientPrefabAi;

    private IPlayer _player;
    private bool _isLocalClient;

    private void Awake()
    {
        _player = GetComponent<IPlayer>();
    }

    private void Start()
    {
        _isLocalClient = photonView.IsMine;
        CreateClient(_isLocalClient);
        DontDestroyOnLoad(gameObject);
    }

    private GameObject GetPrefab(bool isLocalClient, ControllerType controllerType)
    {
        if (_player.IsBot
            && (SharedControllerType.IsAdmin || _player.OwnerID == PlayerManager.Instance.GetOwnPlayer()?.OwnerID))
            return _localClientPrefabAi;
        if (isLocalClient)
        {
            switch (controllerType)
            {
                case ControllerType.NormalFPS:
                    return _localClientPrefabFps;
                case ControllerType.VR:
                    return _localClientPrefabVr;
                default: return null;
            }
        }

        return _remoteClientPrefab;
    }

    private static string GetClientRole(bool isLocal)
    {
        return isLocal ? "local client" : "remote client";
    }

    // Builder Pattern: connect all subParts in own Functions
    private void CreateClient(bool isLocalClient)
    {
        if (_player.IsMe) {
            TeamID teamID = (TeamID) ConfigurationManager.Configuration.TeamID;
            _player.RequestTeamChange(teamID);
        }

        // Instantiate client Prefab
        GameObject prefab = GetPrefab(isLocalClient, SharedControllerType.Singleton);

        if (prefab == null)
        {
            Debug.LogError("Cannot initialize player: prefab is null");
            return;
        }

        InstantiateWrapper.InstantiateWithMessage(prefab, transform, false);

        // configure player related stuff
        ConfigurePlayerAvatar();

        // connect modules (SyncTransformSender & SyncTransformReceiver) for Transform Sync & Interpolation
        // Transform Syncs: Syncs Player Transforms (Head, Gun, ..)
        ConnectTransformSyncer(isLocalClient);

        // Connect InputController to GunController & Gun Visuals, Rope etc.
        ConnectGunControllerInput(isLocalClient);

        // connect visuals & Network Synchronisation for DamageModel (Health handling)
        InitPlayerHealth();
        // Connect Teleporter to Gun & Visuals, etc.
        InitializeTeleportVisualization(isLocalClient);
        // configure & connect Gun to external rumble Controller (external Arduino gun), only relevant for local Player
        ConfigureRumbleController();

        // Init Player and his subParts (TeleportHandler & PlayerStateHandler)
        _player.Init();
    }

    // configure player related stuff
    private void ConfigurePlayerAvatar()
    {
        var playerAvatar = gameObject.GetComponentInChildren<PlayerAvatar>();
        if (playerAvatar == null)
        {
            Debug.LogError("Failed to configure Player Avatar: Player Avatar not found");
            return;
        }

        // Connect PlayerAvatar Listener Functions
        _player.PlayerAvatar = playerAvatar;
        _player.GunCollider = _player.GameObject.CheckForNull()?.GetComponentsInChildren<DamageDetectorBase>()
            .Where(damageDetector => damageDetector.DetectorType == DamageDetectorBase.ColliderType.Weapon)
            .SelectMany(damageDetector => damageDetector.Collider)
            .ToArray();

        if (!_player.IsMe)
        {
            // Init & connect Name Badge over Remote Avatar
            var badge = gameObject.GetComponentInChildren<NameBadge>();
            if (badge != null) badge.Init(_player, _player.GameObject.CheckForNull()?.GetComponent<PhotonVoiceView>());
        }

        // create Player Avatar (remote presentation)
        playerAvatar.Init(_player, transform, GetComponentInChildren<AvatarMovement>());
    }

    // Connect Teleporter to Gun & Visuals, etc.
    private void InitializeTeleportVisualization(bool isLocalClient)
    {
        // configure for local & remote client
        var teleportMovement = gameObject.GetComponentInChildren<TeleportMovement>();
        if (teleportMovement == null)
        {
            Debug.LogError($"Failed to initialize teleporter for player {_player.PlayerName} (ID={_player.PlayerID})");
            return;
        }

        // set Player as owner on teleporter to receive TeleportTransform
        teleportMovement.Player = _player;

        if (isLocalClient)
        {
            if (!_player.IsBot)
            {
                var rotatePlaySpaceMovement = gameObject.GetComponentInChildren<RotatePlaySpaceMovement>();
                if (rotatePlaySpaceMovement == null)
                {
                    Debug.LogError(
                        $"Failed to initialize rotate playspace for player {_player.PlayerName} (ID={_player.PlayerID})");
                    return;
                }

                rotatePlaySpaceMovement.Player = _player;
            }

            var gunController = gameObject.GetComponentInChildren<GunController>();
            _player.TeleportHandler.GunController = gunController;
            _player.RotatePlayspaceHandler.GunController = gunController;

            // Switch ClaimVisuals of Pillar Neighbours at Teleport
            var claimVisuals = gameObject.GetComponentInChildren<ActivatePillarClaimVisualsByTeleport>();
            if (claimVisuals != null)
            {
                claimVisuals.TeleportMovement = teleportMovement;
            }
        }
    }

    // Connect InputController to GunController & Gun Visuals, Rope etc.
    private void ConnectGunControllerInput(bool isLocalClient)
    {

		var ropeIntersectionVisuals = gameObject.GetComponentInChildren<SimpleRopeIntersectionVisuals>();
		if (ropeIntersectionVisuals != null)
			ropeIntersectionVisuals.InitVisuals();

		// configure for local & remote client
		var chargerBeam = gameObject.GetComponentInChildren<IChargerBeamRenderer>(true);
        if (chargerBeam != null)
        {
            // Player to ChargerBeam -> RoperChargeBeamRenderTess
            _player.PlayerTeamChanged += chargerBeam.OnTeamChanged;
            chargerBeam.OnTeamChanged(_player, _player.TeamID);
            chargerBeam.SetOwner(_player);
        }
        else {
            Debug.LogError(
                "PrefabFactory_LCA.ConnectGunControllerInput: can't find IChargerBeamRenderer Component! -> " +
                GetClientRole(isLocalClient));
        }

        GetComponentInChildren<GrapplingHookController>()?.Init(_player);

        // configure Light bars (radial & length) on the Gun to change color to teamColors
        var lightBars = gameObject.GetComponentInChildren<LightBars>();
        if (lightBars != null)
        {
            lightBars.Init(_player);
            _player.PlayerTeamChanged += lightBars.OnTeamChanged;
        }
        else {
            Debug.Log("PrefabFactory_LCA.ConnectGunControllerInput: can't find LightBars Component!  -> " +
                      GetClientRole(isLocalClient));
        }

        // configure only for local client
        if (isLocalClient)
        {
            var inputController = gameObject.GetComponentInChildren<IInputController>();
            var gunController = gameObject.GetComponentInChildren<GunController>();
            if (inputController != null && gunController != null)
            {
                // connect Input handler to GunController
                inputController.GripPressed += gunController.OnGripPressed;
                inputController.GripReleased += gunController.OnGripReleased;
                inputController.TriggerPressed += gunController.OnTriggerPressed;
                inputController.TriggerReleased += gunController.OnTriggerReleased;
                // set Member for GunController
                gunController.Init(_player);

                // GunController to ChargerBeam
                if (chargerBeam != null)
                {
                    // ChargerBeam to GunController
                    // VR-Controller: let the rope decide if teleport was triggered
                    if (SharedControllerType.VR && !_player.IsBot)
                    {
                        chargerBeam.TeleportTriggered += gunController.OnTeleportTriggered;
                    }
                    // FPS- or AI-Controller let the input controller decide if teleport was triggered
                    else
                    {
                        inputController.TeleportTriggered += gunController.OnTeleportTriggered;
                    }

                    if (ropeIntersectionVisuals != null)
                        ropeIntersectionVisuals.Init(_player);

                    var sfx = gameObject.GetComponentInChildren<ChargerBeamSFX>();
                    if (sfx != null)
                    {
                        sfx.Init(chargerBeam, _player);
                    }
                    else
                    {
                        Debug.LogError("Cannot initialize ChargerBeamSFX: not found");
                    }
                }
                else
                {
                    Debug.LogError("Cannot initialize charger beam: not found");
                }

                if (SharedControllerType.VR && _player.IsMe)
                {
                    var chaperone = gameObject.GetComponentInChildren<Chaperone>();
                    if (chaperone != null)
                    {
                        _player.InitChaperone(chaperone);
                    }
                    else
                    {
                        Debug.LogError("Cannot find Chaperone Component of local VR client.");
                    }
                }
            }
        }

        // configure only for remote client
        if (!_player.IsMe)
        {
            var remoteHealthVisuals = gameObject.GetComponentInChildren<RemoteHealthVisuals>();
            if (remoteHealthVisuals != null)
            {
                _player.PlayerStateHandler.PlayerStateChanged += remoteHealthVisuals.OnSetActive;
            }
            else
            {
                Debug.LogError("Cannot initialize remote health visuals: not found");
            }
        }
    }

    // configure & connect Gun to external rumble Controller (external Arduino gun), only relevant for local Player
    private void ConfigureRumbleController()
    {
        if (SharedControllerType.VR && _player.IsMe)
        {
            var rumbleControllerWrapper = gameObject.GetComponentInChildren<RumbleControllerWrapper>();
            if (rumbleControllerWrapper == null)
            {
                Debug.LogError("Cannot initialize rumble controller: not found");
                return;
            }

            rumbleControllerWrapper.Init(_player);

            var gunController = gameObject.GetComponentInChildren<GunController>();
            if (gunController != null)
            {
                gunController.EnergyChanged += rumbleControllerWrapper.GunEnergyChanged;
                gunController.ShotTriggered += rumbleControllerWrapper.TriggerShot;
            }
            else
            {
                Debug.LogError("Cannot attach rumble controller to gun controller: not found");
            }

            var beamRenderer = gameObject.GetComponentInChildren<IChargerBeamRenderer>(true);
            if (beamRenderer != null)
            {
                beamRenderer.RollingOut += rumbleControllerWrapper.OnStartBeamRollOut;
                beamRenderer.RolledOut += rumbleControllerWrapper.OnFinishBeamRollOut;
            }
            else
            {
                Debug.LogError("Cannot attach rumble controller to beam renderer: not found");
            }

            var playerHealth = gameObject.GetComponentInChildren<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TookDamage += rumbleControllerWrapper.OnTookDamage;
            }
            else
            {
                Debug.LogError("Cannot attach rumble controller to player health: not found");
            }

            var rayCaster = gameObject.GetComponentInChildren<RayCaster>();
            if (rayCaster != null)
            {
                rayCaster.HighlightChanged += rumbleControllerWrapper.OnHighlighterChanged;
            }
            else
            {
                Debug.LogError("Cannot attach rumble controller to ray caster: not found");
            }
        }
    }

    private void InitPlayerHealth()
    {
        var playerHealth = GetComponentInChildren<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("Cannot initialize player health: not found");
            return;
        }

        // configure  for local & remote client
        playerHealth.Player = _player;

        playerHealth.RestoreMaxHealth();
        var healthHandler = GetComponent<HealthNetworkEventHandler>();

        // establish health synchronization
        healthHandler.PlayerHealth = playerHealth;

        // visualize health change
        var visuals = gameObject.GetComponentInChildren<IHealthVisuals>();
        playerHealth.RegisterEvents(visuals);
    }


    // connect modules (SyncTransformSender & SyncTransformReceiver) for Transform Sync & Interpolation
    private void ConnectTransformSyncer(bool isLocalClient)
    {
        var transformSyncer = GetComponent<SyncTransforms>();
        if (transformSyncer == null)
        {
            Debug.LogError("Cannot connect transform syncer: not found");
            return;
        }

        if (isLocalClient)
        {
            AbstractTransformSync sendSync;

            if (!_player.IsBot)
                sendSync = gameObject.GetComponentInChildren<SyncAndSendPlayerTransformsAndOthers>();
            else
                sendSync = gameObject.GetComponentInChildren<SyncTransformsSend>();
            if (sendSync == null)
            {
                Debug.LogError("Cannot initialize SyncTransformsSend: not found");
                return;
            }

            sendSync.TransformSync = transformSyncer;
        }
        else
        {
            var receiveSync = gameObject.GetComponentInChildren<SyncTransformsReceive>();
            if (receiveSync == null)
            {
                Debug.LogError("Cannot initialize SyncTransformsReceive: not found");
            }

            receiveSync.TransformSync = transformSyncer;
        }
    }

    private void OnDestroy()
    {
        // cleanup Listener by OnDestroy-Functions of the event classes
        _remoteClientPrefab = null;
        _localClientPrefabFps = null;
        _localClientPrefabVr = null;
        _localClientPrefabAi = null;
        _player = null;
    }
}
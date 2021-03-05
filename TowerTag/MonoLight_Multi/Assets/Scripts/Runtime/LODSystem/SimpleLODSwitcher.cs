using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Class to realize own LOD-System to switch whole GameObject hierarchies as LODLevels
/// (so we can enable/disable Lights/Particle Effects etc. and not only Renderer like Unity's build in LOD System).
/// One of these should be in every scene in which we want to use this system.
/// Uses custom SimpleLODGroup class. If no LODGroups set in lod groups array we search for all available
/// LODGroups in scene and register them (so be sure you set them right, if you have more than one SimpleLODSwitcher
/// Component in a scene, which should be avoided if not necessary).
/// </summary>
public class SimpleLODSwitcher : MonoBehaviour {
    private SimpleLODGroup[] _lodGroups;

    [SerializeField, Tooltip("Distances for LOD Levels ([10,20,30] describe distance bands from 0..10, 10..20 and 20..30)")]
    private float[] _distances;

    /// <summary>
    /// Should the Init function get called in Start?
    /// </summary>
    [FormerlySerializedAs("InitializeInStartFunction")] [SerializeField] private bool _initializeInStartFunction = true;

    /// <summary>
    /// Max number of LODLevels used in LODGroups (to draw LOD-Switch Buttons in custom Inspector),
    /// just for convenience in Editor -> to switch all groups to specific LOD Level in Inspector
    /// (see SwitchAllLODGroupsToLODLevel(int lodLevel))
    /// </summary>
    [FormerlySerializedAs("MaxLodLevels")] [SerializeField] private int _maxLodLevels = 3;

    public int MaxLodLevels => _maxLodLevels;

    /// <summary>
    /// Currently set LODLevel (for Highlighting in custom Inspector), just for convenience in
    /// Editor -> to switch all groups to specific LOD Level in Inspector
    /// (see SwitchAllLODGroupsToLODLevel(int lodLevel))
    /// </summary>
    public int CurrentLodLevel { get; private set; }

    private void Start() {
        if (_initializeInStartFunction)
            Init();
    }

    /// <summary>
    /// Initialize CullingGroup with Camera, Distance bands & Bounding spheres of our LODGroups (also searches for LODGroups in scene if none set).
    /// </summary>
    private void Init() {
        // Try to find LOD Groups in current scene
        GetLodGroupsFromScene();

        // if we could not find any LOD groups return
        if (_lodGroups == null || _lodGroups.Length == 0) {
            Debug.LogError("SimpleLODSwitcher.Init: Can't init -> no LOD Groups available (array is null)!");
            return;
        }

        if (SharedControllerType.IsAdmin || SharedControllerType.Spectator) {
            SwitchAllLodGroupsToLodLevel(1);
            return;
        }

        // cleanup event listener before we initialize new
        Cleanup();

        // if own player not found, cancel Init & deactivate LOD-System
        IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (ownPlayer == null) {
            Debug.LogError("SimpleLODSwitcher.Init: Can't find local Player! Deactivated LOD-Switcher!");
            Cleanup();
            return;
        }

        ownPlayer.TeleportHandler.PlayerTeleporting += OnPlayerTeleporting;
    }

    /// <summary>
    /// Listener function for local Player teleport Event
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="timeToTeleport"></param>
    private void OnPlayerTeleporting(TeleportHandler sender, Pillar origin, Pillar target, float timeToTeleport) {
        ChangeLodGroups(target);
    }

    /// <summary>
    /// update LOD Level on LODGroup if the distance changed
    /// </summary>
    /// <param name="playerPillar">Current local Player Pillar used to find out the Distance to other Pillar</param>
    private void ChangeLodGroups(Component playerPillar) {
        if (SharedControllerType.IsAdmin) {
            return;
        }

        if (_lodGroups == null || _lodGroups.Length == 0) {
            Debug.LogError("SimpleLODSwitcher.LODGroupStateChanged: Can't Update state -> no LOD Groups available (array is null)!");
            return;
        }

        foreach (var pillar in _lodGroups) {

            if (pillar == null)
                continue;

            if (pillar.GetComponentInParent<Pillar>() == playerPillar) {
                pillar.SwitchToLOD(0);
                continue;
            }

            var distance = (int)Vector3.Distance(playerPillar.transform.position, pillar.transform.position);
            var lod = 0;

            for (var i = 0; i < _distances.Length; i++) {
                if (i == 0)
                    continue;
                if (distance >= _distances[i - 1] && distance < _distances[i])
                    lod = i;
            }

            if (lod != pillar.CurrentLODLevel)
                pillar.GetComponentInChildren<SimpleLODGroup>().SwitchToLOD(lod);
        }
    }

    /// <summary>
    /// Switch all registered LODGroups to a certain LOD Level (just for convenience in Editor -> for baking etc.)
    /// </summary>
    /// <param name="lodLevel">LOD Level to switch registered LODGroups to.</param>
    public void SwitchAllLodGroupsToLodLevel(int lodLevel) {
        GetLodGroupsFromScene();
        if (_lodGroups == null || _lodGroups.Length == 0) {
            Debug.LogError("SimpleLODSwitcher.SwitchAllLODGroupsToLODLevel: Can't Update state -> no LOD Groups available (array is null)!");
            return;
        }

        if (lodLevel < 0 || lodLevel >= _maxLodLevels)
            return;
        CurrentLodLevel = lodLevel;

        foreach (SimpleLODGroup pillar in _lodGroups) {
            if (pillar != null)
                pillar.SwitchToLOD(lodLevel);
        }
    }

    /// <summary>
    /// Find LODGroups (objects of type SimpleLODGroup) in scene and register them.
    /// </summary>
    private void GetLodGroupsFromScene() {
        _lodGroups = FindObjectsOfType<SimpleLODGroup>();
    }

    /// <summary>
    /// Cleanup (dispose CullingGroup)
    /// </summary>
    private void Cleanup() {
        if (SharedControllerType.IsAdmin)
            return;

        IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (ownPlayer != null)
            ownPlayer.TeleportHandler.PlayerTeleporting -= OnPlayerTeleporting;
    }

    private void OnDestroy() {
        Cleanup();
    }
}

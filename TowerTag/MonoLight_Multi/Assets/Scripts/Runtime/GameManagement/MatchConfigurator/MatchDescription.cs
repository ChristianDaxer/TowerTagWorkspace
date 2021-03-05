using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
#if !UNITY_ANDROID
using Valve.VR.InteractionSystem;
#endif

[CreateAssetMenu(fileName = "MatchDescription", menuName = "TowerTag/MatchDescription")]
public class MatchDescription : ScriptableObject, ISerializationCallbackReceiver {
    /// <summary>
    /// MatchID used for Serialization
    /// </summary>
    [SerializeField] [FormerlySerializedAs("matchID")]
    private int _matchID;

    // Admin UI Infos
    [SerializeField] [FormerlySerializedAs("mapName")] [Tooltip("Name to show in the AdminUI.")]
    private string _mapName;

    // Match values
    [SerializeField] [EnumFlag] [Tooltip("Game mode to load for this Match.")]
    private GameMode _gameMode;

    // Scene to load
    [HideInInspector] [SerializeField] [FormerlySerializedAs("sceneName")] [Tooltip("Scene to load with for this Match.")]
    private string _sceneName;

#if UNITY_EDITOR
    [SerializeField] [Tooltip("Scene asset to serialize into a string.")]
    private UnityEditor.SceneAsset _sceneAsset;
#endif

    [SerializeField] [FormerlySerializedAs("minPlayers")] [Tooltip("Number of min Players for this Match.")]
    private int _minPlayers;

    public int MinPlayers => _minPlayers;

    // to filter in UI & check if enough/to much player/teams in Game
    [SerializeField] [FormerlySerializedAs("matchUp")] [Tooltip("Number of Teams and Players allowed for this Match.")]
    private MatchUp _matchUp;

    [SerializeField] [Tooltip("The Prefab for the Mission Briefing with Stereo Map and Pins")]
    private GameObject _missionBriefingPrefab;

    [SerializeField] [Tooltip("The Sprite that is displayed in Create Match Menu")]
    private Sprite _mapScreenshot;

    public int MatchID {
        get => _matchID;
        set {
            _matchID = value;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }

    public string MapName => _mapName;
    public GameMode GameMode => _gameMode;
    public string SceneName => _sceneName;
    public MatchUp MatchUp => _matchUp;
    public GameObject MissionBriefingPrefab => _missionBriefingPrefab;

    public Sprite MapScreenshot => _mapScreenshot;

    private void SerializeSceneAsset () { 
#if UNITY_EDITOR
        if (_sceneAsset != null)
            _sceneName = _sceneAsset.name;
#endif
    }

    private void OnValidate() => SerializeSceneAsset();
    public void OnBeforeSerialize() => SerializeSceneAsset();
    public void OnAfterDeserialize() {}
}
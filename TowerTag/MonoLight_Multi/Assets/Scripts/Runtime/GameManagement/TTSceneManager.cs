using System.Collections.Generic;
using GameManagement;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

#if !UNITY_ANDROID
using Valve.VR;
#endif

public class TTSceneManager : TTSingleton<TTSceneManager> {
#if UNITY_EDITOR
    [SerializeField] private SceneAsset _licensingSceneAsset;
    [SerializeField] private SceneAsset _pillarOffsetSceneAsset;
    [SerializeField] private SceneAsset _commendationsSceneAsset;
    [SerializeField] private SceneAsset _offboardingSceneAsset;
    [SerializeField] private SceneAsset _connectSceneAsset;
    [SerializeField] private SceneAsset _hubSceneAsset;
    [SerializeField] private SceneAsset _tutorialSceneAsset;

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        _licensingScene = _licensingSceneAsset.name;
        _pillarOffsetScene = _pillarOffsetSceneAsset.name;
        _commendationsScene = _commendationsSceneAsset.name;
        _offboardingScene = _offboardingSceneAsset.name;
        _connectScene = _connectSceneAsset.name;
        _currentHubScene = _hubSceneAsset.name;
        _tutorialScene = _tutorialSceneAsset.name;
        EditorUtility.SetDirty(this);
    }

#endif

    [HideInInspector][SerializeField] private string _licensingScene;
    public string LicensingScene => _licensingScene;

    [HideInInspector][SerializeField] private string _pillarOffsetScene;
    public string PillarOffsetScene => _pillarOffsetScene;

    [HideInInspector][SerializeField] private string _commendationsScene;

    public string CommendationsScene => _commendationsScene;

    [HideInInspector][SerializeField] private string _offboardingScene;

    public string OffboardingScene => _offboardingScene;

    [HideInInspector][SerializeField] private string _connectScene;
    public string ConnectScene => _connectScene;

    [HideInInspector][SerializeField] private string _currentHubScene;
    public string CurrentHubScene => _currentHubScene;

    [HideInInspector][SerializeField] private string _tutorialScene;
    public string TutorialScene => _tutorialScene;

    public string PreviousScene { get; private set; }

    public event SceneLoadedHandler ConnectSceneLoaded;
    public event SceneLoadedHandler HubSceneLoaded;
    public event SceneLoadedHandler CommendationSceneLoaded;
    public event SceneLoadedHandler OffboardingSceneLoaded;
    public event SceneLoadedHandler PillarOffsetSceneLoaded;

    // finished Loading scene but blocking new scene loads by SteamVR_LoadLevel (Fade-in phase)
    private bool _isBlocked = false;
    private readonly Queue<string> _loadLevelQueue = new Queue<string>();

    public bool IsInHubScene => SceneManager.GetActiveScene().name == _currentHubScene;
    public bool IsInCommendationsScene =>
        SceneManager.GetActiveScene().name == _commendationsScene;

    public bool IsInConnectScene => SceneManager.GetActiveScene().name == _connectScene;
    public bool IsInLicensingScene => SceneManager.GetActiveScene().name == _licensingScene;
    public bool IsInTutorialScene => SceneManager.GetActiveScene().name == _tutorialScene;
    public bool IsInOffboardingScene => SceneManager.GetActiveScene().name == _offboardingScene;

    public bool IsInPillarOffsetScene =>
        SceneManager.GetActiveScene().name == _pillarOffsetScene;

    public bool ShowOffboardingInstructions { get; private set; }

    private void Start () {
        SceneManager.sceneLoaded += NewSceneWasLoaded;
        if (PlatformSceneManagementInterface.GetInstance(out var sceneManagement))
            sceneManagement.RegisterDelegate(OnLoading);
    }

    /* SEAN CONNOR not sure why OnEnable/OnDisable is being used for initialization for Start so I've refactored
     * OnEnable to Start.
    private void OnDisable() {
        SceneManager.sceneLoaded -= NewSceneWasLoaded;
        if (PlatformSceneManagementInterface.GetInstance(out var sceneManagement))
            sceneManagement.UnregisterDelegate(OnLoading);
    }
    */

    public void LoadScene(string sceneName) {
        if (_isBlocked) {
            Debug.LogWarning($"------------------------------Loading scene while blocked: {sceneName}");
            _loadLevelQueue.Enqueue(sceneName);
            return;
        }


        Debug.Log($"----------------------------Loading scene: {sceneName}");
        _isBlocked = true;
        PreviousScene = SceneManager.GetActiveScene().name;
        PhotonNetwork.PrepareLoadLevel(sceneName);
        if (PlatformSceneManagementInterface.GetInstance(out var sceneManagement))
            sceneManagement.BeginSceneLoading(sceneName);
    }

    private void NewSceneWasLoaded(Scene newScene, LoadSceneMode mode) {
        Debug.Log($"Loaded new scene: {newScene.name}");

        if (newScene.name.Equals(ConnectScene))
            ConnectSceneLoaded?.Invoke();
        else if (newScene.name.Equals(CurrentHubScene))
            HubSceneLoaded?.Invoke();
        else if (newScene.name.Equals(CommendationsScene))
            CommendationSceneLoaded?.Invoke();
        else if (newScene.name.Equals(PillarOffsetScene))
            PillarOffsetSceneLoaded?.Invoke();
        else if (newScene.name.EndsWith(OffboardingScene))
            OffboardingSceneLoaded?.Invoke();
    }

    // SteamVR_LoadLevel Listener -> is called when SteamVR_LevelLoad is triggered or finished
    private void OnLoading(bool loading) {
        _isBlocked = loading;

        if (!loading) {
            if (_loadLevelQueue.Count > 0) {
                LoadScene(_loadLevelQueue.Dequeue());
            }
        }
    }

    public void LoadConnectScene(bool showOffboardingInstructions) {
        Debug.Log($"Loading     connect scene {showOffboardingInstructions}");
        ShowOffboardingInstructions = showOffboardingInstructions;
        LoadScene(_connectScene);
    }

    public void LoadHubScene() {
        LoadScene(_currentHubScene);
    }

    public void LoadLicensingScene() {
        LoadScene(_licensingScene);
    }

    public void LoadPillarOffsetScene() {
        LoadScene(_pillarOffsetScene);
    }

    public void LoadOffboardingScene() {
        LoadScene(_offboardingScene);
    }

    public void LoadTutorialScene() {
        LoadScene(_tutorialScene);
    }

    protected override void Init()
    {
    }
}
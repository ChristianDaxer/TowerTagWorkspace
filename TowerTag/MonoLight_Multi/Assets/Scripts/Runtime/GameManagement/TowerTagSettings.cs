using SOEventSystem.Shared;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;

#endif

/// <summary>
/// Scriptable Object to collect Tower Tag relevant Settings. Create an Instance of "TowerTagSettings"
/// </summary>
[CreateAssetMenu(menuName = "TowerTag/TowerTagSettings", fileName = "TowerTagSettings")]
public class TowerTagSettings : ScriptableObject {

    private static TowerTagSettings _singleton;
    public static TowerTagSettings Singleton {
        get { 
            if (_singleton == null) {
                var towerTagSettingInstances = Resources.FindObjectsOfTypeAll<TowerTagSettings>();
                if (towerTagSettingInstances.Length == 0) {
                    Debug.LogErrorFormat("Unable to find instance of {0} in Resources.", nameof(TowerTagSettings));
                    return null;
                }

                if (towerTagSettingInstances.Length > 1) { 
                    Debug.LogErrorFormat("There is more than one instance of: {0}, use {1} if you want to use more than one instance of {0}.", nameof(TowerTagSettings), nameof(ScriptableObjectRegistry));
                    return null;
                }

                _singleton = towerTagSettingInstances[0];
            }

            return _singleton;
        }
    }

    public enum HomeSpaceModes
    {
        Upright,
        Sitdown,
    }
    
    [SerializeField, HideInInspector] private bool _basicMode;
    [SerializeField, HideInInspector] private bool _hologate;
    [SerializeField, HideInInspector] private bool _steamEditorId;
    [SerializeField, HideInInspector] private bool _home;
    [SerializeField, HideInInspector] private HomeTypes _homeType;
    [SerializeField] private int _maxTeamSize;
    [SerializeField] private int _maxSpectatorCount = 1;
    [SerializeField] private int _basicModeMatchTime = 360;
    [SerializeField] private int _basicModeStartMatchCountdownTime = 20;
    [SerializeField] private int _trainingModeStartMatchCountdownTime = 30;

    private PlatformServices _platformServices = null;
    private bool _platformSet = false;

    public static bool PlatformSet => Singleton._platformSet;
    public static int MaxTeamSize => Singleton._maxTeamSize;
    public static int MaxUsersPerRoom => Singleton._basicMode
        ? 2 * Singleton._maxTeamSize + Singleton._maxSpectatorCount
        : 2 * Singleton._maxTeamSize + Singleton._maxSpectatorCount + 1;

    public static int MaxPlayers => 2 * Singleton._maxTeamSize;

    private void Awake () => _singleton = this;

    public static bool BasicMode {
        get => Singleton._basicMode;
        set {
            if (Singleton._basicMode == value) return;
            Singleton._basicMode = value;
#if UNITY_EDITOR
            EditorUtility.SetDirty(Singleton);
#endif
        }
    }

    public static HomeTypes HomeType
    {
        get => Singleton._homeType;
        set {
            if (Singleton._homeType == value) return;
            Singleton._homeType = value;
#if UNITY_EDITOR
            EditorUtility.SetDirty(Singleton);
#endif
        }
    }

    public static bool IsHomeTypeSteam => HomeType == HomeTypes.SteamVR;
    public static bool IsHomeTypeViveport => HomeType == HomeTypes.Viveport;

    public static bool IsHomeTypeOculus => HomeType == HomeTypes.Oculus;

    public static int MaxSpectatorCount => Singleton._maxSpectatorCount;

    public static bool Home {
        get => Singleton._home;
        set {
            if (Singleton._home == value) return;
            Singleton._home = value;
#if UNITY_EDITOR
            EditorUtility.SetDirty(Singleton);
#endif
        }
    }

    public static int BasicModeMatchTime => Singleton._basicModeMatchTime;
    public static bool Hologate {
        get => Singleton._hologate;
        set => Singleton._hologate = value;
    }

    public static bool SteamEditorId
    {
        get => Singleton._steamEditorId;
        set {
            if (Singleton._steamEditorId == value) return;
            Singleton._steamEditorId = value;
#if UNITY_EDITOR
            EditorUtility.SetDirty(Singleton);
#endif
        }
    }
    public static int BasicModeStartMatchCountdownTime => Singleton._basicModeStartMatchCountdownTime;
    public static int TrainingModeStartMatchCountdownTime => Singleton._trainingModeStartMatchCountdownTime;
    public static ILeaderboardManager LeaderboardManager => Singleton._platformServices.LeaderboardManager;
    public static IVRChaperone Chaperone => Singleton._platformServices.Chaperone;

    public static void SetPlatformServices(PlatformServices platformServices)
    {
        if (platformServices != null) {
            Singleton._platformServices = platformServices;
            Singleton._platformSet = true;
        }
    }
}
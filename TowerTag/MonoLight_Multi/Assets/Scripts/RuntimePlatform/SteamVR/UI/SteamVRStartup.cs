using Valve.VR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameManagement;
using TowerTag;
using TowerTagSOES;
using Viveport;
using UI;

public class SteamVRStartup : StartUp
{
    [SerializeField] private SteamManager _steamManagerPrefab;
    [SerializeField] private ViveportSDKManager _viveportSdkManager;

    private SteamVR_LoadLevel _loader;
    private bool _steamVRReady;

    public override void OnInit()
    {
        PlatformServices platform = null;
        //TODO: separate viveport init from SteamVR with its own assembly
        switch (TowerTagSettings.HomeType)
        {
            case HomeTypes.SteamVR:
                SteamVRPlatformServices.GetInstance(out platform);
                break;
            case HomeTypes.Viveport:
                ViveportPlatformServices.GetInstance(out platform);
                break;
        }
        if (platform != null)
            TowerTagSettings.SetPlatformServices(platform);
        if (SharedControllerType.VR)
        {
            SteamVR_Events.Initialized.AddListener(OnSteamVRInitialized);
            SteamVR.Initialize(true);
        }
    }

    public override void OnLoadedSceneHome()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefKeys.Tutorial) || PlayerPrefs.GetInt(PlayerPrefKeys.Tutorial) == 0)
            StartCoroutine(StartTutorial());
        else {
            StartCoroutine(LoadLevel(TTSceneManager.Instance.ConnectScene));
        }
    }

    private void OnSteamVRInitialized(bool active)
    {
        _steamVRReady = active;
    }

    public override void OnInitHome()
    {
        if (TowerTagSettings.IsHomeTypeSteam && SharedControllerType.IsPlayer)
            AchievementManager.Init(new SteamAchievementManager());

    }

    public override IEnumerator OnPlatformLoadLevel(string sceneName)
    {
        System.Random rnd = new System.Random();

        _loader = SteamVR_LoadLevel.Loader;
        _loader.levelName = sceneName;
        _loader.loadingScreen = _gameTips[rnd.Next(_gameTips.Length)];
        _loader.Trigger();

        while (SteamVR_LoadLevel.progress < 1f)
            yield return null;
    }

    public override IEnumerator PlatformLoadNextScene()
    {
        yield return new WaitUntil(() => GameInitialization.Initialized
                                         && (_steamVRReady || !SharedControllerType.VR));
    }
}

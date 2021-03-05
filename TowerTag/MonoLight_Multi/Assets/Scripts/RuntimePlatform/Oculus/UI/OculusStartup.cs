using System.Collections;
using UI;
using UnityEngine;
using GameManagement;
using ExitGames.Client.Photon;

public class OculusStartup : StartUp
{
    private OculusVR_SceneLoader _loader;

#if !ENABLE_IL2CPP
    SocketUdpAsync test = new SocketUdpAsync(null); //Here to prevent this class to be stripped from the project when building with Mono.
#endif

    public override void OnInit()
    {
        if (OculusPlatformServices.GetInstance(out var platform)) {
            TowerTagSettings.SetPlatformServices(platform);
        }
        else {
            StartCoroutine(WaitForPlatformServices());
        }

        _loader = OculusVR_SceneLoader.Instance;
        
        // Setup foveated rendering
        SetupFoveatedRendering();
    }

    public override void OnInitHome()
    {
    }

    public override void OnLoadedSceneHome()
    {
        StartCoroutine(LoadLevel(TTSceneManager.Instance.ConnectScene)); //TODO setup tutorial
    }

    public override IEnumerator OnPlatformLoadLevel(string sceneName)
    {
        /* _loader.levelName = sceneName;
         _loader.Trigger();*/
        OculusVR_SceneLoader.Begin(sceneName);


        while (OculusVR_SceneLoader.Progress < 1f)
            yield return null;
    }

    public override IEnumerator PlatformLoadNextScene()
    {
        yield return new WaitUntil(() => GameInitialization.Initialized
                                        && OVRManager.OVRManagerinitialized);
    }

    public void SetupFoveatedRendering()
    {
        // Setup foveated rendering based on configuration
        switch (ConfigurationManager.Configuration.FoveatedRenderingLevel)
        {
            case 1:
                OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.Low;
                OVRManager.useDynamicFixedFoveatedRendering = true;
                break;
            case 2:
                OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.Medium;
                OVRManager.useDynamicFixedFoveatedRendering = true;
                break;
            case 3:
                OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.High;
                OVRManager.useDynamicFixedFoveatedRendering = true;
                break;
            default:
                OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.Off;
                OVRManager.useDynamicFixedFoveatedRendering = false;
                break;
        }
    }

    private IEnumerator WaitForPlatformServices() {
        PlatformServices platformServices;

        while(!OculusPlatformServices.GetInstance(out platformServices)) {
            yield return new WaitForEndOfFrame();
        }

        TowerTagSettings.SetPlatformServices(platformServices);
    }
}

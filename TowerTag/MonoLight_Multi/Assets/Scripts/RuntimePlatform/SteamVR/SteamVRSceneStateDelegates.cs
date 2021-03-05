using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class SteamVRSceneStateDelegates : PlatformSceneManagementInterface
{
    public override void BeginSceneLoading(string sceneName)
    {
        SteamVR_LoadLevel.Begin(sceneName);
    }

    public override void RegisterDelegate(UnityAction<bool> loadingCallback)
    {
        SteamVR_Events.Loading.AddListener(loadingCallback);
    }

    public override void UnregisterDelegate(UnityAction<bool> loadingCallback)
    {
        SteamVR_Events.Loading.RemoveListener(loadingCallback);
    }
}

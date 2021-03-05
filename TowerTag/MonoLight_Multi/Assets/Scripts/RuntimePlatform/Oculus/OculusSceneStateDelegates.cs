using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class OculusSceneStateDelegates : PlatformSceneManagementInterface
{
    protected override void Init() {}

    public override void BeginSceneLoading(string sceneName)
    {
        OculusVR_SceneLoader.Begin(sceneName);
    }

    public override void RegisterDelegate(UnityAction<bool> loadingCallback)
    {
        OculusVR_SceneLoader.Loading.AddListener(loadingCallback);
    }

    public override void UnregisterDelegate(UnityAction<bool> loadingCallback)
    {
        OculusVR_SceneLoader.Loading.RemoveListener(loadingCallback);
    }
}

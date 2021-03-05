using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class PlatformSceneManagementInterface : TTSingleton<PlatformSceneManagementInterface>
{
    protected override void Init() {}
    public abstract void RegisterDelegate(UnityAction<bool> loadingCallback);
    public abstract void UnregisterDelegate(UnityAction<bool> loadingCallback);

    public abstract void BeginSceneLoading(string sceneName);
}

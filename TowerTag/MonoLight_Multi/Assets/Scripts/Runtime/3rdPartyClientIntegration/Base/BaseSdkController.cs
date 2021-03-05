using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseSdkController : TTSingleton<BaseSdkController>
{
    protected abstract void OnAwake();
    protected override void Init() 
    {
        OnAwake();
    }

    protected abstract void OnHubSceneLoaded();
    protected abstract void OnStart();
    private void Start()
    {
        OnStart();
    }
}

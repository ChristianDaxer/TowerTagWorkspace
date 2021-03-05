using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PlatformSceneReconfigureTaskWrapper : PlatformReconfigureTaskScriptableObject, IPlatformReconfigureTask, IPlatformReconfigureTaskDescriptor {

    public string TaskDescription => _platformSceneReconfigureTask.SceneTaskDescription;

    private PlatformSceneReconfigureTaskScriptableObject _platformSceneReconfigureTask;
    private SceneWrapper _sceneWrapper;
    public bool ValidSceneAsset => _sceneWrapper.GetStagedSceneAsset() != null;
    public bool ValidPlatformSceneReconfigureTask => _platformSceneReconfigureTask != null;

    public IEnumerator Reconfigure(HomeTypes homeType, Action<IPlatformReconfigureTaskDescriptor> startTaskCallback, Action<bool> completedTaskCallback)
    {
        if (startTaskCallback != null)
            startTaskCallback(this);

        yield return null;

        yield return _platformSceneReconfigureTask.ReconfigureScene(homeType, _sceneWrapper, startTaskCallback, completedTaskCallback);
    }

    public void Setup (PlatformSceneReconfigureTaskScriptableObject platformSceneReconfigureTask, SceneWrapper sceneWrapper)
    {
        _platformSceneReconfigureTask = platformSceneReconfigureTask;
        _sceneWrapper = sceneWrapper;
    }
}

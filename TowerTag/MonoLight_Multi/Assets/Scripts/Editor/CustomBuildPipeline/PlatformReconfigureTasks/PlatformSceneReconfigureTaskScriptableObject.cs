using System;
using System.Collections;
using UnityEditor;

public abstract class PlatformSceneReconfigureTaskScriptableObject : PlatformReconfigureTaskScriptableObject {
    public abstract string SceneTaskDescription { get; }
    public abstract IEnumerator ReconfigureScene(HomeTypes homeType, SceneWrapper sceneWrapper, System.Action<IPlatformReconfigureTaskDescriptor> startTaskCallback, System.Action<bool> taskCallback);
}

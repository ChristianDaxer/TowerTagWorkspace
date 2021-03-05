using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AsyncLoadPlatformSystems : MonoBehaviour
{
    public string oculusAndroidSceneName;
    public string steamvrSceneName;

    public delegate void OnSceneLoadedDelegate(string sceneName);
    public OnSceneLoadedDelegate onSceneLoadedDel;

    private string loadedSceneName;

    private void Awake()
    {
        string sceneName = null;
#if !UNITY_ANDROID
        sceneName = steamvrSceneName;
#else
        sceneName = oculusAndroidSceneName;
#endif
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadedSceneName = sceneName;
    }

    private IEnumerator WaitTillSceneIsLoaded (AsyncOperation asyncOp)
    {
        while (asyncOp.isDone)
            yield return null;

        if (onSceneLoadedDel != null)
            onSceneLoadedDel(loadedSceneName);
    }
}

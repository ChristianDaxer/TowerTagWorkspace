using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SingletonInstanceRegistory
{
    private static readonly Dictionary<System.Type, MonoBehaviour> cachedSingletons = new Dictionary<System.Type, MonoBehaviour>();

    public static void AddToCache (System.Type type, MonoBehaviour singletonMonobehaviour)
    {
        if (cachedSingletons.ContainsKey(type))
            return;
        cachedSingletons.Add(type, singletonMonobehaviour);
        Debug.LogFormat("Registered instance of: {0} attached to GameObject: \"{1}\" in singleton registry.", type.FullName, singletonMonobehaviour.gameObject);
    }

    public static void RemoveFromCache (System.Type type)
    {
        if (cachedSingletons.ContainsKey(type))
        {
            Debug.LogFormat("Instance of: {0} destroyed, unregistering singleton.", type.FullName);
            cachedSingletons.Remove(type);
            return;
        }

        Debug.LogErrorFormat("Attempted to remove instance of: {0}. However, it was either never registered, or it was already unregistered.", type.FullName);
    }

    public static bool SearchForInstanceInRegistory<T> (out T instance) where T : TTSingleton<T>
    {
        instance = null;

        if (cachedSingletons.TryGetValue(typeof(T), out var monoBehaviour))
        {
            if (monoBehaviour == null)
            {
                Debug.LogFormat("Found instance of: {0} was destroyed.", typeof(T).FullName);
                return false;
            }

            Debug.LogFormat("Found instance of: {0} attached to GameObject: \"{1}\" in the singleton registory.", typeof(T).FullName, monoBehaviour.gameObject.name);
            instance = monoBehaviour as T;
            return true;
        }

        if (Application.isPlaying)
            Debug.LogErrorFormat("There are no instances of type: {0} in the scene. The singleton registory cache only populates upon the first call, on SceneManager.sceneLoaded or SceneManager.sceneUnloaded. Therefore, in order to access a valid instance the \"GenericSingleton<T>.GetInstance(out T instance)\" must be called after \"MonoBehaviour.Awake()\".", typeof(T).FullName);
        return false;
    }
}

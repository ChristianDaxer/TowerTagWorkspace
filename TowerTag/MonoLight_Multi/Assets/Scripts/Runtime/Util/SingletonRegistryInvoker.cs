using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
public static class SingletonRegistryInvokerEditor
{
    [MenuItem("Unity/Create Singleton Registory Invoker")]
    public static void Create ()
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            Scene scene = EditorSceneManager.GetSceneAt(i);
            GameObject go = new GameObject("SingletonRegistoryInvoker");
            go.AddComponent<SingletonRegistryInvoker>();
            SceneManager.MoveGameObjectToScene(go, scene);
        }
    }
}
#endif

/// <summary>
/// If any instance of this class exists in a scene, it's Awake() will be called first and it'll
/// collect ONLY the classes that derrive from GenericSingleton<T> in the scene this the GameObject
/// this instance is attached to and register those in the singleton registry. This allows classes to 
/// reference each other early in awake without issues.
/// </summary>
[DefaultExecutionOrder(-999999999)]
public class SingletonRegistryInvoker : MonoBehaviour
{
    private void Awake()
    {
        GameObject[] sceneRootGameObjects = gameObject.scene.GetRootGameObjects();
        List<MonoBehaviour> singletons = new List<MonoBehaviour>();

        System.Type genericType = typeof(TTSingleton<>);
        for (int i = 0; i < sceneRootGameObjects.Length; i++)
        {
            MonoBehaviour[] monoBehaviours = sceneRootGameObjects[i].GetComponentsInChildren<MonoBehaviour>();
            if (monoBehaviours.Length == 0)
                continue;

            for (int mi = 0; mi < monoBehaviours.Length; mi++)
            {
                System.Type type = monoBehaviours[mi].GetType();

                if (!type.BaseType.IsGenericType || type.BaseType.GetGenericTypeDefinition().IsAssignableFrom(genericType))
                    continue;

                SingletonInstanceRegistory.AddToCache(type, monoBehaviours[mi]);
            }

        }

        Destroy(gameObject);
    }
}

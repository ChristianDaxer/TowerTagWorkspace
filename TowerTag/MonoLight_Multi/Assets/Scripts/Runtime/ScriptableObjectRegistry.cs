using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class ScriptableObjectRegistry : TTSingleton<ScriptableObjectRegistry>
{
    private readonly Dictionary<System.Type, ScriptableObject> registeredScriptableObjects = new Dictionary<System.Type, ScriptableObject>();
    [SerializeField] private ScriptableObject[] serializedScriptableObjects;

    protected override void Init()
    {
        if (serializedScriptableObjects != null && serializedScriptableObjects.Length > 0) {
            for (int i = 0; i < serializedScriptableObjects.Length; i++)
            {
                System.Type scriptableObjectType = serializedScriptableObjects[i].GetType();
                if (registeredScriptableObjects.ContainsKey(scriptableObjectType)) {
                    Debug.LogErrorFormat("Unable to register multiple instances of {0} in {1}", scriptableObjectType.FullName, nameof(ScriptableObjectRegistry));
                    continue;
                }

                registeredScriptableObjects.Add(scriptableObjectType, serializedScriptableObjects[i]);
                Debug.LogFormat("Reigstered singleton instance of {0} in {1}.", scriptableObjectType.FullName, nameof(ScriptableObjectRegistry));
            }

            return;
        }

        Debug.LogErrorFormat("No referenced {0} assigned to instance of: {1} attached to GameObject: \"{2}\".", nameof(ScriptableObject), nameof(ScriptableObjectRegistry), gameObject.name);
    }

    public bool TryGet<T> (out T scriptableObjectInstance) where T : ScriptableObject {

        scriptableObjectInstance = null;
        System.Type type = typeof(T);

        var success = registeredScriptableObjects.TryGetValue(type, out var instance);
        if (!success) {
            Debug.LogErrorFormat("No instance of type: {0} was registered in {1}.", type.FullName, nameof(ScriptableObjectRegistry));
            return false;
        }

        if (instance == null) { 
            Debug.LogErrorFormat("Object was registered as type: {0} in {1}. However, the registered Object is null.", type.FullName, nameof(ScriptableObjectRegistry));
            return false;
        }

        scriptableObjectInstance = (T)instance; 
        if (scriptableObjectInstance == null) {
            Debug.LogErrorFormat("Object was registered as type: {0} in {1}. However, the registered Object is type of: {2}", type.FullName, nameof(ScriptableObjectRegistry), instance.GetType().FullName);
            return false;
        }

        return success;
    }
}

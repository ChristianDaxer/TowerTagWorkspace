using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObjectSingleton<T> {
    private static T _singleton;

    private static T FindSingleton ()
    {
        Debug.LogFormat("Unable to find singleton of type: {0}, attempting {1} search.", typeof(T).FullName, nameof(Resources));

        T[] singletons = Resources.FindObjectsOfTypeAll<T>();

        if (singletons.Length > 1) {

            string homeTypeStr = string.Format("_{0}", TowerTagSettings.HomeType.ToString().ToLower());
            _singleton = singletons.FirstOrDefault(instance => instance.name.ToLower().EndsWith(homeTypeStr));

            if (_singleton == null) {
                _singleton = singletons.FirstOrDefault();

                if (_singleton == null) { 
                    Debug.LogErrorFormat("Unable to find instance of: {0} in {1}.", typeof(T).FullName, nameof(Resources));
                    return null;
                }
            }

            return _singleton;
        }

        return singletons.FirstOrDefault();
    }

    public static T Singleton {
        get {
            if (_singleton == null) {

                if (!ScriptableObjectRegistry.GetInstance(out var registry)) { 
                    if (!Application.isPlaying)
                        _singleton = FindSingleton();
                    return _singleton;
                }

                if (!registry.TryGet<T>(out var instance)) { 
                    if (!Application.isPlaying)
                        _singleton = FindSingleton();
                    return _singleton;
                }

                _singleton = instance;
            }

            return _singleton;
        }
    }
}

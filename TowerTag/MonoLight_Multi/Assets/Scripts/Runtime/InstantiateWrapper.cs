using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InstantiateWrapper
{
    public static T InstantiateWithMessage<T>(T original) where T : Object {
        #if VERBOSE_INSTANTIATION
        Debug.LogFormat("Instantiating object: \"{0}\".", original);
        #endif
        return Object.Instantiate<T>(original);
    }

    public static T InstantiateWithMessage<T>(T original, Transform parent) where T : Object {
        #if VERBOSE_INSTANTIATION
        Debug.LogFormat("Instantiating object: \"{0}\" under parent: \"{1}\".", original, parent.name);
        #endif
        return Object.Instantiate<T>(original, parent);
    }
    public static T InstantiateWithMessage<T>(T original, Transform parent, bool instantiateInWorldSpace) where T : Object {
        #if VERBOSE_INSTANTIATION
        Debug.LogFormat("Instantiating object: \"{0}\" under parent: \"{1}\" in world space: {2}.", original, parent.name, instantiateInWorldSpace);
        #endif
        return Object.Instantiate<T>(original, parent, instantiateInWorldSpace);
    }
    public static T InstantiateWithMessage<T>(T original, Vector3 position, Quaternion rotation) where T : Object {
        #if VERBOSE_INSTANTIATION
        Debug.LogFormat("Instantiating object: \"{0}\" at position: ({1}) and rotation: ({2}).", position, rotation);
        #endif
        return Object.Instantiate<T>(original, position, rotation);
    }
    public static Object InstantiateWithMessage<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object {
        #if VERBOSE_INSTANTIATION
        Debug.LogFormat("Instantiating object: \"{0}\" at position: ({1}) and rotation: ({2}) under parent: \"{3}\".", position, rotation, parent.name);
        #endif
        return Object.Instantiate<T>(original, position, rotation, parent);
    }
}

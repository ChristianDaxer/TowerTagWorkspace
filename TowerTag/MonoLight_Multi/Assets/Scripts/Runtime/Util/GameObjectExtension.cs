using JetBrains.Annotations;
using UnityEngine;

public static class GameObjectExtension {
    [CanBeNull]
    public static GameObject CheckForNull(this GameObject go) {
        return go == null ? null : go;
    }
}
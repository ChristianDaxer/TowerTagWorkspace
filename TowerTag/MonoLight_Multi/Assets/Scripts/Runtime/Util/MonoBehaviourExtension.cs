using JetBrains.Annotations;
using UnityEngine;

public static class MonoBehaviourExtension {
    [CanBeNull]
    public static T CheckForNull<T>(this T mb) where T : MonoBehaviour {
        return mb == null ? null : mb;
    }
}
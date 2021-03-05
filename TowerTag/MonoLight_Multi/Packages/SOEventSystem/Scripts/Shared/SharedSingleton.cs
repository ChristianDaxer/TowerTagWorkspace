using System.Linq;
using UnityEngine;

namespace SOEventSystem.Shared {
    /// <summary>
    /// Wrapper of ScriptableObject that grants static access to a singletons instance.
    /// The instance is found in the currently loaded assets by using <see cref="Resources"/>.
    /// The instance is null, if there are no available assets. There is no lazy instantiation.
    /// Logs a warning, if there are multiple instances, and returns the first it finds.
    ///
    /// </summary>
    /// <typeparam name="T">The own type of the singleton class.</typeparam>
    /// <seealso cref="SharedSingletonVariable{T,TValue}"/>
    public class SharedSingleton<T> : ScriptableObject where T : SharedSingleton<T> {
        private static T _singleton;

        public static T Singleton {
            get {
                if (_singleton == null) {
                    T[] singletons = Resources.FindObjectsOfTypeAll<T>();
                    if (singletons.Length > 1)
                        Debug.LogWarning("There are multiple instances of singleton type " + typeof(T) +
                                                     ". Returning a random instance.");
                    _singleton = singletons.FirstOrDefault();
                }

                return _singleton;
            }
        }
    }
}
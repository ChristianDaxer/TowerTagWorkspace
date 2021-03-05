using System.Linq;
using UnityEngine;

namespace SOEventSystem.Shared {
    /// <summary>
    /// Wrapper of SharedVariable that grants static access to a singletons instance.
    /// The instance is found in the currently loaded assets by using <see cref="Resources"/>.
    /// The instance is null, if there are no available assets. There is no lazy instantiation.
    /// Logs a warning, if there are multiple instances, and returns the first it finds.
    /// 
    /// </summary>
    /// <typeparam name="T">The own type of the singleton class.</typeparam>
    /// <typeparam name="TValue">The type of the wrapped variable.</typeparam>
    /// <seealso cref="SharedSingleton{T}"/>
    public class SharedSingletonVariable<T, TValue> : SharedVariable<TValue> where T : SharedSingletonVariable<T, TValue> {
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
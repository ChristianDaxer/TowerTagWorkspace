using UnityEngine;

namespace SOEventSystem.Shared {
    /// <inheritdoc />
    /// 
    /// Implementation for UnityEngine.Object.
    /// Consider creating a type-safe implementation of SharedObject instead.
    [CreateAssetMenu(menuName = "Shared/UnityEngine.Object", fileName = "Consider type safety", order = 1000)]
    public class SharedObject : SharedVariable<Object> { }
}
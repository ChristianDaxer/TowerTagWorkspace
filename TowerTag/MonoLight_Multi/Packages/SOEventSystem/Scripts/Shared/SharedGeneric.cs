using UnityEngine;

namespace SOEventSystem.Shared {
    /// <inheritdoc />
    /// 
    /// Implementation for generic object. Avoid this. Consider using a type-safe implementation instead.
    [CreateAssetMenu(menuName = "Shared/object (c#)", fileName = "use a condom, be type safe!", order = 0)]
    public class SharedGeneric : SharedVariable<object> { }
}
using System;
using SOEventSystem.Shared;
using UnityEngine.Events;

namespace SOEventSystem.Listeners {
    /// <inheritdoc />
    /// <remarks>Implementation for generic object. For type-safety, consider using a less generic implementation.</remarks>
    public class SharedGenericListener : SharedVariableListener<object, SharedGeneric, GenericResponse> { }

    [Serializable]
    public class GenericResponse : UnityEvent<object, object> { }
}
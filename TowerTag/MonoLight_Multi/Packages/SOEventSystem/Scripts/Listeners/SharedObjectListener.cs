using System;
using SOEventSystem.Shared;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace SOEventSystem.Listeners {
    /// <inheritdoc />
    /// <remarks>Implementation for UnityEngine.Object. For type-safety, consider using a less generic implementation.</remarks>
    public class SharedObjectListener : SharedVariableListener<Object, SharedObject, ObjectResponse> { }

    [Serializable]
    public class ObjectResponse : UnityEvent<object, Object> { }
}
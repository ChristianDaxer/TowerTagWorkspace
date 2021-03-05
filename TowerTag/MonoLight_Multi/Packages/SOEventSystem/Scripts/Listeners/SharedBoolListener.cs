using System;
using SOEventSystem.Shared;
using UnityEngine.Events;

namespace SOEventSystem.Listeners {
    /// <inheritdoc />
    /// <remarks>Implementation for bool</remarks>
    public class SharedBoolListener : SharedVariableListener<bool, SharedBool, BoolResponse> { }

    [Serializable]
    public class BoolResponse : UnityEvent<object, bool> { }
}
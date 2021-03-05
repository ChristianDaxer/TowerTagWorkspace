using System;
using SOEventSystem.Shared;
using UnityEngine.Events;

namespace SOEventSystem.Listeners {
    /// <inheritdoc />
    /// <remarks>Implementation for int</remarks>
    public class SharedIntListener : SharedVariableListener<int, SharedInt, IntResponse> { }

    [Serializable]
    public class IntResponse : UnityEvent<object, int> { }
}
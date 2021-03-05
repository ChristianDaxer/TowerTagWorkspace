using System;
using SOEventSystem.Shared;
using UnityEngine.Events;

namespace SOEventSystem.Listeners {
    /// <inheritdoc />
    /// <remarks>Implementation for string</remarks>
    public class SharedStringListener : SharedVariableListener<string, SharedString, StringResponse> { }

    [Serializable]
    public class StringResponse : UnityEvent<object, string> { }
}
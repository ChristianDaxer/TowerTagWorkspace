using System;
using SOEventSystem.Shared;
using UnityEngine.Events;

namespace SOEventSystem.Listeners {
    /// <inheritdoc />
    /// <remarks>Implementation for float</remarks>
    public class SharedFloatListener : SharedVariableListener<float, SharedFloat, FloatResponse> { }
    
    [Serializable]
    public class FloatResponse : UnityEvent<object, float> { }
}
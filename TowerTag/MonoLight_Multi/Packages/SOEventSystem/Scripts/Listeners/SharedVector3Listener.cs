using System;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace SOEventSystem.Listeners {
    /// <inheritdoc />
    /// <remarks>Implementation for UnityEngine.Vector3.</remarks>
    public class SharedVector3Listener : SharedVariableListener<Vector3, SharedVector3, Vector3Response> { }

    [Serializable]
    public class Vector3Response : UnityEvent<object, Vector3> { }
}
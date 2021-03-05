using System;
using SOEventSystem.Listeners;
using UnityEngine.Events;

namespace TowerTagSOES {
    public class SharedPillarListener : SharedVariableListener<Pillar, SharedPillar, PillarUnityEvent> {}

    [Serializable]
    public class PillarUnityEvent : UnityEvent<object, Pillar> { }
}
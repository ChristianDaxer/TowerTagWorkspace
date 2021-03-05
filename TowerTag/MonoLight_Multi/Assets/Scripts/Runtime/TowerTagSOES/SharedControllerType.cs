using SOEventSystem.Shared;
using UnityEngine;

namespace TowerTagSOES {
    [CreateAssetMenu(menuName = "Shared/TowerTag/Controller Type")]
    public class SharedControllerType : SharedSingletonVariable<SharedControllerType, ControllerType> {
        public static bool IsAdmin => Singleton == ControllerType.Admin;
        public static bool PillarOffsetController => Singleton == ControllerType.PillarOffsetController;
        public static bool VR => Singleton == ControllerType.VR;
        public static bool NormalFPS => Singleton == ControllerType.NormalFPS;
        public static bool Spectator => Singleton == ControllerType.Spectator;
        public static bool IsPlayer => Singleton == ControllerType.NormalFPS || Singleton == ControllerType.VR;
    }

    public enum ControllerType {
        NormalFPS,
        VR,
        Spectator,
        Admin,
        PillarOffsetController
    }
}
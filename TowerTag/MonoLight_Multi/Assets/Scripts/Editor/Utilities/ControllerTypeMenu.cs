using TowerTagSOES;
using UnityEditor;
using UnityEngine;

namespace Utilities {
    public class ControllerTypeMenu : MonoBehaviour {
        [MenuItem("ControllerType/Operator (Admin) %F1")]
        public static void Admin() {
            Debug.Log("Set Controller Type to Operator (Admin)");
            SharedControllerType.Singleton.Set(typeof(ControllerTypeMenu), ControllerType.Admin);
        }

        [MenuItem("ControllerType/VR %F2")]
        public static void VR() {
            Debug.Log("Set Controller Type to VR Player");
            SharedControllerType.Singleton.Set(typeof(ControllerTypeMenu), ControllerType.VR);
        }

        [MenuItem("ControllerType/FPS %F4")]
        public static void FPS() {
            Debug.Log("Set Controller Type to FPS");
            SharedControllerType.Singleton.Set(typeof(ControllerTypeMenu), ControllerType.NormalFPS);
        }

        [MenuItem("ControllerType/PillarOffest %F5")]
        public static void PillarOffest() {
            Debug.Log("Set Controller Type to PillarOffest");
            SharedControllerType.Singleton.Set(typeof(ControllerTypeMenu), ControllerType.PillarOffsetController);
        }

        [MenuItem("ControllerType/Spectator %F6")]
        public static void Spectator() {
            Debug.Log("Set Controller Type to Spectator");
            SharedControllerType.Singleton.Set(typeof(ControllerTypeMenu), ControllerType.Spectator);
        }
    }
}
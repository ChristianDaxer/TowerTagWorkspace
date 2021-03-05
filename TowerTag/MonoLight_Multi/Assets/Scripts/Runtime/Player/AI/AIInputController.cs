using System;
using BehaviorDesigner.Runtime;
using UnityEngine;

namespace AI {
    /// <summary>
    /// AI Input Controller for invoking controller trigger input behaviour.
    /// </summary>
    public class AIInputController : MonoBehaviour, IInputController {
        public event Action GripPressed;
        public event Action GripReleased;
        public event Action<GunController.GunControllerState.TriggerAction> TriggerPressed;
        public event Action TriggerReleased;
        public event Action TeleportTriggered;

        [SerializeField] private Behavior _behavior;

        private void Start() {
            // restart behaviour in case anything stops it
            InvokeRepeating(nameof(StartBehaviour), 0, 1);
        }

        private void StartBehaviour() {
            _behavior.EnableBehavior();
        }

        [ContextMenu("Press")]
        public void Press(GunController.GunControllerState.TriggerAction triggerAction) {
            TriggerPressed?.Invoke(triggerAction);
            GripPressed?.Invoke();
        }

        [ContextMenu("Release")]
        public void Release() {
            TriggerReleased?.Invoke();
            GripReleased?.Invoke();
        }

        [ContextMenu("Teleport")]
        public void Teleport() {
            TeleportTriggered?.Invoke();
        }
    }
}
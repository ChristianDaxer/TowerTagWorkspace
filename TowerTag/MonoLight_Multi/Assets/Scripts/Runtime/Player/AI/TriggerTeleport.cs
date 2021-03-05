using System;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    [TaskCategory("TT Bot")]
    [Serializable]
    public class TriggerTeleport : Action {
        [SerializeField] private SharedBotBrain _botBrain;
        private AIInputController InputController => _botBrain.Value.InputController;

        public override TaskStatus OnUpdate() {
            if (InputController == null) return TaskStatus.Failure;
            InputController.Teleport();
            InputController.Release();
            return TaskStatus.Success;
        }
    }
}
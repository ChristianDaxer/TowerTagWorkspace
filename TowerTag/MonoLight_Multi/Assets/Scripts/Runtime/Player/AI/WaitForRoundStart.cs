using System;
using BehaviorDesigner.Runtime.Tasks;
using TowerTag;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    /// <summary>
    /// Wait until countdown ends and new round starts.
    /// Immediately evaluates to success if not during countdown time.
    /// </summary>
    [TaskCategory("TT Bot")]
    [TaskDescription("Wait until countdown ends and new round starts.")]
    [Serializable]
    public class WaitForRoundStart : Action {
        [SerializeField] private SharedBotBrain _botBrain;

        private IPlayer Player => _botBrain.Value.Player;

        public override TaskStatus OnUpdate() {
            if (Player.GunController.StateMachine.CurrentStateIdentifier ==
                GunController.GunControllerStateMachine.State.Disabled)
                return TaskStatus.Running;

            return TaskStatus.Success;

        }
    }
}
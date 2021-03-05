using System;
using BehaviorDesigner.Runtime.Tasks;
using TowerTag;
using UnityEngine;

namespace AI {
    /// <summary>
    /// Returns success if the AI Player is currently charging the target pillar and this pillar is claimed.
    /// This state can be the case if the 'Search & Claim' Sequence was aborted before the 'Trigger Teleport' behaviour was executed
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class ClaimingTower : Conditional {
        [SerializeField] private SharedBotBrain _botBrain;

        private IPlayer Player => _botBrain.Value.Player;

        public override TaskStatus OnUpdate() {
            if (Player.GunController.StateMachine.CurrentStateIdentifier ==
                GunController.GunControllerStateMachine.State.Charge) {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}
using System;
using BehaviorDesigner.Runtime.Tasks;
using TowerTag;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

namespace AI {
    /// <summary>
    /// This Task handles the charging of a pillar and keeps on running until the pillar is claimed.
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class ClaimPillar : Action {
        [SerializeField] private SharedBotBrain _botBrain;

        private IPlayer Player => _botBrain.Value.Player;
        private Pillar TargetPillar => _botBrain.Value.TargetPillar;
        private Pillar _currentPillar;

        private AIInputController InputController => _botBrain.Value.InputController;

        private float _currentCharge;
        private float _timer;
        private float _checkChargeInterval = 2f;
        public override void OnStart() {
            _currentPillar = Player.CurrentPillar;
            _currentCharge = TargetPillar.CurrentCharge.value;
            _timer = 0;
            if (Player.GunController.StateMachine.CurrentStateIdentifier ==
                GunController.GunControllerStateMachine.State.Shoot) {
                InputController.Release();
            }
        }

        public override TaskStatus OnUpdate() {
            if (Player == null || Player.GunController == null
                               || InputController == null
                               || TargetPillar == null
                               || _currentPillar != Player.CurrentPillar) {
                return TaskStatus.Failure;
            }

            InputController.Press(GunController.GunControllerState.TriggerAction.Claim);

            GunController gunController = Player.GunController;
            if (gunController.StateMachine.CurrentStateIdentifier !=
                GunController.GunControllerStateMachine.State.Charge) {
                InputController.Release();
                return TaskStatus.Failure;
            }

            if (TargetPillar.OwningTeamID == Player.TeamID)
                return TaskStatus.Success;

            _timer += Time.deltaTime;
            // start/keep claiming if any chargeable collider is active
            foreach (ChargeableCollider chargeableCollider in TargetPillar.ChargeableCollider) {
                if (chargeableCollider.gameObject.activeSelf) {
                    if (_timer >= _checkChargeInterval && Math.Abs(_currentCharge - TargetPillar.CurrentCharge.value) < 0.05f)
                        return TaskStatus.Success;
                    return TaskStatus.Running;
                }
            }

            return TaskStatus.Failure;
        }
    }
}
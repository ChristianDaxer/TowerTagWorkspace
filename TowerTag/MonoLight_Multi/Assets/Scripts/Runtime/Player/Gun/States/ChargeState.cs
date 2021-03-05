using System;
using TowerTagSOES;
using UnityEngine;

public partial class GunController {
    [Serializable]
    public class ChargeState : GunControllerState {
        /// <summary>
        /// Identifies the state.
        /// </summary>
        public override GunControllerStateMachine.State StateIdentifier => GunControllerStateMachine.State.Charge;

        [SerializeField] private float _energyToChargePerSecond = 0.1f;
        [SerializeField] private float _energyToHealPlayerPerSecond = 0.05f;
        [SerializeField] private float _timeoutBeforeStartCharging = 0.1f;

        private float _enterTime;
        private bool _detachingOnPurpose;

        protected override void Init() {
            InitMemberFromDebugConfig();
        }

        public override void EnterState() {
            _enterTime = Time.time;
            _detachingOnPurpose = false;
        }

        public override void UpdateState() {
            if (Time.time - _enterTime < _timeoutBeforeStartCharging)
                return;

            if (Chargeable == null) {
                StateMachine.ChangeState(GunControllerStateMachine.State.Idle);
                return;
            }

            if (Chargeable.CanCharge(Player)) {
                GunController.CurrentEnergy -= Time.deltaTime *
                                               (Chargeable is ChargePlayer
                                                   ? _energyToHealPlayerPerSecond
                                                   : _energyToChargePerSecond);
            }
        }

        public override void TeleportTriggered() {
            var pillar = Chargeable as Pillar;
            if (pillar == null) {
                RotatePlaySpaceHook hook = Chargeable as RotatePlaySpaceHook;
                if (hook != null) {
                    _detachingOnPurpose = true;
                    GunController.Teleport(hook);
                }

                return;
            }

            if (pillar.CanTeleport(Player)) {
                _detachingOnPurpose = true;
                GunController.Teleport(pillar);
            }
            else {
                GunController.DenyTeleport(pillar);
            }
        }

        public override void ExitState() {
            if (Chargeable != null)
                GunController.DisconnectRope(_detachingOnPurpose);
        }

        public override void GripUp() {
            if (ConfigurationManager.Configuration.SingleButtonControl) return;
            _detachingOnPurpose = true;
            StateMachine.ChangeState(GunControllerStateMachine.State.Idle);
        }

        public override void TriggerUp() {
            if (SharedControllerType.VR && !ConfigurationManager.Configuration.SingleButtonControl)
                return;
            _detachingOnPurpose = true;
            StateMachine.ChangeState(GunControllerStateMachine.State.Idle);
        }

        private void InitMemberFromDebugConfig() {
            _energyToChargePerSecond = BalancingConfiguration.Singleton.EnergyToChargePerSecond;
            _energyToHealPlayerPerSecond = BalancingConfiguration.Singleton.EnergyToHealPlayerPerSecond;
        }

        public override void DisconnectBeam(bool onPurpose) {
            _detachingOnPurpose = onPurpose;
            StateMachine.ChangeState(GunControllerStateMachine.State.Idle);
        }
    }
}
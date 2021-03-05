using UnityEngine;

public partial class GunController {
    [System.Serializable]
    public class TeleportState : GunControllerState {
        /// <summary>
        /// Identifies the state.
        /// </summary>
        public override GunControllerStateMachine.State StateIdentifier => GunControllerStateMachine.State.Teleport;

        private float _energyToTeleport;
        private float _enterTime;
        private const float WaitForTeleportStartTimeout = 1f;

        protected override void Init() {
            InitMemberFromDebugConfig();
        }

        public override void EnterState() {
            if (GunController.CurrentEnergy >= _energyToTeleport) {
                _enterTime = Time.time;
                GunController.CurrentEnergy -= _energyToTeleport;
            }
            else {
                StateMachine.ChangeState(GunControllerStateMachine.State.Idle);
            }
        }

        public override void UpdateState() {
            GunController.DoRaycast();

            // teleport timeout
            if (Time.time > _enterTime + WaitForTeleportStartTimeout) {
                StateMachine.ChangeState(GunControllerStateMachine.State.Idle);
            }
        }

        public override void ExitState() {
            GunController.DoRaycast();
        }

        public override void TeleportFinished() {
            StateMachine.ChangeState(GunControllerStateMachine.State.Idle);
        }

        private void InitMemberFromDebugConfig() {
            _energyToTeleport = BalancingConfiguration.Singleton.EnergyToTeleport;
        }
    }
}
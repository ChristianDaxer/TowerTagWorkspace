using System;
using UnityEngine;

public partial class GunController {
    [Serializable]
    public class FireState : GunControllerState {
        /// <summary>
        /// Identifies the state.
        /// </summary>
        public override GunControllerStateMachine.State StateIdentifier => GunControllerStateMachine.State.Shoot;

        public bool IsFireAllowed =>
            !GunController.Player.IsInIngameMenu && !GunController.Player.PlayerState.IsGunDisabled
                                                 && GunController.Player.IsAlive;

        private float _fireDelay;
        private float _energyToFire;

        private float _timeOfLastShot;

        protected override void Init() {
            InitMemberFromDebugConfig();
        }

        public override void EnterState() {
            if (IsFireAllowed)
                Fire();
            else if (GunController.ShotDeniedSound != null)
                GunController.ShotDeniedSound.Play();
        }

        public override void UpdateState() {
            if (IsFireAllowed && (_timeOfLastShot + _fireDelay * GunController.NoEnergyMultiplier < Time.time)) {
                Fire();
            }
        }

        private void Fire() {
            _timeOfLastShot = Time.time;
            GunController.CurrentEnergy -= _energyToFire;
            GunController.Fire();
        }

        public override void TriggerUp() {
            StateMachine.ChangeState(GunControllerStateMachine.State.Idle);
        }

        private void InitMemberFromDebugConfig() {
            _fireDelay = BalancingConfiguration.Singleton.FireProjectileTimeOut;
            _energyToFire = BalancingConfiguration.Singleton.EnergyToFireProjectile;
        }
    }
}
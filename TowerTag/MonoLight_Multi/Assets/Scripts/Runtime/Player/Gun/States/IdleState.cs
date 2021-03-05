using System;
using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.Serialization;

public partial class GunController {
    [Serializable]
    public class IdleState : GunControllerState {
        /// <summary>
        /// Identifies the state.
        /// </summary>
        public override GunControllerStateMachine.State StateIdentifier => GunControllerStateMachine.State.Idle;

        [SerializeField] private GrapplingHookController _grapplingHookController;

        [FormerlySerializedAs("LoadEnergyTimeoutInSeconds")] [SerializeField]
        private float _loadEnergyTimeoutInSeconds = 2f;

        [FormerlySerializedAs("energyRegenerationPerSecond")] [SerializeField]
        private float _energyRegenerationPerSecond = 0.25f;

        private float _enterTime;
        private Chargeable _lastRaycastHit;

        protected override void Init() {
            InitMemberFromDebugConfig();
        }

        public override void EnterState() {
            _enterTime = Time.time;
            _lastRaycastHit = null;
        }

        public override void UpdateState() {
            if (_enterTime + _loadEnergyTimeoutInSeconds < Time.time) {
                LoadEnergy();
            }

            //did the target change?
            Chargeable currentTarget = GunController.DoRaycast();
            if (_lastRaycastHit != currentTarget) {
                if (ConfigurationManager.Configuration.SingleButtonControl) {
                    _grapplingHookController.TriggerGrapplingAnimation(
                        currentTarget != null && currentTarget.CanTryToAttach(Player));
                }
            }

            _lastRaycastHit = currentTarget;
        }

        public override void GripDown() {
            // early return when user toggled single button control
            if (ConfigurationManager.Configuration.SingleButtonControl)
                return;

            if (_lastRaycastHit != null && _lastRaycastHit.CanTryToAttach(Player)) {
                // Check Chargeable => if its Team Based Pillar of the enemy team, try to attach rope and fail
                var pillar = _lastRaycastHit.GetComponent<Pillar>();
                if (_lastRaycastHit.GetComponent<Pillar>() != null
                    && pillar.IsTeamBased
                    && pillar.OwningTeam.ID != TeamID.Neutral
                    && pillar.OwningTeam.ID != GunController.Player.TeamID) {
                    GunController.TryToAttachRopeAndFail(_lastRaycastHit);
                    return;
                }

                if (_lastRaycastHit.CanAttach(GunController.Player)) {
                    GunController.ConnectBeam(_lastRaycastHit);
                    StateMachine.ChangeState(GunControllerStateMachine.State.Charge);
                }
                else {
                    GunController.TryToAttachRopeAndFail(_lastRaycastHit);
                }
            }
            else {
                if (GunController != null && GunController.ShotDeniedSound != null)
                    GunController.ShotDeniedSound.Play();
            }
        }

        public override void TriggerDown(TriggerAction triggerAction = TriggerAction.DetectByRaycast) {
            if (ConfigurationManager.Configuration.SingleButtonControl || (Player.IsBot && triggerAction != TriggerAction.Shoot)
                                                                       || !SharedControllerType.VR) {
                if (_lastRaycastHit != null && _lastRaycastHit.CanTryToAttach(Player)) {
                    // Check Chargeable => if its Team Based Pillar of the enemy team, try to attach rope and fail
                    var pillar = _lastRaycastHit.GetComponent<Pillar>();
                    if (_lastRaycastHit.GetComponent<Pillar>() != null
                        && pillar.IsTeamBased
                        && pillar.OwningTeam.ID != TeamID.Neutral
                        && pillar.OwningTeam.ID != GunController.Player.TeamID) {
                        GunController.TryToAttachRopeAndFail(_lastRaycastHit);
                        return;
                    }

                    if (_lastRaycastHit.CanAttach(GunController.Player)) {
                        GunController.ConnectBeam(_lastRaycastHit);
                        StateMachine.ChangeState(GunControllerStateMachine.State.Charge);
                    }
                    else {
                        GunController.TryToAttachRopeAndFail(_lastRaycastHit);
                    }

                    return;
                }
            }

            if (triggerAction != TriggerAction.Claim)
                // if nothing chargeable in gun direction shoot
                StateMachine.ChangeState(GunControllerStateMachine.State.Shoot);
        }

        private void LoadEnergy() {
            GunController.CurrentEnergy += _energyRegenerationPerSecond * Time.deltaTime;
        }

        private void InitMemberFromDebugConfig() {
            _loadEnergyTimeoutInSeconds = BalancingConfiguration.Singleton.EnergyRegenerationTimeout;
            _energyRegenerationPerSecond = BalancingConfiguration.Singleton.EnergyRegenerationPerSecond;
        }
    }
}
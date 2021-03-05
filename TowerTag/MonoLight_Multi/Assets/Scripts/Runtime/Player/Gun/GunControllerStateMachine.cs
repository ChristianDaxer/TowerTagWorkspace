using UnityEngine;

public partial class GunController {
    [System.Serializable]
    public class
        GunControllerStateMachine : GenericStateMachine<GunControllerStateMachine, GunController, GunControllerState> {
        public enum State {
            Idle,
            Shoot,
            Charge,
            Teleport,
            Rotate,
            Disabled,
            None
        }

        [SerializeField] private IdleState _idleState;
        [SerializeField] private FireState _fireState;
        [SerializeField] private ChargeState _chargeState;
        [SerializeField] private TeleportState _teleportState;
        [SerializeField] private DisabledState _disabledState;

        public GunController GunController => StateMachineContext;
        public State CurrentStateIdentifier => CurrentState?.StateIdentifier ?? State.None;

        public override void InitStateMachine(GunController owner) {
            base.InitStateMachine(owner);

            _idleState.InitState(this);
            _fireState.InitState(this);
            _chargeState.InitState(this);
            _teleportState.InitState(this);
            _disabledState.InitState(this);
            ChangeState(_idleState);
        }

        public void ChangeState(State newStateToSet) {
            switch (newStateToSet) {
                case State.Idle:
                    ChangeState(_idleState);
                    return;
                case State.Shoot:
                    ChangeState(_fireState);
                    return;
                case State.Charge:
                    ChangeState(_chargeState);
                    return;
                case State.Teleport:
                    ChangeState(_teleportState);
                    return;
                case State.Disabled:
                    ChangeState(_disabledState);
                    return;
            }
        }

        public void GripPressed() {
            CurrentState?.GripDown();
        }

        public void GripReleased() {
            CurrentState?.GripUp();
        }

        public void TriggerPressed(GunControllerState.TriggerAction triggerAction) {
            CurrentState?.TriggerDown(triggerAction);
        }

        public void TriggerReleased() {
            CurrentState?.TriggerUp();
        }

        public void TeleportTriggered() {
            CurrentState?.TeleportTriggered();
        }

        public void TeleportFinished() {
            CurrentState?.TeleportFinished();
        }

        public void DisconnectBeam(bool onPurpose) {
            CurrentState?.DisconnectBeam(onPurpose);
        }

        public void SetActive(bool active) {
            if (active) {
                if (CurrentStateIdentifier == State.Disabled) ChangeState(State.Idle);
            }
            else {
                ChangeState(State.Disabled);
            }
        }
    }
}
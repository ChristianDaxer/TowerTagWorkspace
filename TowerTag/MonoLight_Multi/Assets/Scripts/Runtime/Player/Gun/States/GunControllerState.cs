using TowerTag;

public partial class GunController {
    public abstract class
        GunControllerState : AGenericBaseState<GunControllerStateMachine, GunController, GunControllerState> {
        /// <summary>
        /// Identifies the state.
        /// </summary>
        public abstract GunControllerStateMachine.State StateIdentifier { get; }

        public enum TriggerAction {
            Shoot,
            Claim,
            DetectByRaycast
        }
        protected Chargeable Chargeable => GunController.Chargeable;
        protected IPlayer Player => GunController.Player;
        protected GunController GunController => StateMachineContext;

        // state functions
        public override void EnterState() { }
        public override void UpdateState() { }
        public override void ExitState() { }

        // Input functions
        public virtual void GripDown() {}
        public virtual void GripUp() {}
        public virtual void TriggerDown(TriggerAction triggerAction = TriggerAction.DetectByRaycast) { }
        public virtual void TriggerUp() { }
        public virtual void TeleportTriggered() { }
        public virtual void TeleportFinished() { }
        public virtual void DisconnectBeam(bool onPurpose) { }
    }
}
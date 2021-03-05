using System;

public partial class GunController {
    [Serializable]
    public class DisabledState : GunControllerState {
        /// <summary>
        /// Identifies the state.
        /// </summary>
        public override GunControllerStateMachine.State StateIdentifier => GunControllerStateMachine.State.Disabled;

        public override void EnterState() {
            if (GunController != null)
                GunController.ResetRayCaster();
        }

        public override void GripDown() {
            GunController.ShotDeniedSound.Play();
        }

        public override void TriggerDown(TriggerAction triggerAction = TriggerAction.DetectByRaycast)
        {
            GunController.ShotDeniedSound.Play();
        }
    }
}
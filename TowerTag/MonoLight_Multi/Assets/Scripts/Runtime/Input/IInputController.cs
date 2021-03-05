using System;

public interface IInputController {
    event Action GripPressed;
    event Action GripReleased;
    event Action<GunController.GunControllerState.TriggerAction> TriggerPressed;
    event Action TriggerReleased;
    event Action TeleportTriggered;
}

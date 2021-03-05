using UnityEngine;

public class VRControllerShotCurve : RumbleCurve
{
    [SerializeField] private float _duration;
    [SerializeField] private InputControllerVR _inputControllerVr;

    public override void Init() {
        _inputControllerVr.ActiveController.Rumble(1f / _duration, 1, _inputControllerVr.ActiveController, 0, _duration);
    }

    public override void UpdateCurve(float delta) {
    }

    public override void Exit() {
    }

    private void OnDestroy() {
        Exit();
    }
}

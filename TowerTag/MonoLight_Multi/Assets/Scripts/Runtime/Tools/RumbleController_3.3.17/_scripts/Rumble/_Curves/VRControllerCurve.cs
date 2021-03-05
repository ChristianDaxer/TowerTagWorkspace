using System;
using UnityEngine;
using UnityEngine.Serialization;

public class VRControllerCurve : RumbleCurve {

    [FormerlySerializedAs("minValue")] [SerializeField]
    private float _minValue;

    [FormerlySerializedAs("maxValue")] [SerializeField]
    private float _maxValue = 3999f;

    [FormerlySerializedAs("curve")] [SerializeField]
    private AnimationCurve _curve;

    [SerializeField] private InputControllerVR _inputControllerVr;

    public override void Init() { }

    public override void UpdateCurve(float delta) {
        var discreteValue = (ushort) Mathf.RoundToInt(Mathf.Lerp(_minValue, _maxValue, _curve.Evaluate(delta)));
        TriggerHapticPulse(discreteValue);
    }

    public override void Exit() {
        TriggerHapticPulse(0);
    }

    private void TriggerHapticPulse(ushort microSecondsDuration) {
        float seconds = microSecondsDuration / 1000000f;
        _inputControllerVr.ActiveController.Rumble(1f / seconds, 1, _inputControllerVr.ActiveController, 0, seconds);
    }

    private void OnDestroy() {
        Exit();
        _curve = null;
    }
}
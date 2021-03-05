using System;
using UnityEngine;
using UnityEngine.Serialization;

public class BlurController : MonoBehaviour {
    [FormerlySerializedAs("blurScript")] [SerializeField]
    private BlurWithMask _blurScript;

    [FormerlySerializedAs("innerRadiusMinMaxValues")] [SerializeField]
    private Vector2 _innerRadiusMinMaxValues = new Vector2(0f, 1f);

    [FormerlySerializedAs("outerRadiusMinMaxValues")] [SerializeField]
    private Vector2 _outerRadiusMinMaxValues = new Vector2(1.5f, 0.1f);

    [FormerlySerializedAs("increaseEffectStrengthStep")] [SerializeField]
    private float _increaseEffectStrengthStep = 0.7f;

    [FormerlySerializedAs("deacreaseEffectAutomatically")] [SerializeField]
    private bool _decreaseEffectAutomatically;

    [FormerlySerializedAs("decreaseEffectPerSecondFactor")] [SerializeField]
    private float _decreaseEffectPerSecondFactor = 0.3f;

    [FormerlySerializedAs("decreasePerSecond")] [SerializeField]
    private AnimationCurve _decreasePerSecond;

    [SerializeField] [Range(0, 1)] private float _currentEffectStrength;
    private float _lastAppliedEffectStrengthValue;

    private const float FloatTolerance = 0.001f;

    private void Update() {
        if (Input.GetKeyUp(KeyCode.H))
            IncreaseStrength();

        if (_decreaseEffectAutomatically) {
            _currentEffectStrength = Mathf.Clamp01(_currentEffectStrength -
                                                   Time.deltaTime *
                                                   _decreasePerSecond.Evaluate(_currentEffectStrength) *
                                                   _decreaseEffectPerSecondFactor);
            ApplyEffect(_currentEffectStrength);
        }
    }

    private void IncreaseStrength() {
        _currentEffectStrength += _increaseEffectStrengthStep;
        ApplyEffect(Mathf.Clamp01(_currentEffectStrength));
    }

    private void ApplyEffect(float strength) {
        if (Math.Abs(strength - _lastAppliedEffectStrengthValue) > FloatTolerance) {
            _lastAppliedEffectStrengthValue = strength;
            _blurScript.OuterRadius = Mathf.Lerp(_outerRadiusMinMaxValues.x, _outerRadiusMinMaxValues.y, strength);
            _blurScript.InnerRadius = Mathf.Lerp(_innerRadiusMinMaxValues.y, _innerRadiusMinMaxValues.x, strength);
        }
    }
}
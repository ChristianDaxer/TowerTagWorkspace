using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class FloatLerpAnimation : CurveAnimation {
    public float ResultingFloat { get; private set; }

    [FormerlySerializedAs("_ValueToLerpTo")] [SerializeField]
    private float _valueToLerpTo;

    private float _startValue;

    public void StartAnimation(float startValue) {
        _startValue = startValue;
        base.StartAnimation();
    }

    protected override void Lerp(float t) {
        ResultingFloat = Mathf.Lerp(_startValue, _valueToLerpTo, t);
    }
}
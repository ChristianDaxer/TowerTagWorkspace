using System;
using UnityEngine;

[Serializable]
public class FloatArrayLerpAnimationNegativePositiveLerp : CurveAnimation {
    public float[] ResultingValues { get; private set; }
    private float[] _negativeClampValues;
    private float[] _positiveClampValues;
    private float[] _curveZeroValues;

    public void StartAnimation(float[] curveZeroValues, float[] negativeClampValues, float[] positiveClampValues) {
        if (curveZeroValues == null) {
            Debug.LogError("Cannot start animation: curveZeroValues are null");
            return;
        }

        if (negativeClampValues == null) {
            Debug.LogError("Cannot start animation: negativeClampValues are null");
            return;
        }

        if (positiveClampValues == null) {
            Debug.LogError("Cannot start animation: positiveClampValues are null");
            return;
        }

        if (curveZeroValues.Length != positiveClampValues.Length) {
            Debug.LogError("Cannot start animation: curveZeroValues and positiveClampValues have different lengths");
            return;
        }

        if (curveZeroValues.Length != negativeClampValues.Length) {
            Debug.LogError("Cannot start animation: curveZeroValues and negativeClampValues have different lengths");
            return;
        }

        _curveZeroValues = curveZeroValues;
        _negativeClampValues = negativeClampValues;
        _positiveClampValues = positiveClampValues;

        ResultingValues = new float[_curveZeroValues.Length];

        base.StartAnimation();
    }

    protected override void Lerp(float t) {
        if (t >= 0) {
            for (var i = 0; i < ResultingValues.Length; i++) {
                ResultingValues[i] = Mathf.Lerp(_curveZeroValues[i], _positiveClampValues[i], t);
            }
        }
        else {
            for (var i = 0; i < ResultingValues.Length; i++) {
                ResultingValues[i] = Mathf.Lerp(_curveZeroValues[i], _negativeClampValues[i], -t);
            }
        }
    }
}
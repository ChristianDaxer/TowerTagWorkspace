using System;
using System.Collections;
using UnityEngine;

[Serializable]
public abstract class CurveAnimation : IDisposable {
    public bool IsPlaying {
        get {
            bool result = _isPlaying || !_wasLastFrameFetched;

            if (!_isPlaying)
                _wasLastFrameFetched = true;

            return result;
        }
    }

    private bool _isPlaying;
    private bool _wasLastFrameFetched = true;

    [SerializeField] private AnimationCurve _lerpCurve;
    [SerializeField] private float _timeToPlay = 1f;

    private Coroutine _coroutine;

    protected void StartAnimation() {
        if (_coroutine != null)
            StaticCoroutine.StopStaticCoroutine(_coroutine);

        _coroutine = StaticCoroutine.StartStaticCoroutine(Animate());
    }

    public void StopAnimation() {
        _isPlaying = false;
        _wasLastFrameFetched = true;

        if (_coroutine != null) {
            StaticCoroutine.StopStaticCoroutine(_coroutine);
            Lerp(_lerpCurve.Evaluate(1));
        }
    }

    private IEnumerator Animate() {
        if (_timeToPlay > 0) {
            _isPlaying = true;
            _wasLastFrameFetched = false;

            float timePlayed = 0;
            while (timePlayed <= _timeToPlay) {
                Lerp(_lerpCurve.Evaluate(timePlayed / _timeToPlay));
                timePlayed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        Lerp(_lerpCurve.Evaluate(1));
        _isPlaying = false;
        _wasLastFrameFetched = false;
    }

    protected abstract void Lerp(float t);

    public void Dispose() {
        StopAnimation();
    }
}
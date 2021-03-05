using JetBrains.Annotations;
using UnityEngine;

public class IdleDetector : TTSingleton<IdleDetector> {
    public delegate void IdleDetectionHandler();

    public IdleDetectionHandler OnIdleTimeExpired;
    public IdleDetectionHandler OnEnterIdleState;
    public IdleDetectionHandler OnExitIdleState;

    private float _expirationTime;
    private float? _idleStarted;
    private bool _expired;

    private void OnEnable() {
        OnIdleTimeExpired += SetExpired;
        OnEnterIdleState += SetUnexpired;
        OnExitIdleState += SetUnexpired;
    }

    private void OnDisable() {
        OnIdleTimeExpired -= SetExpired;
        OnEnterIdleState -= SetUnexpired;
        OnExitIdleState -= SetUnexpired;
    }

    public void StartIdleDetection(float checkingInterval, float expirationTime) {
        InvokeRepeating(nameof(DetectIdle), checkingInterval, checkingInterval);
        _expirationTime = expirationTime;
    }

    [UsedImplicitly]
    public void StopIdleDetection() {
        CancelInvoke();
    }

    private void DetectIdle() {
        if (IsHeadsetInIdleState()) {
            if (_idleStarted == null) {
                _idleStarted = Time.realtimeSinceStartup;
                OnEnterIdleState.Invoke();
                return;
            }

            if (Time.realtimeSinceStartup - _idleStarted.Value > _expirationTime && !_expired) {
                OnIdleTimeExpired.Invoke();
            }
        }
        else {
            if (_idleStarted != null) {
                OnExitIdleState.Invoke();
            }

            _idleStarted = null;
        }
    }

    private void SetExpired() {
        _expired = true;
    }

    private void SetUnexpired() {
        _expired = false;
    }

    protected virtual bool IsHeadsetInIdleStateImpl () { return false; }

    private static bool IsHeadsetInIdleState() {
        if (IdleDetector.GetInstance(out var idleDetector))
            return idleDetector.IsHeadsetInIdleStateImpl();
        return false;
    }

    protected override void Init()
    {
    }
}
using UnityEngine;
using System.Collections;

public class RumbleCurveSetup : MonoBehaviour {
    [field: SerializeField]
    public float Duration { get; private set; }

    public bool IsPlaying { get; private set; }

    [SerializeField]
    RumbleCurve[] _curves;


    void Init() {
        foreach (var c in _curves) {
            if (c != null)
                c.Init();
        }
    }

    void UpdateCurve(float delta) {
        foreach (var c in _curves) {
            if (c != null)
                c.UpdateCurve(delta);
        }
    }

    void Exit() {
        foreach (var c in _curves) {
            if (c != null)
                c.Exit();
        }
    }

    // OneShot events
    public void TriggerOneShot() {
        StopOneShot();
        StartCoroutine(StartOneShot());
    }
    public void StopOneShot() {
        StopCoroutine(StartOneShot());
        Exit();
    }

    IEnumerator StartOneShot() {
        float timer = 0;
        Init();
        while (timer <= Duration) {
            UpdateCurve(timer / Duration);
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
        Exit();
    }

    // Loop events
    public float _loopStartTime;

    public void StartLoop() {
        StopLoop();
        Init();
        _loopStartTime = Time.time;
        IsPlaying = true;
    }

    public void StopLoop() {
        IsPlaying = false;
        Exit();
    }

    void Update() {
        if (IsPlaying) {
            float value = ((Time.time - _loopStartTime) % Duration) / Duration;
            UpdateCurve(value);
        }
    }

    public void Stop() {
        StopLoop();
        StopOneShot();
    }

    private void OnDestroy() {
        Stop();

        _curves = null;
    }
}

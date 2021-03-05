using UnityEngine;

public class FlyByFake : MonoBehaviour {
    [SerializeField] private AudioSource _source;
    [SerializeField] private string _soundName;
    [SerializeField] private Vector2 _rollOffDistanceMinMax = new Vector2(0, 2);
    [SerializeField] private AnimationCurve _rollOffCurve;
    private Sound _sound;
    private float _halfClipLength;

    private void Awake() {
        InitSound();
    }

    private void InitSound() {
        if (SoundDatabase.Instance != null) _sound = SoundDatabase.Instance.GetSound(_soundName);

        if (_sound == null) {
            Debug.LogWarning("Could not find Sound (" + _soundName + ") in Database!");
            return;
        }

        _sound.InitSource(_source);
        _halfClipLength = _source.clip.length * 0.5f;
    }

    public void TriggerFlyBySoundFake(Vector3 targetPosition, Vector3 currentObjectPosition, Vector3 objectVelocity) {
        //Vector3 p = Vector3.ProjectOnPlane(objectVelocity, currentObjectPosition - targetPosition).normalized * Vector3.Distance(currentObjectPosition, targetPosition) + targetPosition;

        //v1 = perpendicular to shot direction and shot distance to target
        Vector3 v1 = Vector3.Cross(objectVelocity.normalized, targetPosition - currentObjectPosition);

        float distance = v1.magnitude; //min distance that will be reached

        //direction from target to point where shot is closest to target
        Vector3 v2 = Vector3.Cross(objectVelocity, v1);

        //nearest point
        Vector3 p = targetPosition + v2.normalized * distance;

        //InverseLerp delivers t for value between min and max
        float volume =
            _sound.GetVolume(_rollOffCurve.Evaluate(Mathf.InverseLerp(_rollOffDistanceMinMax.x, _rollOffDistanceMinMax.y,
                distance)));
        float delay = Vector3.Distance(currentObjectPosition, p) / objectVelocity.magnitude - _halfClipLength;

        if (volume > 0) {
            _source.volume = volume;
            _source.PlayDelayed(delay);
        }
    }
}
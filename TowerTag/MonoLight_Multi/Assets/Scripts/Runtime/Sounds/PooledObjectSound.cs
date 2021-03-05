using UnityEngine;

public class PooledObjectSound : MonoBehaviour {
    [SerializeField] private AudioSource _source;
    [SerializeField] private string _soundName;
    [SerializeField] private bool _useRandomSoundWithName;

    private Sound _sound;
    private bool _initLater;

    private void Awake() {
        InitSound();
    }

    private void Start() {
        if (_initLater) {
            InitSound();
        }
    }

    private void OnEnable() {
        if (_useRandomSoundWithName || _initLater) {
            InitSound();
        }

        if (!_source.isPlaying) {
            _source.Play();
        }
    }

    private void OnDisable() {
        if (_source.isPlaying) {
            _source.Stop();
        }
    }

    private void InitSound() {
        if (SoundDatabase.Instance != null) {
            _initLater = false;
            _sound = _useRandomSoundWithName ? SoundDatabase.Instance.GetRandomSound(_soundName) : SoundDatabase.Instance.GetSound(_soundName);

            if (_sound == null) {
                Debug.LogWarning("Could not find Sound (" + _soundName + ") in Database!");
                return;
            }

            _sound.InitSource(_source);
            _source.Play();
        }
        else {
            _initLater = true;
        }
    }
}
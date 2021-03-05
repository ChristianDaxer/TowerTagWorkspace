using UnityEngine;

public class GunDisabledSound : MonoBehaviour {
    [SerializeField] private AudioSource _shootSource;
    [SerializeField] private string _soundName;

    private Sound _sound;

    private void Awake()
    {
        _sound = SoundDatabase.Instance.GetSound(_soundName);
        if (_sound == null) {
            Debug.LogWarning("Could not find Sound (" + _soundName + ") in Database!");
            enabled = false;
        }
    }
    public void Play() {

        if (_shootSource.clip != _sound.Clip) {
            _shootSource.clip = _sound.Clip;
        }

        if (!_shootSource.isPlaying)
            _sound.InitSource(_shootSource);
        _shootSource.Play();
        }
}
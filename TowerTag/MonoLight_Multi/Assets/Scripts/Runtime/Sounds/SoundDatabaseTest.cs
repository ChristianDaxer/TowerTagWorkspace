using UnityEngine;

[ExecuteInEditMode]
public class SoundDatabaseTest : MonoBehaviour {
    // GUI To manipulate Source & Trigger Sound
    [SerializeField] private SoundDatabase _soundDatabase;
    [SerializeField] private AudioSource _source;
    private string _soundName;

    public string SoundName {
        get => _soundName;
        set {
            _soundName = value;
            SetSound();
        }
    }

    public float Volume {
        get => _volume;
        private set => _volume = value;
    }

    public float Pitch {
        get => _pitch;
        private set => _pitch = value;
    }

    public SoundDatabase SoundDatabase => _soundDatabase;

    [SerializeField] private float _volume = 1;
    [SerializeField] private float _pitch = 1;
    [SerializeField] private Sound _sound;

    public void SetSound() {
        SoundDatabase.Init();
        _sound = SoundDatabase.GetSound(_soundName);

        if (_sound != null) {
            _sound.InitSource(_source);

            Volume = Mathf.InverseLerp(_sound.MinMaxVolume.x, _sound.MinMaxVolume.y, _sound.DefaultVolume);
            Pitch = Mathf.InverseLerp(_sound.MinMaxPitch.x, _sound.MinMaxPitch.y, _sound.DefaultPitch);
        }
    }

    public void ChangeVolume(float vol01) {
        Volume = vol01;
        _source.volume = _sound.GetVolume(vol01);
    }

    public void ChangePitch(float pitch01) {
        Pitch = pitch01;
        _source.pitch = _sound.GetPitch(pitch01);
    }

    public void Play() {
        _source.Play();
    }

    public void Stop() {
        _source.Stop();
    }
}
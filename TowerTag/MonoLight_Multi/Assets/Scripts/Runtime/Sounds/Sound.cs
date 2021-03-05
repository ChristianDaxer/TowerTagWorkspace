using System;
using UnityEngine;
using UnityEngine.Audio;

// todo make this a scriptable object. Get rid of sound database. reference sounds directly, not by name.
[Serializable]
public class Sound : IDisposable {
    [SerializeField] private string _name;
    [SerializeField] private AudioClip _clip;
    [SerializeField] private AudioMixerGroup _outputTo;
    [SerializeField] private bool _playOnAwake = true;
    [SerializeField] private bool _loop;
    [SerializeField] private int _priority = 128;
    [SerializeField] private float _defaultVolume = 1f;
    [SerializeField] private Vector2 _minMaxVolume = Vector2.one;
    [SerializeField] private float _defaultPitch = 1f;
    [SerializeField] private Vector2 _minMaxPitch = Vector2.one;
    [SerializeField] private float _spatialBlend = 1f;
    [SerializeField] private AudioRolloffMode _rolloffMode = AudioRolloffMode.Logarithmic;
    [SerializeField] private Vector2 _minMaxDistance = new Vector2(1, 150);

    public string Name => _name;
    public AudioClip Clip => _clip;
    public float DefaultVolume => _defaultVolume;
    public Vector2 MinMaxVolume => _minMaxVolume;
    public float DefaultPitch => _defaultPitch;
    public Vector2 MinMaxPitch => _minMaxPitch;

    public void InitSource(AudioSource source) {
        if (source == null) {
            Debug.LogWarning($"SoundDatabase.InitSource: can't init Sound({_name})AudioSource is null!");
            return;
        }

        if (_clip == null) {
            Debug.LogError(
                $"Sound.InitSource: can't play sound({_name ?? "-"}) because the audio clip is null! " +
                "Did you forget to set an audio clip for this sound?");
        }

        source.volume = _defaultVolume;
        source.pitch = _defaultPitch;
        source.clip = _clip;
        source.loop = _loop;
        source.rolloffMode = _rolloffMode;
        source.minDistance = _minMaxDistance.x;
        source.maxDistance = _minMaxDistance.y;
        source.spatialBlend = _spatialBlend;
        source.priority = _priority;
        source.playOnAwake = _playOnAwake;
        source.outputAudioMixerGroup = _outputTo;
    }

    public float GetVolume(float volume01) {
        return _minMaxVolume.x + volume01 * (_minMaxVolume.y - _minMaxVolume.x);
    }

    public float GetPitch(float pitch01) {
        return _minMaxPitch.x + pitch01 * (_minMaxPitch.y - _minMaxPitch.x);
    }

    public void Dispose() {
        _name = null;
        _clip = null;
        _outputTo = null;
    }
}
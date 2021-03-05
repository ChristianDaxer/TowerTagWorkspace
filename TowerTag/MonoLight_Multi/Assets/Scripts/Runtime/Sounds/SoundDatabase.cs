using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class SoundDatabase : MonoBehaviour {
    [SerializeField] private Sound[] _sounds;
    private readonly Dictionary<string, Sound[]> _soundDatabase = new Dictionary<string, Sound[]>();
    public static SoundDatabase Instance { get; private set; }

    private void Awake() {
        Init();
    }

    public void Init() {
        Instance = this;

        _soundDatabase.Clear();
        foreach (Sound sound in _sounds) {
            if (!_soundDatabase.ContainsKey(sound.Name)) {
                _soundDatabase.Add(sound.Name, new[] {sound});
            }
            else {
                Sound[] tmp = _soundDatabase[sound.Name];
                Array.Resize(ref tmp, tmp.Length + 1);
                tmp[tmp.Length - 1] = sound;
                _soundDatabase[sound.Name] = tmp;
            }
        }
    }

    public void OnDestroy() {
        _soundDatabase.Clear();
        _sounds = null;
        if (Instance == this) {
            Instance = null;
        }
    }

    public Sound GetSound(string soundName) {
        if (string.IsNullOrEmpty(soundName)) {
            Debug.LogError("SoundDatabase.GetSound: can't return sound because soundName is null or empty!");
            return null;
        }

        if (_soundDatabase.ContainsKey(soundName)) {
            return _soundDatabase[soundName][0];
        }

        Debug.LogError("SoundDatabase.GetSound: can't return sound because there are no sounds with given Name!");
        return null;
    }

    public Sound GetRandomSound(string soundName) {
        if (string.IsNullOrEmpty(soundName)) {
            Debug.LogError("SoundDatabase.GetRandomSound: can't return sound because soundName is null or empty!");
            return null;
        }

        if (_soundDatabase.ContainsKey(soundName)) {
            Sound[] tmp = _soundDatabase[soundName];
            return tmp[Random.Range(0, tmp.Length)];
        }

        Debug.LogError("SoundDatabase.GetRandomSound: can't return sound because there are no sounds with given Name!");
        return null;
    }

    public Sound[] GetSounds(string soundName) {
        if (string.IsNullOrEmpty(soundName)) {
            Debug.LogError("SoundDatabase.GetSounds: can't return sound because soundName is null or empty!");
            return null;
        }

        if (_soundDatabase.ContainsKey(soundName)) {
            return _soundDatabase[soundName];
        }

        Debug.LogError("SoundDatabase.GetSounds: can't return sound because there are no sounds with given Name!");
        return null;
    }

    public Sound[] GetAllSounds() {
        return _sounds;
    }


    public void PlaySound(AudioSource source, string soundName) {
        PlaySound(source, GetSound(soundName));
    }

    public void PlaySound(AudioSource[] sources, string soundName) {
        if (sources == null) {
            Debug.LogError($"Cannot play sound {soundName}: audio sources are null");
            return;
        }

        Sound sound = GetSound(soundName);
        if (sources.Length == 1) {
            PlaySound(sources[0], sound);
        }
        else {
            foreach (AudioSource source in sources) {
                PlaySound(source, sound);
            }
        }
    }

    private static void PlaySound(AudioSource source, Sound sound) {
        if (source == null) {
            Debug.LogError($"Cannot play sound {sound?.Name}: audio source is null");
            return;
        }

        if (sound != null) {
            if (source.isPlaying) {
                source.Stop();
            }

            sound.InitSource(source);
            source.Play();
        }
    }

    public void PlayRandomSoundWithName(AudioSource source, string soundName) {
        PlaySound(source, GetRandomSound(soundName));
    }

    public void PlayRandomSoundWithName(AudioSource[] sources, string soundName) {
        if (sources == null) {
            Debug.LogError($"Cannot play sound {soundName}: audio sources are null");
            return;
        }

        Sound sound = GetRandomSound(soundName);
        if (sources.Length == 1) {
            PlaySound(sources[0], sound);
        }
        else {
            foreach (AudioSource source in sources) {
                PlaySound(source, sound);
            }
        }
    }
}
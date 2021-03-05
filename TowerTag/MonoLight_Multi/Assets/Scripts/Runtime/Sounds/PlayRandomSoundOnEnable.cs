using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayRandomSoundOnEnable : MonoBehaviour {
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip[] _audioClips;

    private void OnEnable() {
        _source.clip = _audioClips[Random.Range(0, _audioClips.Length)];
        _source.Play();
    }
}
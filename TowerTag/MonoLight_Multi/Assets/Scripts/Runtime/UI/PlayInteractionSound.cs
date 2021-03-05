using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayInteractionSound : MonoBehaviour {
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioClip _interactionSound;
    private Vector2 _lastScrollViewPosition = Vector2.zero;
    private const float TickSoundTriggerDistance = 0.01f;

    public void PlaySimpleUIInteractionSound() {
        //Maybe just remove early return?
        // if (!TowerTagSettings.Home) return;
        if (_source.isPlaying) return;
        _source.clip = _interactionSound;
        _source.Play();
    }

    [UsedImplicitly]
    public void PlayScrollSound(Vector2 changeVector) {
        if (Vector2.Distance(_lastScrollViewPosition, changeVector) > TickSoundTriggerDistance) {
            PlaySimpleUIInteractionSound();
            _lastScrollViewPosition = changeVector;
        }
    }
}
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    [SerializeField] private AudioSource _audio;

    public void PlaySound()
    {
        if(!_audio.isPlaying)
            _audio.Play();
    }
}

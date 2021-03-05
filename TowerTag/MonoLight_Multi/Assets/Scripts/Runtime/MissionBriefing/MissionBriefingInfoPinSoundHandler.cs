using JetBrains.Annotations;
using UnityEngine;

public class MissionBriefingSoundHandler : MonoBehaviour
{
    AudioSource _audio;

    [UsedImplicitly] // used from animation event
    private void PlaySound()
    {
        _audio = GetComponent<AudioSource>();
        _audio.Play();
    }
}

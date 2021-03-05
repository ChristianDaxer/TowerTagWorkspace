using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(AudioSource))]
public class MissionBriefingAnnouncerSoundHandler : MonoBehaviour {
    [Header("Announcers"), SerializeField] private AudioClip _elimination;
    [FormerlySerializedAs("_deatchmatch")] [SerializeField] private AudioClip _deathMatch;
    [SerializeField] private AudioClip _goalTower;

    [Header("References")] [SerializeField]
    private AudioSource _source;

    public void Announce(GameMode mode) {
        // insert at a later point
        /*
        switch (mode)
        {
            case GameMode.Elimination:
                _source.clip = _elimination;
                break;
            case GameMode.GoalTower:
                _source.clip = _goalTower;
                break;
            case GameMode.DeathMatch:
                _source.clip = _deathMatch;
                break;
        }

        _source.Play();
        */
    }
}
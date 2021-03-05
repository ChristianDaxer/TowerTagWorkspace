using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Sounds/Soundtrack")]
public class Soundtrack : ScriptableObject {
    [SerializeField] private AudioClip[] _clips;
    [SerializeField] private float _volume;

    public float Volume => _volume;

    public AudioClip GetClipByLength(int matchDurationInMinutes) {
        return _clips.FirstOrDefault(clip => (int)(clip.length / 60)== matchDurationInMinutes);
    }
}

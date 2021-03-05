using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class OverrideSpatialBlend : MonoBehaviour {
    [SerializeField, Range(0, 1)] private float _spatialBlend;

    private void Start() {
        var audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.spatialBlend = _spatialBlend;
    }
}
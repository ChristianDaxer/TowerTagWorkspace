using UnityEngine;

public class ApplyTransform : MonoBehaviour {
    [SerializeField] private Transform _sourceValues;
    [SerializeField] private bool _applyPosition;
    [SerializeField] private bool _applyRotation;

    public Transform Source {
        set => _sourceValues = value;
    }

    private void Start() {
        ApplyValues();
    }

    private void LateUpdate() {
        ApplyValues();
    }

    private void ApplyValues() {
        if (_applyPosition)
            transform.position = _sourceValues.position;

        if (_applyRotation)
            transform.rotation = _sourceValues.rotation;
    }
}
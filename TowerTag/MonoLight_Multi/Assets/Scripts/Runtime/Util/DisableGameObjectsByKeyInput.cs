using UnityEngine;

public class DisableGameObjectsByKeyInput : MonoBehaviour {
    [SerializeField] private GameObject[] _objects;
    [SerializeField] private bool _startActive = true;
    [SerializeField] private KeyCode _keycode = KeyCode.C;

    private bool _currentActive;

    private void Start() {
        SetObjectsActive(_startActive);
    }

    private void Update() {
        if (Input.GetKeyUp(_keycode)) {
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation - only called when key up (one frame)
            SetObjectsActive(!_currentActive);
        }
    }

    private void SetObjectsActive(bool setActive) {
        _currentActive = setActive;
        foreach (GameObject obj in _objects) {
            // ReSharper disable once Unity.PerformanceCriticalCodeNullComparison - only called when key up (one frame)
            if (obj != null) {
                obj.SetActive(setActive);
            }
        }
    }
}
using UnityEngine;

public class LookToMainCam : MonoBehaviour {
    [SerializeField] private bool _onlyRotateAroundYAxis;
    [SerializeField] private bool _useCameraForward;

    Camera _mainCamera;

    public bool OnlyRotateAroundYAxis
    {
        set => _onlyRotateAroundYAxis = value;
    }

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void Update() {
        if (_mainCamera == null)
            return;

        var cameraTransform = _mainCamera.transform;
        Vector3 lookDirection = _useCameraForward ? -cameraTransform.forward : cameraTransform.position - transform.position;

        if (_onlyRotateAroundYAxis)
            lookDirection.y = 0;

        transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    }
}
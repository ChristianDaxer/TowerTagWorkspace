using UnityEngine;

public class MoveFreeCam : MonoBehaviour {

    [SerializeField, ReadOnly] private float _tmp;

    [SerializeField, Tooltip("The operator camera")] private GameObject _cam;
    [SerializeField, Range(0.1f,15)] private float _movementSpeed = 0.5f;
    [SerializeField, Range(10,50), Tooltip("Speed while you hold down space")] private float _spaceSpeed = 20;
    [SerializeField] private float _maxSpeed = 15;
    [SerializeField] private float _minSpeed = 0.5f;

    private void OnEnable() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update() {
        if (Input.GetKey(KeyCode.W)) {
            transform.position += _movementSpeed * Time.deltaTime * _cam.transform.forward;
        }
        if (Input.GetKey(KeyCode.S)) {
            transform.position -= _movementSpeed * Time.deltaTime * _cam.transform.forward;
        }
        if (Input.GetKey(KeyCode.D)) {
            transform.position += _movementSpeed * Time.deltaTime * _cam.transform.right;
        }
        if (Input.GetKey(KeyCode.A)) {
            transform.position -= _movementSpeed * Time.deltaTime * _cam.transform.right;
        }
        if (Input.GetKey(KeyCode.Q)) {
            transform.position -= _movementSpeed * Time.deltaTime * _cam.transform.up;
        }
        if (Input.GetKey(KeyCode.E)) {
            transform.position += _movementSpeed * Time.deltaTime * _cam.transform.up;
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
            if (_movementSpeed < _maxSpeed)
                _movementSpeed++;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
            if (_movementSpeed >_minSpeed)
                _movementSpeed--;
        }
        if (Input.GetKeyDown(KeyCode.Space)) {
            _tmp = _movementSpeed;
            _movementSpeed = _spaceSpeed;
        }
        if (Input.GetKeyUp(KeyCode.Space)) {
            _movementSpeed = _tmp;
        }
    }
}

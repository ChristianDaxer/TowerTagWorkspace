using UnityEngine;

public class LoadingIndicatorSpin : MonoBehaviour {
    [SerializeField] private float _rotationSpeed;

    private void Update() {
        transform.Rotate(transform.forward, Time.deltaTime * _rotationSpeed, Space.World);
    }
}
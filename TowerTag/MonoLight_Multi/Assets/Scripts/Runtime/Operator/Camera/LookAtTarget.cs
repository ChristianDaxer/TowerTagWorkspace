using UnityEngine;

public class LookAtTarget : MonoBehaviour {
    [SerializeField] private Transform _target;

    private Transform Target => _target;

    private void Update() {
        if(Target != null)
            transform.LookAt(Target);
    }
}

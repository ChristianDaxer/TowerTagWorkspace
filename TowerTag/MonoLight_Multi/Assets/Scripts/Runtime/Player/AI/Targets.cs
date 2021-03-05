using UnityEngine;

public class Targets : MonoBehaviour {
    [SerializeField] private Transform[] _prioritizedTargets;
    [SerializeField] private Transform _head;
    [SerializeField] private Transform _body;

    public Transform[] PrioritizedTargets => _prioritizedTargets;
    public Transform Head => _head;
    public Transform Body => _body;
}
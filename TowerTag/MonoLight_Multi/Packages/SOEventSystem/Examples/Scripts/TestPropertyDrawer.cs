using SOEventSystem.References;
using UnityEngine;

namespace SOEventSystem.Examples {
    public class TestPropertyDrawer : MonoBehaviour {
        [SerializeField] private FloatReference _float;
        [SerializeField] private BoolReference _bool;
        [SerializeField] private ObjectReference _object;
        [SerializeField] private IntReference _int;
        [SerializeField] private StringReference _string;
        [SerializeField] private Vector3Reference _vector3;
    }
}
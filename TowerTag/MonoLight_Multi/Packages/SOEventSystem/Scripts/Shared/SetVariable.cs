using UnityEngine;

namespace SOEventSystem.Shared {
    /// <summary>
    /// Component to automatically assign a value to a <see cref="SharedVariable{T}"/>.
    /// On Awake, finds a <see cref="Component"/> of the type of <see cref="_variable"/> and sets its value to it.
    /// </summary>
    public class SetVariable : MonoBehaviour {
        [SerializeField] private SharedVariable _variable;

        private void Awake() {
            Component component = GetComponent(_variable.ValueType);
            if (component == null) {
                Debug.LogError($"Failed to set variable {_variable.name}: GameObject with name \"{name}\""
                               + $" does not have a component of type {_variable.ValueType}");
            }
            _variable.SetUnsafe(this, component);
        }

        private void OnDestroy() {
            _variable.SetUnsafe(this, null);
        }
    }
}
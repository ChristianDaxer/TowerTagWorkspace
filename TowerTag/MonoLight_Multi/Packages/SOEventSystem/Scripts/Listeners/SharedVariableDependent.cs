using System.Linq;
using SOEventSystem.Shared;
using UnityEngine;

namespace SOEventSystem.Listeners {
    /// <summary>
    /// Steers the activity of a set of controlled objects based on the value of a <see cref="SharedVariable"/>.
    /// The selected <see cref="Policy"/> determined whether the objects will be active or inactive, depending on
    /// whether the value of the variable is contained in the provided list of values.
    ///
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// </summary>
    public class SharedVariableDependent : MonoBehaviour {
        public enum Policy {
            ActiveWhenContained,
            DeactivatedWhenContained
        }
    }

    public class SharedVariableDependent<T, TVariable> : SharedVariableDependent where TVariable : SharedVariable<T> {
        [SerializeField, Tooltip("These objects activity will be controlled by this behaviour.")]
        private GameObject[] _controlledObjects;

        [SerializeField, Tooltip("This variable value will determine the activity of the target objects.")]
        private TVariable _sharedVariable;

        [SerializeField, Tooltip("The variable value is checked against this list.")]
        private T[] _values;

        [SerializeField, Tooltip("The policy determines how the value affects the controlled objects activity.")]
        private Policy _controlPolicy;

        /// <summary>
        /// These objects activity will be controlled by this behaviour.
        /// </summary>
        public GameObject[] ControlledObjects {
            private get { return _controlledObjects; }
            set {
                _controlledObjects = value;
                UpdateActivityOfTargets(Variable);
            }
        }

        /// <summary>
        /// This variable value will determine the activity of the target objects.
        /// </summary>
        public TVariable Variable {
            private get { return _sharedVariable; }
            set {
                if (_sharedVariable != null) _sharedVariable.ValueChanged -= OnValueChanged;
                _sharedVariable = value;
                if (_sharedVariable != null) _sharedVariable.ValueChanged += OnValueChanged;
                UpdateActivityOfTargets(_sharedVariable);
            }
        }

        /// <summary>
        /// The variable value is checked against this list.
        /// </summary>
        public T[] Values {
            private get { return _values; }
            set {
                _values = value;
                UpdateActivityOfTargets(_sharedVariable);
            }
        }

        /// <summary>
        /// The policy determines how the value affects the controlled objects activity.
        /// </summary>
        public Policy ControlPolicy {
            private get { return _controlPolicy; }
            set {
                _controlPolicy = value;
                UpdateActivityOfTargets(_sharedVariable);
            }
        }

        private void OnEnable() {
            if (Variable != null) Variable.ValueChanged += OnValueChanged;
        }

        private void OnDisable() {
            if (Variable != null) Variable.ValueChanged -= OnValueChanged;
        }

        private void OnValueChanged(object sender, T value) {
            UpdateActivityOfTargets(value);
        }

        private void UpdateActivityOfTargets(T value) {
            foreach (GameObject controlledObject in ControlledObjects) {
                if (controlledObject == null) continue;
                if (ControlPolicy == Policy.ActiveWhenContained)
                    controlledObject.SetActive(value != null && Values != null && Values.Contains(value));
                if (ControlPolicy == Policy.DeactivatedWhenContained)
                    controlledObject.SetActive(value == null || Values == null || !Values.Contains(value));
            }
        }

        private void Start() {
            OnValueChanged(this, Variable);
        }

        /// <summary>
        /// Creates a <see cref="GameObject"/> with a <see cref="SharedVariableDependent{T,TVariable}"/> component.
        /// </summary>
        /// <param name="controlledObjects">These objects activity will be controlled by this behaviour.</param>
        /// <param name="sharedVariable">This variable value will determine the activity of the target objects.</param>
        /// <param name="values">The variable value is checked against this list.</param>
        /// <param name="policy">The policy determines how the value affects the controlled objects activity.</param>
        /// <typeparam name="TSharedVariableDependent">
        ///     The non-generic implementation of
        ///     <see cref="SharedVariableDependent{T,TVariable}"/>
        /// </typeparam>
        public static TSharedVariableDependent Create<TSharedVariableDependent>(
            GameObject[] controlledObjects,
            TVariable sharedVariable,
            T[] values,
            Policy policy) where TSharedVariableDependent : SharedVariableDependent<T, TVariable> {
            var sharedVariableDependent = new GameObject("SharedVariableDependent")
                .AddComponent<TSharedVariableDependent>();
            sharedVariableDependent.ControlledObjects = controlledObjects;
            sharedVariableDependent.Variable = sharedVariable;
            sharedVariableDependent.Values = values;
            sharedVariableDependent.ControlPolicy = policy;
            return sharedVariableDependent;
        }
    }
}
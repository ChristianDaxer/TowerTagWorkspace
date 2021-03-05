using System;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace SOEventSystem.Listeners {
    public abstract class SharedVariableListener : MonoBehaviour, ISharedVariableListener {
        public enum ListenerPolicy {
            ValueChanged,
            ValueSet
        }

        [SerializeField, Tooltip("Configures what event is listened to")]
        private ListenerPolicy _policy;

        public ListenerPolicy Policy {
            protected get { return _policy; }
            set { _policy = value; }
        }

        public abstract void ListenTo<T>(SharedVariable<T> variable);
        public abstract void StopListeningTo<T>(SharedVariable<T> sharedVariable);
    }

    /// <summary>
    /// An Event Listener for a SharedVariable. Use to trigger a UnityEvent as a response to a SharedVariable event. 
    /// A SharedVariable is a ScriptableObject wrapper for a variable with an associated event Action.
    /// <typeparam name="T">The type of the event parameter</typeparam>
    /// <typeparam name="TVariable">The type of the SharedVariable to listen to</typeparam>
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    [Serializable]
    public abstract class SharedVariableListener<T, TVariable, TResponse> : SharedVariableListener
        where TVariable : SharedVariable<T>
        where TResponse : UnityEvent<object, T>, new() {
        [SerializeField, Tooltip("The SharedVariable to listen to.")]
        private TVariable _sharedVariable;

        public TVariable SharedVariable {
            private get { return _sharedVariable; }
            set {
                StopListeningTo(_sharedVariable);
                _sharedVariable = value;
                ListenTo(_sharedVariable);
            }
        }

        [Header("Response takes sender and shared variable value as parameters")]
        [SerializeField, Tooltip("The UnityEvent raised in response to the GameEvent.")]
        protected TResponse _response = new TResponse();

        public TResponse Response {
            get { return _response; }
            set { _response = value; }
        }

        private void OnEnable() {
            ListenTo(SharedVariable);
        }

        private void OnDisable() {
            StopListeningTo(SharedVariable);
        }

        public override void ListenTo<T1>(SharedVariable<T1> variable) {
            var sharedVariable = variable as SharedVariable<T>;
            if (sharedVariable != null) {
                sharedVariable.ValueSet += OnValueSet;
                sharedVariable.ValueChanged += OnValueChanged;
            }
        }

        public override void StopListeningTo<T1>(SharedVariable<T1> variable) {
            var sharedVariable = variable as SharedVariable<T>;
            if (sharedVariable != null) {
                sharedVariable.ValueSet -= OnValueSet;
                sharedVariable.ValueChanged -= OnValueChanged;
            }
        }

        private void OnValueSet(object sender, T t) {
            if (Response != null && Policy == ListenerPolicy.ValueSet)
                Response.Invoke(sender, t);
        }

        private void OnValueChanged(object sender, T t) {
            if (Response != null && Policy == ListenerPolicy.ValueChanged)
                Response.Invoke(sender, t);
        }
    }
}
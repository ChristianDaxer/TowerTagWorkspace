using System;
using SOEventSystem.Addressable;
using SOEventSystem.Listeners;
using UnityEngine;

namespace SOEventSystem.Shared {
    /// <summary>
    /// Parent type for the generic <see cref="SharedVariable{T}"/>.
    /// This class' purpose is to allow for custom editors in Unity. Do not inherit.
    ///
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    public abstract class SharedVariable : AddressableAsset {
        [SerializeField, Tooltip("Toggle verbose logging.")]
        private bool _verbose;

        /// <summary>
        /// When true, the variable will log set calls.
        /// </summary>
        public bool Verbose { protected get { return _verbose; } set { _verbose = value; } }

        public abstract Type ValueType { get; }

        public abstract void SetUnsafe(object sender, object value);

        /// <summary>
        /// Raises the event that is raised on every value set.
        /// </summary>
        /// <param name="sender">The object that triggered the event</param>
        public abstract void RaiseSetEvent(object sender);

        /// <summary>
        /// Raises the event that is raised when the value is changed.
        /// </summary>
        /// <param name="sender">The object that triggered the event</param>
        public abstract void RaiseChangeEvent(object sender);

        public static TVariable Create<T, TVariable>(T value) where TVariable : SharedVariable<T> {
            var variable = CreateInstance<TVariable>();
            variable.Set(typeof(SharedVariable), value);
            return variable;
        }

        /// <summary>
        /// Registers the given <see cref="ISharedVariableListener"/> as a listener to this variable.
        /// </summary>
        /// <param name="listener">The listener that will listen to this variable.</param>
        public abstract void Register(ISharedVariableListener listener);

        /// <summary>
        /// Unregisters the given <see cref="ISharedVariableListener"/> as a listener to this variable.
        /// </summary>
        /// <param name="listener">The listener that will no longer listen to this variable.</param>
        public abstract void Unregister(ISharedVariableListener listener);
    }

    /// <summary>
    /// A scriptable object wrapper for a variable of type T.
    /// Can be used for shared state, as well as for events with one parameter.
    ///
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// <typeparam name="T">Type of the variable. Should be Serializable.</typeparam>
    public abstract class SharedVariable<T> : SharedVariable {
        /// <summary>
        /// This event is raised when the variable value was <see cref="Set"/>.
        /// The parameters are those passed to <see cref="Set"/>.
        /// </summary>
        public event Action<object, T> ValueSet;

        /// <summary>
        /// This event is raised when the <see cref="Value"/> of the variable changed.
        /// Whether the value changed is determined by <see cref="T"/>
        /// <see cref="Value"/>. <see cref="ValueSet"/> will also be called.
        /// </summary>
        public event Action<object, T> ValueChanged;

        [SerializeField] private T _value;

        /// <summary>
        /// The Value of the shared variable.
        /// </summary>
        public T Value { get { return _value; } private set { _value = value; } }

        public override Type ValueType => typeof(T);

        /// <summary>
        /// Sets the <see cref="Value"/> of the variable and fires <see cref="ValueSet"/>.
        /// If the value changed, also fires <see cref="ValueChanged"/>.
        ///
        /// Example: Set(this, new T());
        /// </summary>
        /// <param name="sender">The object calling this method</param>
        /// <param name="value">The new <see cref="Value"/> of the variable</param>
        public void Set(object sender, T value) {
            bool changed = Value == null && value != null
                          || Value != null && !Value.Equals(value);
            Value = value;
            ValueSet?.Invoke(sender, value);
            if (changed) ValueChanged?.Invoke(sender, value);
            if (Verbose && changed) {
                Debug.Log(sender + " changed " + name + " to " + Value);
            }

            if (Verbose && !changed) {
                Debug.Log(sender + " set " + name + " to the same value " + Value);
            }
        }

        public override void SetUnsafe(object sender, object value) {
            if (!(value is T)) {
                Debug.LogError($"Cannot set value of {name} to {value}");
            }
            Set(sender, (T) value);
        }

        /// <inheritdoc />
        public override void RaiseSetEvent(object sender) {
            ValueSet?.Invoke(sender, Value);
        }

        /// <inheritdoc />
        public override void RaiseChangeEvent(object sender) {
            ValueChanged?.Invoke(sender, Value);
        }

        /// <inheritdoc />
        public override void Register(ISharedVariableListener listener) {
            listener.ListenTo(this);
        }

        /// <inheritdoc />
        public override void Unregister(ISharedVariableListener listener) {
            listener.StopListeningTo(this);
        }

        /// <summary>
        /// Allow to access this shared variable as if it was of the wrapped type T.
        /// </summary>
        public static implicit operator T(SharedVariable<T> sharedVariable) {
            return sharedVariable == null ? default(T) : sharedVariable.Value;
        }

        /// <summary>
        /// Print the Type of the sharedVariable and its Value.
        /// </summary>
        public override string ToString() {
            return GetType().Name + ": " + Value;
        }
    }
}
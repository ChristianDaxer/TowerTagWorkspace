using System;
using SOEventSystem.Shared;
using UnityEngine;

namespace SOEventSystem.References {
    [Serializable]
    public abstract class SharedReference {
        [SerializeField] private bool _usePrivateValue = true;
        public bool UsePrivateValue { protected get { return _usePrivateValue; } set { _usePrivateValue = value; } }
    }

    [Serializable]
    public class SharedReference<T, TVariable> : SharedReference where TVariable : SharedVariable<T> {
        [SerializeField] private T _privateValue;
        public T Private { set { _privateValue = value; } private get { return _privateValue; } }
        [SerializeField] private TVariable _shared;

        public TVariable Shared {
            set { _shared = value; }
            private get {
                if (_shared == null) _shared = ScriptableObject.CreateInstance<TVariable>();
                return _shared;
            }
        }

        public T Value {
            get { return UsePrivateValue ? Private : Shared; }
            set {
                if (UsePrivateValue) Private = value;
                else Shared.Set(this, value);
            }
        }


        public static implicit operator T(SharedReference<T, TVariable> reference) {
            return reference.Value;
        }
    }
}
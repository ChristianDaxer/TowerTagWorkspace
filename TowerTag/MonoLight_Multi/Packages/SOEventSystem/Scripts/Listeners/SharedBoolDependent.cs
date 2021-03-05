using SOEventSystem.Shared;
using UnityEngine;

namespace SOEventSystem.Listeners {
    public class SharedBoolDependent : MonoBehaviour {
        public enum ActivityPolicy {
            ActiveWhenTrue,
            ActiveWhenFalse
        }

        [SerializeField, Tooltip("This object will be en/disabled depending on the shared bool")]
        private GameObject _target;

        public GameObject Target {
            private get { return _target; }
            set {
                _target = value;
                OnChanged(this, _sharedBool.Value);
            }
        }

        [SerializeField, Tooltip("This shared bool will drive the activeness of the target game object")]
        private SharedBool _sharedBool;

        [SerializeField, Tooltip("This determines how the shared bool drives the activeness of the target game object")]
        private ActivityPolicy _policy;

        public ActivityPolicy Policy {
            private get { return _policy; }
            set {
                _policy = value;
                if (_sharedBool != null) OnChanged(this, _sharedBool.Value);
            }
        }

        public SharedBool SharedBool {
            private get { return _sharedBool; }
            set {
                if (_sharedBool != null) _sharedBool.ValueChanged -= OnChanged;
                _sharedBool = value;
                if (_sharedBool != null) {
                    _sharedBool.ValueChanged += OnChanged;
                    OnChanged(this, _sharedBool.Value);
                }
            }
        }

        private void OnValidate() {
            if (SharedBool != null) OnChanged(this, SharedBool.Value);
        }

        private void Start() {
            if (SharedBool != null) OnChanged(this, SharedBool.Value);
        }

        private void OnEnable() {
            if (SharedBool != null) SharedBool.ValueChanged += OnChanged;
        }

        private void OnDisable() {
            if (SharedBool != null) SharedBool.ValueChanged -= OnChanged;
        }

        private void OnChanged(object sender, bool value) {
            if (_target != null) {
                Target.SetActive(Policy == ActivityPolicy.ActiveWhenTrue && value
                                 || Policy == ActivityPolicy.ActiveWhenFalse && !value);
            }
        }
    }
}
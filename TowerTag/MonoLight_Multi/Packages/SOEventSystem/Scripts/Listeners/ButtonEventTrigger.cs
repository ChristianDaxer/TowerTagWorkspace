using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.UI;

namespace SOEventSystem.Listeners {
    [RequireComponent(typeof(Button))]
    public class ButtonEventTrigger : MonoBehaviour {
        [SerializeField] private SharedEvent _sharedEvent;
        private Button _button;
        public SharedEvent SharedEvent { set { _sharedEvent = value; } }

        private void Start() {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked() {
            if (_sharedEvent != null) {
                _sharedEvent.Trigger(this);
            }
        }
    }
}
using System;
using SOEventSystem.Shared;
using UnityEngine;
using UnityEngine.Events;

namespace SOEventSystem.Listeners {
    /// <summary>
    /// An Event Listener for a SharedEvent. Use to trigger actions as a response to a SharedEvent. 
    /// A SharedEvent is a ScriptableObject wrapper for an event Action.
    /// </summary>
    public class SharedEventListener : MonoBehaviour {
        [SerializeField, Tooltip("The SharedVariable to listen to.")]
        private SharedEvent _sharedEvent;

        [Header("Response takes sender as parameter")]
        [SerializeField, Tooltip("The UnityEvent raised in response to the GameEvent.")]
        private GameUnityEvent _response;

        private void OnEnable() {
            _sharedEvent.Triggered += OnEventRaised;
        }

        private void OnDisable() {
            _sharedEvent.Triggered -= OnEventRaised;
        }

        private void OnEventRaised(object sender) {
            if (_response != null) _response.Invoke(sender);
        }
    }

    [Serializable]
    public class GameUnityEvent : UnityEvent<object> { }
}
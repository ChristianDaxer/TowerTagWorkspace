using System;
using UnityEngine;

namespace SOEventSystem.Shared {
    /// <summary>
    /// An event that can be referenced, listened to, and triggered from multiple components throughout the project.
    /// 
    /// Technically, a <see cref="SharedEvent"/> is a <see cref="UnityEngine.ScriptableObject"/> wrapper for an
    /// <see cref="Action{T}"/> where the parameter is usually the caller object of the event.
    ///
    /// Use <see cref="SOEventSystem.Listeners.SharedEventListener"/> to listen to the event.
    /// You can also directly subscribe to <see cref="Triggered"/>.
    /// To fire the event, call <see cref="Trigger"/>.
    ///
    /// </summary>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    [CreateAssetMenu(menuName = "Shared/event", order = int.MinValue)]
    public class SharedEvent : ScriptableObject {
        [SerializeField, Tooltip("Toggle verbose logging.")]
        private bool _debug;

        public event Action<object> Triggered;

        /// <summary>
        /// When true, the event will log calls to <see cref="Trigger"/>.
        /// </summary>
        public bool Debug {
            private get { return _debug; }
            set { _debug = value; }
        }

        /// <summary>
        /// Triggers <see cref="Triggered"/> with the passed sender as parameter.
        /// </summary>
        /// <param name="sender"></param>
        public void Trigger(object sender) {
            if (Debug) {
                UnityEngine.Debug.Log(sender + " triggered " + name);
            }

            Triggered?.Invoke(sender);
        }
    }
}
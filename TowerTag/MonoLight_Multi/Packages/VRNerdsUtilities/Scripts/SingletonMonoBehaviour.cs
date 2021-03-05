using System;
using UnityEngine;

namespace VRNerdsUtilities {
    public enum DuplicatePolicy {
        DestroyNewInstance,
        DestroyExistingInstance,
        DestroyNewGameObject,
        DestroyExistingGameObject,
        IgnoreNewInstance,
        IgnoreExistingInstance
    }

    public abstract class SingletonMonoBehaviour : MonoBehaviour {
        protected static object InstanceLock { get; } = new object();
        protected static bool ApplicationIsQuitting { get; set; }
        [Header("Singleton")] [SerializeField] private bool _persistent;
        protected bool Persistent => _persistent;
        [SerializeField] private DuplicatePolicy _duplicatePolicy = DuplicatePolicy.DestroyNewInstance;
        protected DuplicatePolicy DuplicatePolicy => _duplicatePolicy;
    }

    public class SingletonMonoBehaviour<T> : SingletonMonoBehaviour where T : SingletonMonoBehaviour<T> {
        private static T _instance;
        private static bool Instantiated => _instance != null;

        /// <summary>
        /// Singleton instance for MonoBehaviour.
        /// </summary>
        public static T Instance {
            get {
                lock (InstanceLock) {
                    if (ApplicationIsQuitting) {
                        // do not recreate singleton instances during application quit
                        return null;
                    }

                    if (!Instantiated) {
                        T[] objects = FindObjectsOfType<T>();
                        if (objects == null || objects.Length < 1) {
                            var singleton = new GameObject {name = $"{typeof(T)} singleton"};
                            _instance = singleton.AddComponent<T>();
                            Debug.LogWarning($"An instance of {typeof(T)} was automatically created in the scene.");
                        }
                        else if (objects.Length >= 1) {
                            _instance = objects[0];
                            if (objects.Length > 1) {
                                Debug.LogWarning($"{objects.Length} instances found. Using first one only.");
                            }
                        }
                    }

                    return _instance;
                }
            }
            private set {
                lock (InstanceLock) {
                    if (Instantiated) {
                        if (value != null && _instance.GetInstanceID() != value.GetInstanceID()) {
                            Debug.LogWarning($"An instance of {typeof(T)} is already set.");

                            switch (_instance.DuplicatePolicy) {
                                case DuplicatePolicy.IgnoreNewInstance:
                                    break;
                                case DuplicatePolicy.DestroyNewInstance:
                                    Destroy(value);
                                    break;
                                case DuplicatePolicy.IgnoreExistingInstance:
                                    _instance = value;
                                    break;
                                case DuplicatePolicy.DestroyExistingInstance:
                                    Destroy(_instance);
                                    _instance = value;
                                    break;
                                case DuplicatePolicy.DestroyNewGameObject:
                                    Destroy(value.gameObject);
                                    break;
                                case DuplicatePolicy.DestroyExistingGameObject:
                                    Destroy(_instance.gameObject);
                                    _instance = value;
                                    break;
                                default:
                                    throw new NotSupportedException(
                                        $"Unsupported duplicate policy {_instance.DuplicatePolicy}");
                            }
                        }

                        return;
                    }

                    _instance = value;
                }
            }
        }

        protected virtual void Awake() {
            Instance = (T) this;
            if (Persistent && transform.parent == null) GameObject.DontDestroyOnLoad(gameObject);
        }

        protected virtual void OnApplicationQuit() {
            ApplicationIsQuitting = true;
        }
    }
}
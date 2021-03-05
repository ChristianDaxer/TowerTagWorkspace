using System;
using System.Collections.Generic;

namespace SOEventSystem.Shared {

    /// <summary>
    /// A shared dictionary.
    /// </summary>
    /// <typeparam name="TKey">Type of dictionary keys</typeparam>
    /// <typeparam name="TValue">Type of dictionary values</typeparam>
    public abstract class SharedDictionary<TKey, TValue> : SharedVariable<Dictionary<TKey, TValue>> {
        public event Action<object, TKey, TValue> ItemAdded;
        public event Action<object, TKey, TValue> ItemRemoved;
        public event Action<object> DictionaryCleared;

        private void InitDictionary() {
            Set(this, new Dictionary<TKey, TValue>());
            if (DictionaryCleared != null) {
                if (Verbose) {
                    UnityEngine.Debug.Log("Initialized shared Dictionary< " + typeof(TKey) + ", " + typeof(TValue) + "> " + name);
                }

                DictionaryCleared.Invoke(this);
            }
        }

        /// <summary>
        /// Add item to shared dictionary.
        /// </summary>
        /// <param name="sender">Sender object that adds the item</param>
        /// <param name="key">Dictionary key of the item</param>
        /// <param name="item">The item to add</param>
        public void Add(object sender, TKey key, TValue item) {
            if (Value == null) {
                InitDictionary();
            }

            if (key == null || item == null) {
                if (Verbose) {
                    UnityEngine.Debug.Log(sender + " attempted to add invalid data (" + key + ", " + item +
                              ") to shared dictionary " + name);
                }

                return;
            }

            if (Value.ContainsKey(key)) {
                if (Verbose) {
                    UnityEngine.Debug.Log(sender + " attempted to add item with already present key" + key +
                              " to shared dictionary " + name);
                }

                return;
            }

            Value.Add(key, item);
            if (Verbose) {
                UnityEngine.Debug.Log(sender + " added (" + key + ", " + item + ") to shared dictionary" + name);
            }

            ItemAdded?.Invoke(sender, key, item);
        }

        /// <summary>
        /// Remove an item from the shared dictionary.
        /// </summary>
        /// <param name="sender">Sender object that removes the item</param>
        /// <param name="key">The key of the item to remove</param>
        public void Remove(object sender, TKey key) {
            if (Value == null) {
                InitDictionary();
            }

            TValue value;
            Value.TryGetValue(key, out value);
            if (value == null) {
                if (Verbose) {
                    UnityEngine.Debug.Log(sender + " attempted to remove item with absent key " + key + " from shared dictionary " +
                              name);
                }

                return;
            }

            Value.Remove(key);
            if (Verbose) {
                UnityEngine.Debug.Log(sender + " removed item with key " + key + " from shared dictionary " + name);
            }

            ItemRemoved?.Invoke(sender, key, value);
        }

        /// <summary>
        /// Clear the dictionary.
        /// </summary>
        /// <param name="sender">Sender object that clears the dictionary</param>
        public void Clear(object sender) {
            if (Value == null) {
                InitDictionary();
            }
            else {
                Value.Clear();
            }

            if (Verbose) {
                UnityEngine.Debug.Log(sender + " cleared dictionary " + name);
            }

            DictionaryCleared?.Invoke(sender);
        }
    }
}
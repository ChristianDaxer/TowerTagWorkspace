using System;
using System.Collections.Generic;

namespace SOEventSystem.Shared {
    /// <summary>
    /// A shared list to store items in.
    /// </summary>
    /// <typeparam name="T">Type of list items</typeparam>
    public abstract class SharedList<T> : SharedVariable<List<T>> {
        public event Action<object, T> ItemAdded;
        public event Action<object, T> ItemRemoved;
        public event Action<object> ListCleared;

        private new void OnEnable() {
            base.OnEnable();
            if (Value == null) InitList();
        }

        private void InitList() {
            Set(this, new List<T>());
            if (ListCleared != null) {
                if (Verbose) {
                    UnityEngine.Debug.Log("Initialized shared List< " + typeof(T) + "> " + name);
                }

                ListCleared.Invoke(this);
            }
        }

        /// <summary>
        /// Add an item to the shared list.
        /// </summary>
        /// <param name="sender">Sender object that adds the item</param>
        /// <param name="item">Item to add</param>
        public void Add(object sender, T item) {
            if (Value == null) InitList();
            if (Value.Contains(item)) {
                if (Verbose) {
                    UnityEngine.Debug.Log(sender + " attempted to add already present item " + item +
                                          " to shared list " + name);
                }

                return;
            }

            Value.Add(item);
            if (Verbose) {
                UnityEngine.Debug.Log(sender + " added " + item + " to shared list " + name);
            }

            ItemAdded?.Invoke(sender, item);
        }

        public void Insert(object sender, int index, T item) {
            if (Value == null) InitList();
            if (Value.Contains(item)) {
                if (Verbose) {
                    UnityEngine.Debug.Log(sender + " attempted to add already present item " + item +
                                          " to shared list " + name);
                }

                return;
            }

            Value.Insert(index, item);
            if (Verbose) {
                UnityEngine.Debug.Log(sender + " added " + item + " to shared list " + name);
            }

            ItemAdded?.Invoke(sender, item);
        }

        /// <summary>
        /// Remove an item from the shared list.
        /// </summary>
        /// <param name="sender">Sender object that removes the item</param>
        /// <param name="item">Item to remove</param>
        public void Remove(object sender, T item) {
            if (Value == null || !Value.Contains(item)) {
                if (Verbose) {
                    UnityEngine.Debug.Log(sender + " attempted to remove absent item " + item + " from shared list " +
                                          name);
                }

                return;
            }

            Value.Remove(item);
            if (Verbose) {
                UnityEngine.Debug.Log(sender + " removed " + item + " from shared list " + name);
            }

            ItemRemoved?.Invoke(sender, item);
        }

        /// <summary>
        /// Clear the shared list.
        /// </summary>
        /// <param name="sender">Sender object that clears the list</param>
        public void Clear(object sender) {
            if (Value == null) {
                InitList();
            }
            else {
                Value.Clear();
            }

            if (Verbose) {
                UnityEngine.Debug.Log(sender + " cleared list " + name);
            }

            ListCleared?.Invoke(sender);
        }
    }
}
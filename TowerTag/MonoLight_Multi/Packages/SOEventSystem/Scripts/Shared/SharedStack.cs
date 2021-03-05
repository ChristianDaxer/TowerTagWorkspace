using System;
using System.Collections.Generic;

namespace SOEventSystem.Shared {

    /// <summary>
    /// A shared stack to store items in.
    /// TODO: No Custom Editor available
    /// </summary>
    /// <typeparam name="T">Type of stack items</typeparam>
    public abstract class SharedStack<T> : SharedVariable<Stack<T>> {
        public event Action<object, T> ItemAdded;
        public event Action<object, T> ItemRemoved;
        public event Action<object> StackCleared;

        /// <summary>
        /// Initializing the stack by setting and clearing the value
        /// </summary>
        private void InitStack() {
            Set(this, new Stack<T>());
            if( StackCleared != null ) {
                if( Verbose ) {
                    UnityEngine.Debug.Log("Initialized shared stack< " + typeof(T) + "> " + name);
                }

                StackCleared.Invoke(this);
            }
        }

        /// <summary>
        /// Push an item to the shared stack.
        /// </summary>
        /// <param name="sender">Sender object that pushed the item</param>
        /// <param name="item">Item to push</param>
        public void Push(object sender, T item) {
            if( Value == null )
                InitStack();

            Value.Push(item);
            if( Verbose ) {
                UnityEngine.Debug.Log(sender + " added " + item + " to shared stack " + name);
            }

            ItemAdded?.Invoke(sender, item);
        }

        /// <summary>
        /// Pop an item from the shared stack.
        /// </summary>
        /// <param name="sender">Sender object that popped the item</param>
        public T Pop(object sender) {
            if( Value == null )
                InitStack();

            T item = Value.Pop();
            if( Verbose ) {
                UnityEngine.Debug.Log(sender + " popped " + item + " from shared stack " + name);
            }

            ItemRemoved?.Invoke(sender, item);

            return item;
        }

        /// <summary>
        /// Peek the top item of the shared stack.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns>The item on top of the shared stack</returns>
        public T Peek(object sender) {
            if( Value == null )
                InitStack();

            if( Verbose ) {
                UnityEngine.Debug.Log(sender + " peeked " + Value.Peek()
                + " from shared stack " + name);
            }

            return Value.Peek();
        }

        /// <summary>
        /// Clear the shared stack.
        /// </summary>
        /// <param name="sender">Sender object that clears the stack</param>
        public void Clear(object sender) {
            if( Value == null ) {
                InitStack();
            } else {
                Value.Clear();
            }

            if( Verbose ) {
                UnityEngine.Debug.Log(sender + " cleared stack " + name);
            }

            StackCleared?.Invoke(sender);
        }

    }
}
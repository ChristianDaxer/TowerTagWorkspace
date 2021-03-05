using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SOEventSystem.Addressable;
using UnityEditor;
using UnityEngine;

namespace SOEventSystem.Shared {
    /// <summary>
    /// A byte-serializable extension of <see cref="SharedVariable{T}"/>.
    /// By default, the variable value is synchronized using a <see cref="BinaryFormatter"/>. This can
    /// be changed by overriding <see cref="SerializeValue()"/> and <see cref="Deserialize"/>. The must complement
    /// each other so that the output of <see cref="SerializeValue()"/> can be processed by <see cref="Deserialize"/>. 
    /// </summary>
    /// <typeparam name="T">The underlying type of the <see cref="SharedVariable{T}"/></typeparam>
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    public abstract class SerializableSharedVariable<T> : SharedVariable<T> {
        /// <summary>
        /// Raised when <see cref="SerializedData"/> was updated. Happens after every value change.
        /// </summary>
        public event Action<string, byte[]> ValueSerialized;

        /// <summary>
        /// Byte-serialized value of the variable. Can be passed into <see cref="Set"/>.
        /// </summary>
        public byte[] SerializedData { get; private set; }

        protected new void OnEnable() {
            base.OnEnable();
            ValueChanged += SerializeValue;
            if (AddressableAssetDatabase.Singleton != null) AddressableAssetDatabase.Singleton.Register(this);
            SerializeValue();
        }

        protected new void OnDisable() {
            base.OnDisable();
            ValueChanged -= SerializeValue;
            if (AddressableAssetDatabase.Singleton != null) AddressableAssetDatabase.Singleton.Unregister(this);
        }

        private void SerializeValue(object sender, T value) {
            SerializedData = SerializeValue();
            ValueSerialized?.Invoke(AssetGuid, SerializedData);
        }

        /// <summary>
        /// Update the variable value with the deserialized value of the byte array.
        /// </summary>
        /// <param name="sender">Caller object.</param>
        /// <param name="byteArray">Byte-serialized data of the new variable value.</param>
        public void Set(object sender, byte[] byteArray) {
            T newValue = Deserialize(byteArray);
            Set(sender, newValue);
        }

        protected virtual byte[] SerializeValue() {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            if (Value != null) binaryFormatter.Serialize(memoryStream, Value);
            return memoryStream.ToArray();
        }

        protected virtual T Deserialize(byte[] byteArray) {
            var memoryStream = new MemoryStream();
            var binaryFormatter = new BinaryFormatter();
            memoryStream.Write(byteArray, 0, byteArray.Length);
            memoryStream.Position = 0;
            if (memoryStream.Length == 0) return default(T);
            return (T) binaryFormatter.Deserialize(memoryStream);
        }
    }
}
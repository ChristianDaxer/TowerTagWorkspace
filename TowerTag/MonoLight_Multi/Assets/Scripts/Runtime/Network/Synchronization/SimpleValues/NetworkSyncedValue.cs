using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Network {
    public abstract class NetworkSyncedValue : MonoBehaviour {
        public bool Dirty { get; protected set; }

        public void OnSerializeView(PhotonStream stream, PhotonMessageInfo info, bool forceSend) {
            if (stream.IsWriting) {
                Serialize(stream, info, forceSend);
                Dirty = false;
            }
            else {
                Deserialize(stream, info);
            }
        }

        protected abstract void Serialize(PhotonStream stream, PhotonMessageInfo info, bool forceSend);

        protected abstract void Deserialize(PhotonStream stream, PhotonMessageInfo info);
    }

    public abstract class NetworkSyncedValue<T> : NetworkSyncedValue {
        public T Value { get; private set; }

        public delegate void ValueChangeDelegate(NetworkSyncedValue<T> sender, T value);

        public event ValueChangeDelegate ValueChanged;

        protected override void Serialize(PhotonStream stream, PhotonMessageInfo info, bool forceSend) {
            WriteValue(stream);
        }

        protected virtual void WriteValue(PhotonStream stream) {
            stream.SendNext(Value);
        }

        protected override void Deserialize(PhotonStream stream, PhotonMessageInfo info) {
            T tmp = ReadValue(stream);
            if (!ValuesAreEqual(tmp, Value)) {
                Value = tmp;
                ValueChanged?.Invoke(this, Value);
            }
        }

        protected virtual T ReadValue(PhotonStream stream) {
            return (T) stream.ReceiveNext();
        }

        public void SetValue(T newValue) {
            if (!ValuesAreEqual(newValue, Value)) {
                Value = newValue;
                Dirty = true;
            }
        }

        protected virtual bool ValuesAreEqual(T a, T b) {
            return EqualityComparer<T>.Default.Equals(a, b);
        }
    }
}
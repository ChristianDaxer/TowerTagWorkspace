using Photon.Pun;
using UnityEngine;

namespace Network {
    /// <summary>
    /// This class handles the synchronization of an array of <see cref="NetworkSyncedValue"/> efficiently.
    /// On Awake, triggers an RPC that makes the owner send a full update for initialization.
    /// It implements the <see cref="IPunObservable"/> interface and is a registered PUN callback target.
    /// Therefore, <see cref="OnPhotonSerializeView"/> is called frequently and the <see cref="NetworkSyncedValue"/>s
    /// are serialized when dirty.
    /// </summary>
    public class NetworkValueSyncer : MonoBehaviourPun, IPunObservable {
        [SerializeField] private NetworkSyncedValue[] _valuesToSync;

        private int _maxValueCount;
        private bool _fullUpdateRequest;

        private const int DirtyBitMaskSize = 32;

        private void Awake() {
            if (_valuesToSync.Length > DirtyBitMaskSize) {
                Debug.LogError($"DirtyBits mask {DirtyBitMaskSize} is too small " +
                               $"for number of values to sync {_valuesToSync.Length}!");
            }

            _maxValueCount = Mathf.Min(DirtyBitMaskSize, _valuesToSync.Length);

            if (!photonView.IsMine) {
                Debug.Log("View: " + photonView.ViewID + " Send full sync request");
                photonView.RPC(nameof(OnSyncRequest), photonView.Owner);
            }
        }

        private void OnEnable() {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable() {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        [PunRPC]
        private void OnSyncRequest() {
            _fullUpdateRequest = true;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            var dirtyBits = 0;

            if (stream.IsWriting) {
                if (_fullUpdateRequest) {
                    dirtyBits = ~0;
                } else {
                    for (var i = 0; i < _maxValueCount; i++) {
                        if (_valuesToSync[i].Dirty) {
                            dirtyBits = dirtyBits | (1 << i);
                        }
                    }
                }

                stream.Serialize(ref dirtyBits);

                for (var i = 0; i < _maxValueCount; i++) {
                    if ((dirtyBits & (1 << i)) != 0) {
                        _valuesToSync[i].OnSerializeView(stream, info, _fullUpdateRequest);
                    }
                }

                _fullUpdateRequest = false;
            } else {
                stream.Serialize(ref dirtyBits);

                for (var i = 0; i < _maxValueCount; i++) {
                    if ((dirtyBits & (1 << i)) != 0) {
                        _valuesToSync[i].OnSerializeView(stream, info, false);
                    }
                }
            }
        }
    }
}
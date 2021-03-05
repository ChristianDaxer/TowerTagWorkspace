using Network;
using Network.InterpolateValues;
using Photon.Pun;
using UnityEngine;

public class SyncTransforms : NetworkSyncedValue {
    public delegate void PositionChangeDelegate(
        SyncTransforms syncTransforms, InterpolationTransformPhoton transform, Vector3 position);

    public delegate void RotationChangeDelegate(
        SyncTransforms syncTransforms, InterpolationTransformPhoton transform, Quaternion rotation);

    public event PositionChangeDelegate PositionChanged;
    public event RotationChangeDelegate RotationChanged;

    [SerializeField] private int _transformCount;
    [SerializeField] private double _interpolationDelayInSeconds = 0.15;
    [SerializeField] private float _minVector3DifferenceToSync = 0.000099f;
    [SerializeField] private float _minQuaternionAngleToSync = 1f;
    [SerializeField] private Transform _rootTransform;
    [SerializeField] private float _lerpSpeed;

    private InterpolationTransformPhoton[] _interpolationTransforms;
    private int _dirtyBits;

    private void Awake() {
        InitSyncTransformArray(_transformCount);
    }

    private void InitSyncTransformArray(int transformCount) {
        _interpolationTransforms = new InterpolationTransformPhoton[transformCount];
        for (var i = 0; i < _interpolationTransforms.Length; i++) {
            _interpolationTransforms[i] = new InterpolationTransformPhoton(5, 1.25 / PhotonNetwork.SerializationRate);
        }
    }

    protected override void Serialize(PhotonStream stream, PhotonMessageInfo info, bool forceSend) {
        if (forceSend)
            _dirtyBits = ~0;

        if (_dirtyBits == 0)
            return;

        // send only dirty
        stream.SendNext(_dirtyBits);
        for (var i = 0; i < _interpolationTransforms.Length; i++) {
            int bit = 1 << (i * 2);

            InterpolationTransformPhoton interpolationTransform = _interpolationTransforms[i];
            if ((bit & _dirtyBits) != 0) {
                stream.SendNext(interpolationTransform.SendPosition);
                PositionChanged?.Invoke(this, interpolationTransform, interpolationTransform.SendPosition);
            }

            bit = bit << 1;
            if ((bit & _dirtyBits) != 0) {
                stream.SendNext(interpolationTransform.SendRotation);
                RotationChanged?.Invoke(this, interpolationTransform, interpolationTransform.SendRotation);
            }
        }

        _dirtyBits = 0;
    }

    protected override void Deserialize(PhotonStream stream, PhotonMessageInfo info) {
        var dirtyBits = (int) stream.ReceiveNext();

        if (dirtyBits == 0) {
            Debug.LogWarning("SyncTransforms:Empty dirtyBits received!");
            return;
        }

        for (var i = 0; i < _interpolationTransforms.Length; i++) {
            int bit = 1 << (i * 2);

            InterpolationTransformPhoton interpolationTransform = _interpolationTransforms[i];
            if ((bit & dirtyBits) != 0) {
                var pos = (Vector3) stream.ReceiveNext();
                interpolationTransform.SetPosition(info.SentServerTime, pos);
                PositionChanged?.Invoke(this, interpolationTransform, pos);
            }

            bit = bit << 1;
            if ((bit & dirtyBits) != 0) {
                var rot = (Quaternion) stream.ReceiveNext();
                interpolationTransform.SetRotation(info.SentServerTime, rot);
                RotationChanged?.Invoke(this, interpolationTransform, rot);
            }
        }
    }


    /// <summary>
    /// Read current values and mark changed as dirty
    /// </summary>
    public void ReadDataFromTransforms(Transform[] fromTransforms) {
        if (fromTransforms.Length != _interpolationTransforms.Length) {
            Debug.LogWarning("transformsArray and SyncedTransformsArray have different size");
            return;
        }

        for (var i = 0; i < fromTransforms.Length; i++) {
            //Debug.Log("Send: " + fromTransforms[i].position);
            Vector3 relPosition = _rootTransform.InverseTransformPoint(fromTransforms[i].position);
            if (Vector3.SqrMagnitude(_interpolationTransforms[i].SendPosition - relPosition) >
                _minVector3DifferenceToSync) {
                _interpolationTransforms[i].SendPosition = relPosition;
                _dirtyBits = _dirtyBits | (1 << (i * 2));
            }

            Quaternion relRotation = Quaternion.Inverse(_rootTransform.rotation) * fromTransforms[i].rotation;
            if (Quaternion.Angle(_interpolationTransforms[i].SendRotation, relRotation) > _minQuaternionAngleToSync) {
                _interpolationTransforms[i].SendRotation = relRotation;
                _dirtyBits = _dirtyBits | (1 << (i * 2 + 1));
            }
        }

        Dirty = _dirtyBits != 0;
    }


    /// <summary>
    /// Write interpolated values back to the transforms
    /// </summary>
    public void WriteSyncedDataToTransforms(Transform[] toTransforms) {
        if (_interpolationTransforms == null) {
            Debug.LogWarning("SyncedTransformsArray is not initialized!");
            return;
        }

        if (toTransforms.Length != _interpolationTransforms.Length) {
            Debug.LogWarning("transformsArray and SyncedTransformsArray have different size!");
            return;
        }

        double timestamp = PhotonNetwork.Time - _interpolationDelayInSeconds;

        for (var i = 0; i < toTransforms.Length; i++) {
            //Vector3 oldPos = toTransforms[i].position;
            Vector3 relPosition = _rootTransform.InverseTransformPoint(toTransforms[i].position);
            Quaternion rootTransformRotation = _rootTransform.rotation;
            Quaternion relRotation = Quaternion.Inverse(rootTransformRotation) * toTransforms[i].rotation;
            toTransforms[i].position =
                _rootTransform.TransformPoint(_interpolationTransforms[i]
                    .GetInterpolatedPosition(timestamp, relPosition));
            toTransforms[i].rotation = rootTransformRotation *
                                       _interpolationTransforms[i].GetInterpolatedRotation(timestamp, relRotation);

            //Debug.Log("Before: " + oldPos + "After: " + toTransforms[i].position);
        }
    }

    public void LerpRemoteTransforms(Transform[] toTransforms) {
        if (_interpolationTransforms == null) {
            Debug.LogWarning("SyncedTransformsArray is not initialized!");
            return;
        }

        if (toTransforms.Length != _interpolationTransforms.Length) {
            Debug.LogWarning("transformsArray and SyncedTransformsArray have different size!");
            return;
        }

        for (var i = 0; i < toTransforms.Length; i++) {
            Vector3 relPosition = _rootTransform.InverseTransformPoint(toTransforms[i].position);
            Quaternion rootTransformRotation = _rootTransform.rotation;
            Quaternion relRotation = Quaternion.Inverse(rootTransformRotation) * toTransforms[i].rotation;
            toTransforms[i].position = _rootTransform.TransformPoint(Vector3.Lerp(relPosition,
                _interpolationTransforms[i].SendPosition, Time.deltaTime * _lerpSpeed));
            toTransforms[i].rotation = rootTransformRotation * Quaternion.Lerp(relRotation,
                                           _interpolationTransforms[i].SendRotation, Time.deltaTime * _lerpSpeed);
        }
    }

    protected void OnDestroy() {
        PositionChanged = null;
        RotationChanged = null;
        _rootTransform = null;
        _interpolationTransforms = null;
    }
}
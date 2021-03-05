using UnityEngine;

namespace Network.InterpolateValues {
    public class InterpolationTransform {
        private Vector3 _position;
        private Vector3 _lastPosition;
        private float _lastPositionInterval = 1;
        private float _lastPositionUpdate;
        private Quaternion _rotation;
        private Quaternion _lastRotation;
        private float _lastRotationInterval = 1;
        private float _lastRotationUpdate;


        public InterpolationTransform() {
            _position = _lastPosition = Vector3.zero;
            _rotation = _lastRotation = Quaternion.identity;
            _lastPositionUpdate = Time.time;
            _lastRotationUpdate = Time.time;
        }

        public InterpolationTransform(Vector3 position, Quaternion rotation) {
            _position = _lastPosition = position;
            _rotation = _lastRotation = rotation;
            _lastPositionUpdate = Time.time;
            _lastRotationUpdate = Time.time;
        }

        public Vector3 RealPosition => _position;

        public Vector3 InterpolatedPosition {
            set {
                _lastPosition = _position;
                _position = value;

                _lastPositionInterval = Time.time - _lastPositionUpdate;
                _lastPositionUpdate = Time.time;
            }
            get {
                if (_lastPositionInterval > 0) {
                    return Vector3.Lerp(_lastPosition, _position,
                        (Time.time - _lastPositionUpdate) / _lastPositionInterval);
                }

                return _lastPosition;
            }
        }

        public Quaternion RealRotation => _rotation;

        public Quaternion InterpolatedRotation {
            set {
                _lastRotation = _rotation;
                _rotation = value;

                _lastRotationInterval = Time.time - _lastRotationUpdate;
                _lastRotationUpdate = Time.time;
            }
            get {
                if (_lastRotationInterval > 0) {
                    return Quaternion.Lerp(_lastRotation, _rotation,
                        (Time.time - _lastRotationUpdate) / _lastRotationInterval);
                }

                return _lastRotation;
            }
        }
    }
}
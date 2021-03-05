using UnityEngine;

namespace Network.InterpolateValues {
    public class InterpolationTransformPhoton {
        private const float FadeoutLerpFactor = 30f;
        private readonly double _maxSendInterval;

        private readonly int _bufferSize;

        private struct PositionPackage {
            public Vector3 Value;
            public double Timestamp;
        }

        private readonly PositionPackage[] _bufferedPositions;
        private int _positionPackageCount;
        private int _lastPositionIndex;

        private struct RotationPackage {
            public Quaternion Value;
            public double Timestamp;
        }

        private readonly RotationPackage[] _bufferedRotations;
        private int _rotationPackageCount;
        private int _lastRotationIndex;

        public InterpolationTransformPhoton(int bufferSize, double maxSendInterval) {
            _bufferSize = bufferSize;
            _maxSendInterval = maxSendInterval;

            _bufferedPositions = new PositionPackage[_bufferSize];
            _bufferedRotations = new RotationPackage[_bufferSize];

            for (var i = 0; i < _bufferedRotations.Length; i++) {
                _bufferedRotations[i].Value = Quaternion.identity;
            }
        }

        public Vector3 SendPosition {
            set {
                _bufferedPositions[_lastPositionIndex].Value = value;
            }
            get {
                return _bufferedPositions[_lastPositionIndex].Value;
            }
        }

        public void SetPosition(double timestamp, Vector3 newPosition) {
            if (timestamp < _bufferedPositions[_lastPositionIndex].Timestamp)
                return;

            PositionPackage current;
            current.Value = newPosition;
            current.Timestamp = timestamp;

            _lastPositionIndex = (_lastPositionIndex + 1) % _bufferSize;
            _positionPackageCount = Mathf.Min(_positionPackageCount + 1, _bufferSize);
            _bufferedPositions[_lastPositionIndex] = current;
        }

        public Vector3 GetInterpolatedPosition(double timestamp, Vector3 currentPosition) {
            Vector3 position = Vector3.zero;
            if (timestamp < _bufferedPositions[_lastPositionIndex].Timestamp) {
                // von neu zu alt
                for (var i = 0; i < _positionPackageCount; i++) {
                    int index = Mod(_lastPositionIndex - i, _positionPackageCount);
                    if (_bufferedPositions[index].Timestamp <= timestamp || i == (_positionPackageCount - 1)) {
                        PositionPackage last = _bufferedPositions[index];
                        PositionPackage current = _bufferedPositions[(index + 1) % _positionPackageCount];
                        double interval = current.Timestamp - last.Timestamp;
                        var delta = 0f;

                        if (interval > _maxSendInterval) {
                            delta = (float)((timestamp - (current.Timestamp - _maxSendInterval)) / _maxSendInterval);
                        } else if (interval > 0.0001) {
                            delta = (float)((timestamp - last.Timestamp) / interval);
                        }

                        return Vector3.Lerp(last.Value, current.Value, delta);
                    }
                }
            } else {
                position = Vector3.Lerp(currentPosition, _bufferedPositions[_lastPositionIndex].Value, Time.deltaTime * FadeoutLerpFactor);
            }

            return position;
        }

        public Quaternion SendRotation {
            set {
                _bufferedRotations[_lastRotationIndex].Value = value;
            }
            get {
                return _bufferedRotations[_lastRotationIndex].Value;
            }
        }

        public void SetRotation(double timestamp, Quaternion newRotation) {
            if (timestamp < _bufferedRotations[_lastRotationIndex].Timestamp)
                return;

            RotationPackage current;
            current.Value = newRotation;
            current.Timestamp = timestamp;

            _lastRotationIndex = (_lastRotationIndex + 1) % _bufferSize;
            _rotationPackageCount = Mathf.Min(_rotationPackageCount + 1, _bufferSize);
            _bufferedRotations[_lastRotationIndex] = current;
        }

        public Quaternion GetInterpolatedRotation(double timestamp, Quaternion currentRotation) {
            Quaternion rotation = Quaternion.identity;
            if (timestamp < _bufferedRotations[_lastRotationIndex].Timestamp) {
                // von neu zu alt
                for (var i = 0; i < _rotationPackageCount; i++) {
                    int index = Mod((_lastRotationIndex - i), _rotationPackageCount);
                    if (_bufferedRotations[index].Timestamp <= timestamp || i == (_rotationPackageCount - 1)) {
                        RotationPackage last = _bufferedRotations[index];
                        RotationPackage current = _bufferedRotations[(index + 1) % _rotationPackageCount];
                        double interval = current.Timestamp - last.Timestamp;
                        var delta = 0f;

                        if (interval > _maxSendInterval) {
                            delta = (float)((timestamp - (current.Timestamp - _maxSendInterval)) / _maxSendInterval);
                        } else if (interval > 0.0001) {
                            delta = (float)((timestamp - last.Timestamp) / interval);
                        }

                        return Quaternion.Lerp(last.Value, current.Value, delta);
                    }
                }
            } else {
                rotation = Quaternion.Lerp(currentRotation, _bufferedRotations[_lastRotationIndex].Value, Time.deltaTime * FadeoutLerpFactor);
            }

            return rotation;
        }

        private static int Mod(int value, int module) {
            int result = value % module;
            return result < 0 ? result + module : result;
        }
    }
}
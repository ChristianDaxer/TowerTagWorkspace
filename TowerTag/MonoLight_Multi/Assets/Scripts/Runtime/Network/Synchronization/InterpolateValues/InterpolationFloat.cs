using UnityEngine;

namespace Network.InterpolateValues {
    public class InterpolationFloat {
        private const float FadeoutLerpFactor = 30f;
        private readonly double _maxSendInterval;

        private readonly int _bufferSize;

        private struct ValuePackage {
            public float Value;
            public double Timestamp;
        }

        private readonly ValuePackage[] _bufferedPositions;
        private int _valuePackageCount;
        private int _lastValueIndex;

        public InterpolationFloat(int bufferSize, double maxSendInterval) {
            _bufferSize = bufferSize;
            _maxSendInterval = maxSendInterval;
            _bufferedPositions = new ValuePackage[_bufferSize];
        }

        public void AddValue(double timestamp, float newValue) {
            if (timestamp < _bufferedPositions[_lastValueIndex].Timestamp)
                return;

            ValuePackage current;
            current.Value = newValue;
            current.Timestamp = timestamp;

            _lastValueIndex = (_lastValueIndex + 1) % _bufferSize;
            _valuePackageCount = Mathf.Min(_valuePackageCount + 1, _bufferSize);
            _bufferedPositions[_lastValueIndex] = current;
        }

        public float GetInterpolatedValue(double timestamp, float currentValue) {
            float interpolatedValue = 0;
            if (timestamp < _bufferedPositions[_lastValueIndex].Timestamp) {
                // von neu zu alt
                for (var i = 0; i < _valuePackageCount; i++) {
                    int index = Mod(_lastValueIndex - i, _valuePackageCount);
                    if (_bufferedPositions[index].Timestamp <= timestamp || i == _valuePackageCount - 1) {
                        ValuePackage last = _bufferedPositions[index];
                        ValuePackage current = _bufferedPositions[(index + 1) % _valuePackageCount];
                        double interval = current.Timestamp - last.Timestamp;
                        var delta = 0f;

                        if (interval > _maxSendInterval) {
                            delta = (float)((timestamp - (current.Timestamp - _maxSendInterval)) / _maxSendInterval);
                        } else if (interval > 0.0001) {
                            delta = (float)((timestamp - last.Timestamp) / interval);
                        }

                        return Mathf.Lerp(last.Value, current.Value, delta);
                    }
                }
            } else {
                interpolatedValue = Mathf.Lerp(currentValue, _bufferedPositions[_lastValueIndex].Value, Time.deltaTime * FadeoutLerpFactor);
            }

            return interpolatedValue;
        }

        private static int Mod(int value, int module) {
            int result = value % module;
            return result < 0 ? result + module : result;
        }
    }
}

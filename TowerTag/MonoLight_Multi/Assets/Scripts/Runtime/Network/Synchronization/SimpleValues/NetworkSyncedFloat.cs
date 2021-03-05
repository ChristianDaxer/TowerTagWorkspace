using UnityEngine;

namespace Network {
    public class NetworkSyncedFloat : NetworkSyncedValue<float> {
        [SerializeField] private float _minDiffToSync = 0.01f;

        protected override bool ValuesAreEqual(float a, float b) {
            return Mathf.Abs(a - b) < _minDiffToSync;
        }
    }
}
using System;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace Home.UI {
    public class UIZLayerManager : MonoBehaviour {
        [Serializable]
        public class ZLayerToZValue : SerializableDictionaryBase<ZLayer, float> {
        }

        public enum ZLayer {
            Z0,
            Z1,
            Z2
        }

        [SerializeField] private ZLayerToZValue _zLayerToZValue;
        [SerializeField] private RectTransform[] _zLayer0;
        [SerializeField] private RectTransform[] _zLayer1;
        [SerializeField] private RectTransform[] _zLayer2;
        private const float FloatTolerance = 0.01f;


        private void OnValidate() {
            ValidateLayerValues(_zLayer0, ZLayer.Z0);
            ValidateLayerValues(_zLayer1, ZLayer.Z1);
            ValidateLayerValues(_zLayer2, ZLayer.Z2);
        }

        private void ValidateLayerValues(RectTransform[] transforms, ZLayer zLayer) {
            foreach (RectTransform layer in transforms) {
                if (layer == null) continue;
                if (HasTransformCorrectZValue(layer, zLayer)) {
                    Vector3 position = layer.anchoredPosition3D;
                    layer.anchoredPosition3D = new Vector3(position.x, position.y, _zLayerToZValue[zLayer]);
                }
            }
        }

        private bool HasTransformCorrectZValue(RectTransform layer, ZLayer zLayer) {
            return Math.Abs(layer.anchoredPosition3D.z - _zLayerToZValue[zLayer]) > FloatTolerance;
        }
    }
}
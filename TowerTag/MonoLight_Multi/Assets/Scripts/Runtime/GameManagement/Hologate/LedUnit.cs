using System;
using UnityEngine;

namespace Hologate {
    [Serializable]
    public class HologateLedSerialization {
        [Serializable]
        public enum Leds {
            Player01LeftLedBar,
            Player01RightLedBar,
            Player02LeftLedBar,
            Player02RightLedBar,
            Player03RightLedBar,
            Player03LeftLedBar,
            Player04RightLedBar,
            Player04LeftLedBar,
            PillarTopFront,
            PillarTopRight,
            PillarTopBack,
            PillarTopLeft,
            PillarBottomRight,
            PillarBottomLeft,
            PillarBottomFront,
            PillarBottomBack
        }

        [SerializeField] private Leds _ledDescription;

        public Leds LedDescription {
            get => _ledDescription;
            set => _ledDescription = value;
        }

        [SerializeField] private int _ledId;

        public int LedId {
            get => _ledId;
            set => _ledId = value;
        }
    }

    public class LedUnit : MonoBehaviour {
        [SerializeField] private HologateLedSerialization _ledSerialization;
        public HologateLedSerialization LedSerialization => _ledSerialization;

        [SerializeField] private Color _ledColor;

        public Color LedColor {
            get => _ledColor;
            set {
                if (value != _ledColor) {
                    _ledColor = value;
                    _light.color = _ledColor;
                }
            }
        }
        [SerializeField] private Light _light;
    }
}
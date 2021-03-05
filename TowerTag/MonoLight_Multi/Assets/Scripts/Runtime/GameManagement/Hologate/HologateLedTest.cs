using System;
using Hologate;
using UnityEngine;

public class HologateLedTest : MonoBehaviour {
    [Serializable]
    private struct PlaySpace {
        public int ID;
        public LedUnit LeftLed;
        public LedUnit RightLed;
        public Color LeftColor;
        public Color RightColor;
    }

    [SerializeField] private PlaySpace[] _playSpaces;

    [ContextMenu("SetPlaySpaceColor")]
    public void SetPlaySpaceColors() {
        _playSpaces.ForEach(playSpace => {
            playSpace.LeftLed.LedColor = playSpace.LeftColor;
            playSpace.RightLed.LedColor = playSpace.RightColor;
        });
    }

}

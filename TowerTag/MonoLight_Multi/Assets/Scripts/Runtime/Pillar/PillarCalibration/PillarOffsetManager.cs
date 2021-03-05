using System;
using TowerTagSOES;
using UnityEngine;

namespace Runtime.Pillar.PillarCalibration {
    public class PillarOffsetManager : MonoBehaviour {
        public enum PillarOffsetCalibrationMode {
            Position,
            Rotation,
            None
        }

        #region Singleton

        private static PillarOffsetManager _instance;

        public static PillarOffsetManager Instance {
            get {
                if (_instance != null)
                    return _instance;

                try {
                    var pillarOffsetManager = FindObjectOfType<PillarOffsetManager>();
                    if (pillarOffsetManager == null) {
                        if (PlayerManager.Instance.GetOwnPlayer() == null || !SharedControllerType.VR) {
                            Debug.LogError("cant find local vr player");
                            return null;
                        }
                    }

                    _instance = pillarOffsetManager;
                    return _instance;
                }
                catch (NullReferenceException e) {
                    Console.WriteLine(e);
                    return null;
                }
            }
        }

        #endregion

        [field: SerializeField]
        public ApplyPillarOffset ApplyPillarOffset { get; set; }

        [field: SerializeField]
        public InGameReCalibration InGameReCalibration { get; set; }

        [field: SerializeField]
        public RotatePlaySpaceMovement RotatePlaySpaceMovement { get; set; }

        [SerializeField, Range(0, 0.5f)] private float _offsetStepsPosition = 0.1f;

        public float OffsetStepsPosition => _offsetStepsPosition;

        [SerializeField, Range(0, 45f)] private float _offsetStepsRotation = 5f;

        public float OffsetStepsRotation => _offsetStepsRotation;


        private void Awake() {
            _instance = this;
        }

        private void OnEnable() {
            if (_instance == null)
                _instance = this;
        }

        private void OnDestroy() {
            if (_instance != this)
                return;
            _instance = null;
        }
    }
}
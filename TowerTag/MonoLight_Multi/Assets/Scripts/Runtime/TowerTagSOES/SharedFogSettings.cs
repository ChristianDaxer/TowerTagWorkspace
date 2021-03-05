using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerTagSOES {
    [CreateAssetMenu(menuName = "Shared/TowerTag/FogSettings")]
    public class SharedFogSettings : ScriptableObject {
        [SerializeField, Tooltip("If selected, these settings will be drawn from the scene settings")]
        private bool _getFromSceneOnLoad;

        [SerializeField, Tooltip("The fog settings")]
        private FogSettings _fogSettings;

        public FogSettings FogSettings {
            get { return _fogSettings; }
        }

        private void OnEnable() {
            if (_getFromSceneOnLoad) {
                SceneManager.sceneLoaded += GetFromSceneSettings;
            }
        }

        private void OnDisable() {
            SceneManager.sceneLoaded -= GetFromSceneSettings;
        }

        private void GetFromSceneSettings(Scene scene, LoadSceneMode loadSceneMode) {
            GetFromSceneSettings();
        }

        public void GetFromSceneSettings() {
            _fogSettings = FogSettings.FromRenderSettings();
        }

        public bool IsSettingAlreadyApplied() {
            return RenderSettings.fog == _fogSettings.FogEnabled
                   && RenderSettings.fogMode == _fogSettings.FogMode
                   && Math.Abs(RenderSettings.fogStartDistance - _fogSettings.FogStartDistance) < 0.01f
                   && Math.Abs(RenderSettings.fogEndDistance - _fogSettings.FogEndDistance) < 0.01f
                   && RenderSettings.fogColor == _fogSettings.FogColor
                   && Math.Abs(RenderSettings.fogDensity - _fogSettings.FogDensity) < 0.01f
                   && RenderSettings.skybox == _fogSettings.SkyBox;
        }

        public void Apply() {
            RenderSettings.fog = _fogSettings.FogEnabled;
            RenderSettings.fogMode = _fogSettings.FogMode;
            RenderSettings.fogStartDistance = _fogSettings.FogStartDistance;
            RenderSettings.fogEndDistance = _fogSettings.FogEndDistance;
            RenderSettings.fogColor = _fogSettings.FogColor;
            RenderSettings.fogDensity = _fogSettings.FogDensity;
            RenderSettings.skybox = _fogSettings.SkyBox;
        }
    }

    [Serializable]
    public class FogSettings {
        [SerializeField] private bool _fogEnabled;
        [SerializeField] private FogMode _fogMode;
        [SerializeField] private float _fogStartDistance;
        [SerializeField] private float _fogEndDistance;
        [SerializeField] private Color _fogColor;
        [SerializeField] private float _fogDensity;
        [SerializeField] private Material _skyBox;

        public FogMode FogMode {
            get { return _fogMode; }
        }

        public float FogStartDistance {
            get { return _fogStartDistance; }
        }

        public float FogEndDistance {
            get { return _fogEndDistance; }
        }

        public bool FogEnabled {
            get { return _fogEnabled; }
        }

        public Color FogColor {
            get { return _fogColor; }
        }

        public float FogDensity {
            get { return _fogDensity; }
        }

        public Material SkyBox {
            get { return _skyBox; }
        }

        public FogSettings(bool fogEnabled, FogMode fogMode, float fogStartDistance, float fogEndDistance,
            Color fogColor, float fogDensity, Material skyBox) {
            _fogEnabled = fogEnabled;
            _fogMode = fogMode;
            _fogStartDistance = fogStartDistance;
            _fogEndDistance = fogEndDistance;
            _fogColor = fogColor;
            _fogDensity = fogDensity;
            _skyBox = skyBox;
        }

        public static FogSettings FromRenderSettings() {
            return new FogSettings(
                RenderSettings.fog,
                RenderSettings.fogMode,
                RenderSettings.fogStartDistance,
                RenderSettings.fogEndDistance,
                RenderSettings.fogColor,
                RenderSettings.fogDensity,
                RenderSettings.skybox
            );
        }
    }
}
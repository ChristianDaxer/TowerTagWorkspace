using SOEventSystem.Shared;
using UnityEngine;

namespace TowerTag {
    [CreateAssetMenu(menuName = "TowerTag/Team Color Manager", fileName = "TeamColorManager")]
    public class TeamColorManager : ScriptableObjectSingleton<TeamColorManager> {
        [SerializeField, Range(0, 360)] private int _hue;
        public int Hue => _hue;

        [SerializeField, HideInInspector] private Vector2 _main;
        [SerializeField, HideInInspector] private Vector2 _ui;
        [SerializeField, HideInInspector] private Vector2 _darkUI;
        [SerializeField, HideInInspector] private Vector2 _avatar;
        [SerializeField, HideInInspector] private Vector2 _contrastLights;
        [SerializeField, HideInInspector] private Vector2 _rope;
        [SerializeField, HideInInspector] private Vector2 _wallCracks;
        [SerializeField, HideInInspector] private Vector2 _effect;
        [SerializeField, HideInInspector] private Vector2 _dark;
        [SerializeField, HideInInspector] private Vector2 _mediumDark;
        [SerializeField, HideInInspector] private AnimationCurve _emissiveSaturation;
        [SerializeField, HideInInspector] private AnimationCurve _emissiveValue;

        public void Init() { }

        public Vector2 Main {
            get => _main;
            set => _main = value;
        }

        public Vector2 UI {
            get => _ui;
            set => _ui = value;
        }

        public Vector2 DarkUI {
            get => _darkUI;
            set => _darkUI = value;
        }

        public Vector2 Dark {
            get => _dark;
            set => _dark = value;
        }
        
        public Vector2 MediumDark {
            get => _mediumDark;
            set => _mediumDark = value;
        }

        public Vector2 Avatar {
            get => _avatar;
            set => _avatar = value;
        }

        public Vector2 ContrastLights {
            get => _contrastLights;
            set => _contrastLights = value;
        }

        public Vector2 Rope {
            get => _rope;
            set => _rope = value;
        }

        public Vector2 WallCracks {
            get => _wallCracks;
            set => _wallCracks = value;
        }

        public Vector2 Effect {
            get => _effect;
            set => _effect = value;
        }

        public AnimationCurve EmissiveSaturation {
            get => _emissiveSaturation;
            set => _emissiveSaturation = value;
        }

        public AnimationCurve EmissiveValue {
            get => _emissiveValue;
            set => _emissiveValue = value;
        }
    }
}
using System;
using UnityEngine;

namespace TowerTag {
    [Serializable]
    public struct TeamColors {
        [Tooltip("The hue that forms the base for all team colors. Angle on color wheel in degrees."), Range(0, 360)]
        public int Hue;

        public Color Main;
        public Color UI;
        public Color DarkUI;
        public Color Avatar;
        public Color Emissive;
        public Color Dark;
        public Color MediumDark;
        public Color ContrastLights;
        public Color Rope;
        public Color WallCracks;
        public Color Effect;
        public Gradient RopeIntersectionProjector;

        public static TeamColors GenerateFromHue(int hue) {
            float h = hue / 360f;
            Color effect = Color.HSVToRGB(h, TeamColorManager.Singleton.Effect.x, TeamColorManager.Singleton.Effect.y);
            Color uiColor = Color.HSVToRGB(h, TeamColorManager.Singleton.UI.x, TeamColorManager.Singleton.UI.y);
            Color dark = Color.HSVToRGB(h, TeamColorManager.Singleton.Dark.x, TeamColorManager.Singleton.Dark.y);
            Color mediumDark = Color.HSVToRGB(h, TeamColorManager.Singleton.MediumDark.x, TeamColorManager.Singleton.MediumDark.y);
            return new TeamColors {
                Hue = hue,
                Main = Color.HSVToRGB(h, 1, 1),
                UI = uiColor,
                DarkUI = Color.HSVToRGB(h, TeamColorManager.Singleton.DarkUI.x, TeamColorManager.Singleton.DarkUI.y),
                Avatar = Color.HSVToRGB(h, TeamColorManager.Singleton.Avatar.x, TeamColorManager.Singleton.Avatar.y),
                Emissive = Color.HSVToRGB(h, TeamColorManager.Singleton.EmissiveSaturation.Evaluate(h),
                    TeamColorManager.Singleton.EmissiveValue.Evaluate(h)),
                Dark = dark,
                MediumDark = mediumDark,
                ContrastLights = Color.HSVToRGB(h, TeamColorManager.Singleton.ContrastLights.x,
                    TeamColorManager.Singleton.ContrastLights.y),
                Rope = Color.HSVToRGB(h, TeamColorManager.Singleton.Rope.x, TeamColorManager.Singleton.Rope.y),
                WallCracks = Color.HSVToRGB(h, TeamColorManager.Singleton.WallCracks.x,
                    TeamColorManager.Singleton.WallCracks.y),
                Effect = effect,
                RopeIntersectionProjector = new Gradient {
                    colorKeys = new[] {
                        new GradientColorKey(effect, 0f),
                        new GradientColorKey(Color.HSVToRGB(h, 1, 1), 0.33f),
                        new GradientColorKey(Color.HSVToRGB(h, 1, 1), 0.66f),
                        new GradientColorKey(Color.black, 1f)
                    },
                    alphaKeys = new[] {
                        new GradientAlphaKey(1, 0),
                        new GradientAlphaKey(0, 1)
                    }
                }
            };
        }
    }
}
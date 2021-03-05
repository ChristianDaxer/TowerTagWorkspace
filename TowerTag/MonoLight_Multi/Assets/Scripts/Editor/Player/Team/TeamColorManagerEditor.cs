
using TowerTag;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TeamColorManager))]
public class TeamColorManagerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        var teamColorManager = (TeamColorManager) target;
        int hue = teamColorManager.Hue;

        teamColorManager.Main = RenderColorDrawer("Main", hue, teamColorManager.Main);
        teamColorManager.UI = RenderColorDrawer("UI", hue, teamColorManager.UI);
        teamColorManager.DarkUI = RenderColorDrawer("DarkUI", hue, teamColorManager.DarkUI);
        teamColorManager.Avatar = RenderColorDrawer("Avatar", hue, teamColorManager.Avatar);
        teamColorManager.Rope = RenderColorDrawer("Rope", hue, teamColorManager.Rope);
        teamColorManager.WallCracks = RenderColorDrawer("Wall Cracks", hue, teamColorManager.WallCracks);
        teamColorManager.Effect = RenderColorDrawer("Effect", hue, teamColorManager.Effect);
        teamColorManager.Dark = RenderColorDrawer("Dark", hue, teamColorManager.Dark);
        teamColorManager.MediumDark = RenderColorDrawer("Medium Dark", hue, teamColorManager.MediumDark);
        (teamColorManager.EmissiveSaturation, teamColorManager.EmissiveValue) = RenderColorCurveDrawer(
            "Emissive", hue, teamColorManager.EmissiveSaturation,
            teamColorManager.EmissiveValue);
        teamColorManager.ContrastLights = RenderColorDrawer("Contrast Lights", hue, teamColorManager.ContrastLights);
        EditorUtility.SetDirty(teamColorManager);
    }

    private static Vector2 RenderColorDrawer(string name, int hue, Vector2 sv) {
        GUILayout.Space(10);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ColorField(Color.HSVToRGB(hue / 360f, sv.x, sv.y));
        EditorGUI.EndDisabledGroup();
        float s = EditorGUILayout.Slider("Saturation", sv.x, 0, 1);
        float v = EditorGUILayout.Slider("Value", sv.y, 0, 1);
        return new Vector2(s, v);
    }

    private static (AnimationCurve, AnimationCurve) RenderColorCurveDrawer(string name, int hue, AnimationCurve s,
        AnimationCurve v) {
        GUILayout.Space(10);
        EditorGUILayout.LabelField(name, EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        float h = hue / 360f;
        Color color = Color.HSVToRGB(h, s.Evaluate(h), v.Evaluate(h));
        EditorGUILayout.ColorField(color);
        EditorGUI.EndDisabledGroup();
        s = EditorGUILayout.CurveField("Saturation", s, color, new Rect(0,0, 1, 1));
        v = EditorGUILayout.CurveField("Value", v, color, new Rect(0,0, 1, 1));
        return (s, v);
    }
}
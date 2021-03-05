using System.Reflection;
using TowerTag;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Team))]
public class TeamEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate colors from hue")) {
            var team = (Team) target;
            typeof(Team).GetField("_colors", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.SetValue(team, TeamColors.GenerateFromHue(team.Colors.Hue));
            EditorUtility.SetDirty(team);
        }
    }
}
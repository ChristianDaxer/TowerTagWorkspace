using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MissionBriefingController))]
public class MissionBriefingControllerEditor : Editor
{
    private MissionBriefingController _target;
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        _target = (MissionBriefingController)target;
        if (GUILayout.Button("Start Mission Briefing")) {
            _target.InitializeMissionBriefing(_target.CurrentMatchDescription, GameMode.UserVote);
        }
    }
}

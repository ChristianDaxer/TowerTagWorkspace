using UnityEditor;
using UnityEngine;

namespace TowerTag.Divider {
    [CustomEditor(typeof(DividerHighlight))]
    public class DividerHighlighterInspector : Editor {
        private bool _debugging;
        private DividerHighlight _dividerHighlight;
        private TeamID _winningTeamID = TeamID.Neutral;
        private float _duration = 1;
        private float _blinksPerSecond = 2;
        private bool _randomPhase = true;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            _dividerHighlight = (DividerHighlight) target;
            _debugging = EditorGUILayout.Toggle("Debug", _debugging);
            if (_debugging) {
                _randomPhase = EditorGUILayout.Toggle("Random Phase", _randomPhase);
                EditorGUILayout.LabelField("Debug Helpers", EditorStyles.boldLabel);
                _winningTeamID = (TeamID) EditorGUILayout.EnumPopup("Winning Team ID:", _winningTeamID);
                _duration = EditorGUILayout.FloatField("Duration", _duration);
                _blinksPerSecond = EditorGUILayout.FloatField("Blinks per second", _blinksPerSecond);
                if (GUILayout.Button("Blink")) {
                    _dividerHighlight.StartPulse(_duration, _blinksPerSecond, _winningTeamID, _randomPhase);
                }

                if (GUILayout.Button("Reset")) {
                    _dividerHighlight.Reset();
                }
            }
        }
    }
}
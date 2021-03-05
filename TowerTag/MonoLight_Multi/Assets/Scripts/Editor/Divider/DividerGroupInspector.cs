using GameManagement;
using NSubstitute;
using UnityEditor;
using UnityEngine;

namespace TowerTag.Divider {
    [CustomEditor(typeof(DividerGroup))]
    public class DividerGroupInspector : Editor {
        private bool _debugging;
        private DividerGroup _target;
        private TeamID _winningTeamID;
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            _target = (DividerGroup) target;
            _debugging = EditorGUILayout.Toggle("Debug", _debugging);
            if (_debugging) {
                EditorGUILayout.LabelField("Debug helper", EditorStyles.boldLabel);
                _winningTeamID = (TeamID) EditorGUILayout.EnumPopup("Winning Team ID:", _winningTeamID);

                if (GUILayout.Button("StartRoundFinishedShow")) {
                    _target.StartRoundFinishedShow(Substitute.For<IGameManager>().CurrentMatch, _winningTeamID);
                }

                if (GUILayout.Button("StopRoundFinishedShow")) {
                    _target.StopRoundFinishedShow();
                }

                if (GUILayout.Button("StartRoundHighlight")) {
                    _target.StartNewRoundHighlight(Substitute.For<IGameManager>().CurrentMatch, 0);
                }
            }
        }
    }
}
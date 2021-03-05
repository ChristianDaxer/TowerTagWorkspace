using UnityEditor;
using UnityEngine;

namespace CustomBuildPipeline {
    /// <summary>
    /// Custom Editor for SharedVersion.
    /// <author>Ole Jürgensen</author>
    /// <date>2018-04-26</date>
    /// </summary>
    [CustomEditor(typeof(SharedVersion))]
    public class SharedVersionEditor : Editor {
        private SerializedProperty _value;

        private void OnEnable() {
            _value = serializedObject.FindProperty("_value");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"), true);
            EditorGUILayout.PropertyField(_value, true);

            var sharedVersion = serializedObject.targetObject as SharedVersion;
            if (sharedVersion == null) {
                return;
            }

            if (GUILayout.Button("Update Version")) {
                sharedVersion.RaiseChangeEvent(this);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
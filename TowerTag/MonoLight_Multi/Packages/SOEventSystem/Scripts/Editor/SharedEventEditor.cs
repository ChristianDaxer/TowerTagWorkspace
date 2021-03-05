using SOEventSystem.Shared;
using UnityEditor;
using UnityEngine;

namespace SOEventSystem {
    [CustomEditor(typeof(SharedEvent), true)]
    public class SharedEventEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Trigger Event")) {
                ((SharedEvent) serializedObject.targetObject).Trigger(this);
            }
        }
    }
}
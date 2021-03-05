using System;
using System.Reflection;
using SOEventSystem.Shared;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SOEventSystem {
    [CustomEditor(typeof(SharedVariable), true)]
    public class SharedVariableEditor : Editor {
        private SerializedProperty _value;
        private SerializedProperty _script;
        private bool _arrayFoldedOut;

        private void OnEnable() {
            _value = serializedObject.FindProperty("_value");
            _script = serializedObject.FindProperty("m_Script");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(_script, true);

            var sharedVariable = serializedObject.targetObject as SharedVariable;
            if (sharedVariable == null) {
                return;
            }

            // change check to trigger change event if value changed in editor. Wow, so change, much value.
            EditorGUI.BeginChangeCheck();
            // Handle UnityEngine.Object manually to allow assigning scene objects
            Type baseType = sharedVariable.GetType();
            do {
                baseType = baseType.BaseType;
            } while (baseType != null && 
                     baseType.BaseType != null && 
                     baseType.BaseType.GetProperty("Value") != null);
            if (baseType == null) return;
            PropertyInfo propertyInfo = baseType.GetProperty("Value");
            if (propertyInfo == null) return;
            object currentValue = propertyInfo.GetValue(sharedVariable, null);
            Type valueType = propertyInfo.PropertyType;
            if (typeof(Object).IsAssignableFrom(valueType)) {
                Object newValue = EditorGUILayout.ObjectField((Object) currentValue, valueType, true);
                propertyInfo.SetValue(sharedVariable, newValue, null);
            }
            else if (typeof(Object[]).IsAssignableFrom(valueType)) {
                _arrayFoldedOut = EditorGUILayout.Foldout(_arrayFoldedOut, "Value");
                if (_arrayFoldedOut) {
                    EditorGUI.indentLevel++;
                    var current = (Object[]) currentValue;
                    int arraySize = current == null ? 0 : EditorGUILayout.IntField("Size", current.Length);
                    bool changed = current == null || current.Length != arraySize;
                    var newValue = (object[]) Activator.CreateInstance(valueType, arraySize);
                    if (current != null) {
                        Array.Copy(current, newValue, Mathf.Min(current.Length, newValue.Length));
                    }

                    for (var i = 0; i < arraySize; i++) {
                        newValue[i] =
                            EditorGUILayout.ObjectField((Object) newValue[i], valueType.GetElementType(), true);
                        changed = changed || !Equals(newValue[i], current[i]);
                    }

                    propertyInfo.SetValue(sharedVariable, newValue, null);
                    EditorGUI.indentLevel--;
                }
            }
            else if (_value != null) {
                EditorGUILayout.PropertyField(_value, true);
            }

            // render all additional properties
            SerializedProperty iterator = serializedObject.GetIterator();
            for (var enterChildren = true; iterator.NextVisible(enterChildren); enterChildren = false) {
                if (iterator.propertyPath == "m_Script" || iterator.propertyPath == "_value") continue;
                EditorGUILayout.PropertyField(iterator, true);
            }

            // render buttons to trigger events manually
            if (GUILayout.Button("Raise Set Event")) {
                sharedVariable.RaiseSetEvent(this);
            }

            if (GUILayout.Button("Raise Change Event")) {
                sharedVariable.RaiseChangeEvent(this);
            }

            // apply changes before triggering change event
            serializedObject.ApplyModifiedProperties();
        }
    }
}
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FindObjectsWithTag : EditorWindow
{
        static string tagStr = "";

        [MenuItem("Helpers/Select By Tag")]
        static void Init()
        {
            EditorWindow window = GetWindow(typeof(FindObjectsWithTag));
            window.Show();
        }

        void OnGUI()
        {
            tagStr = EditorGUILayout.TagField("Tag for Objects:", tagStr);
            if (GUILayout.Button("Get Objects with Tag")) {
                SelectObjectsWithTagDec_Glass(tagStr);
            }
        }

        static void SelectObjectsWithTagDec_Glass(string tag)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
            Selection.objects = objects;
        }
}

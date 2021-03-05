using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnumerateAudioSourcesWindow : EditorWindow
{
    private EnumerateAudioSourcesSettings settings => EnumerateAudioSourcesSettings.Settings;

    [MenuItem("Unity/Enumerate Audio Sources")]
    public static void Open ()
    {
        EnumerateAudioSourcesWindow window = EnumerateAudioSourcesWindow.GetWindow<EnumerateAudioSourcesWindow>();
        window.Show();
    }

    private void OnGUI()
    {
        Editor editor = EnumerateAudioSourcesSettingsEditor.CreateEditor(settings);
        editor.OnInspectorGUI();

        if (GUILayout.Button("Cache Audio Source Library"))
        {
        }
    }
}

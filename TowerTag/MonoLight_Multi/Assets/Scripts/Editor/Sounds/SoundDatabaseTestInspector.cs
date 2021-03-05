using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(SoundDatabaseTest))]
public class SoundDatabaseTestInspector : Editor {
    private bool _showSoundSelection;
    private int _selectedSoundIndex = -1;

    public override void OnInspectorGUI() {
        var soundDatabaseTest = (SoundDatabaseTest) target;

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        GUILayout.Label("Sound: " + soundDatabaseTest.SoundName);
        _showSoundSelection = EditorGUILayout.Foldout(_showSoundSelection, "Select Sound");

        if (_showSoundSelection) {
            string tmpName = SelectSound(soundDatabaseTest.SoundDatabase, soundDatabaseTest.SoundName,
                out int tmpIndex);
            if (tmpIndex >= 0 && _selectedSoundIndex != tmpIndex) {
                soundDatabaseTest.SoundName = tmpName;
                _selectedSoundIndex = tmpIndex;
                _showSoundSelection = false;
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Reload Sound")) {
            soundDatabaseTest.SetSound();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Play")) {
            soundDatabaseTest.Play();
        }

        if (GUILayout.Button("Stop")) {
            soundDatabaseTest.Stop();
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.Label("Volume: ");
        float tmp = GUILayout.HorizontalSlider(soundDatabaseTest.Volume, 0, 1);
        if (tmp != soundDatabaseTest.Volume) {
            soundDatabaseTest.ChangeVolume(tmp);
        }

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        GUILayout.Label("Pitch: ");
        tmp = GUILayout.HorizontalSlider(soundDatabaseTest.Pitch, 0, 1);
        if (tmp != soundDatabaseTest.Pitch) {
            soundDatabaseTest.ChangePitch(tmp);
        }

        GUILayout.EndHorizontal();
    }

    static string SelectSound(SoundDatabase sDB, string oldSoundName, out int index) {
        index = -1;

        if (sDB == null)
            return oldSoundName;

        Sound[] sounds = sDB.GetAllSounds();

        for (var i = 0; i < sounds.Length; i++) {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Select", GUILayout.Width(50))) {
                index = i;
                return sounds[i].Name;
            }

            GUILayout.Label(sounds[i].Name);

            GUILayout.EndHorizontal();
        }

        return oldSoundName;
    }
}
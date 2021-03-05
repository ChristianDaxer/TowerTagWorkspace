using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExplodingAvatar))]
[CanEditMultipleObjects]
public class ExplodingAvatarInspector : Editor {
    private ExplodingAvatar _target;

    public override void OnInspectorGUI() {
        _target = (ExplodingAvatar) target;

        DrawDefaultInspector();

        if (GUILayout.Button("CopyDefaultValues")) {
            _target.CopyDefaultValues();
            EditorUtility.SetDirty(_target);
        }

        if (GUILayout.Button("Explode")) {
            _target.gameObject.SetActive(false);
            _target.gameObject.SetActive(true);
            Transform transform = _target.transform;
            _target.InitAvatar(transform, transform, _target.TeamID);
            _target.Explode();
            if (_target._explosionEffectParent != null) {
                _target._explosionEffectParent.SetActive(false);
                _target._explosionEffectParent.SetActive(true);
            }

            EditorApplication.isPaused = true;
        }
    }
}
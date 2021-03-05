using TowerTagSOES;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerHealth))]
public class PlayerHealthInspector : Editor {
    public override void OnInspectorGUI() {
        var playerHealth = (PlayerHealth) target;

        //DrawDefaultInspector();
        serializedObject.Update();

        if (playerHealth != null) {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_sharedHealthChanged"));
            GUILayout.BeginHorizontal();
            GUILayout.Label("Maximum Damage: (" + playerHealth.MaxHealth + ")");
            playerHealth.MaxHealth = EditorGUILayout.IntField(playerHealth.MaxHealth);
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUILayout.Label("CollisionDetectors: ");
            if (playerHealth.CollisionDetectors != null) {
                foreach (DamageDetectorBase damageDetector in playerHealth.CollisionDetectors) {
                    DamageDetectorBase detector = damageDetector;
                    if (detector == null)
                        continue;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(detector.gameObject.name + ": ");
                    detector =
                        EditorGUILayout.ObjectField(detector, typeof(DamageDetectorBase), true) as DamageDetectorBase;
                    if (detector == null)
                        continue;
                    GUILayout.EndHorizontal();

                    if (GUI.changed)
                        EditorUtility.SetDirty(detector);
                }
            }

            if (GUILayout.Button("Get CollisionDetectors from children")) {
                playerHealth.CollisionDetectors = GetCollisionDetectorsFromChildGameObjects(playerHealth.gameObject);
            }

            if (GUI.changed)
                EditorUtility.SetDirty(playerHealth);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static DamageDetectorBase[] GetCollisionDetectorsFromChildGameObjects(GameObject parent) {
        return parent.GetComponentsInChildren<DamageDetectorBase>();
    }
}
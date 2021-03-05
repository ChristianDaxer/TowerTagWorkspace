using UnityEditor;
using UnityEngine;

namespace TowerTag {
    [CustomEditor(typeof(EffectDatabase))]
    public class EffectDatabaseEditor : Editor {
        public override void OnInspectorGUI() {
            var effectDatabase = (EffectDatabase) target;

            int count = EditorGUILayout.IntField("Number of Decals:", effectDatabase.DecalTags.Length);
            if (count != effectDatabase.DecalTags.Length) {
                ResizeArrays(count, effectDatabase);
            }

            for (var i = 0; i < count; i++) {
                effectDatabase.DecalTags[i] = EditorGUILayout.TextField("Tag: ", effectDatabase.DecalTags[i]);
                effectDatabase.DecalPrefabs[i] =
                    EditorGUILayout.ObjectField("Prefab: ", effectDatabase.DecalPrefabs[i], typeof(GameObject), true) as
                        GameObject;
                effectDatabase.DecalPoolSizes[i] =
                    EditorGUILayout.IntField("PoolSize: ", effectDatabase.DecalPoolSizes[i]);
            }
        }

        private static void ResizeArrays(int count, EffectDatabase effectDatabase) {
            var newTags = new string[count];
            var newPrefabs = new GameObject[count];
            var newPoolSizes = new int[count];

            for (var i = 0; i < Mathf.Min(effectDatabase.DecalTags.Length, count); i++) {
                newTags[i] = effectDatabase.DecalTags[i];
                newPrefabs[i] = effectDatabase.DecalPrefabs[i];
                newPoolSizes[i] = effectDatabase.DecalPoolSizes[i];
            }

            effectDatabase.DecalTags = newTags;
            effectDatabase.DecalPrefabs = newPrefabs;
            effectDatabase.DecalPoolSizes = newPoolSizes;
        }
    }
}
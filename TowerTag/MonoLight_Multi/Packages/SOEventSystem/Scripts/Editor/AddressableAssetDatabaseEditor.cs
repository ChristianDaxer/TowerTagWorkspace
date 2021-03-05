using System.Linq;
using SOEventSystem.Addressable;
using SOEventSystem.Shared;
using UnityEditor;
using UnityEngine;

namespace SOEventSystem {
    [CustomEditor(typeof(AddressableAssetDatabase))]
    public class AddressableAssetDatabaseEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var addressableAssetDatabase = serializedObject.targetObject as AddressableAssetDatabase;
            if (addressableAssetDatabase != null && GUILayout.Button("Find Assets")) {
                AssetDatabase.FindAssets("t:AddressableAsset")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<AddressableAsset>)
                    .Where(asset => asset != null)
                    .ToList()
                    .ForEach(addressableAssetDatabase.Register);
                Debug.Log(string.Format("Updated sync var database with {0} assets",
                    addressableAssetDatabase.AddressableAssets.Count));
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Utilities {
    /// <summary>
    /// 2021-03-03 mu
    /// Helps with serialization errors in assets and scenes. Select objects in Project window first, then go to menu in Assets/Serialization/ 
    /// </summary>
    public static class AssetsReserializeMenu {
        
        [MenuItem("Assets/Serialization/Selection Set Dirty")]
        private static void SetDirty() {
            foreach (Object o in Selection.objects) {
                EditorUtility.SetDirty(o);
            }
            Debug.Log("Menu: Set Dirty complete.");
        }

        [MenuItem("Assets/Serialization/Selection Force Reserialization")]
        private static void ForceReserialization() {
            List<string> assetPathsOfSelected = new List<string>();

            foreach (Object o in Selection.objects) {
                assetPathsOfSelected.Add(AssetDatabase.GetAssetOrScenePath(o));
            }
            Debug.Log("Menu: Force Reserialization started...");
            AssetDatabase.ForceReserializeAssets(assetPathsOfSelected, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
            Debug.Log("Menu: Force Reserialization complete.");
        }
    }
}

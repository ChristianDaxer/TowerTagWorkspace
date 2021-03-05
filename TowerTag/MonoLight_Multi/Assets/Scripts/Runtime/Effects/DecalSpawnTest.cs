using UnityEngine;

namespace TowerTag {
    public class DecalSpawnTest : MonoBehaviour {
        private void OnGUI() {
            if (GUILayout.Button("Place Decal")) {
                Transform t = transform;
                EffectDatabase.PlaceDecal(t.tag, t.position, Quaternion.LookRotation(t.forward));
            }
        }
    }
}
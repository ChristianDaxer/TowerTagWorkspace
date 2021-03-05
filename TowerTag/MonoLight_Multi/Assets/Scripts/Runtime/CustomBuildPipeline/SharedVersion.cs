using SOEventSystem.Shared;
using UnityEngine;

namespace CustomBuildPipeline {
    /// <summary>
    /// Shared Version to Make Player Version available in built code and for easy updating of the Player Version.
    /// <author>Ole Jürgensen</author>
    /// <date>2018-04-26</date>
    /// </summary>
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Shared/TowerTag/PlayerVersion")]
#endif
    public class SharedVersion : SharedString {
#if UNITY_EDITOR
        private new void OnEnable() {
            base.OnEnable();
            Set(this, UnityEditor.PlayerSettings.bundleVersion);
            ValueChanged += OnVersionUpdated;
        }

        private void OnVersionUpdated(object sender, string version) {
            UnityEditor.PlayerSettings.bundleVersion = Value;
            UnityEngine.Debug.Log("Updated Version to " + version);
        }
#endif
    }
}
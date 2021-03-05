using TMPro;
using TowerTag;
using UnityEngine;

namespace UI {
    public class CustomizeUIColors : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI[] _teamFireTexts;
        [SerializeField] private TextMeshProUGUI[] _teamIceTexts;

        private void OnEnable() {
            Customization.CustomizationChanged += TintAllTexts;
        }

        private void OnDisable() {
            Customization.CustomizationChanged -= TintAllTexts;
        }

        private void Start() {
            TintAllTexts();
        }

        [ContextMenu("Tint all texts")]
        private void TintAllTexts() {
            _teamFireTexts.ForEach(txt => {
                txt.color = TeamManager.Singleton.TeamFire.Colors.UI;
                txt.SetAllDirty();
            });
            _teamIceTexts.ForEach(txt => {
                txt.color = TeamManager.Singleton.TeamIce.Colors.UI;
                txt.SetAllDirty();
            });
        }

        [ContextMenu("Get TMP Texts in Ice")]
        private void GetAllTMPTextsFromChildrenIce() {
            _teamIceTexts = GetComponentsInChildren<TextMeshProUGUI>();
        }
    }
}
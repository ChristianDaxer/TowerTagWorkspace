using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    [RequireComponent(typeof(TMP_Dropdown))]
    public class CustomizeDropdown : MonoBehaviour {
        [SerializeField] private TeamID _teamID;
        [SerializeField] private TMP_Text _currentSelection;
        private bool _currentSelectionValid = true;
        private Animator _animator;

        private Animator Animator => _animator != null
            ? _animator
            : _animator = GetComponent<Animator>();

        public bool CurrentSelectionValid {
            get => _currentSelectionValid;
            set {
                _currentSelectionValid = value;
                Refresh();
            }
        }

        public TeamID TeamID {
            set {
                _teamID = value;
                Refresh();
            }
        }

        private void Refresh() {
            if (Animator == null || !Animator.isActiveAndEnabled || !Animator.isInitialized) return;
            if(Animator.GetCurrentAnimatorStateInfo(0).IsName("Normal")) SetDefault();
            if(Animator.GetCurrentAnimatorStateInfo(0).IsName("Highlighted")) SetHighlighted();
            if(Animator.GetCurrentAnimatorStateInfo(0).IsName("Pressed")) SetPressed();
            if(Animator.GetCurrentAnimatorStateInfo(0).IsName("Disabled")) SetDisabled();
        }

        private void OnValidate() {
            if (gameObject.scene.buildIndex == -1) return;
            Refresh();
        }

        private void Start() {
            Refresh();
        }

        public void SetDefault() {
            Material uiMaterial = TeamMaterialManager.Singleton.GetFlatUI(_teamID);
            Material uiDarkMaterial = TeamMaterialManager.Singleton.GetFlatUIDark(_teamID);
            GetComponentsInChildren<Image>(true)
                .ForEach(img => img.material = uiMaterial);
            GetComponentsInChildren<Toggle>(true)
                .ForEach(toggle => toggle.GetComponentsInChildren<TMP_Text>(true)
                    .ForEach(txt => txt.color = toggle.interactable ? uiMaterial.color : uiDarkMaterial.color));
            _currentSelection.color = CurrentSelectionValid ? uiMaterial.color : uiDarkMaterial.color;
        }

        public void SetHighlighted() {
            Material uiMaterial = TeamMaterialManager.Singleton.GetFlatUI(_teamID);
            Material uiDarkMaterial = TeamMaterialManager.Singleton.GetFlatUIDark(_teamID);
            GetComponentsInChildren<Image>(true)
                .ForEach(img => img.material = uiMaterial);
            GetComponentsInChildren<Toggle>(true)
                .ForEach(toggle => toggle.GetComponentsInChildren<TMP_Text>(true)
                    .ForEach(txt => txt.color = toggle.interactable ? uiMaterial.color : uiDarkMaterial.color));
            _currentSelection.color = CurrentSelectionValid ? uiMaterial.color : uiDarkMaterial.color;
        }

        public void SetDisabled() {
            Material uiDarkMaterial = TeamMaterialManager.Singleton.GetFlatUIDark(_teamID);
            GetComponentsInChildren<Image>(true)
                .ForEach(img => img.material = uiDarkMaterial);
            GetComponentsInChildren<TextMeshProUGUI>(true)
                .ForEach(txt => txt.color = uiDarkMaterial.color);
        }

        public void SetPressed() {
            Material uiMaterial = TeamMaterialManager.Singleton.GetFlatUI(_teamID);
            Material uiDarkMaterial = TeamMaterialManager.Singleton.GetFlatUIDark(_teamID);
            GetComponentsInChildren<Image>(true)
                .ForEach(img => img.material = uiMaterial);
            GetComponentsInChildren<Toggle>(true)
                .ForEach(toggle => toggle.GetComponentsInChildren<TMP_Text>(true)
                    .ForEach(txt => txt.color = toggle.interactable ? uiMaterial.color : uiDarkMaterial.color));
            _currentSelection.color = CurrentSelectionValid ? uiMaterial.color : uiDarkMaterial.color;
        }
    }
}
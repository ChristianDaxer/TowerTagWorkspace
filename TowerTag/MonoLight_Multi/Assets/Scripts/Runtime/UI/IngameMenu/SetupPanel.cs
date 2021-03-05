using JetBrains.Annotations;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class SetupPanel : HomeMenuPanel {
        [SerializeField] private Toggle _rightHand;
        [SerializeField] private Toggle _enableSmallSpace;

        public override void OnEnable() {
            base.OnEnable();

            if (InputControllerVR.Instance != null)
                _rightHand.isOn = InputControllerVR.Instance.TargetHand == PlayerHand.Right;

            _enableSmallSpace.isOn = ConfigurationManager.Configuration.SmallPlayArea;

            _rightHand.onValueChanged.AddListener(SwitchController);
            _enableSmallSpace.onValueChanged.AddListener(ToggleSmallSpace);
        }

        public override void OnDisable() {
            base.OnDisable();

            if (InputControllerVR.Instance != null)
                _rightHand.isOn = InputControllerVR.Instance.TargetHand == PlayerHand.Right;

            _enableSmallSpace.isOn = ConfigurationManager.Configuration.SmallPlayArea;
        }

        private void ToggleSmallSpace(bool SmallSpaceEnabled) {
            ConfigurationManager.Configuration.SmallPlayArea = SmallSpaceEnabled;
            ConfigurationManager.WriteConfigToFile();
        }

        private void SwitchController(bool rightHandEnable) => InputControllerVR.Instance.SetControllerAsPreferred(rightHandEnable);
        
        [UsedImplicitly]
        public void OnOkButtonPressed() => UIController.SwitchPanel(HubUIController.PanelType.StartTutorial); 
    }
}
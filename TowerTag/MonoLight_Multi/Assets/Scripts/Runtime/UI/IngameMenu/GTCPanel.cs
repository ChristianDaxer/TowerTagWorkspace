using JetBrains.Annotations;
using TowerTag;
using UI;
using UnityEngine;

namespace Home.UI {
    public class GTCPanel : HomeMenuPanel {
        [SerializeField] private bool _showIfAlreadyAccepted;
        [SerializeField] private int _gtcVersion = 1;

        public int GTCVersion => _gtcVersion;

        public override void OnEnable() {
            base.OnEnable();
            if (!_showIfAlreadyAccepted && PlayerPrefs.HasKey(PlayerPrefKeys.GTC) &&
                PlayerPrefs.GetInt(PlayerPrefKeys.GTC) == _gtcVersion) {
                OnAcceptButtonPressed();
            }
        }

        [UsedImplicitly]
        public void OnAcceptButtonPressed() {
            PlayerPrefs.SetInt(PlayerPrefKeys.GTC, _gtcVersion);
            if (TTSceneManager.Instance.IsInConnectScene) {
                UIController.SwitchPanel(HubUIController.PanelType.Loading, false);
            }
            else if (TTSceneManager.Instance.IsInTutorialScene) {
                UIController.SwitchPanel(PlayerPrefs.GetInt(PlayerPrefKeys.Tutorial) == 1 ? HubUIController.PanelType.StartTutorial : HubUIController.PanelType.Setup);
            }
        }

        [UsedImplicitly]
        public void OnDeclineButtonPressed() {
            MessageQueue.Singleton.AddYesNoMessage(
                "You cannot play Tower Tag without agreeing to the terms and conditions",
                "Decline terms and conditions",
                null,
                null,
                "QUIT APP",
                Application.Quit,
                "CANCEL");
        }
    }
}
using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class ConnectingPanel : HomeMenuPanel {
        public static HubUIController.PanelType PanelTypeToLoadIn = HubUIController.PanelType.MainMenu;
        [SerializeField] private Text _headlineText;
        [SerializeField] private string _connectingHeadlineText = "CONNECTION";

        private new void Awake() {
            base.Awake();
            _headlineText.text = _connectingHeadlineText;
        }

        public override void OnEnable() {
            base.OnEnable();
            if (!PhotonNetwork.InLobby)
                StartCoroutine(WaitForLobby());
            else {
                UIController.SwitchPanel(HubUIController.PanelType.MainMenu);
            }
        }

        public override void OnDisable() {
            base.OnDisable();
            StopAllCoroutines();
        }

        private IEnumerator WaitForLobby() {
            while (!PhotonNetwork.InLobby) {
                yield return null;
            }

            JoinedLobby();
        }

        public void JoinedLobby() {
            UIController.SwitchPanel(PanelTypeToLoadIn);
            PanelTypeToLoadIn = HubUIController.PanelType.MainMenu;
        }
    }
}
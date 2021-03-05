using Photon.Pun;

namespace Home.UI {
    public class HomeMenuPanel : MonoBehaviourPunCallbacks {
        protected HubUIController UIController;

        protected void Awake() {
            UIController = GetComponentInParent<HubUIController>();
            if (UIController == null) {
                Debug.LogError("No HubUIController found. Disabling Panel");
                enabled = false;
            }
        }
    }
}
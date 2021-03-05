using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class ReadyTowerShieldToggle : MonoBehaviour {
    private Toggle _shieldToggle;

    private void Start() {
        _shieldToggle = GetComponent<Toggle>();

        if (!_shieldToggle) {
            Debug.LogWarning("No toggle was assigned to ReadyTowerShieldToggle. Script is useless.");
            return;
        }

        _shieldToggle.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy() {
        _shieldToggle.onValueChanged.RemoveAllListeners();
    }

    private void OnValueChanged(bool value) {
        IPlayer player = PlayerManager.Instance.GetOwnPlayer();

        player?.PlayerNetworkEventHandler.ToggleHubShield(player.PlayerID, value);
    }
}
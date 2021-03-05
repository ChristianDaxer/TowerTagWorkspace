using Photon.Pun;
using TowerTag;
using UnityEngine;

public class HealthNetworkEventHandler : MonoBehaviourPun {
    [SerializeField] private bool _useEncryption;

    private PlayerHealth _playerHealth;

    public PlayerHealth PlayerHealth {
        get => _playerHealth;
        set {
            UnregisterEventListeners();
            _playerHealth = value;
            InitialHealthSync();
            RegisterEventListeners();
        }
    }

    private void OnEnable() {
        RegisterEventListeners();
    }

    private void OnDisable() {
        UnregisterEventListeners();
    }

    private void RegisterEventListeners() {
        if (PlayerHealth != null) {
            PlayerHealth.HealthChanged += OnHealthChanged;
            PlayerHealth.PlayerRevived += OnPlayerRevived;
            if (PhotonNetwork.IsMasterClient) {
                OnHealthChanged(PlayerHealth, PlayerHealth.CurrentHealth, null,
                    (byte) DamageDetectorBase.ColliderType.Undefined);
            }
        }
        PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
    }

    private void InitialHealthSync() {
        if (PhotonNetwork.IsMasterClient && PlayerHealth != null) {
            OnHealthChanged(PlayerHealth, PlayerHealth.CurrentHealth, null,
                (byte) DamageDetectorBase.ColliderType.Undefined);
        }
    }

    private void UnregisterEventListeners() {
        if (PlayerHealth != null) {
            PlayerHealth.HealthChanged -= OnHealthChanged;
            PlayerHealth.PlayerRevived -= OnPlayerRevived;
        }
        PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
    }

    private void OnPlayerAdded(IPlayer player) {
        InitialHealthSync();
    }

    private void OnHealthChanged(PlayerHealth playerHealth, int newHealthValue, IPlayer otherPlayer,
        byte colliderType) {
        int otherID = -1;
        if (otherPlayer != null) {
            otherID = otherPlayer.PlayerID;
        }

        if (PhotonNetwork.IsMasterClient) {
            photonView.RpcSecure(nameof(HealthChanged), RpcTarget.Others, _useEncryption, newHealthValue, otherID,
                colliderType);
        }
    }

    // ReSharper disable once UnusedMember.Local - PUN callback
    [PunRPC]
    private void HealthChanged(int newValue, int otherPlayerID, byte colliderType) {
        IPlayer otherPlayer = PlayerManager.Instance.GetPlayer(otherPlayerID);
        if (PlayerHealth != null) PlayerHealth.OnHealthChangedRemote(newValue, otherPlayer, colliderType);
    }


    private void OnPlayerRevived(IPlayer player) {
        if (PhotonNetwork.IsMasterClient)
            photonView.RpcSecure(nameof(PlayerRevived), RpcTarget.Others, _useEncryption);
    }

    [PunRPC]
    private void PlayerRevived() {
        if (PlayerHealth != null)
            PlayerHealth.OnPlayerRevivedRemote();
    }
}
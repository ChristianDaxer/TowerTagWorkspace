using TowerTag;
using UnityEngine;

/// <summary>
/// Script to disable Players Enemy Shots while Player in Safety Zone(s)
/// </summary>
/// <author>Sebastian Krebs (sebastian.krebs@vrnerds.de)</author>
public class SafetyZone : MonoBehaviour {
    [SerializeField] private ShotManager _shotManager;
    private Camera _playerCamera;
    private IPlayer _ownPlayer;

    private void Awake() {
        if (TowerTagSettings.Home) {
            enabled = false;
            return;
        }
        _ownPlayer = GetComponentInParent<IPlayer>();
    }

    private void OnEnable() {
        _shotManager.ShotFired += OnShotFired;
    }

    private void OnDisable() {
        _shotManager.ShotFired -= OnShotFired;
    }

    private void OnShotFired(ShotManager shotManager, string id, IPlayer player, Vector3 position,
        Quaternion rotation) {
        if (player != null
            && id != null
            && GameManager.Instance.IsInConfigureState
            && !player.IsMe
            && _ownPlayer != null
            && !_ownPlayer.PlayerIsReady)
            _shotManager.DestroyShot(id);
    }
}
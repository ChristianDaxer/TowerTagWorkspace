using Photon.Pun;
using TowerTag;
using UnityEngine;

/// <summary>
/// Frequently fetches the current server time to synchronize clocks within the network.
/// This is essential for synchronized game actions. E.g., when a shot is fired, the respective timestamp is sent
/// to the master and eventually to all clients. The spawn position of the shot is then computed from the muzzle
/// position, shot speed and age to compensate for latency. The age can only be determined, if all clients have a
/// synchronized clock.
/// </summary>
/// <author>Ole Jürgensen (ole@vrnerds.de)</author>
public class SynchronizeNetworkTime : MonoBehaviour {
    private static IMatch _match;

    private void OnEnable() {
        GameManager.Instance.MatchHasChanged += OnMatchHasChanged;
    }

    private void OnDisable() {
        GameManager.Instance.MatchHasChanged -= OnMatchHasChanged;
    }

    private static void OnMatchHasChanged(IMatch match) {
        if (_match != null)
            _match.RoundFinished -= OnRoundFinished;
        if (match == null) return;
        _match = match;
        _match.RoundFinished += OnRoundFinished;
        SynchronizeClocks();
    }

    private static void OnRoundFinished(IMatch match, TeamID teamID) {
        SynchronizeClocks();
    }

    private static void SynchronizeClocks() {
        Debug.Log("Fetching server time to synchronize clocks. Essential to allow for synchronized game actions.");
        PhotonNetwork.FetchServerTimestamp();
    }
}
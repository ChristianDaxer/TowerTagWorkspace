using System;
using System.Collections.Generic;
using TowerTag;

/// <summary>
/// Simple implementation of a Barrier to await in sync messages from remote players.
/// </summary>
public class PlayerSyncBarrier {
    /// <summary>
    /// Number of players we are waiting for.
    /// </summary>
    public int UnSyncedPlayerCount => _players.Count;

    /// <summary>
    /// Callback we trigger if we received all messages.
    /// </summary>
    private readonly Action<PlayerSyncBarrier> _allPlayersSyncedCallback;

    /// <summary>
    /// MatchID we have to check if the Players have send the right message.
    /// </summary>
    private readonly int _matchID;

    /// <summary>
    /// List of players we wait for
    /// </summary>
    private readonly List<IPlayer> _players = new List<IPlayer>();

    /// <summary>
    /// Create a new Barrier (List of Players to await a message from).
    /// </summary>
    /// <param name="players">Players to wait for.</param>
    /// <param name="matchID">The match ID for which the player reportedly loaded the match scene</param>
    /// <param name="allSyncedCallback">Callback function called when all player have send the right value.</param>
    public PlayerSyncBarrier(IPlayer[] players, int matchID, Action<PlayerSyncBarrier> allSyncedCallback) {
        _matchID = matchID;
        _players.Clear();
        _players.AddRange(players);
        _allPlayersSyncedCallback = allSyncedCallback;
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
    }

    ~PlayerSyncBarrier() {
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
    }

    private void OnPlayerRemoved(IPlayer player) {
        if (_players.Contains(player)) {
            _players.Remove(player);

            if (_players.Count == 0) {
                _allPlayersSyncedCallback?.Invoke(this);
            }
        }
    }

    /// <summary>
    /// Check if send match ID value corresponds to this barrier.
    /// If the matchID is right, the Player will be removed from the waiting List.
    /// If the List is empty afterwards we call the callback given by constructor.
    /// </summary>
    /// <param name="player">Player who send message.</param>
    /// <param name="matchID">The match ID for which the player reportedly loaded the match scene</param>
    public void CheckPlayerMessage(int matchID, IPlayer player) {
        if (matchID == _matchID) {
            _players.Remove(player);

            if (_players.Count == 0) {
                _allPlayersSyncedCallback?.Invoke(this);
            }
        }
    }
}
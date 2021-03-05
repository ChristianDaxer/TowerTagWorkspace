using System;
using UnityEngine;

/// <summary>
/// A map sequence that iterates over an array of <see cref="MatchDescription"/>s in order and in a looped fashion.
/// Each element of the sequence has boundary conditions with respect to the number of players.
/// Invalid elements are skipped.
/// </summary>
[CreateAssetMenu(menuName = "TowerTag/Match Sequence")]
public class MatchSequence : ScriptableObject {
    [SerializeField] private MatchSequenceElement[] _sequence;
    private int _index;

    [Serializable]
    public struct MatchSequenceElement {
        [SerializeField] private MatchDescription _matchDescription;
        [SerializeField] private int _minPlayers;
        [SerializeField] private int _maxPlayers;

        public MatchDescription MatchDescription => _matchDescription;
        public int MinPlayers => _minPlayers;
        public int MaxPlayers => _maxPlayers;
    }

    /// <summary>
    /// Returns the next viable match for the given number of participating players. The MatchSequence holds an index
    /// internally which is increased after every call to this function. This acts like an iterable.
    /// </summary>
    /// <param name="playerCount">The number of participating players</param>
    /// <returns>The next viable match description</returns>
    public MatchDescription Next(int playerCount) {
        foreach (MatchSequenceElement t in _sequence) {
            MatchSequenceElement element = _sequence[_index];
            _index = (_index + 1) % _sequence.Length;
            if (playerCount >= element.MinPlayers && playerCount <= element.MaxPlayers) return element.MatchDescription;
        }

        Debug.LogWarning($"No suitable match found for {playerCount} players");
        return null;
    }

    public MatchDescription Next(int playerCount, GameMode gameMode) {
        for (var i = 0; i < _sequence.Length; i++) {
            MatchSequenceElement element = _sequence[_index];
            _index = (_index + 1) % _sequence.Length;
            if (gameMode != GameMode.UserVote && !element.MatchDescription.GameMode.HasFlag(gameMode)) continue;
            if (playerCount >= element.MinPlayers && GetHighestTeamPlayerCount() <= element.MaxPlayers / 2)
                return element.MatchDescription;
        }

        Debug.LogWarning($"No suitable match found for {playerCount} players and game mode {gameMode}");
        return null;
    }

    private int GetHighestTeamPlayerCount() {
        int iceCount = PlayerManager.Instance.GetParticipatingIcePlayerCount();
        int fireCount = PlayerManager.Instance.GetParticipatingFirePlayerCount();
        return iceCount > fireCount ? iceCount : fireCount;
    }
}
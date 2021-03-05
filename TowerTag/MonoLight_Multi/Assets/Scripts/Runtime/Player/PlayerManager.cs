using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Photon.Pun;
using TowerTag;

/// <summary>
/// Registers all current players.
/// </summary>
public sealed class PlayerManager : IPlayerManager, IDisposable
{
    private struct CategorizedPlayerData
    {
        public IPlayer[] playerData;
        public int index;
        public CategorizedPlayerData (int initialAllocationSize)
        {
            playerData = new IPlayer[initialAllocationSize];
            index = 0;
        }

        public int Count => index;

        public IPlayer this[int i]
        {
            get { return playerData[i]; }
            set
            {
                if (i > playerData.Length -1)
                {
                    // Reallocate new array, copy old data into larger 
                    // array and replace old array with larger array.
                    IPlayer[] newPlayerData = new IPlayer[i+1];
                    Array.Copy(playerData, newPlayerData, playerData.Length);
                    playerData = newPlayerData; 
                }

                playerData[i] = value;
            }
        }

        public void Append (IPlayer player)
        {
            // This calls the overriding indexor method in this struct.
            this[index++] = player;
        }

        public void Reset () { index = 0; }

        // Automatically convert this struct into an IPlayer array by returning the reference inside the struct.
        public static implicit operator IPlayer[] (CategorizedPlayerData categorizedPlayerData) { return categorizedPlayerData.playerData; }
    }

    private struct CachedCategorizedData
    {
        public CategorizedPlayerData connectedPlayers;

        public CategorizedPlayerData humanPlayers;
        public CategorizedPlayerData aiPlayers;

        public CategorizedPlayerData participatingFirePlayers;
        public CategorizedPlayerData participatingIcePlayers;

        public CategorizedPlayerData participatingHumanFirePlayers;
        public CategorizedPlayerData participatingHumanIcePlayers;

        public CategorizedPlayerData allParticipatingPlayers;
        public CategorizedPlayerData humanParticipatingPlayers;
        public CategorizedPlayerData aiParticipatingPlayers;

        public CategorizedPlayerData spectatingPlayers;

        public CachedCategorizedData (int initialAllocationSize)
        {
            connectedPlayers = new CategorizedPlayerData(initialAllocationSize);

            humanPlayers = new CategorizedPlayerData(initialAllocationSize);
            aiPlayers = new CategorizedPlayerData(initialAllocationSize);

            participatingFirePlayers = new CategorizedPlayerData(initialAllocationSize);
            participatingIcePlayers = new CategorizedPlayerData(initialAllocationSize);

            participatingHumanFirePlayers = new CategorizedPlayerData(initialAllocationSize);
            participatingHumanIcePlayers = new CategorizedPlayerData(initialAllocationSize);

            allParticipatingPlayers = new CategorizedPlayerData(initialAllocationSize);
            humanParticipatingPlayers = new CategorizedPlayerData(initialAllocationSize);
            aiParticipatingPlayers = new CategorizedPlayerData(initialAllocationSize);

            spectatingPlayers = new CategorizedPlayerData(initialAllocationSize);
        }

        /* 
         * This resets all the indices for each array to 0 so
         * we can fill the array with IPlayer references from
         * the beginning.
        */
        public void ResetIndices ()
        {
            connectedPlayers.Reset();

            humanPlayers.Reset();
            aiPlayers.Reset();

            participatingFirePlayers.Reset();
            participatingIcePlayers.Reset();

            participatingFirePlayers.Reset();
            participatingIcePlayers.Reset();

            allParticipatingPlayers.Reset();
            humanParticipatingPlayers.Reset();
            aiParticipatingPlayers.Reset();

            spectatingPlayers.Reset();
        }
    }

    public event PlayerDelegate PlayerAdded;
    public event PlayerDelegate PlayerRemoved;
    public event PlayerDelegate OwnPlayerSet;

    private static PlayerManager _instance;

    [NotNull] public static PlayerManager Instance => _instance ?? (_instance = new PlayerManager());
    public bool IsAtLeastOneBotParticipating => GetAIPlayerCount() > 0;

    private readonly Dictionary<int, IPlayer> _players = new Dictionary<int, IPlayer>();
    private IPlayer _ownPlayer;

    private CachedCategorizedData cachedCategorizedData;

    private int participatingPlayerCount = 0;

    PlayerManager()
    {
        cachedCategorizedData = new CachedCategorizedData(8);
    }

    ~PlayerManager()
    {
        Dispose();
    }

    // This is called:
    // 1. At the beginning of every frame by MonoBehaviour : PlayerManagerUpdateData
    // 2. When a player is added.
    // 3. When a player is removed.
    public void UpdatePlayerReferencesCache ()
    {
        cachedCategorizedData.ResetIndices();

        foreach (KeyValuePair<int, IPlayer> keyValue in _players)
        {
            if (keyValue.Value != null && keyValue.Value.GameObject != null)
            {
                if (keyValue.Value.IsParticipating)
                {
                    if (!keyValue.Value.IsBot)
                        cachedCategorizedData.humanParticipatingPlayers.Append(keyValue.Value);
                    else cachedCategorizedData.aiParticipatingPlayers.Append(keyValue.Value);

                    cachedCategorizedData.allParticipatingPlayers.Append(keyValue.Value);

                    if (keyValue.Value.TeamID == TeamID.Fire)
                        cachedCategorizedData.participatingFirePlayers.Append(keyValue.Value);
                    else cachedCategorizedData.participatingIcePlayers.Append(keyValue.Value);
                }

                else cachedCategorizedData.spectatingPlayers.Append(keyValue.Value);

                if (!keyValue.Value.IsBot)
                    cachedCategorizedData.humanPlayers.Append(keyValue.Value);
                else cachedCategorizedData.aiPlayers.Append(keyValue.Value);

                cachedCategorizedData.connectedPlayers.Append(keyValue.Value);
            }
        }
    }

    public void AddPlayer(IPlayer player)
    {
        Debug.Log($"Adding {player}");
        if (!_players.ContainsKey(player.PlayerID))
        {
            _players.Add(player.PlayerID, player);
            if (player.IsMe)
            {
                _ownPlayer = player;
                OwnPlayerSet?.Invoke(player);
            }

            PlayerAdded?.Invoke(player);
            player.InitPlayerFromPlayerProperties(); // init after add, because now the player can be updated on changes

            UpdatePlayerReferencesCache();
        }
    }

    public IPlayer GetOwnPlayer()
    {
        return _ownPlayer;
    }

    public IPlayer GetPlayer(int playerID)
    {
        if (_players.ContainsKey(playerID) && _players[playerID].IsValid)
            return _players[playerID];

        return null;
    }

    public void RemovePlayer(int playerID)
    {
        if (_players.ContainsKey(playerID))
        {
            IPlayer player = _players[playerID];
            _players.Remove(playerID);

            if (player == _ownPlayer) _ownPlayer = null;

            if (player != null)
                PlayerRemoved?.Invoke(player);

            UpdatePlayerReferencesCache();
        }
    }

    /// <summary>
    /// A team counts as unbalanced if:
    /// * any team has less than 2 players
    /// * a team has more players than the other team
    /// 
    /// team id neutral is used for both teams unbalanced!
    /// </summary>
    public bool GetUnbalancedTeam(out (int ice, int fire) difference)
    {
        GetParticipatingFirePlayers(out var fire, out var fireCount);
        GetParticipatingIcePlayers(out var ice, out var iceCount);

        if (iceCount < 2 && fireCount < 2)
        {
            difference = (0, 0);

            if (fireCount < 2)
                difference.fire = 2 - fireCount;

            if (iceCount < 2)
                difference.ice = 2 - iceCount;
        }

        else difference = (iceCount < fireCount) ? (fireCount - iceCount, 0) : (0, iceCount - fireCount);
        
        Debug.Log("REPORT: " + (fireCount != iceCount || fireCount < 2 && iceCount < 2 ? "Teams are uneven." : "Teams are even.") + 
                  $"\nTeam ice must be corrected by {difference.ice}" +
                  $"\nTeam fire must be corrected by {difference.fire}");

        if (fireCount != iceCount)
            return true;

        return fireCount < 2 && iceCount < 2;
    }

    public bool OneTeamIsZero()
    {
        return PlayerManager.Instance.GetParticipatingFirePlayerCount() == 0 || PlayerManager.Instance.GetParticipatingIcePlayerCount() == 0;
    }

    public void RemovePlayer(IPlayer player)
    {
        foreach (KeyValuePair<int, IPlayer> pair in _players)
        {
            if (pair.Value == player)
            {
                RemovePlayer(pair.Key);
                UpdatePlayerReferencesCache();
                return;
            }
        }
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetParticipatingIcePlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.participatingIcePlayers;
        count = cachedCategorizedData.participatingIcePlayers.Count;
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetParticipatingFirePlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.participatingFirePlayers;
        count = cachedCategorizedData.participatingFirePlayers.Count;
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetParticipatingHumanFirePlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.participatingHumanFirePlayers;
        count = cachedCategorizedData.participatingHumanFirePlayers.Count;
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetParticipatingHumanIcePlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.participatingHumanIcePlayers;
        count = cachedCategorizedData.participatingHumanIcePlayers.Count;
    }

    public int GetParticipatingFirePlayerCount () { return cachedCategorizedData.participatingFirePlayers.Count; }
    public int GetParticipatingIcePlayerCount () { return cachedCategorizedData.participatingIcePlayers.Count; }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetAllConnectedPlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.connectedPlayers;
        count = cachedCategorizedData.connectedPlayers.Count;
    }

    public int GetAllConnectedPlayerCount()
    {
        return cachedCategorizedData.connectedPlayers.Count;
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetAllAIPlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.aiPlayers;
        count = cachedCategorizedData.aiPlayers.Count;
    }

    public int GetAIPlayerCount()
    {
        return cachedCategorizedData.aiPlayers.Count;
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetAllHumanPlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.humanPlayers;
        count = cachedCategorizedData.humanPlayers.Count;
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetAllParticipatingHumanPlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.humanParticipatingPlayers;
        count = cachedCategorizedData.humanParticipatingPlayers.Count;
    }

    public int GetAllParticipatingHumanPlayerCount()
    {
        return cachedCategorizedData.humanParticipatingPlayers.Count;
    }

    public int GetAllParticipatingAIPlayerCount()
    {
        return cachedCategorizedData.aiParticipatingPlayers.Count;
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetAllParticipatingAIPlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.aiParticipatingPlayers;
        count = cachedCategorizedData.aiParticipatingPlayers.Count;
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetParticipatingPlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.allParticipatingPlayers;
        count = cachedCategorizedData.allParticipatingPlayers.Count;
    }

    public int GetParticipatingPlayersCount()
    {
        return cachedCategorizedData.allParticipatingPlayers.Count;
    }

    /* The array returned by this function can return null players, therefore you need to use
     * the count index to determin how many non-null players exist in the array. The reason
     * this array may contain null entires is due to an to an optimization to prevent the
     * allocation of a player array every frame. */
    public void GetSpectatingPlayers(out IPlayer[] players, out int count)
    {
        players = cachedCategorizedData.spectatingPlayers;
        count = cachedCategorizedData.spectatingPlayers.Count;
    }
	
	public bool GetAIPlayerFromTeamWithMoreAIPlayer([CanBeNull] out IPlayer bot) {
        GetAllAIPlayers(out var bots, out var botCount);
        bot = null;
        if (botCount <= 0) return false;
        bot = bots.Count(player => player != null && player.TeamID == TeamID.Fire)
              >= bots.Count(player => player != null && player.TeamID == TeamID.Ice)
            ? bots.First(player => player != null && player.TeamID == TeamID.Fire)
            : bots.First(player => player != null && player.TeamID == TeamID.Ice);
        return true;
    }

    public void Dispose()
    {
        _players.Clear();
        _instance = null;

        cachedCategorizedData.ResetIndices();
    }
}
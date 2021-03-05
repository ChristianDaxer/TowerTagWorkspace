using System;
using System.Collections.Generic;
using System.Linq;
using Commendations;
using JetBrains.Annotations;
using TowerTag;

public interface IMatchStats
{
    GameMode GameMode { get; }
    Dictionary<int, PlayerStats> GetPlayerStats();
}

public struct PlayerStats
{
    public int PlayerID;
    public TeamID TeamID;
    public int Kills;
    public int Assists;
    public int Deaths;

    public int ShotsFired;
    public int HitsDealt;
    public int HitsTaken;
    public int DamageDealt;
    public int DamageTaken;
    public int HealthHealed;
    public int HealingReceived;
    public int GoalPillarsClaimed;
    public int PillarsClaimed;
    public int Teleports;
    public int HeadShots;
    public int SniperShots;
    public int Doubles;
    public string Commendation;
    public float PlayTime;

    public PlayerStats(int playerID, TeamID teamID)
    {
        PlayerID = playerID;
        TeamID = teamID;
        Kills = 0;
        Assists = 0;
        Deaths = 0;

        ShotsFired = 0;
        HitsDealt = 0;
        HitsTaken = 0;
        DamageDealt = 0;
        DamageTaken = 0;
        HealthHealed = 0;
        HealingReceived = 0;
        GoalPillarsClaimed = 0;
        PillarsClaimed = 0;
        Teleports = 0;
        HeadShots = 0;
        SniperShots = 0;
        Doubles = 0;
        Commendation = "";
        PlayTime = 0;
    }

    public bool Serialize(BitSerializer stream)
    {
        //success = success &&
        bool success = stream.Serialize(ref PlayerID, BitCompressionConstants.MinPlayerID,
            BitCompressionConstants.MaxPlayerID);
        success = success && stream.Serialize(ref TeamID);
        success = success && stream.Serialize(ref Kills, 0, BitCompressionConstants.MaxKillsPerPlayer);
        success = success && stream.Serialize(ref Assists, 0, BitCompressionConstants.MaxAssistsPerPlayer);
        success = success && stream.Serialize(ref Deaths, 0, BitCompressionConstants.MaxDeathsPerPlayer);
        return success;
    }
}

public abstract class MatchStats : IMySerializable, IMatchStats
{
    #region globalStats

    public static Action<TeamID, int> OnTeamPointAdded;
    
    public abstract GameMode GameMode { get; }
    private Dictionary<int, PlayerStats> _playerStats;
    private Dictionary<TeamID, TeamStats> _teamStats;
    private readonly List<RoundStats> _roundStats;

    public DateTime StartTime { get; set; }

    private int _roundsStarted;

    public int RoundsStarted
    {
        get => _roundsStarted;
        set => _roundsStarted = value;
    }

    // all Pillars in Scene
    private int _numberOfPillarsInScene;
    public int NumberOfPillarsInScene => _numberOfPillarsInScene;

    private int _numberOfGoalPillarsInScene;

    #endregion

    #region TeamStats

    public struct TeamStats
    {
        public TeamID TeamID;
        public int Points;

        // number of Pillars owned by this Team
        public int CapturedPillars;
        public int CapturedGoalPillars;

        public TeamStats(TeamID teamID, int startPoints)
        {
            CapturedPillars = 0;
            CapturedGoalPillars = 0;
            TeamID = teamID;
            Points = startPoints;
        }

        public bool Serialize(BitSerializer stream)
        {
            bool success = stream.Serialize(ref TeamID);
            success = success && stream.Serialize(ref Points, 0, BitCompressionConstants.MaxTeamPoints);
            success = success && stream.Serialize(ref CapturedPillars, 0, BitCompressionConstants.MaxPillarCount);
            success = success && stream.Serialize(ref CapturedGoalPillars, 0,
                          BitCompressionConstants.MaxPillarCount);
            return success;
        }
    }

    #endregion

    #region PlayerStats

    #endregion

    #region round stats

    public struct RoundStats
    {
        public TeamID WinningTeamID;
        public int PlayTimeInSeconds;
    }

    #endregion

    #region CoreFunctions

    /// <summary>
    /// Create new GameModeTeamDeathMatchStats instance.
    /// </summary>
    protected MatchStats()
    {
        _teamStats = new Dictionary<TeamID, TeamStats>();
        _playerStats = new Dictionary<int, PlayerStats>();
        _roundStats = new List<RoundStats>();
    }

    /// <summary>
    /// Create new GameModeTeamDeathMatchStats instance.
    /// </summary>
    /// <param name="players">Players to register in Stats</param>
    protected MatchStats(IPlayer[] players, int playerCount)
    {
        _teamStats = new Dictionary<TeamID, TeamStats>();
        _playerStats = new Dictionary<int, PlayerStats>();
        _roundStats = new List<RoundStats>();

        for (int i = 0; i < playerCount; i++)
            AddPlayer(players[i]);
    }

    /// <summary>
    /// Add Frag to Match Statistics
    /// </summary>
    /// <param name="killedPlayer">The Player who was killed.</param>
    /// <param name="killingPlayer">Player who set the last shot which killed the Player.</param>
    /// <param name="enemiesWhoAppliedDamage">All Player who applied damage until the Players death (including the killing Player). </param>
    public void AddFrag(int killedPlayer, int killingPlayer, int[] enemiesWhoAppliedDamage)
    {
        if (_playerStats.ContainsKey(killedPlayer))
        {
            PlayerStats kP = _playerStats[killedPlayer];
            kP.Deaths += 1;
            _playerStats[killedPlayer] = kP;
        }
        else
        {
            Debug.LogWarning($"Cannot register death statistics: killingPlayer (id = {killedPlayer}) not found");
        }

        if (_playerStats.ContainsKey(killingPlayer))
        {
            PlayerStats kP = _playerStats[killingPlayer];
            kP.Kills += 1;
            _playerStats[killingPlayer] = kP;
        }
        else if (killingPlayer > -1)
        {
            Debug.LogWarning($"Cannot register kill statistics: killingPlayer (id = {killingPlayer}) not found");
        }

        // apply a assists point to every enemy who applied damage for this kill
        if (enemiesWhoAppliedDamage != null)
        {
            foreach (int enemy in enemiesWhoAppliedDamage)
            {
                if (enemy != killingPlayer && _playerStats.ContainsKey(enemy))
                {
                    PlayerStats e = _playerStats[enemy];
                    e.Assists += 1;
                    _playerStats[enemy] = e;
                }
            }
        }
    }

    public void AddDoubleKill([NotNull] IPlayer player)
    {
        if (!_playerStats.ContainsKey(player.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add double kill to stats of player {player}! Player stats doesn't contain player ID");
            return;
        }

        PlayerStats shootingPlayerStats = _playerStats[player.PlayerID];
        shootingPlayerStats.Doubles++;
        _playerStats[player.PlayerID] = shootingPlayerStats;
    }

    public void AddSniperKill([NotNull] IPlayer player)
    {
        if (!_playerStats.ContainsKey(player.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add sniper kill to stats of player {player}! Player stats doesn't contain player ID");
            return;
        }

        PlayerStats shootingPlayerStats = _playerStats[player.PlayerID];
        shootingPlayerStats.SniperShots++;
        _playerStats[player.PlayerID] = shootingPlayerStats;
    }

    public void AddHeadshot([NotNull] IPlayer player)
    {
        if (!_playerStats.ContainsKey(player.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add headshot to stats of player {player}! Player stats doesn't contain player ID");
            return;
        }

        PlayerStats shootingPlayerStats = _playerStats[player.PlayerID];
        shootingPlayerStats.HeadShots++;
        _playerStats[player.PlayerID] = shootingPlayerStats;
    }

    public void AddShot([NotNull] IPlayer shootingPlayer)
    {
        if (!_playerStats.ContainsKey(shootingPlayer.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add shot to stats! Player stats doesn't contain player ID of {shootingPlayer}");
            return;
        }

        PlayerStats shootingPlayerStats = _playerStats[shootingPlayer.PlayerID];
        shootingPlayerStats.ShotsFired++;
        _playerStats[shootingPlayer.PlayerID] = shootingPlayerStats;
    }

    public void AddHit([NotNull] IPlayer targetPlayer, [NotNull] IPlayer shootingPlayer, int damage)
    {
        if (!_playerStats.ContainsKey(targetPlayer.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add hit taken to stats! Player stats doesn't contain player ID of {targetPlayer}");
            return;
        }

        if (!_playerStats.ContainsKey(shootingPlayer.PlayerID))
        {
            Debug.LogWarning($"Can't add hit to stats! Player stats doesn't contain player ID of {shootingPlayer}");
            return;
        }

        PlayerStats targetPlayerStats = _playerStats[targetPlayer.PlayerID];
        PlayerStats shootingPlayerStats = _playerStats[shootingPlayer.PlayerID];
        targetPlayerStats.HitsTaken++;
        targetPlayerStats.DamageTaken += damage;
        shootingPlayerStats.HitsDealt++;
        shootingPlayerStats.DamageDealt += damage;
        _playerStats[targetPlayer.PlayerID] = targetPlayerStats;
        _playerStats[shootingPlayer.PlayerID] = shootingPlayerStats;
    }

    public void AddHeal([NotNull] IPlayer targetPlayer, [NotNull] IPlayer healingPlayer, int amount)
    {
        if (!_playerStats.ContainsKey(targetPlayer.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add heal receive to stats! Player stats doesn't contain player ID of {targetPlayer}");
            return;
        }

        if (!_playerStats.ContainsKey(healingPlayer.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add heal given to stats! Player stats doesn't contain player ID of {healingPlayer}");
            return;
        }

        PlayerStats targetPlayerStats = _playerStats[targetPlayer.PlayerID];
        PlayerStats healingPlayerStats = _playerStats[healingPlayer.PlayerID];
        targetPlayerStats.HealingReceived += amount;
        healingPlayerStats.HealthHealed += amount;
        _playerStats[targetPlayer.PlayerID] = targetPlayerStats;
        _playerStats[healingPlayer.PlayerID] = healingPlayerStats;
    }

    public void AddTeleport([NotNull] IPlayer player)
    {
        if (!_playerStats.ContainsKey(player.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add teleport to stats of player {player}! PlayerStats doesn't contain player ID");
            return;
        }

        PlayerStats playerStats = _playerStats[player.PlayerID];
        playerStats.Teleports++;
        _playerStats[player.PlayerID] = playerStats;
    }

    public void AddClaim([NotNull] IPlayer player, [NotNull] Pillar pillar)
    {
        if (!_playerStats.ContainsKey(player.PlayerID))
        {
            Debug.LogWarning($"Can't add claim to stats of player {player}! PlayerStats doesn't contain player ID");
            return;
        }

        PlayerStats playerStats = _playerStats[player.PlayerID];
        playerStats.PillarsClaimed++;
        if (pillar.IsGoalPillar)
            playerStats.GoalPillarsClaimed++;
        _playerStats[player.PlayerID] = playerStats;
    }

    public void AddCommendation([NotNull] IPlayer player, ICommendation commendation)
    {
        if (!_playerStats.ContainsKey(player.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add commendation to stats of player {player}! PlayerStats doesn't contain player ID");
            return;
        }

        PlayerStats playerStats = _playerStats[player.PlayerID];
        playerStats.Commendation = commendation.DisplayName;
        _playerStats[player.PlayerID] = playerStats;
    }

    public void AddRound(RoundStats roundStats)
    {
        _roundStats.Add(roundStats);
    }

    public void SetPlayTime([NotNull] IPlayer player, float playTime)
    {
        if (!_playerStats.ContainsKey(player.PlayerID))
        {
            Debug.LogWarning(
                $"Can't add playtime to stats of player {player}! PlayerStats doesn't contain player ID");
            return;
        }

        PlayerStats playerStats = _playerStats[player.PlayerID];
        playerStats.PlayTime = playTime;
        _playerStats[player.PlayerID] = playerStats;
    }

    /// <summary>
    /// Add points to TeamStats for given Team.
    /// </summary>
    /// <param name="teamID">Team who gets the points.</param>
    public void AddTeamPoint(TeamID teamID)
    {
        if (!_teamStats.ContainsKey(teamID))
        {
            Debug.LogError("Cannot add team points: team to credit is not registered.");
            return;
        }

        TeamStats teamStats = _teamStats[teamID];
        teamStats.Points++;
        _teamStats[teamID] = teamStats;
        
        OnTeamPointAdded?.Invoke(teamID, teamStats.Points);
    }

    /// <summary>
    /// Set the number of Pillars captured by a Team. To set the overall number of Pillars in the current scene (independent of team occupancy) use -1 as teamID.
    /// </summary>
    /// <param name="teamID">TeamID of the Team you want to set the number captured Pillars for. (-1: if you want to set the overall count of Pillars in the scene, 0 or 1: set the number of Pillars captured by Team Fire or team Ice)</param>
    /// <param name="numberOfPillars">Number of Pillars captured by the Team associated with the given teamID (if teamID is -1, the number of Pillars is saved in numberOfPillarsInScene)</param>
    /// <param name="setGoalPillars"></param>
    public void SetNumberOfCapturedPillarsForTeam(TeamID teamID, int numberOfPillars, bool setGoalPillars = false)
    {
        if (_teamStats == null)
        {
            Debug.LogError("Cannot set number of captured pillars: team stats are null");
            return;
        }

        // neutral scene Pillars
        if (teamID == TeamID.Neutral)
        {
            if (setGoalPillars)
                _numberOfGoalPillarsInScene = numberOfPillars;
            else
                _numberOfPillarsInScene = numberOfPillars;
        }

        // if Team not available -> Add it as new Team
        if (!_teamStats.ContainsKey(teamID))
            AddTeam(teamID);

        // check again to be sure Team was added successfully
        if (_teamStats.ContainsKey(teamID))
        {
            TeamStats teamStats = _teamStats[teamID];

            if (setGoalPillars)
                teamStats.CapturedGoalPillars = numberOfPillars;
            else
                teamStats.CapturedPillars = numberOfPillars;

            _teamStats[teamID] = teamStats;
        }
        else
        {
            Debug.LogError(
                "GameModeTeamDeathMatchStats.SetNumberOfCapturedPillarsForTeam: Can't set number of Pillars for team because teamID(" +
                teamID + ") is not valid (no Team with this ID could be found)!");
        }
    }

    /// <summary>
    /// Add new Team to stats (if not registered yet).
    /// </summary>
    /// <param name="teamID">Team to add.</param>
    private void AddTeam(TeamID teamID)
    {
        if (!_teamStats.ContainsKey(teamID))
            _teamStats.Add(teamID, new TeamStats(teamID, 0));
    }

    /// <summary>
    /// Add Player to stats (if not already registered).
    /// </summary>
    /// <param name="player">Player to add.</param>
    public void AddPlayer(IPlayer player)
    {
        if (player == null)
        {
            Debug.LogError("GameModeStats:TeamDeathMatch.AddPlayer: Player is null!");
            return;
        }

        if (_playerStats.ContainsKey(player.PlayerID))
            return;

        _playerStats.Add(player.PlayerID, new PlayerStats(player.PlayerID, player.TeamID));
        AddTeam(player.TeamID);
    }

    /// <summary>
    /// Remove Player from stats.
    /// </summary>
    /// <param name="player">Player to remove.</param>
    public void RemovePlayer(IPlayer player)
    {
        if (player == null)
        {
            Debug.LogError("Cannot remove player from stats: player is null!");
            return;
        }

        if (!_playerStats.ContainsKey(player.PlayerID))
        {
            //            Debug.LogError("GameModeStats:TeamDeathMatch.RemovePlayer: Player is not in Dictionary (Player: " +
            //                           player.PlayerID + ")!");
            return;
        }

        _playerStats.Remove(player.PlayerID);
    }

    #region Getter

    /// <summary>
    /// Get Player stats Dictionary from stats.
    /// Key:    PlayerID of the Player
    /// Value:  PlayerStats of the Player
    /// </summary>
    /// <returns>Returns Dictionary with Player stats of all registered players.</returns>
    public Dictionary<int, PlayerStats> GetPlayerStats()
    {
        return _playerStats;
    }

    /// <summary>
    /// Get TeamStats Dictionary from stats.
    /// Key:    teamID of the Team
    /// Value:  Team stats of the Team
    /// </summary>
    /// <returns>Returns Dictionary with Team stats of all registered Teams.</returns>
    public Dictionary<TeamID, TeamStats> GetTeamStats()
    {
        return _teamStats;
    }

    public List<RoundStats> GetRoundStats()
    {
        return _roundStats;
    }

    #endregion

    #endregion

    #region Serialization

    /// <summary>
    /// Serialize the internal state:
    /// - call it with writeStream to write the internal state to stream
    /// - call it with readStream to deserialize the internal state from stream
    /// </summary>
    /// <param name="stream">Stream to read from or write your data to.</param>
    /// <returns>True if succeeded read/write, false otherwise.</returns>
    public bool Serialize(BitSerializer stream)
    {
        // Serialize/Deserialize simple Member
        bool success = stream.Serialize(ref _roundsStarted, BitCompressionConstants.MinMatchRoundsToPlay,
            BitCompressionConstants.MaxMatchRoundsToPlay);
        success = success && stream.Serialize(ref _numberOfPillarsInScene, 0,
                      BitCompressionConstants.MaxPillarCount);
        success = success && stream.Serialize(ref _numberOfGoalPillarsInScene, 0,
                      BitCompressionConstants.MaxPillarCount);

        // write Player-/TeamStats
        if (stream.IsWriting)
        {
            // Serialize PlayerStats to stream
            stream.WriteInt(_playerStats.Count, 0, BitCompressionConstants.MaxPlayerCount);
            foreach (PlayerStats t in _playerStats.Values)
            {
                success = success && t.Serialize(stream);
            }

            // Serialize TeamStats to stream
            stream.WriteInt(_teamStats.Count, 0, BitCompressionConstants.MaxTeamCount);
            foreach (TeamStats t in _teamStats.Values)
            {
                success = success && t.Serialize(stream);
            }
        }
        // read Player-/TeamStats
        else
        {
            // Deserialize PlayerStats from stream
            int playerCount = stream.ReadInt(0, BitCompressionConstants.MaxPlayerCount);

            if (_playerStats == null)
                _playerStats = new Dictionary<int, PlayerStats>();
            else
                _playerStats.Clear();


            for (var i = 0; i < playerCount; i++)
            {
                var playerStats = new PlayerStats();
                success = success && playerStats.Serialize(stream);

                if (!_playerStats.ContainsKey(playerStats.PlayerID))
                    _playerStats.Add(playerStats.PlayerID, playerStats);
            }

            // Deserialize TeamStats from stream
            int teamCount = stream.ReadInt(0, BitCompressionConstants.MaxTeamCount);

            if (_teamStats == null)
                _teamStats = new Dictionary<TeamID, TeamStats>();
            else
                _teamStats.Clear();

            for (var i = 0; i < teamCount; i++)
            {
                var teamStats = new TeamStats();
                teamStats.Serialize(stream);

                if (!_teamStats.ContainsKey(teamStats.TeamID))
                    _teamStats.Add(teamStats.TeamID, teamStats);
            }
        }

        return success;
    }

    #endregion

    #region Helper

    public TeamID WinningTeamID
    {
        get
        {
            if (_teamStats.Count == 0)
                return TeamID.Neutral;

            if (_teamStats.Count == 1)
                return _teamStats.First().Key;

            TeamStats[] orderedTeamStats = _teamStats.Values.OrderByDescending(x => x.Points).ToArray();
            if (orderedTeamStats[0].Points == orderedTeamStats[1].Points)
                return TeamID.Neutral;

            return orderedTeamStats[0].TeamID;
        }
    }

    public bool Draw => WinningTeamID == TeamID.Neutral;

    #endregion

    public void ResetStats(IPlayer player)
    {
        _playerStats[player.PlayerID] = new PlayerStats(player.PlayerID, player.TeamID);
    }
}
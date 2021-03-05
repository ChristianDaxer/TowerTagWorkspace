using System;
using System.Collections.Generic;
using TowerTag;

[Serializable]
public class PreviousMatchResults
{
    private static FinalMatchStats _previousMatchStats;

    [Serializable]
    public struct FinalPlayerStats {
        public string Name;
        public int Score;
        public int Outs;
        public int Assists;

        public FinalPlayerStats(string name, int score, int outs, int assists) {
            Name = name;
            Score = score;
            Outs = outs;
            Assists = assists;
        }
    }

    [Serializable]
    public struct FinalTeamStats {
        public string Name;
        public int Points;
        public Dictionary<int,FinalPlayerStats> PlayerStats;

        public FinalTeamStats(string name, int points, Dictionary<int, FinalPlayerStats> playerStats) {
            Name = name;
            Points = points;
            PlayerStats = playerStats;
        }
    }

    [Serializable]
    public struct FinalMatchStats {
        public TeamID WinningTeamID;
        public Dictionary<TeamID,FinalTeamStats> TeamStats;

        public FinalMatchStats(TeamID winningTeamID, Dictionary<TeamID, FinalTeamStats> finalMatchStats) {
            WinningTeamID = winningTeamID;
            TeamStats = finalMatchStats;
        }
    }

    public static void SaveMatchStats(IMatch match) {
        Dictionary<int, FinalPlayerStats> firePlayerStats = new Dictionary<int, FinalPlayerStats>();
        Dictionary<int, FinalPlayerStats> icePlayerStats = new Dictionary<int, FinalPlayerStats>();
        match.Stats.GetPlayerStats().ForEach(player => {
            string playerName = PlayerManager.Instance.GetPlayer(player.Key)?.PlayerName;
            FinalPlayerStats playerStats = new FinalPlayerStats(playerName, player.Value.Kills, player.Value.Deaths, player.Value.Assists);
            if (player.Value.TeamID == TeamID.Fire)
                firePlayerStats.Add(player.Key, playerStats);
            else if (player.Value.TeamID == TeamID.Ice)
                icePlayerStats.Add(player.Key, playerStats);
        });
        Dictionary<TeamID, FinalTeamStats> finalTeamStats = new Dictionary<TeamID, FinalTeamStats>();
        match.Stats.GetTeamStats().ForEach(teamStats => {
            string teamName = TeamManager.Singleton.Get(teamStats.Key)?.Name;
            if(teamStats.Key == TeamID.Fire)
                finalTeamStats.Add(teamStats.Key, new FinalTeamStats(teamName, teamStats.Value.Points, firePlayerStats));
            if (teamStats.Key == TeamID.Ice)
                finalTeamStats.Add(teamStats.Key, new FinalTeamStats(teamName, teamStats.Value.Points, icePlayerStats));
        });
        _previousMatchStats = new FinalMatchStats(match.Stats.WinningTeamID, finalTeamStats);
    }

    public static FinalMatchStats GetPreviousMatchStats() {
        return _previousMatchStats;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Commendations;
using Newtonsoft.Json;
using Photon.Pun;
using TowerTag;
using TowerTagAPIClient;
using TowerTagAPIClient.Store;
using UnityEngine;
using Match = TowerTagAPIClient.Model.Match;

public class MatchReportService : MonoBehaviour {
    [SerializeField] private CommendationsController _commendationsController;
    [SerializeField] private bool _sendReport = true;

    private void OnEnable() {
        _commendationsController.CommendationsAwarded += OnCommendationsAwarded;
    }

    private void OnDisable() {
        _commendationsController.CommendationsAwarded -= OnCommendationsAwarded;
    }

    private void OnCommendationsAwarded(CommendationsController sender,
        Dictionary<IPlayer, (ICommendation, int)> commendations) {
        if (!PhotonNetwork.IsMasterClient) return;

        try {
            if (GameManager.Instance.TrainingVsAI) return;
            Match matchReport = GenerateMatchReport();
            Debug.Log($"Created Report ${JsonConvert.SerializeObject(matchReport)}");
            if (_sendReport) {
                Debug.Log("Sending match report");

                PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);

                int loggedInCount = 0, isBot = 0;
                for (int i = 0; i < count; i++)
                {
                    if (players[i].IsLoggedIn)
                        loggedInCount++;
                    if (players[i].IsBot)
                        isBot++;
                }

                AnalyticsController.MatchReport(
                    PhotonNetwork.CurrentRoom.Name,
                    ConfigurationManager.Configuration.LocationName,
                    count,
                    loggedInCount,
                    isBot);

                MatchStore.Report(Authentication.OperatorApiKey, matchReport);
            }
        }
        catch (Exception e) {
            Debug.LogException(e);
        }
    }

    private static Match GenerateMatchReport() {
        IMatch match = GameManager.Instance.CurrentMatch;
        MatchDescription matchDescription = MatchDescriptionCollection.Singleton.GetMatchDescription(match.MatchID);
        MatchStats matchStats = match.Stats;
        Dictionary<int, PlayerStats> playerStats = matchStats.GetPlayerStats();
        Dictionary<TeamID, MatchStats.TeamStats> teamStats = matchStats.GetTeamStats();

        PlayerManager.Instance.GetParticipatingPlayers(out var player, out var count);
        List<IPlayer> notLoggedInPlayersList = new List<IPlayer>();
        for (int i = 0; i < count; i++)
        {
            if (!player[i].IsLoggedIn)
                notLoggedInPlayersList.Add(player[i]);
        }

        IPlayer[] notLoggedInPlayers = notLoggedInPlayersList.ToArray();

        for (var i = 0; i < notLoggedInPlayers.Length; i++) {
            IPlayer notLoggedInPlayer = notLoggedInPlayers[i];
            if (notLoggedInPlayer.IsBot)
                notLoggedInPlayer.MembershipID = $"Bot_{notLoggedInPlayer.BotDifficulty}_{i}";
            else
                notLoggedInPlayer.MembershipID = ConfigurationManager.Configuration.LocationName + "_" + i;
        }

        // game mode name: this must correspond to the old naming that the backend still works with
        // to this date (2019-08-05), only matches with game mode "DeathMatch" are processed.
        var gameModeName = "";
        if (match.GameMode == GameMode.Elimination) gameModeName = "DeathMatch";
        if (match.GameMode == GameMode.DeathMatch) gameModeName = "Arcade";
        if (match.GameMode == GameMode.GoalTower) gameModeName = "GoalTower";

        PlayerManager.Instance.GetParticipatingPlayers(out var players, out var participatingPlayerCount);
        PlayerManager.Instance.GetParticipatingFirePlayers(out var firePlayers, out var fireCount);
        PlayerManager.Instance.GetParticipatingIcePlayers(out var icePlayers, out var iceCount);

        var matchReport = new Match {
            id = "", // filled out by backend
            roomName = PhotonNetwork.CurrentRoom.Name,
            basicMode = TowerTagSettings.BasicMode,
            appVersion = Application.version,
            date = DateTime.Now,
            map = matchDescription.MapName,
            startTime = matchStats.StartTime,
            teams = new[] {
                new Match.Team {
                    id = 0,
                    name = TeamManager.Singleton.TeamFire.Name,
                    players = firePlayers
                        .Where(p => p != null)
                        .Select(p => p.MembershipID).ToArray()
                },
                new Match.Team {
                    id = 1,
                    name = TeamManager.Singleton.TeamIce.Name,
                    players = icePlayers
                        .Where(p => p != null)
                        .Select(p => p.MembershipID).ToArray()
                }
            },
            players = players
                .Where(p => p != null)
                .Select(p => new Match.Player {
                    id = p.MembershipID,
                    isBot = p.IsBot,
                    isMember = p.IsLoggedIn,
                    teamId = (int)p.TeamID
                })
                .ToArray(),
            location = TowerTagSettings.Home ? "Home" : ConfigurationManager.Configuration.LocationName,
            gameMode = gameModeName,
            rounds = matchStats.GetRoundStats().Count,
            matchTime = (int) HelperFunctions.GetTimeDifferenceInSecFromPhotonServerTimestamps(
                match.MatchStartAtTimestamp, match.MatchFinishedAtTimestamp),
            teamScores = new Dictionary<int, int> {
                {0, teamStats.ContainsKey(TeamID.Fire) ? teamStats[TeamID.Fire].Points : 0},
                {1, teamStats.ContainsKey(TeamID.Ice) ? teamStats[TeamID.Ice].Points : 0}
            },
            winningTeam = (int) matchStats.WinningTeamID,
            playerPerformances = players
                .Where(p => p != null)
                .ToDictionary(p => p.MembershipID, p => {
                    PlayerStats stats = playerStats[p.PlayerID];
                    return new Match.PlayerPerformance {
                        outs = stats.Deaths,
                        score = stats.Kills,
                        assists = stats.Assists,
                        shotsFired = stats.ShotsFired,
                        hitsDealt = stats.HitsDealt,
                        hitsTaken = stats.HitsTaken,
                        damageDealt = stats.DamageDealt,
                        damageTaken = stats.DamageTaken,
                        teleports = stats.Teleports,
                        healthHealed = stats.HealthHealed,
                        healingReceived = stats.HealingReceived,
                        pillarsClaimed = stats.PillarsClaimed,
                        goalPillarsClaimed = stats.GoalPillarsClaimed,
                        playTime = stats.PlayTime,
                        doubles = stats.Doubles,
                        headshots = stats.HeadShots,
                        snipershots = stats.SniperShots,
                        commendation = stats.Commendation
                    };
                }),
            roundDetails = matchStats.GetRoundStats().Select(rs => new Match.Round {
                winningTeam = (int) rs.WinningTeamID,
                playTimeInSeconds = rs.PlayTimeInSeconds
            }).ToList(),
        };
        return matchReport;
    }
}
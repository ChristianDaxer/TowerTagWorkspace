using System.Collections.Generic;
using System.Linq;
using Toornament.Store;
using Toornament.Store.Model;
using TowerTag;
using UnityEngine;
using VRNerdsUtilities;
using Match = Toornament.Store.Model.Match;

namespace Toornament {
    public class TowerTagToornamentController : SingletonMonoBehaviour<TowerTagToornamentController> {
        private Match _selectedToornamentMatch;

        private void Start() {
            transform.parent = ToornamentContainer.Instance.transform;
            ToornamentManager.Instance.OnSelectMatch += SelectMatch;
            // TODO add event listener for scores and results
            GameManager.Instance.MatchHasChanged += RegisterMatchCallbacks;
        }

        public static void Init() {
            Debug.Log("Initializing Toornament manager");
        }

        private void RegisterMatchCallbacks(IMatch match)
        {
            if (match == null) return;
            match.RoundFinished += OnRoundFinished;
            match.Finished += OnMatchFinished;
        }

        private void OnMatchFinished(IMatch match) {
            if (_selectedToornamentMatch == null)
                return;
            MatchStats teamDeathMatchStats = match.Stats;
            if (teamDeathMatchStats == null) {
                LogWarning("Cannot report match result: no team death match stats available");
                return;
            }

            if (teamDeathMatchStats.Draw) {
                LogMessage("reporting draw");
                ToornamentManager.Instance.ReportResult(null);
            }
            else {
                ITeam winningTeam = TeamManager.Singleton.Get(teamDeathMatchStats.WinningTeamID);
                if (winningTeam == null || teamDeathMatchStats.WinningTeamID == TeamID.Neutral) {
                    LogWarning("Cannot report match result: no winning team");
                    return;
                }

                LogMessage("reporting winning team " + winningTeam.Name);
                ToornamentManager.Instance.ReportResult(winningTeam.Name);
            }

            match.RoundFinished -= OnRoundFinished;
            match.Finished -= OnMatchFinished;
            _selectedToornamentMatch = null;
        }

        private void OnRoundFinished(IMatch match, TeamID teamID) {
            if (_selectedToornamentMatch == null)
                return;
            MatchStats teamDeathMatchStats = match.Stats;
            if (teamDeathMatchStats == null) {
                LogWarning("Cannot report match result: no team death match stats");
                return;
            }

            Dictionary<TeamID,MatchStats.TeamStats> teamStats = teamDeathMatchStats.GetTeamStats();
            ITeam teamFire = TeamManager.Singleton.TeamFire;
            ITeam teamIce = TeamManager.Singleton.TeamIce;
            ToornamentManager.Instance.ReportScore(new Dictionary<string, int> {
                {teamFire.Name, teamStats[TeamID.Fire].Points},
                {teamIce.Name, teamStats[TeamID.Ice].Points}
            });
            LogMessage("reporting scores");
        }

        private static void LogWarning(string message) {
            ToornamentImgui.Instance.ShowAsText(message);
            Debug.LogWarning(message);
        }

        private static void LogMessage(string message) {
            ToornamentImgui.Instance.ShowAsText(message);
            Debug.Log(message);
        }

        private void SelectMatch(Match match) {
            _selectedToornamentMatch = match;
            Opponent[] opponents = match.opponents;

            // randomly assign toornament opponents to team fire or ice
            opponents = opponents.OrderBy(x => Random.value).ToArray();
            for (var teamIndex = 0; teamIndex < 2; teamIndex++) {
                AdminController.Instance.SetTeamName((TeamID) teamIndex, opponents[teamIndex].ParticipantName);
                Participant.Lineup[] participantLineup = opponents[teamIndex].Participant.lineup;
                if (participantLineup == null) {
                    ToornamentImgui.Instance.ShowAsText("Failed to assign player names: Participant "
                                                        + opponents[teamIndex].ParticipantName
                                                        + " has no lineup.");
                    continue;
                }

                string[] playerNames = participantLineup
                    .Select(lineup => lineup.name)
                    .Where(playerName => !string.IsNullOrEmpty(playerName))
                    .ToArray();
                AdminController.SetTeamPlayerNames((TeamID)teamIndex, playerNames);
            }
        }

        private void OnDestroy() {
            ToornamentManager.Instance.OnSelectMatch -= SelectMatch;
        }
    }
}
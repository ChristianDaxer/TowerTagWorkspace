using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TowerTagAPIClient.Model;
using TowerTagAPIClient.Store;
using UnityEngine;

namespace TowerTagAPIClient.UI {
    public class TowerTagAPIIMGUI : MonoBehaviour {
        private bool _china;
        private string _version;
        private string _playerID;
        private string _playerName;
        private string _selectedMatchID;
        private Match _selectedMatch;
        private string _newPlayerID;
        private List<string> _players = new List<string>();
        private MatchOverview[] _matchOverviews;

        private void OnEnable() {
            VersionStore.VersionReceived += OnVersionReceived;
            PlayerStore.PlayerReceived += OnReceivedPlayer;
            MatchStore.OverviewReceived += OnOverviewReceived;
            MatchStore.MatchReceived += OnMatchReceived;
            PlayerStatisticsStore.PlayerStatisticsReceived += OnStatisticsReceived;
        }

        private void OnDisable() {
            VersionStore.VersionReceived -= OnVersionReceived;
            PlayerStore.PlayerReceived -= OnReceivedPlayer;
            MatchStore.OverviewReceived -= OnOverviewReceived;
            MatchStore.MatchReceived -= OnMatchReceived;
            PlayerStatisticsStore.PlayerStatisticsReceived -= OnStatisticsReceived;
        }

        private void OnGUI() {
            GUILayout.BeginScrollView(Vector2.zero, GUILayout.Width(500));
            RenderPlayerInfo();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            _china = GUILayout.Toggle(_china, "China");
            if (GUILayout.Button("Get Version")) {
                VersionStore.GetLatestVersion(Authentication.OperatorApiKey, _china, true);
            }

            GUILayout.Label(_version);
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Get My Player Info")) {
                PlayerStore.GetMyPlayer(Authentication.PlayerApiKey, Authentication.JWT);
            }

            if (GUILayout.Button("Get Player Info by ID")) {
                PlayerStore.GetPlayer(Authentication.OperatorApiKey, _playerID, true);
            }

            if (GUILayout.Button("Get Match Overview by ID")) {
                MatchStore.GetOverviewByID(Authentication.PlayerApiKey, _playerID, true);
            }

            if (GUILayout.Button("Get Statistics by ID")) {
                PlayerStatisticsStore.GetStatistics(Authentication.PlayerApiKey, _playerID, true);
            }

            RenderMatchList();

            GUILayout.Space(10);

            if (_selectedMatch != null && _selectedMatch.id == _selectedMatchID) {
                GUILayout.BeginScrollView(Vector2.zero);
                GUILayout.Label("Selected Match:");
                GUILayout.TextArea(_selectedMatch.id);
                GUILayout.Label("Players:");
                foreach (Match.Player player in _selectedMatch.players) {
                    GUILayout.TextArea($"{player.id}");
                }

                GUILayout.EndScrollView();
            }
            else if (_selectedMatchID != null) {
                GUILayout.Label("Loading...");
            }
            else {
                GUILayout.Label("Please select a match");
            }

            GUILayout.EndScrollView();

            // report match area
            GUILayout.BeginArea(new Rect(525, 0, 500, 1000));

            GUILayout.BeginHorizontal();
            _newPlayerID = GUILayout.TextField(_newPlayerID);
            if (GUILayout.Button("Add Player")) {
                _players.Add(_newPlayerID);
            }

            GUILayout.EndHorizontal();
            GUILayout.Label("Players");
            foreach (string playerID in _players) {
                GUILayout.BeginHorizontal();
                GUILayout.Label(playerID);
                if (GUILayout.Button("Remove")) {
                    _players.Remove(playerID);
                    break;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Report Dummy Match Result")) {
                MatchStore.Report(Authentication.OperatorApiKey, Match.DummyMatch(_players
                    .Select(id => new Match.Player{id = id, isBot = false, isMember = true, teamId = 0})
                    .ToArray()));
                _players.Clear();
            }

            GUILayout.EndArea();
        }

        private void RenderPlayerInfo() {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player ID");
            _playerID = GUILayout.TextField(_playerID);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Name");
            GUILayout.TextArea(_playerName);
            GUILayout.EndHorizontal();
        }

        private void OnVersionReceived(Version version) {
            _version = version.version;
        }

        private void OnReceivedPlayer(Player player) {
            _playerID = player.id;
            _playerName = player.name;
        }

        private void RenderMatchList() {
            if (_matchOverviews != null)
                foreach (MatchOverview matchOverview in _matchOverviews) {
                    string id = matchOverview.id;
                    GUILayout.BeginHorizontal();
                    GUILayout.TextArea($"{matchOverview.date} | {matchOverview.location} | {matchOverview.teamScores[0]}:{matchOverview.teamScores[1]}");
                    if (GUILayout.Button("Load Details")) {
                        _selectedMatchID = id;
                        MatchStore.GetMatch(Authentication.PlayerApiKey, id);
                    }

                    GUILayout.EndHorizontal();
                }
            else GUILayout.Label("Please load your matches");
        }

        private void OnOverviewReceived(string playerID, MatchOverview[] overview) {
            _matchOverviews = overview;
        }

        private void OnMatchReceived(Match match) {
            if (match.id != _selectedMatchID) return;
            _selectedMatch = match;
        }

        private static void OnStatisticsReceived(PlayerStatistics playerStatistics) {
            Debug.Log($"Received statistics: {JsonConvert.SerializeObject(playerStatistics)}");
        }
    }
}

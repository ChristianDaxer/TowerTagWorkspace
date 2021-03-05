using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TowerTag;
using UnityEngine;

public class PreviousMatchScoreboard : MonoBehaviour {
    [Serializable]
    public struct TextComponents {
        public TextMeshProUGUI ScoreText;
        public TextMeshProUGUI NameText;
        public RectTransform PlayerLineBox;
    }

    [Serializable]
    public struct TeamComponents {
        public TeamID TeamID;
        public TextComponents TextComponents;
    }

    [SerializeField] private List<TeamComponents> _teamComponents;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private PlayerStatsScoreBoard _playerLinePrefab;

    private IMatch _previousMatchStats;

    private void Awake() {
        PreviousMatchResults.FinalMatchStats previousMatchStats = PreviousMatchResults.GetPreviousMatchStats();
        if (Equals(previousMatchStats, default(PreviousMatchResults.FinalMatchStats))) {
            return;
        }

        InitScoreboard(previousMatchStats);
    }

    private void InitScoreboard(PreviousMatchResults.FinalMatchStats matchStats) {
        if (matchStats.TeamStats.Count <= 0)
            return;

        matchStats.TeamStats.ForEach(team => {
            TeamComponents currentTeamComponents =
                _teamComponents.FirstOrDefault(component => component.TeamID == team.Key);
            if (Equals(currentTeamComponents, default(TeamComponents))) {
                return;
            }

            TextComponents currentTextComponents = currentTeamComponents.TextComponents;
            currentTextComponents.ScoreText.text = team.Value.Points.ToString();
            currentTextComponents.NameText.text = team.Value.Name;

            team.Value.PlayerStats.ForEach(keyValuePair => {
                IPlayer player = PlayerManager.Instance.GetPlayer(keyValuePair.Key);
                if (player != null && player.IsParticipating)
                    _playerLinePrefab.CreatePostMatch(player, keyValuePair.Value, currentTextComponents.PlayerLineBox);
            });
        });
        _time.text = "--:--";
    }
}
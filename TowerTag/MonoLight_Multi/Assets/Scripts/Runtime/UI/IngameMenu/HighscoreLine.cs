using JetBrains.Annotations;
using TMPro;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class HighscoreLine : MonoBehaviour {
        [SerializeField] private TMP_Text _rank;
        [SerializeField] private TMP_Text _player;
        [SerializeField] private TMP_Text _rating;
        [SerializeField] private Image _background;

        private RoomLine.BackgroundStyle _backgroundStyle;

        public void Init(string playerName, int rank, int score, RoomLine.BackgroundStyle style) {
            SetPlayerText(playerName);
            SetPlayerRankText(rank);
            SetPlayerRatingText(score);
            SetStyle(style);
        }

        [UsedImplicitly]
        public void SetPlayerText(string player) {
            _player.text = player;
        }

        [UsedImplicitly]
        public void SetPlayerRankText(int rank) {
            _rank.text = rank.ToString();
        }

        [UsedImplicitly]
        public void SetPlayerRatingText(float rating) {
            _rating.text = rating.ToString("F0");
        }

        private void SetStyle(RoomLine.BackgroundStyle style) {
            Color baseColor = TeamManager.Singleton.TeamIce.Colors.UI;
            if (style == RoomLine.BackgroundStyle.Dark) {
                _background.color = baseColor * 0.0f;
                _rank.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _player.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _rating.color = TeamManager.Singleton.TeamIce.Colors.UI;
            }

            if (style == RoomLine.BackgroundStyle.Light) {
                _background.color = baseColor * 0.3f;
                _rank.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _player.color = TeamManager.Singleton.TeamIce.Colors.UI;
                _rating.color = TeamManager.Singleton.TeamIce.Colors.UI;
            }
        }
    }
}
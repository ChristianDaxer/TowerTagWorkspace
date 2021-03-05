using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI
{
    public class OculusLeaderboardPanelController : MonoBehaviour
    {
        [Header("Highscore Tab")]
        [SerializeField]
        private HighscoreLine _highscoreLine;

        [SerializeField] private Transform _highscoreContainer;
        [SerializeField] private Text _headline;
        private readonly List<HighscoreLine> _highScoreLineList = new List<HighscoreLine>();

        [Header("Highscore Tab/Own Score")]
        [SerializeField]
        private TMP_Text _rank;

        [SerializeField] private TMP_Text _player;
        [SerializeField] private TMP_Text _rating;

        private RoomLine.BackgroundStyle _currentStyle = RoomLine.BackgroundStyle.Light;

        private RoomLine.BackgroundStyle StyleForNewLine => _currentStyle == RoomLine.BackgroundStyle.Light
            ? RoomLine.BackgroundStyle.Dark
            : RoomLine.BackgroundStyle.Light;

        protected void Awake() {
            TowerTagSettings.LeaderboardManager.InitLeaderboard();
        }

        private void Start() {
            //TowerTagSettings.LeaderboardManager.GetLeaderboardGlobal();
        }

        private void OnEnable() {
            TowerTagSettings.LeaderboardManager.OnLeaderboardDownloaded += OnLeaderboardDownloaded;
            TowerTagSettings.LeaderboardManager.OnPlayerDataDownloaded += OnPlayerDataDownloaded;

            _headline.text = "OCULUS LEADERBOARD (WIP)";
        }

        public void OnDisable() {
            _highScoreLineList.ForEach(entry => Destroy(entry.gameObject));
            _highScoreLineList.Clear();

            ResetOwnHighscoreLine();
        }

        private void OnLeaderboardDownloaded(LeaderboardEntry[] entries) {
            Debug.Log("Leaderboard downloaded ------------------ " + entries.Length);
            entries.ForEach(entry => CreateHighscoreLine(entry));
        }

        private void OnPlayerDataDownloaded(LeaderboardEntry entry) {
            CreateOwnHighscoreLine(entry);
        }

        private void CreateHighscoreLine(LeaderboardEntry entry) {
            if (_highscoreContainer == null) {
                return;
            }
            HighscoreLine line = Instantiate(_highscoreLine, _highscoreContainer);

            line.Init(entry.Name, entry.Rank, entry.Score, StyleForNewLine);

            _currentStyle = StyleForNewLine;
            _highScoreLineList.Add(line);

            StartCoroutine(PlaySpawnAnimation(line.GetComponent<RectTransform>(),
                -0.4f, 0, 0.1f));
        }

        private void CreateOwnHighscoreLine(LeaderboardEntry entry) {
            _rank.text = entry.Rank.ToString();
            _player.text = entry.Name;
            _rating.text = entry.Score.ToString();
        }

        private void ResetOwnHighscoreLine() {
            _rank.text = string.Empty;
            _player.text = string.Empty;
            _rating.text = string.Empty;
        }

        private IEnumerator PlaySpawnAnimation(RectTransform rectTransform, float startZValue, float endZValue,
            float duration) {
            float time = 0;

            while (time <= duration)
            {
                var position3D = rectTransform.anchoredPosition3D;

                rectTransform.anchoredPosition3D = new Vector3(position3D.x, position3D.y,
                    Mathf.Lerp(startZValue, endZValue, time / duration));
                time += Time.deltaTime;

                yield return null;
            }

            var anchoredPosition3D = rectTransform.anchoredPosition3D;

            anchoredPosition3D = new Vector3(anchoredPosition3D.x, anchoredPosition3D.y, endZValue);
            rectTransform.anchoredPosition3D = anchoredPosition3D;
        }
    }

}

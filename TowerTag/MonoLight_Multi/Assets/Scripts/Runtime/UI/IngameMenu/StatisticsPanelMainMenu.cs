using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using JetBrains.Annotations;
using TMPro;
using TowerTagAPIClient.Model;
using TowerTagAPIClient.Store;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class StatisticsPanelMainMenu : HomeMenuPanel {
        [Header("Rank Tab")] [SerializeField] private Image _badge;
        [SerializeField] private TMP_Text _epValue;
        [SerializeField] private TMP_Text _rankValue;
        [SerializeField] private TMP_Text _globalScore;
        [SerializeField] private TMP_Text _soRatio;
        [SerializeField] private Image _progressBar;
        [SerializeField] private LineRenderer _rankAllocation;
        [SerializeField] private RectTransform[] _badgeAnchorPoints;
        private const float BadgesPerRow = 3;

        [Header("Stats Tab")] [SerializeField] private StatisticLine _statisticLine;
        [SerializeField] private Transform _container;
        private readonly Dictionary<string, StatisticLine> _statisticToLineDictionary = new Dictionary<string, StatisticLine>();

        [SerializeField] private PlayerStatistic[] _stats;

        private RoomLine.BackgroundStyle _currentStyle = RoomLine.BackgroundStyle.Light;

        private RoomLine.BackgroundStyle StyleForNewLine => _currentStyle == RoomLine.BackgroundStyle.Light
            ? RoomLine.BackgroundStyle.Dark
            : RoomLine.BackgroundStyle.Light;

        public override void OnEnable() {
            base.OnEnable();

            PlayerStatisticsStore.PlayerStatisticsReceived += OnPlayerStatisticsReceived;
            // SteamLeaderboardManager.Instance.GetPlayerData(SteamUser.GetSteamID());

            if (PlayerAccount.ReceivedPlayerStatistics) {
                StartCoroutine(SpawnStatisticLines());
            }

            WriteStatsToRankingTab(PlayerAccount.Statistics);
        }

        public override void OnDisable() {
            base.OnDisable();
            PlayerStatisticsStore.PlayerStatisticsReceived -= OnPlayerStatisticsReceived;
            _statisticToLineDictionary.ForEach(entry => Destroy(entry.Value.gameObject));
            _statisticToLineDictionary.Clear();
        }

        private void WriteStatsToRankingTab([CanBeNull] PlayerStatistics statistics) {
            _badge.sprite = BadgeManager.Instance.GetBadgeByLevel(statistics?.level ?? 0);
            _epValue.text = statistics?.xp.ToString(CultureInfo.InvariantCulture) ?? "0";
            _rankValue.text = statistics?.level.ToString() ?? "0";
            _globalScore.text = Mathf.RoundToInt(statistics?.rating ?? 0).ToString();
            _soRatio.text = statistics != null
                ? (statistics.scorePerRound / statistics.outsPerRound).ToString("F2", CultureInfo.InvariantCulture)
                : "0";
            StartCoroutine(
                SpawnAnimationForFillAmount(PlayerAccount.CalculateProgressOfLevel(statistics?.xp ?? 0, out int _)));
            StartCoroutine(SpawnAnimationForLevel(statistics?.level ?? 1, 0.5f));
        }

        private IEnumerator SpawnAnimationForFillAmount(float fillAmount) {
            if (fillAmount <= 0) yield break;
            _progressBar.fillAmount = 0;
            var time = 0.0f;
            while (_progressBar.fillAmount <= fillAmount) {
                _progressBar.fillAmount = Mathf.Lerp(_progressBar.fillAmount, fillAmount, time / fillAmount);
                time += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator SpawnAnimationForLevel(int level, float duration) {
            float time = 0;
            _rankAllocation.startWidth = 0;
            SetPositionOfLineRendererByLevel(level);
            while (_rankAllocation.startWidth <= 1) {
                _rankAllocation.startWidth = Mathf.Lerp(0, 1, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
        }

        private void SetPositionOfLineRendererByLevel(int level) {
            Vector3 lastPos = _rankAllocation.GetPosition(_rankAllocation.positionCount - 1);

            int i = Mathf.CeilToInt((level) / BadgesPerRow) <= _badgeAnchorPoints.Length
                    ? Mathf.CeilToInt((level) / BadgesPerRow) - 1
                    : _badgeAnchorPoints.Length - 1;
            if (i >= 0 && _badgeAnchorPoints.Length > i) {
                float newYValue = _badgeAnchorPoints[
                    i].anchoredPosition.y;

                _rankAllocation.SetPosition(_rankAllocation.positionCount - 1, new Vector3(lastPos.x,
                    newYValue));

                lastPos = _rankAllocation.GetPosition(_rankAllocation.positionCount - 2);
                _rankAllocation.SetPosition(_rankAllocation.positionCount - 2, new Vector3(lastPos.x,
                    newYValue));
            }
        }

        private void OnPlayerStatisticsReceived(PlayerStatistics statistics) {
            PlayerIdManager.GetInstance(out var playerIdManager);
            if (!statistics.id.Equals(playerIdManager.GetUserId())) return;
            WriteStatsToRankingTab(statistics);
            if (_statisticToLineDictionary.Count <= 0)
                StartCoroutine(SpawnStatisticLines());
        }

        private readonly WaitForSeconds _waitForSeconds = new WaitForSeconds(0.01f);

        private IEnumerator SpawnStatisticLines() {
            foreach (PlayerStatistic stat in _stats) {
                CreateStatisticLine(stat);
                yield return _waitForSeconds;
            }
        }

        private void CreateStatisticLine(PlayerStatistic stat) {
            StatisticLine line = InstantiateWrapper.InstantiateWithMessage(_statisticLine, _container);
            line.Init(stat, StyleForNewLine);
            _currentStyle = StyleForNewLine;
            _statisticToLineDictionary.Add(stat.DisplayName, line);
            StartCoroutine(PlaySpawnAnimation(line.GetComponent<RectTransform>(),
                -0.4f, 0, 0.1f));
        }

        private IEnumerator PlaySpawnAnimation(RectTransform rectTransform, float startZValue, float endZValue,
            float duration) {
            float time = 0;
            while (time <= duration) {
                var position3D = rectTransform.anchoredPosition3D;
                rectTransform.anchoredPosition3D = new Vector3(position3D.x,
                    position3D.y,
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
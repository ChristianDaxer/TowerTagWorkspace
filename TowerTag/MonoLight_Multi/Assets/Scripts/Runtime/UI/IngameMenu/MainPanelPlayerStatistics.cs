using System.Globalization;
using TMPro;
using TowerTagAPIClient;
using TowerTagAPIClient.Model;
using TowerTagAPIClient.Store;
using UnityEngine;
using UnityEngine.UI;

namespace Home.UI {
    public class MainPanelPlayerStatistics : MonoBehaviour {
        [SerializeField] private Image _badge;
        [SerializeField] private TMP_Text _rankValueText;
        [SerializeField] private TMP_Text _epValueText;
        [SerializeField] private Image _nextRankProgressBar;
        private void OnEnable() {
            if(PlayerAccount.ReceivedPlayerStatistics)
                WriteStatisticsToUI(PlayerAccount.Statistics);
            else {
                PlayerIdManager.GetInstance(out var playerIdManager);
                PlayerStatisticsStore.GetStatistics(Authentication.OperatorApiKey, playerIdManager.GetUserId(), true);
                PlayerStatisticsStore.PlayerStatisticsReceived += WriteStatisticsToUI;
            }
        }

        private void OnDisable() {
            PlayerStatisticsStore.PlayerStatisticsReceived -= WriteStatisticsToUI;
        }

        private void WriteStatisticsToUI(PlayerStatistics statistics) {
            PlayerIdManager.GetInstance(out var playerIdManager);
            if (!statistics.id.Equals(playerIdManager.GetUserId())) return;
            _badge.sprite = BadgeManager.Instance.GetBadgeByLevel(statistics.level);
            _rankValueText.text = statistics.level.ToString();
            _epValueText.text = statistics.xp.ToString(CultureInfo.CurrentCulture);
            _nextRankProgressBar.fillAmount = PlayerAccount.CalculateProgressOfLevel(statistics.xp, out int _);
        }
    }
}
using TowerTagAPIClient;
using TowerTagAPIClient.Model;
using UnityEngine;
using TowerTagAPIClient.Store;
using Player = TowerTagAPIClient.Model.Player;

namespace Home {
    public static class PlayerAccount {
        public static Player Player;
        public static PlayerStatistics Statistics;
        public static bool ReceivedPlayer;
        public static bool ReceivedPlayerStatistics;

        public static void Init(string playerId) {
            PlayerStore.PlayerReceived += OnPlayerReceived;
            PlayerStatisticsStore.PlayerStatisticsReceived += OnPlayerStatisticsReceived;
            TTSceneManager.Instance.ConnectSceneLoaded += OnConnectSceneLoaded;
            PlayerStore.GetPlayer(Authentication.PlayerApiKey, playerId);
            PlayerStatisticsStore.GetStatistics(Authentication.PlayerApiKey, playerId);
        }

        private static void OnConnectSceneLoaded() {
            PlayerIdManager.GetInstance(out var playerIdManager);
            PlayerStatisticsStore.GetStatistics(Authentication.PlayerApiKey, playerIdManager.GetUserId());
        }

        private static void OnPlayerStatisticsReceived(PlayerStatistics playerStatistics) {
            PlayerIdManager.GetInstance(out var playerIdManager);
            if (!playerStatistics.id.Equals(playerIdManager.GetUserId())) return;
            Statistics = playerStatistics;
            ReceivedPlayerStatistics = true;
        }

        private static void OnPlayerReceived(Player player) {
            Player = player;
            ReceivedPlayer = true;
        }

        /// <summary>
        /// TODO: This logic is in backend! We need to implement the "CurrentLevelProgress" percentage into the backend!
        /// </summary>
        /// <param name="xp"></param>
        /// <param name="levelsSkipped"></param>
        public static float CalculateProgressOfLevel(float xp, out int levelsSkipped)
        {
            levelsSkipped = 0;
            float lastLevel = 0;
            for(var level = 1; level <= 999; level++){
                var levelThreshold = level < 10
                    ? level * (level - 1) * 25
                    : 2250 + 500 * (level - 10);
                float nextLevel = levelThreshold;
                if (xp >= levelThreshold) {
                    levelsSkipped++;
                    lastLevel = levelThreshold;
                }  else {
                    return Mathf.InverseLerp(lastLevel, nextLevel, xp);
                }
            }

            return 0.0f;
        }
    }
}
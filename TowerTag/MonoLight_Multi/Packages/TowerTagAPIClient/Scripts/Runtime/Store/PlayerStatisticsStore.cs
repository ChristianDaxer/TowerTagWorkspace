using System.Collections.Generic;
using Newtonsoft.Json;
using TowerTagAPIClient.Model;
using UnityEngine;

namespace TowerTagAPIClient.Store {
    public static class PlayerStatisticsStore {
        public delegate void ReceivePlayerStatisticsDelegate(PlayerStatistics playerStatistics);

        public static event ReceivePlayerStatisticsDelegate PlayerStatisticsReceived;

        private static readonly Dictionary<string, PlayerStatistics> _playerStatistics
            = new Dictionary<string, PlayerStatistics>();

        public static Dictionary<string, PlayerStatistics> PlayerStatistics => _playerStatistics;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private const string BaseURI = "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/dev/player/statistics";
#else
        private const string BaseURI = "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/v1/player/statistics";
#endif

        public static void GetStatistics(string apiKey, string playerId, bool forceReload = false)
        {
            if (playerId == null)
            {
                Debug.LogWarning("Trying to get statistic of playerID null! Aborting");
                return;
            };
            if (forceReload || !_playerStatistics.ContainsKey(playerId)) {
                Client.Get($"{BaseURI}/{playerId}", new Dictionary<string, string> {{"x-api-key", apiKey}},
                    OnGetStatistics, OnError);
            }

            if (_playerStatistics.ContainsKey(playerId) && _playerStatistics[playerId] != null)
                PlayerStatisticsReceived?.Invoke(_playerStatistics[playerId]);
        }

        public static void ClearStatisticsCache() {
            _playerStatistics.Clear();
        }

        private static void OnGetStatistics(long statusCode, string response) {
            var jsonSerializerSettings =
                new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Ignore};
            var playerStatistics = JsonConvert.DeserializeObject<PlayerStatistics>(response, jsonSerializerSettings);
            _playerStatistics[playerStatistics.id] = playerStatistics;
            PlayerStatisticsReceived?.Invoke(playerStatistics);
        }

        private static void OnError(long statusCode, string response) {
            Debug.LogWarning($"Failed to get player: {statusCode} | {response}");
        }
    }
}
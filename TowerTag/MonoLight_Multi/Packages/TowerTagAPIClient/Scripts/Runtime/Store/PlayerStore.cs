using System.Collections.Generic;
using Newtonsoft.Json;
using TowerTagAPIClient.Model;
using UnityEngine;

namespace TowerTagAPIClient.Store {
    public static class PlayerStore {
        public delegate void ReceivePlayerDelegate(Player player);

        public static event ReceivePlayerDelegate PlayerReceived;

        private static readonly Dictionary<string, Player> _players = new Dictionary<string, Player>();
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private const string BaseURI = "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/dev/player";
#else
        private const string BaseURI = "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/v1/player";
#endif
        private static string _myPlayerID;

        public static void GetMyPlayer(string apiKey, string jwt, bool forceReload = false) {
            if (forceReload || _myPlayerID == null || !_players.ContainsKey(_myPlayerID)) {
                Client.Get($"{BaseURI}", new Dictionary<string, string> {{"x-api-key", apiKey}, {"Authorization", jwt}},
                    OnGetMyPlayer, OnError);
            }

            if (_myPlayerID != null && _players.ContainsKey(_myPlayerID)) PlayerReceived?.Invoke(_players[_myPlayerID]);
        }

        public static void GetPlayer(string apiKey, string playerID, bool forceReload = false) {
            if (forceReload || !_players.ContainsKey(playerID)) {
                Client.Get($"{BaseURI}/{playerID}", new Dictionary<string, string> {{"x-api-key", apiKey}}, OnGetPlayer,
                    OnError);
            }

            if (_players.ContainsKey(playerID)) PlayerReceived?.Invoke(_players[playerID]);
        }

        private static void OnGetPlayer(long statusCode, string response) {
            var player = JsonConvert.DeserializeObject<Player>(response);
            _players[player.id] = player;
            PlayerReceived?.Invoke(player);
        }

        private static void OnGetMyPlayer(long statusCode, string response) {
            var player = JsonConvert.DeserializeObject<Player>(response);
            _players[player.id] = player;
            PlayerReceived?.Invoke(player);
            _myPlayerID = player.id;
        }

        private static void OnError(long statusCode, string response) {
            Debug.LogWarning($"Failed to get player: {statusCode} | {response}");
        }
    }
}
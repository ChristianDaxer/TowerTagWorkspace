using System.Collections.Generic;
using Newtonsoft.Json;
using TowerTagAPIClient.Model;
using UnityEngine;

namespace TowerTagAPIClient.Store {
    public static class MatchStore {
        // events
        public delegate void ReceiveMatchDelegate(Match match);

        public delegate void ReceiveOverviewDelegate(string playerID, MatchOverview[] overview);

        public delegate void ReportMatchDelegate(string matchID);

        public static event ReceiveMatchDelegate MatchReceived;
        public static event ReceiveOverviewDelegate OverviewReceived;
        public static event ReportMatchDelegate MatchReported;

        // const
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private const string MatchURI = "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/dev/match";

        private const string MatchOverviewURI =
            "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/dev/player/matchoverview";
#else
        private const string MatchURI = "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/v1/match";

        private const string MatchOverviewURI =
            "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/v1/player/matchoverview";
#endif

        private const string APIVersion = "0.1.15";

        // cache
        private static readonly Dictionary<string, Match> _matches = new Dictionary<string, Match>();

        private static readonly Dictionary<string, MatchOverview[]> _matchOverviews =
            new Dictionary<string, MatchOverview[]>();

        public static void GetMatch(string apiKey, string matchID, bool forceReload = false) {
            if (forceReload || !_matches.ContainsKey(matchID)) {
                Client.Get($"{MatchURI}/{matchID}",
                    new Dictionary<string, string> {{"x-api-key", apiKey}}, OnGetMatch, OnError);
            }

            if (_matches.ContainsKey(matchID)) MatchReceived?.Invoke(_matches[matchID]);
        }

        private static void OnGetMatch(long statusCode, string response) {
            var match = JsonConvert.DeserializeObject<Match>(response);
            _matches[match.id] = match;
            MatchReceived?.Invoke(match);
        }

        public static void Report(string apiKey, Match match) {
            match.apiVersion = APIVersion;
            var jsonSerializerSettings =
                new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Include};
            string json = JsonConvert.SerializeObject(match, jsonSerializerSettings);
            Debug.Log(json);
            Client.Post(MatchURI, new Dictionary<string, string> {{"x-api-key", apiKey}}, json, OnReportedMatch,
                OnError, 3);
        }

        private static void OnReportedMatch(long statusCode, string response) {
            var matchIdWrapper = JsonConvert.DeserializeObject<MatchIdWrapper>(response);
            MatchReported?.Invoke(matchIdWrapper.matchId);
            Debug.Log($"Reported match with ID {matchIdWrapper.matchId}");
        }

        public static void GetOverviewByID(string apiKey, string playerID, bool forceReload = false) {
            if (forceReload || !_matchOverviews.ContainsKey(playerID)) {
                Client.Get($"{MatchOverviewURI}/{playerID}",
                    new Dictionary<string, string> {{"x-api-key", apiKey}},
                    (status, response) => OnGetOverview(status, response, playerID), OnError);
            }

            if (_matchOverviews.ContainsKey(playerID)) OverviewReceived?.Invoke(playerID, _matchOverviews[playerID]);
        }

        private static void OnGetOverview(long statusCode, string response, string playerID) {
            if (statusCode / 100 != 2) {
                Debug.LogWarning($"Failed to handle match overviews. Status code {statusCode}");
                return;
            }

            var overviews = JsonConvert.DeserializeObject<MatchOverview[]>(response);
            _matchOverviews[playerID] = overviews;
            OverviewReceived?.Invoke(playerID, overviews);
        }

        private static void OnError(long statusCode, string response) {
            Debug.LogWarning($"Match store failure: {statusCode} | {response}");
        }
    }
}
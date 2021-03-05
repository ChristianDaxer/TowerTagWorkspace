using System.Collections.Generic;
using Newtonsoft.Json;
using TowerTagAPIClient.Model;
using UnityEngine;

namespace TowerTagAPIClient.Store {
    public static class VersionStore {
        public delegate void ReceiveVersionDelegate(Version version);

        public static event ReceiveVersionDelegate VersionReceived;
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private const string BaseURI = "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/dev/version";
#else
        private const string BaseURI = "https://4b1vlaiaj0.execute-api.us-east-2.amazonaws.com/v1/version";
#endif
        private static Version _latestVersion;

        public static void GetLatestVersion(string apiKey, bool china = false, bool forceReload = false) {
            if (forceReload || _latestVersion == null) {
                Client.Get($"{BaseURI}?china={china}", new Dictionary<string, string> {{"x-api-key", apiKey}},
                    OnGetVersion, OnError);
            }

            if (_latestVersion != null) VersionReceived?.Invoke(_latestVersion);
        }

        private static void OnGetVersion(long statusCode, string response) {
            _latestVersion = JsonConvert.DeserializeObject<Version>(response);
            VersionReceived?.Invoke(_latestVersion);
        }

        private static void OnError(long statusCode, string response) {
            Debug.LogWarning($"Failed to get version: {statusCode} | {response}");
        }
    }
}
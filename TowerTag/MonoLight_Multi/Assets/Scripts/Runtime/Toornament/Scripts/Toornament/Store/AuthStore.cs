using Newtonsoft.Json;
using REST;
using System;
using System.Collections.Generic;
using VRNerdsUtilities;

namespace Toornament.Store {
    public class AuthStore : SingletonMonoBehaviour<AuthStore> {
        private const string ServerUrl = "https://api.toornament.com";

        // cached data
        private string _accessToken;

        public Dictionary<string, string> NoAuthHeaders {
            get {
                return new Dictionary<string, string> {
                    {"X-Api-Key", ToornamentProfileHolder.Instance.ApiKey}
                };
            }
        }

        public Dictionary<string, string> AuthHeaders {
            get {
                return new Dictionary<string, string> {
                    {"X-Api-Key", ToornamentProfileHolder.Instance.ApiKey},
                    {"Authorization", "Bearer " + _accessToken}
                };
            }
        }

        private void Start() {
            transform.parent = ToornamentContainer.Instance.transform;
            Authenticate();
        }

        private void Authenticate() {
            if (ToornamentProfileHolder.Instance.ClientId != string.Empty
                && ToornamentProfileHolder.Instance.ClientSecret != string.Empty) {
                RefreshAccessToken();
            } else {
                Debug.LogWarning("No OAuth2 authentication!");
            }
        }

        private void RefreshAccessToken() {
            const string route = "/oauth/v2/token";

            var uri = new Uri(new Uri(ServerUrl), route);

            var data = new Dictionary<string, string> {
                {"grant_type", "client_credentials"},
                {"client_id", ToornamentProfileHolder.Instance.ClientId},
                {"client_secret", ToornamentProfileHolder.Instance.ClientSecret}
            };

            Client.Post(uri.ToString(), data, OnOAuthV2Success, OnOAuthV2Error);
        }

        private void OnOAuthV2Success(long responseCode, string text) {
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            _accessToken = response["access_token"];
            if (response.ContainsKey("expires_in") && float.TryParse(response["expires_in"], out float expirationTime)) {
                Invoke(nameof(Authenticate), expirationTime);
                Debug.Log("Scheduled token refresh in " + expirationTime + " seconds");
            }

            Debug.Log("Successfully obtained access token");
        }

        private static void OnOAuthV2Error(long responseCode, string text) {
            Debug.LogWarning("HTTP Response Code " + responseCode
                                                   + "\n" + "Couldn't get a OAuthV2 Token!"
                                                   + "\n" + text);
        }
    }
}
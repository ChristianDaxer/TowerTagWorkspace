using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SOEventSystem.Shared;
using TowerTag;
using TowerTagAPIClient;
using TowerTagAPIClient.Store;
using UI;
using UnityEngine;
using UnityEngine.Networking;

namespace PopUpMenu {
    [CreateAssetMenu(menuName = "UI Elements/PopUpMenu Option/Scan Member ID")]
    public class ScanMember : PlayerOption {
        [SerializeField] private int _timeOutSeconds = 5;
        [SerializeField] private MessageQueue _messageQueue;
        [SerializeField] private bool _useDartsLiveBackend;
        [SerializeField] private bool _useVRNerdsBackend;
        [SerializeField] private SharedBool _scanningQRCode;
        private const string DartsLiveApiKey = "Z6J13wHlPA2rrgLDbh5vd91qWvd6bHKf2hImIfxu";

        private void OnEnable() {
            PlayerStore.PlayerReceived += OnPlayerStoreOnPlayerReceived;
        }

        private void OnDisable() {
            PlayerStore.PlayerReceived -= OnPlayerStoreOnPlayerReceived;
        }

        public override void UpdateButtonText(IPlayer player) {
            ButtonText = player.IsLoggedIn ? "Log out" : "Scan";
        }

        public override void OptionOnClick(IPlayer player) {
            if (TowerTagSettings.Home) {
                return;
            }

            if (!player.IsLoggedIn) {
                Coroutine coroutine = StaticCoroutine.StartStaticCoroutine(Scan(player));
                _scanningQRCode.Set(this, true);
                _messageQueue.AddVolatileButtonMessage("Please scan your membership QR Code now...",
                    "Scanning",
                    null,
                    () => {
                        _scanningQRCode.Set(this, false);
                        StaticCoroutine.StopStaticCoroutine(coroutine);
                    },
                    "ABORT");
            }
            else {
                _messageQueue.AddYesNoMessage(
                    $"Do you really want to log out the Member {player.PlayerName}?",
                    "Logout Member",
                    () => { },
                    () => { },
                    "YES",
                    player.LogOut);
            }
        }

        private void OnPlayerStoreOnPlayerReceived(TowerTagAPIClient.Model.Player data) {
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            IPlayer player = players.Take(count).FirstOrDefault(p => p.MembershipID == data.id);

            if (player == null) {
                Debug.LogError($"Received Player data {data}, but could not find the player with id {data.id}");
                return;
            }

            UpdateMemberName(data.id, data.name);
        }

        private IEnumerator Scan(IPlayer player) {
            float startScanTime = Time.time;
            var stringBuilder = new StringBuilder();

            while (Time.time - startScanTime < _timeOutSeconds) {
                yield return null;
                string inputString = Input.inputString.Replace("\r", "");
                stringBuilder.Append(inputString);

                if (Input.GetKeyDown(KeyCode.Return)) {
                    string playerMembershipID = stringBuilder.ToString();
                    PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
                    players
                        .Take(count)
                        .Where(p => p.MembershipID == playerMembershipID)
                        .ForEach(p => p.LogOut()); // prevent duplicate login
                    player.LogIn(playerMembershipID);
                    Debug.Log($"Scanned Member ID {playerMembershipID} for {player.PlayerName}");
                    _messageQueue.AddVolatileMessage("Loading Player Details...", "Success", null, null, null, 3);
                    GetUserData(playerMembershipID);

                    yield break;
                }
            }

            _messageQueue.AddVolatileMessage("Please try again!", "Failure");
            Debug.LogWarning("Failed to scan member ID");
        }

        private void GetUserData(string playerMembershipID) {
            if (_useDartsLiveBackend) {
                var headers = new Dictionary<string, string> {
                    // User-Agent
                    {"user-agent", "towertag"},

                    // For Json
                    {"accept", "application/json; charset=UTF-8"},
                    {"content-type", "application/json; charset=UTF-8"},
                    {"X-HTTP-Method-Override", "GET"},
                    {"x-api-key", DartsLiveApiKey}
                };
                UnityWebRequest request =
                    UnityWebRequest.Get($"https://api.tower-tag.net/v1/UserProfile/{playerMembershipID}");
                headers.ForEach(header => request.SetRequestHeader(header.Key, header.Value));
                UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
                unityWebRequestAsyncOperation.completed += operation => OnReceivedDartsLiveUserData(request);
            }

            if (_useVRNerdsBackend) PlayerStore.GetPlayer(Authentication.OperatorApiKey, playerMembershipID, true);
        }

        private void OnReceivedDartsLiveUserData(UnityWebRequest request) {
            if (!request.isHttpError && !request.isNetworkError) {
                string body = request.downloadHandler.text;
                var userDataResponse = JsonConvert.DeserializeObject<UserDataResponse>(body);
                if (userDataResponse == null) {
                    Debug.LogError($"Failed to parse server response {body}");
                    return;
                }

                string[] parts = userDataResponse.UserId.Split(':');
                if (parts.Length != 2) {
                    Debug.LogError($"Illegal user id format {userDataResponse.UserId}. Expected <regionCode>:<id>");
                    return;
                }

                string membershipID = parts[1];
                string playerName = userDataResponse.Name;
                UpdateMemberName(membershipID, playerName);
            }
            else Debug.LogWarning($"Failed to get user data: {request.responseCode}");
        }

        private void UpdateMemberName(string membershipID, string playerName) {
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            IPlayer player = players.Take(count).FirstOrDefault(p => p.MembershipID == membershipID);
            if (player == null) {
                Debug.LogError($"Could not find player with membership id {membershipID}");
                return;
            }

            _messageQueue.AddVolatileMessage($"Welcome back, {playerName}!", "Success", null, null, null,
                1);
            player.SetName(playerName);
        }

        public class UserDataResponse {
            public string UserId = ""; // contains region code. format : <regionCode>:<id>

            public string Name = "";
//            public DateTime CreatedTimestamp = default;
        }
    }
}
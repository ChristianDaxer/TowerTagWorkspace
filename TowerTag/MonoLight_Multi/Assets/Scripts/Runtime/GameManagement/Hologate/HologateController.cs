using System;
using System.Collections;
using System.Linq;
using Photon.Realtime;
using TowerTag;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Hologate.HologateManager;
using static Hologate.HologateManager.GameSessionConfig;
using PHashtable = ExitGames.Client.Photon.Hashtable;
using IPlayer = TowerTag.IPlayer;

namespace Hologate {
    public class HologateController : MonoBehaviour {
        private const string HologateSystemPlayerCount = "PC";
        private MatchManager _matchManager;
        private MatchDescriptionCollection _matchDescriptionCollection;
        private bool _initialized;
        private Coroutine _lightShow;

        /// <summary>
        /// The content of the GameStatusMcp file for Hologate state changes
        /// </summary>
        private string GameStatusMcp { get; set; }

        public static GameSessionConfig GameSession { get; private set; }

        private const float ReadRate = 0.5f;

        private void Awake() {
            //wanted to do this by state change, but no access to a state change event
            SceneManager.sceneLoaded += OnSceneWasLoaded;
            PlayerManager.Instance.PlayerAdded += OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
            DevicePlayerAdded += OnDevicePlayerAdded;
            GameManager.Instance.MatchHasFinishedLoading += OnMatchFinishedLoading;
            GameSession = GetGameSessionConfig();
            LedBarConfigurationManager.Init();
            StartCoroutine(ReadFiles());
        }

        private void OnJoinedRoom(ConnectionManager connectionManager, IRoom room) {
            var newRoomProperties = new PHashtable {
                [HologateSystemPlayerCount] = int.Parse(GameSession.PlayerCount)
            };
            room.SetCustomProperties(newRoomProperties);
        }

        private void Start() {
            //Todo: Does not do anything atm, need to find a good place for this (admin overtake)
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            ConnectionManager.Instance.JoinedRoom += OnJoinedRoom;
            for (int i = 0; i < count; i++)
                OnPlayerAdded(players[i]);
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneWasLoaded;
            PlayerManager.Instance.PlayerAdded -= OnPlayerAdded;
            PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
            if (ConnectionManager.Instance != null)
                ConnectionManager.Instance.JoinedRoom -= OnJoinedRoom;
            DevicePlayerAdded -= OnDevicePlayerAdded;
        }

        private void OnPlayerAdded(IPlayer player) {
            player.PlayerNetworkEventHandler.RequestDeviceID();
            player.PlayerHealth.PlayerDied += OnPlayerDied;
            player.PlayerHealth.PlayerRevived += OnPlayerRevived;
        }

        private void OnMatchFinishedLoading(IMatch match) {
            match.RoundStartingAt += OnRoundStartAt;
            match.RoundFinished += OnRoundFinished;
            match.Finished += OnMatchFinished;
        }

        private void OnMatchFinished(IMatch match) {
            OnRoundFinished(match, match.Stats.WinningTeamID);
        }

        private void OnRoundFinished(IMatch match, TeamID roundWinningTeamId) {
            if (_lightShow != null)
                StopCoroutine(_lightShow);
            _lightShow = StartCoroutine(LedBarConfigurationManager.LightShow(roundWinningTeamId));
        }

        private void OnRoundStartAt(IMatch match, int time) {
            if (_lightShow != null)
                StopCoroutine(_lightShow);
            LedBarConfigurationManager.ResetLights();
        }

        private void OnPlayerRemoved(IPlayer player) {
            if (DeviceIDToPlayer.ContainsValue(player)) {
                RemovePlayerFromDeviceList(player);
            }
            player.PlayerHealth.PlayerDied -= OnPlayerDied;
            player.PlayerHealth.PlayerRevived -= OnPlayerRevived;

        }

        private void OnPlayerDied(PlayerHealth playerHealth, IPlayer dmgApplyingPlayer, byte colliderType) {
            int deviceID = GetDeviceByPlayer(playerHealth.Player);
            Color color = playerHealth.Player.TeamID == TeamID.Fire
                ? TeamManager.Singleton.TeamFire.Colors.Dark
                : TeamManager.Singleton.TeamIce.Colors.Dark;
            LedBarConfigurationManager.ChangePlaySpaceColor(deviceID, color, color);
        }

        private void OnPlayerRevived(IPlayer player) {
            int deviceID = GetDeviceByPlayer(player);
            Color color = player.TeamID == TeamID.Fire
                ? TeamManager.Singleton.TeamFire.Colors.Main
                : TeamManager.Singleton.TeamIce.Colors.Main;
            LedBarConfigurationManager.ChangePlaySpaceColor(deviceID, color, color);
        }

        private void OnDevicePlayerAdded(int deviceID, IPlayer player) {
            AdminController.SetPlayerName(player, GetNameByDeviceID(deviceID));
            LedBarConfigurationManager.TogglePlaySpace(deviceID, player.IsParticipating, player.TeamID);
        }

        private string GetNameByDeviceID(int deviceID) {
            Device currentDevice
                = GameSession.Devices.FirstOrDefault(device => device.DeviceID == deviceID);
            return currentDevice != default(Device) ? currentDevice.UserInfo.UserName : "Hologate User";
        }

        /// <summary>
        /// Set up variables and start values of the game session file
        /// </summary>
        private void InitializeGameSession() {
            _matchDescriptionCollection = MatchDescriptionCollection.Singleton;

            InitGameSessionValues(GameSession);
        }

        /// <summary>
        /// Set the values of the game session file
        /// </summary>
        /// <param name="gameSession"></param>
        private void InitGameSessionValues(GameSessionConfig gameSession) {
            _matchManager.SetCurrentMatchDescription(
                _matchDescriptionCollection.GetMatchDescription(int.Parse(gameSession.LevelName)));
        }

        /// <summary>
        /// Reading the GameStatusMcp file and the GameSession file
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReadFiles() {
            while (true) {
                yield return new WaitForSeconds(ReadRate);
                try {
                    string gameStatus = ReadGameStatusMcpFile();
                    if (!gameStatus.Equals(GameStatusMcp)) {
                        GameStatusMcp = gameStatus;
                        OnMcpStateChanged(GameStatusMcp);
                    }
                } catch (Exception e) {
                    Debug.LogWarning("Could not read Mcp Status\n" + e);
                }
                //Todo: Sometimes I got an IO Exception -> File Violation | check out why
                try {
                    if (GameManager.Instance.IsInConfigureState) {
                        GameSessionConfig content = GetGameSessionConfig();
                        if (GameSession == null) {
                            GameSession = content;
                            continue;
                        }

                        if (!content.Equals(GameSession)) {
                            SetGameSessionChanges(GameSession, content);
                            GameSession = content;
                        }
                    }

                } catch (Exception e) {
                    Debug.LogWarning("Could not read GameSessionConfig!\n" + e);
                }
            }
        }

        /// <summary>
        /// Detect game session changes and apply them
        /// </summary>
        /// <param name="oldSession">Current GameSession</param>
        /// <param name="newSession">Current content of the GameSession file</param>
        private void SetGameSessionChanges(GameSessionConfig oldSession, GameSessionConfig newSession) {
            if (!newSession.Devices.Equals(oldSession.Devices)) {
                newSession.Devices.ForEach(device => {
                    Device fittingDevice
                        = oldSession.Devices.FirstOrDefault(oldDevice => device.DeviceID == oldDevice.DeviceID);
                    if (fittingDevice != default) {
                        CompareAndChangeDevices(fittingDevice, device);
                    }
                    //Player added on NUC
                    else {
                        IPlayer player = DeviceIDToPlayer[device.DeviceID];
                        player.IsParticipating = true;
                        AdminController.SetPlayerName(player, device.UserInfo.UserName);
                        LedBarConfigurationManager.TogglePlaySpace(device.DeviceID, player.IsParticipating, player.TeamID);
                    }
                });
                //Player removed on NUC
                oldSession.Devices.ForEach(device => {
                    Device fittingDevice
                        = newSession.Devices.FirstOrDefault(newDevice => device.DeviceID == newDevice.DeviceID);
                    if (fittingDevice == default) {
                        IPlayer player = DeviceIDToPlayer[device.DeviceID];
                        player.IsParticipating = false;
                        LedBarConfigurationManager.TogglePlaySpace(device.DeviceID, player.IsParticipating, player.TeamID);
                    }
                });
            }

            if (oldSession.Length != newSession.Length) {
                _matchManager.SnapMatchTimeSlider(int.Parse(newSession.Length));

            }

            if (oldSession.LevelName != newSession.LevelName) {
                _matchManager.SetCurrentMatchDescription(
                    _matchDescriptionCollection.GetMatchDescription(int.Parse(newSession.LevelName)));
            }
        }

        /// <summary>
        /// Compares the a device block and applies changes
        /// </summary>
        /// <param name="oldDevice">Old device content</param>
        /// <param name="newDevice">New device content</param>
        private void CompareAndChangeDevices(Device oldDevice, Device newDevice) {
            if (!oldDevice.UserInfo.UserName.Equals(newDevice.UserInfo.UserName)
                                && DeviceIDToPlayer.ContainsKey(newDevice.DeviceID))
                AdminController.SetPlayerName(DeviceIDToPlayer[newDevice.DeviceID], newDevice.UserInfo.UserName);
        }

        private void OnSceneWasLoaded(Scene arg0, LoadSceneMode loadSceneMode) {
            switch (GameManager.Instance.CurrentState) {
                case GameManager.GameManagerStateMachine.State.Configure:
                    if (!_initialized) {
                        _matchManager = FindObjectOfType<MatchManager>();
                        InitializeGameSession();
                    }
                    WriteInGameStatusGameFile("Lobby");
                    _matchManager.SnapMatchTimeSlider(int.Parse(GameSession.Length));
                    break;
                case GameManager.GameManagerStateMachine.State.LoadMatch:
                    WriteInGameStatusGameFile("Game");
                    break;
                case GameManager.GameManagerStateMachine.State.Commendations:
                    if (GameManager.Instance.CurrentMatch != null)
                        WriteInGameSessionResultsFile(GameManager.Instance.CurrentMatch);
                    WriteInGameStatusGameFile("LeaderBoard");
                    if (_lightShow != null)
                        StopCoroutine(_lightShow);
                    _lightShow = null;
                    LedBarConfigurationManager.ResetLights();
                    break;
            }
        }

        /// <summary>
        /// Hologate related function. Gets called when the GameStateMcp file has changed and reacts to it
        /// </summary>
        /// <param name="newContent"></param>
        private void OnMcpStateChanged(string newContent) {
            switch (newContent) {
                case "Lobby": {
                    if (GameManager.Instance.CurrentState != GameManager.GameManagerStateMachine.State.Undefined)
                        GameManager.Instance.TriggerMatchConfigurationOnMaster();
                    ClearGameStatusMcpFile();
                    break;
                }
                case "Game":
                    _matchManager.SetCurrentMatchDescription(
                        _matchDescriptionCollection.GetMatchDescription(int.Parse(GameSession.LevelName)));
                    AdminController.Instance.OnStartMatchButtonPressed();
                    ClearGameStatusMcpFile();
                    break;
            }
        }
    }
}
using System;
using System.Linq;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using IPlayer = TowerTag.IPlayer;

namespace Network {
    /// <summary>
    /// This class' sole purpose is to hold the default <see cref="RoomOptions"/> configuration.
    /// </summary>
    public class RoomConfiguration : MonoBehaviour {
        public enum RoomState {
            Undefined,
            Lobby,
            Match,
            Loading
        }

        private static RoomOptions _roomOptions;

        public static RoomOptions RoomOptions => _roomOptions ??
                                                 (_roomOptions = new RoomOptions {
                                                     MaxPlayers = (byte) TowerTagSettings.MaxUsersPerRoom,
                                                     PublishUserId = true,
                                                     PlayerTtl = 0,
                                                     EmptyRoomTtl = 0,
                                                     CleanupCacheOnLeave = true
                                                 });

        #region HomeRoomSettings

        public static RoomOptions GetTrainingOptions() {
            RoomOptions options = RoomOptions;
            options.IsVisible = false;
            options.IsOpen = false;
            options.MaxPlayers = 1;
            options.CustomRoomProperties = new Hashtable {
                [RoomPropertyKeys.HostName] = PlayerProfileManager.CurrentPlayerProfile.PlayerName,
                [RoomPropertyKeys.HostPing] = 0,
                [RoomPropertyKeys.AllowTeamChange] = false,
                [RoomPropertyKeys.UserVote] = true,
                [RoomPropertyKeys.HomeMatchType] = GameManager.HomeMatchType.TrainingVsAI,
                [RoomPropertyKeys.AutoskillBots] = false,
            };
            return options;
        }

        public static RoomOptions GetCustomRoomOptions(byte maxPlayers, GameMode gameMode, [CanBeNull] string mapName, float matchTimeInMinutes = 5, bool autostart = true, string pin = "", bool fillWithBots = true, bool autoskillBots = true) {
            RoomOptions options = RoomOptions;
            options.IsVisible = true;
            options.IsOpen = true;
            options.MaxPlayers = (byte) (maxPlayers + TowerTagSettings.MaxSpectatorCount);
            options.CustomRoomProperties = new Hashtable {
                [RoomPropertyKeys.HostName] = PlayerProfileManager.CurrentPlayerProfile.PlayerName,
                [RoomPropertyKeys.MaxPlayers] = maxPlayers,
                [RoomPropertyKeys.MinRank] = (byte) 0,
                [RoomPropertyKeys.MaxRank] = (byte) 0,
                [RoomPropertyKeys.HostPing] = 0,
                [RoomPropertyKeys.GameMode] = gameMode,
                [RoomPropertyKeys.AllowTeamChange] = true,
                [RoomPropertyKeys.UserVote] = true,
                [RoomPropertyKeys.MatchDurationInMinutes] = (byte) matchTimeInMinutes,
                [RoomPropertyKeys.RoomState] = RoomState.Lobby,
                [RoomPropertyKeys.HomeMatchType] = GameManager.HomeMatchType.Custom,
                [RoomPropertyKeys.CurrentPlayers] = (byte) 1,
                [RoomPropertyKeys.Autostart] = autostart,
                [RoomPropertyKeys.AutoskillBots] = autoskillBots,
                //[RoomPropertyKeys.BotFill] = fillWithBots,
                [RoomPropertyKeys.PIN] = string.IsNullOrEmpty(pin) ? "" : StringEncoder.EncodeString(pin)
            };

            options.CustomRoomPropertiesForLobby = new[] {
                RoomPropertyKeys.HostName,
                RoomPropertyKeys.MaxPlayers,
                RoomPropertyKeys.HostPing,
                RoomPropertyKeys.GameMode,
                RoomPropertyKeys.MinRank,
                RoomPropertyKeys.MaxRank,
                RoomPropertyKeys.RoomState,
                RoomPropertyKeys.CurrentPlayers,
                RoomPropertyKeys.PIN
            };

            if (!string.IsNullOrEmpty(mapName)) {
                options.CustomRoomProperties.Add(RoomPropertyKeys.Map, mapName);
                options.CustomRoomPropertiesForLobby
                    = options.CustomRoomPropertiesForLobby.Concat(new[] {RoomPropertyKeys.Map}).ToArray();
            }

            return options;
        }

        public static RoomOptions GetRandomRoomOptions() {
            RoomOptions options = RoomOptions;
            options.IsVisible = true;
            options.IsOpen = true;
            options.MaxPlayers = (byte) (TowerTagSettings.MaxPlayers + TowerTagSettings.MaxSpectatorCount);
            options.CustomRoomProperties = new Hashtable {
                [RoomPropertyKeys.HostName] = PlayerProfileManager.CurrentPlayerProfile.PlayerName,
                [RoomPropertyKeys.MaxPlayers] = (byte) TowerTagSettings.MaxPlayers,
                [RoomPropertyKeys.HostPing] = 0,
                [RoomPropertyKeys.AllowTeamChange] = true,
                [RoomPropertyKeys.UserVote] = true,
                [RoomPropertyKeys.RoomState] = RoomState.Lobby,
                [RoomPropertyKeys.MinRank] = (byte) 0,
                [RoomPropertyKeys.MaxRank] = (byte) 0,
                [RoomPropertyKeys.Autostart] = true,
                [RoomPropertyKeys.AutoskillBots] = true,
                [RoomPropertyKeys.MatchDurationInMinutes] = PlayerManager.Instance.GetAllParticipatingHumanPlayerCount() > 1 ? (byte) 5 : (byte) 3,
                //[RoomPropertyKeys.BotFill] = true,
                [RoomPropertyKeys.HomeMatchType] = GameManager.HomeMatchType.Random,
                [RoomPropertyKeys.CurrentPlayers] = (byte) 1,
                [RoomPropertyKeys.PIN] = "",
            };

            options.CustomRoomPropertiesForLobby = new[] {
                RoomPropertyKeys.HostName,
                RoomPropertyKeys.HostPing,
                RoomPropertyKeys.MaxPlayers,
                RoomPropertyKeys.GameMode,
                RoomPropertyKeys.Map,
                RoomPropertyKeys.MinRank,
                RoomPropertyKeys.MaxRank,
                RoomPropertyKeys.RoomState,
                RoomPropertyKeys.CurrentPlayers,
                RoomPropertyKeys.PIN,
            };
            return options;
        }

        public static RoomOptions GetTutorialRoomSettings() {
            RoomOptions options = RoomOptions;
            options.IsVisible = false;
            options.IsOpen = false;
            options.MaxPlayers = 1;
            return options;
        }

        public static RoomOptions GetEmptyRoomSettings() {
            RoomOptions options = RoomOptions;
            options.IsVisible = false;
            options.IsOpen = false;
            options.MaxPlayers = 1;
            return options;
        }

        public static int GetMaxPlayersForCurrentRoom() {
            Room currentRoom = PhotonNetwork.CurrentRoom;
            return currentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.MaxPlayers)
                ? (byte) currentRoom.CustomProperties[RoomPropertyKeys.MaxPlayers]
                : currentRoom.MaxPlayers;
        }

        public static int GetCurrentPlayersForRoom(Room room) {
            return room.CustomProperties.ContainsKey(RoomPropertyKeys.CurrentPlayers)
                ? (byte) room.CustomProperties[RoomPropertyKeys.CurrentPlayers]
                : room.PlayerCount;
        }

        #endregion
    }
}
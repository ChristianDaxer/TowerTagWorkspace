using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using TowerTagSOES;
using UnityEngine;
using IPlayer = TowerTag.IPlayer;

namespace Hologate {
    public static class HologateManager {
        public static Action<int, IPlayer> DevicePlayerAdded;
        public static readonly Dictionary<int, IPlayer> DeviceIDToPlayer = new Dictionary<int, IPlayer>();

        //Function to read from an .ini file
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal,
            int size, string filePath);

#region MachineDataFormat

//Todo: Find solution for working in a build without the hologate system (e.g. for testing)
#if !UNITY_EDITOR
        private const string MachineDataFile = "C:/HoloGate/machineData.ini";
#else
        private static readonly string MachineDataFile = Application.dataPath + "/HoloGate/machineData.ini";
#endif

        public static MachineDataFormat MachineData;

        public struct MachineDataFormat {
            public DataSection Data;
            public PathsSection Paths;

            public MachineDataFormat(DataSection data, PathsSection paths) {
                Data = data;
                Paths = paths;
            }
        }

        public struct DataSection {
            public int ID;
            public int IsServer;
            public int Music;
            public int HUD;
            public string LedBarUrl;
            public string ServerName;
            public int HeadsetType;

            public DataSection(string id, string isServer, string music, string hud, string ledBarUrl, string serverName, string headsetType) {
                ID = !string.IsNullOrEmpty(id) ? int.Parse(id) : 0;
                IsServer = !string.IsNullOrEmpty(isServer) ? int.Parse(isServer) : 0;
                Music = !string.IsNullOrEmpty(music) ? int.Parse(music) : 1;
                HUD = !string.IsNullOrEmpty(hud) ? int.Parse(hud) : 1;
                LedBarUrl = ledBarUrl;
                ServerName = !string.IsNullOrEmpty(serverName) ? serverName : "HologateServer";
                HeadsetType = !string.IsNullOrEmpty(headsetType) ? int.Parse(headsetType) : 1;
            }
        }

        public struct PathsSection {
            public string ConfigFolderPath;
            public readonly string GameSessionConfig;
            public readonly string GameStatusMcp;
            public readonly string GameStatusGame;
            public readonly string GameSessionResults;

            public PathsSection(string configFolderPath, string gameSessionConfigPath, string gameStatusMcp, string gameStatusGame, string gameSessionResults) {
                ConfigFolderPath = configFolderPath;
                GameSessionConfig = gameSessionConfigPath;
                GameStatusMcp = gameStatusMcp;
                GameStatusGame = gameStatusGame;
                GameSessionResults = gameSessionResults;
            }
        }

#endregion

#region GameSessionJsonFormat

        [Serializable]
        public class GameSessionConfig {
            public Device[] Devices;
            public string TeamName;
            public string LevelName;
            public string Length;
            public string Language;
            public string PlayerCount;
            public string IsTutorialActive;
            public string IsBlockedSpectator;

            public class Device {
                public UserInfos UserInfo;
                public int DeviceID;
            }

            public class UserInfos {
                public string UserID;
                public string UserGender;
                public string UserName;
            }
        }

        #endregion

#region GameSessionResultFormat
        [Serializable]
        public class GameSessionResults {
            public string TeamName;
            public UserInfos[] UserInfo;
            public string TeamScore;

            public class UserInfos {
                public string UserID;
                public string UserScore;
                public string UserName;
            }
        }
#endregion

        /// <summary>
        /// Initializes the game for master and clients and configures the Hologate settings
        /// </summary>
        /// <param name="controllerType">The controller type asset which keeps the current controller type</param>
        public static void InitFromMachineData(SharedControllerType controllerType) {
            try {
                string folderPath = "\\..\\PhotonServer";
#if UNITY_EDITOR
                folderPath = "\\.." + folderPath;
#endif
                MachineData = new MachineDataFormat(GetDataSection(), GetPathSection());
                controllerType.Set(typeof(HologateManager), MachineData.Data.IsServer == 0 ? ControllerType.VR : ControllerType.Admin);
                if (SharedControllerType.IsAdmin) {
                    SetLocalIpToLoadBalancing(folderPath);
                    ConfigurationManager.Configuration.LocationName = $"HoloGate{GetSystemIdentifier()}";
                    if (Directory.Exists(Application.dataPath + folderPath)) {
                        Process.Start(Application.dataPath + folderPath + "\\bin_Win64\\StartLoadBalancing.cmd");
#if UNITY_EDITOR
                        ////Process.Start(Application.dataPath + folderPath + "\\..\\BatchScripts\\TowerTagHologate.bat");
#else
                        Process.Start(Application.dataPath + folderPath + "\\..\\TowerTagHologate.bat");
#endif
                    } else {
                        Debug.LogWarning("No on-Permise Server found");
                    }
                }
                else {
                    ConfigurationManager.Configuration.EnableHapticHitFeedback = true;
                    ConfigurationManager.Configuration.TeamID = MachineData.Data.ID % 2 != 0 ? 0 : 1;
                }
                AudioListener.volume = MachineData.Data.Music == 0 ? 0 : 1;
                ConfigurationManager.Configuration.IngamePillarOffset = false;
                ConfigurationManager.Configuration.Room = $"HoloGate{GetSystemIdentifier()}";
                ConfigurationManager.Configuration.TeamVoiceChatEnableVoiceChat = false;
                ConfigurationManager.Configuration.ServerIp = GetGameServerIPAddress();
                ConfigurationManager.Configuration.PlayInLocalNetwork = true;
                ConfigurationManager.Configuration.ServerPort = 5055;
                ConfigurationManager.WriteConfigToFile();
            }
            catch (Exception e) {
                //Todo: Handle exception correctly! The machineData should be on the same path on every device! If not we can't start the hologate version
                Debug.LogError("Can not load Machine Data!\n" + e);
                Application.Quit(1);
            }
        }

        /// <summary>
        /// Writes the game server ip into the LoadBalancing config file, to make the on-premise photon server start with the local ip
        /// </summary>
        /// <param name="folderPath">Path to the Photon Server directory</param>
        private static void SetLocalIpToLoadBalancing(string folderPath) {
            string configPath = Application.dataPath + folderPath + "\\LoadBalancing\\GameServer\\bin\\Photon.LoadBalancing.dll.config";
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            if (File.Exists(configPath)) {
                xmlDocument.LoadXml(File.ReadAllText(configPath));
                XmlNode xmlNode = xmlDocument.SelectSingleNode(
                    "//configuration//applicationSettings//Photon.LoadBalancing.GameServer.GameServerSettings//setting[@name='PublicIPAddress']//value");
                if (xmlNode == null)
                    return;
                xmlNode.InnerText = GetGameServerIPAddress();
            }
            xmlDocument.Save(configPath);
        }

        /// <summary>
        /// Adding a new connected player to the device list and set participating status to false
        /// </summary>
        /// <param name="deviceID">Device ID of the Player</param>
        /// <param name="player">The player</param>
        public static void AddPlayerToDeviceList(int deviceID, IPlayer player) {
            if(HologateController.GameSession.Devices.All(device => device.DeviceID != deviceID))
                player.IsParticipating = false;
            if (DeviceIDToPlayer.ContainsKey(deviceID)) {
                DeviceIDToPlayer.Remove(deviceID);
            }
            DeviceIDToPlayer.Add(deviceID, player);
            DevicePlayerAdded?.Invoke(deviceID, player);
        }

        /// <summary>
        /// Removes the player from the intern device list
        /// </summary>
        /// <param name="player">The player who we want to remove</param>
        public static void RemovePlayerFromDeviceList(IPlayer player) {
            int deviceID = GetDeviceByPlayer(player);
            if(DeviceIDToPlayer.ContainsKey(deviceID))
                DeviceIDToPlayer.Remove(deviceID);
        }

        /// <summary>
        /// Gets the HoloGate Device ID of the Player
        /// </summary>
        /// <param name="player">The player who's ID we want to get</param>
        /// <returns>The device ID according to the player</returns>
        public static int GetDeviceByPlayer(IPlayer player) {
            KeyValuePair<int, IPlayer> firstOrDefault = DeviceIDToPlayer.FirstOrDefault(device => device.Value == player);
            if (!firstOrDefault.Equals(default)) {
                return firstOrDefault.Key;
            }

            return -1;
        }

        /// <summary>
        /// Writes the current status of the game (Lobby, Game, Leaderboard) into the game status game file
        /// </summary>
        /// <param name="content">The current status of the game</param>
        public static void WriteInGameStatusGameFile(string content) {
            string path = MachineData.Paths.ConfigFolderPath + MachineData.Paths.GameStatusGame;
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Writes the match results in the GameSessionResults.json file
        /// </summary>
        /// <param name="match">Current match</param>
        public static void WriteInGameSessionResultsFile(IMatch match) {
            if (string.IsNullOrEmpty(MachineData.Paths.GameSessionResults)) return;
            string path = MachineData.Paths.ConfigFolderPath + MachineData.Paths.GameSessionResults;
            List<GameSessionResults.UserInfos> userInfos = new List<GameSessionResults.UserInfos>();

            match.GetRegisteredPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                userInfos.Add( new GameSessionResults.UserInfos {
                        UserName = players[i].PlayerName,
                        UserScore = $"{match.Stats.GetPlayerStats()[players[i].PlayerID].Kills}" +
                                    $"/{match.Stats.GetPlayerStats()[players[i].PlayerID].Deaths}" +
                                    $"/{match.Stats.GetPlayerStats()[players[i].PlayerID].Assists}",
                        UserID = $"{GetDeviceByPlayer(players[i])}"
                    });
            }

            GameSessionResults results = new GameSessionResults {
                TeamName = "TowerTag",
                UserInfo = userInfos.ToArray(),
                TeamScore = $"Infinite"
            };
            string output = JsonConvert.SerializeObject(results);
            File.WriteAllText(path, output);
        }

        /// <summary>
        /// Deletes the content of the game status mcp file
        /// This happens to signal the mcp, that we received the content
        /// </summary>
        public static void ClearGameStatusMcpFile() {
            string path = MachineData.Paths.ConfigFolderPath + MachineData.Paths.GameStatusMcp;
            File.WriteAllText(path, "");
        }

        /// <summary>
        /// Reading the content of the GameStatusMcp file
        /// </summary>
        /// <returns>The content of the GameStatusMcp file</returns>
        public static string ReadGameStatusMcpFile() {
            string path = MachineData.Paths.ConfigFolderPath + MachineData.Paths.GameStatusMcp;
            var fs = new StreamReader(path);
            string content = fs.ReadToEnd();
            fs.Close();
            return content;
        }

        /// <summary>
        /// Reading the GameSessionConfig
        /// </summary>
        /// <returns>The GameSessionConfig read from the file</returns>
        public static GameSessionConfig GetGameSessionConfig() {
            string content = ReadGameSessionConfig();
            return JsonConvert.DeserializeObject<GameSessionConfig>(content);
        }

        private static string ReadGameSessionConfig() {
            string path = MachineData.Paths.ConfigFolderPath + MachineData.Paths.GameSessionConfig;
            var fs = new StreamReader(path);
            string content = fs.ReadToEnd();
            fs.Close();
            return content;
        }

        /// <summary>
        /// Returns the Values of the Data Section in the machineData.ini file
        /// </summary>
        private static DataSection GetDataSection() {
            DataSection data = new DataSection(IniReadValue("DATA", "Id"), IniReadValue("DATA", "IsServer"),
                IniReadValue("DATA", "Music"), IniReadValue("DATA", "HUD"), IniReadValue("DATA", "LedBarUrl"),
                IniReadValue("DATA", "ServerName"), IniReadValue("DATA", "HeadsetType"));
            return data;
        }

        /// <summary>
        /// Returns the Values of the Path Section in the machineData.ini file
        /// </summary>
        private static PathsSection GetPathSection() {
            PathsSection paths = new PathsSection(IniReadValue("PATHS", "ConfigFolderPath"), IniReadValue("PATHS", "GameSessionConfig"),
                IniReadValue("PATHS", "GameStatusMCP"), IniReadValue("PATHS", "GameStatusGame"), IniReadValue("PATHS", "GameSessionResults"));
#if UNITY_EDITOR
            paths.ConfigFolderPath = Application.dataPath + "/HoloGate/Games/GameConfigs/";
#endif
            return paths;
        }

        /// <summary>
        /// Read Data Value From the Ini File
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <PARAM name="Path"></PARAM>
        /// <returns></returns>
        private static string IniReadValue(string section, string key) {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", temp, 255, MachineDataFile);
            return temp.ToString();
        }

        /// <summary>
        /// Splits the current IP and sets the last number to 10 (HG GameServer always will be 10.10x.xxx.10)
        /// </summary>
        /// <returns>The GameServer IP</returns>
        public static string GetGameServerIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    string localIP = ip.ToString();
                    string[] localIPSplit = localIP.Split('.');
                    if (localIPSplit[0].Equals("10")) {
                        localIPSplit[3] = "10";
                    }
                    return string.Join(".", localIPSplit);
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        /// <summary>
        /// Splits the current IP and returns RegionID.SetupID
        /// </summary>
        /// <returns>The SetupId</returns>
        public static string GetSystemIdentifier() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == AddressFamily.InterNetwork) {
                    string localIP = ip.ToString();
                    string[] localIPSplit = localIP.Split('.');
                    return localIPSplit[1] + "." + localIPSplit[2];
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
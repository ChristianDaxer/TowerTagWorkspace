using System.Collections;
using System.Collections.Generic;
using TowerTag;
using UnityEngine;
using UnityEngine.Networking;

namespace Hologate {
    public static class LedBarConfigurationManager {
        private readonly struct LEDColor {
            public readonly int ID;
            public readonly Color Color;

            public LEDColor(int id, Color color) {
                ID = id;
                Color = color;
            }
        }

        private class PlaySpaceLightsDictionary : Dictionary<int, (int left, int right)> {
            public new void Add(int playSpaceID, (int left, int right) lightIDs) {
                base.Add(playSpaceID, lightIDs);
            }

            public new(int leftLedId, int rightLedId) this[int playSpaceID] => base[playSpaceID];
        }

        //Defined by hologate!
        private static readonly PlaySpaceLightsDictionary _playerSpaces = new PlaySpaceLightsDictionary {
            { 1, (101,102) },
            { 2, (103,104) },
            { 3, (105,106) },
            { 4, (107,108) },
        };

        private enum PillarPositions {
            Front,
            Back,
            Left,
            Right
        }

        private class PillarLightsDictionary : Dictionary<PillarPositions, (int top, int bottom)> {
            public new void Add(PillarPositions pillarPosition, (int top, int bottom) lightIDs) {
                base.Add(pillarPosition, lightIDs);
            }

            public new(int topID, int bottomID) this[PillarPositions pillarPosition] {
                get => base[pillarPosition];
            }
        }

        //Defined by hologate!
        private static readonly PillarLightsDictionary _pillarLights = new PillarLightsDictionary {
            { PillarPositions.Front, (11,6) },
            { PillarPositions.Right, (12,7) },
            { PillarPositions.Back, (13,8) },
            { PillarPositions.Left, (14,9) }
        };


        private static string _ledRequestUrl;
        private static Color _fireMain;
        private static Color _iceMain;
        private static Color _fireDark;
        private static Color _iceDark;

        public static void Init() {
            _ledRequestUrl = HologateManager.MachineData.Data.LedBarUrl;
            CollectColors();
            InitPillarLights();
            InitLedBars();
        }

        private static void CollectColors() {
            _fireMain = TeamManager.Singleton.TeamFire.Colors.Main;
            _iceMain = TeamManager.Singleton.TeamIce.Colors.Main;
            _fireDark = TeamManager.Singleton.TeamFire.Colors.Dark;
            _iceDark = TeamManager.Singleton.TeamIce.Colors.Dark;
        }

        /// <summary>
        /// Initializes the color for all Pillar LEDs
        /// </summary>
        private static void InitPillarLights() {
            LEDColor[] pillarColors = {
                new LEDColor(_pillarLights[PillarPositions.Front].topID, _fireMain),
                new LEDColor(_pillarLights[PillarPositions.Front].bottomID, _iceMain),
                new LEDColor(_pillarLights[PillarPositions.Right].topID, _fireMain),
                new LEDColor(_pillarLights[PillarPositions.Right].bottomID, _iceMain),
                new LEDColor(_pillarLights[PillarPositions.Back].topID, _fireMain),
                new LEDColor(_pillarLights[PillarPositions.Back].bottomID, _iceMain),
                new LEDColor(_pillarLights[PillarPositions.Left].topID, _fireMain),
                new LEDColor(_pillarLights[PillarPositions.Left].bottomID, _iceMain)
            };
            ChangeMultipleLedColors(pillarColors);
        }

        /// <summary>
        /// Initializes the color for all LED bars
        /// </summary>
        private static void InitLedBars() {
            ChangePlaySpaceColor(1, _fireDark, _fireDark);
            ChangePlaySpaceColor(3, _fireDark, _fireDark);
            ChangePlaySpaceColor(2, _iceDark, _iceDark);
            ChangePlaySpaceColor(4, _iceDark, _iceDark);
        }

        /// <summary>
        /// Changes the led colors of a specific play space
        /// </summary>
        /// <param name="id">ID of the play space (acc. to the device ID)</param>
        /// <param name="left">Left LED bar color</param>
        /// <param name="right">Right LED bar color</param>
        public static void ChangePlaySpaceColor(int id, Color left, Color right) {
            ChangeMultipleLedColors(new[] { new LEDColor(_playerSpaces[id].leftLedId, left), new LEDColor(_playerSpaces[id].rightLedId, right) });
        }

        public static void TogglePlaySpace(int deviceID, bool active, TeamID teamId) {
            Color color;
            if (active)
                color = teamId == TeamID.Fire ? _fireMain : _iceMain;
            else
                color = teamId == TeamID.Fire ? _fireDark : _iceDark;
            ChangePlaySpaceColor(deviceID,color,color);
        }

        /// <summary>
        /// Changes the colors of all the given LEDs
        /// </summary>
        /// <param name="led"></param>
        private static void ChangeMultipleLedColors(LEDColor[] led) {
            Color32 color32 = led[0].Color;
            string url = $"{_ledRequestUrl}id={led[0].ID}&r={color32.r}&g={color32.g}&b={color32.b}";
            for (int i = 1; i < led.Length; i++) {
                color32 = led[i].Color;
                url += $"&id_{i - 1}={led[i].ID}&r_{i - 1}={color32.r}&g_{i - 1}={color32.g}&b_{i - 1}={color32.b}";
            }
            Debug.Log($"Changing colors to {url}");
            //Send Change (Calling url)
            StaticCoroutine.StartStaticCoroutine(SendLightValues(url));
        }

        /// <summary>
        /// Sending the url as GET request
        /// </summary>
        /// <param name="url">The path to the led config + color changes</param>
        private static IEnumerator SendLightValues(string url) {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.LogError(www.error);
            }
        }

        /// <summary>
        /// Light show for LEDs
        /// </summary>
        /// <param name="players">Participating players</param>
        /// <param name="winningTeamID">Winning TeamID</param>
        public static IEnumerator LightShow(TeamID winningTeamID) {
            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            if(winningTeamID != TeamID.Neutral) {
                Color lightColor = winningTeamID == TeamID.Fire ? _fireMain : _iceMain;
                Color darkColor = winningTeamID == TeamID.Fire ? _fireDark : _iceDark;
                while (true) {
                    for (int i = 0; i < count; i++)
                    {
                        int deviceByPlayer = HologateManager.GetDeviceByPlayer(players[i]);
                        ChangePlaySpaceColor(deviceByPlayer, darkColor,darkColor);
                        yield return new WaitForSeconds(0.25f);
                        ChangePlaySpaceColor(deviceByPlayer, lightColor, lightColor);
                    }
                    yield return null;
                }
            }
            else {
                while (true) {
                    for (int i = 0; i < count; i++)
                    {
                        int deviceByPlayer = HologateManager.GetDeviceByPlayer(players[i]);
                        Color lightColor = players[i].TeamID == TeamID.Fire ? _fireMain : _iceMain;
                        Color darkColor = players[i].TeamID == TeamID.Fire ? _fireDark : _iceDark;
                        ChangePlaySpaceColor(deviceByPlayer, darkColor, darkColor);
                        yield return new WaitForSeconds(0.25f);
                        ChangePlaySpaceColor(deviceByPlayer, lightColor, lightColor);
                    }
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Setting the play space lights to the team color main
        /// </summary>
        /// <param name="participants"></param>
        public static void ResetLights() {
            PlayerManager.Instance.GetParticipatingPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                Color lightColor = players[i].TeamID == TeamID.Fire ? _fireMain : _iceMain;
                int deviceByPlayer = HologateManager.GetDeviceByPlayer(players[i]);
                ChangePlaySpaceColor(deviceByPlayer, lightColor, lightColor);
            }
        }
    }
}
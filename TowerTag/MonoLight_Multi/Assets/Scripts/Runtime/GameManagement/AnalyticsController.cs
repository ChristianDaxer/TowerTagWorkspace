using System.Collections.Generic;
using UnityEngine.Analytics;

public class AnalyticsController {
    private const string AnalyticsConnectEventName = "connected_to_room";
    private const string AnalyticsLicenseActivatedEventName = "license_activated";
    private const string AnalyticsLicenseDeactivatedEventName = "license_deactivated";
    private const string AnalyticsStartMatchEventName = "match_started";
    private const string AnalyticsEndMatchEventName = "match_finished";
    private const string AnalyticsEndBotMatchEventName = "botmatch_finished";
    private const string AnalyticsPlayerFinishedMatchEventName = "player_finished_match";
    private const string AnalyticsMatchPausedEventName = "match_paused";
    private const string AnalyticsMatchResumedEventName = "match_resumed";
    private const string AnalyticsLoadMatchEventName = "match_loaded";
    private const string AnalyticsLoadCommendationsEventName = "commendations_loaded";
    private const string AnalyticsMissionBriefingEventName = "mission_briefing";
    private const string AnalyticsLoadHubEventName = "hub_loaded";
    private const string AnalyticsEmergencyEventName = "emergency";
    private const string AnalyticsCommendationEventName = "commendation";
    private const string AnalyticsSendMatchReportEventName = "match_reported";

    public static string Version;

    /// <summary>
    /// Set the user id as it appears in Analytics
    /// </summary>
    /// <param name="userid">The user id</param>
    /// <returns>True if the user id was set correctly</returns>
    public static bool SetAnalyticsUserId(string userid) {
        AnalyticsResult res = Analytics.SetUserId(userid);
        return res == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event that the player has successfully connected to a room
    /// </summary>
    /// <param name="gameVersion">The game version that we run</param>
    /// <param name="playerName">The name of the player</param>
    /// <param name="teamName">The name of the team of the player who received the commendation</param>
    /// <param name="roomName">The room name which we connected to</param>
    /// <param name="region">The region where we connected</param>
    /// <param name="controllerType">Our controller type</param>
    /// <param name="playOnLan">True if we play on lan, false if we play on the cloud</param>
    /// <returns>True if the analytics event was successfully send</returns>
    public static bool Connect(
            string gameVersion,
            string roomName,
            string playerName,
            string teamName,
            string region,
            string controllerType,
            bool playOnLan
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsConnectEventName, new Dictionary<string, object> {
            {"game version", gameVersion},
            {"playerName", playerName},
            {"room name", roomName},
            {"team name", teamName},
            {"play on lan", playOnLan},
            {"region", region},
            {"controller type", controllerType},
            {"version", Version},
            {"basic mode", TowerTagSettings.BasicMode},
            {"hologate", TowerTagSettings.Hologate}
        });

        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsConnectEventName + " AnalyticsCustomEvent: " + result);
        return (result == AnalyticsResult.Ok);
    }

    /// <summary>
    /// Send an analytics event that the license was activated
    /// </summary>
    /// <param name="eMail">The eMail that was used to activate this license</param>
    /// <param name="productKey">The product key that was used to activate this license</param>
    /// <param name="productVersion">The product version as defined in cryptlex that is used (demo or standard)</param>
    /// <returns>True if the analytics event was successfully send</returns>
    public static bool LicenseActivation(
            string eMail,
            string productKey,
            string productVersion
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsLicenseActivatedEventName,
            new Dictionary<string, object> {
                {"email", eMail},
                {"product key", productKey},
                {"product version", productVersion},
                {"version", Version},
                {"basic mode", TowerTagSettings.BasicMode}
            });

        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsLicenseActivatedEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event that the license was deactivated
    /// </summary>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool LicenseDeactivation() {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsLicenseDeactivatedEventName,
            new Dictionary<string, object> {
                {"version", Version},
                {"basic mode", TowerTagSettings.BasicMode}
            });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsLicenseDeactivatedEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event if a match starts
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="numberOfPlayers">The number of players that are participating</param>
    /// <param name="automaticStartIfAllPlayersReady">True if automatic start when all players are ready is on</param>
    /// <param name="matchTime">The configured match time in minutes</param>
    /// <param name="mapName">The name of the map where the match was played on</param>
    /// <param name="gameMode">The type of match, e.g., DeathMatch</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool StartMatch(
            string roomName,
            int numberOfPlayers,
            bool automaticStartIfAllPlayersReady,
            int matchTime,
            string mapName,
            string gameMode
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsStartMatchEventName, new Dictionary<string, object> {
            {"room name", roomName},
            {"number of players", numberOfPlayers},
            {"automatic match start", automaticStartIfAllPlayersReady},
            {"match time", matchTime},
            {"map name", mapName},
            {"game mode", gameMode},
            {"basic mode", TowerTagSettings.BasicMode},
            {"hologate", TowerTagSettings.Hologate},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsStartMatchEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event if a match ends
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="numberOfPlayers">The number of players that are participating</param>
    /// <param name="automaticStartIfAllPlayersReady">True if automatic start when all players are ready is on</param>
    /// <param name="matchTime">The configured match time in minutes</param>
    /// <param name="roundsPlayed">The number of rounds which were played in the match</param>
    /// <param name="mapName">The name of the map where the match was played on</param>
    /// <param name="gameMode">The type of match, e.g., DeathMatch</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool FinishBotMatch(
        string roomName,
        int numberOfPlayers,
        bool automaticStartIfAllPlayersReady,
        int matchTime,
        int roundsPlayed,
        string mapName,
        string gameMode
    ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsEndBotMatchEventName, new Dictionary<string, object> {
            {"room name", roomName},
            {"number of players", numberOfPlayers},
            {"automatic start if all players are ready", automaticStartIfAllPlayersReady},
            {"match time", matchTime},
            {"rounds played", roundsPlayed},
            {"map name", mapName},
            {"game mode", gameMode},
            {"basic mode", TowerTagSettings.BasicMode},
            {"hologate", TowerTagSettings.Hologate},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsEndBotMatchEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event if a match ends
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="numberOfPlayers">The number of players that are participating</param>
    /// <param name="matchTime">The configured match time in minutes</param>
    /// <param name="roundsPlayed">The number of rounds which were played in the match</param>
    /// <param name="botCount">The count of participating bots</param>
    /// <param name="mapName">The name of the map where the match was played on</param>
    /// <param name="gameMode">The type of match, e.g., DeathMatch</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool FinishMatch(
            string roomName,
            int numberOfPlayers,
            int matchTime,
            int roundsPlayed,
            int botCount,
            string mapName,
            string gameMode
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsEndMatchEventName, new Dictionary<string, object> {
            {"room name", roomName},
            {"number of players", numberOfPlayers},
            {"match time", matchTime},
            {"rounds played", roundsPlayed},
            {"participating bots", botCount},
            {"map name", mapName},
            {"game mode", gameMode},
            {"basic mode", TowerTagSettings.BasicMode},
            {"hologate", TowerTagSettings.Hologate},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsEndMatchEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event if a match ends
    /// </summary>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool PlayerFinishedMatch(
        int matchTime,
        int roundsPlayed,
        string mapName,
        string gameMode,
        string roomName,
        bool loggedIn
    ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsPlayerFinishedMatchEventName, new Dictionary<string, object> {
            {"match time", matchTime},
            {"rounds played", roundsPlayed},
            {"map name", mapName},
            {"game mode", gameMode},
            {"room name", roomName},
            {"version", Version},
            {"basic mode", TowerTagSettings.BasicMode},
            {"hologate", TowerTagSettings.Hologate},
            {"member", loggedIn}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsPlayerFinishedMatchEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event when the match is paused
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="numberOfPlayers">The number of players that are participating</param>
    /// <param name="roundsPlayed"></param>
    /// <param name="elapsedSeconds">The number of seconds that were played before the pause</param>
    /// <param name="mapName">The name of the map where the match was played on</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool PauseMatch(
            string roomName,
            int numberOfPlayers,
            int roundsPlayed,
            int elapsedSeconds,
            string mapName
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsMatchPausedEventName, new Dictionary<string, object> {
            {"room name", roomName},
            {"number of players", numberOfPlayers},
            {"elapsed seconds", elapsedSeconds},
            {"rounds played", roundsPlayed},
            {"map name", mapName},
            {"basic mode", TowerTagSettings.BasicMode},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsMatchPausedEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event when the match is resumed
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="numberOfPlayers">The number of players that are participating</param>
    /// <param name="roundsPlayed"></param>
    /// <param name="elapsedSeconds">The number of seconds that were played before the pause</param>
    /// <param name="mapName">The name of the map where the match was played on</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool ResumeMatch(
            string roomName,
            int numberOfPlayers,
            int roundsPlayed,
            int elapsedSeconds,
            string mapName
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsMatchResumedEventName, new Dictionary<string, object> {
            {"room name", roomName},
            {"number of players", numberOfPlayers},
            {"elapsed seconds", elapsedSeconds},
            {"rounds played", roundsPlayed},
            {"map name", mapName},
            {"basic mode", TowerTagSettings.BasicMode},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsMatchResumedEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event when the match is loaded
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="numberOfPlayers">The number of players that are participating</param>
    /// <param name="matchTime">The configured match time in minutes</param>
    /// <param name="mapName">The name of the map where the match was played on</param>
    /// <param name="controllerType">Our controller type</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool LoadMatch(
            string roomName,
            int numberOfPlayers,
            int matchTime,
            string mapName,
            string controllerType
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsLoadMatchEventName, new Dictionary<string, object> {
            {"room name", roomName},
            {"number of players", numberOfPlayers},
            {"match time", matchTime},
            {"map name", mapName},
            {"controller type", controllerType},
            {"basic mode", TowerTagSettings.BasicMode},
            {"hologate", TowerTagSettings.Hologate},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsLoadMatchEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event when the commendations scene is loaded
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="numberOfPlayers">The number of players that are participating</param>
    /// <param name="commendationsSceneName">The name of the map where the match was played on</param>
    /// <param name="controllerType">Our controller type</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool LoadCommendations(
            string roomName,
            int numberOfPlayers,
            string commendationsSceneName,
            string controllerType
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsLoadCommendationsEventName,
            new Dictionary<string, object> {
                {"room name", roomName},
                {"number of players", numberOfPlayers},
                {"commendations scene name", commendationsSceneName},
                {"controller type", controllerType},
                {"basic mode", TowerTagSettings.BasicMode},
                {"hologate", TowerTagSettings.Hologate},
                {"version", Version}
            });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsLoadCommendationsEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    public static void ShowMissionBriefing(
        string mapName,
        string roomName) {
        Analytics.CustomEvent(AnalyticsMissionBriefingEventName,
                new Dictionary<string, object> {
                    {"room name", roomName},
                    {"map name", mapName},
                    {"version", Version},
                    {"basic mode", TowerTagSettings.BasicMode},
                    {"hologate", TowerTagSettings.Hologate}
                });
    }

    /// <summary>
    /// Send an analytics event when the Hub Scene is loaded
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="numberOfPlayers">The number of players that are participating</param>
    /// <param name="hubSceneName">The name of the Hub Scene that we loaded</param>
    /// <param name="controllerType">Our controller type</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool LoadHub(
            string roomName,
            int numberOfPlayers,
            string hubSceneName,
            string controllerType
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsLoadHubEventName, new Dictionary<string, object> {
            {"room name", roomName},
            {"number of players", numberOfPlayers},
            {"hub scene name", hubSceneName},
            {"controller type", controllerType},
            {"basic mode", TowerTagSettings.BasicMode},
            {"hologate", TowerTagSettings.Hologate},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsLoadHubEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event when an emergency occurs
    /// </summary>
    /// <param name="region">The region where we connected</param>
    /// <param name="playOnLan">True if we play on lan, false if we play on the cloud</param>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="numberOfPlayers">The number of players that are participating</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool Emergency(
            string region,
            bool playOnLan,
            string roomName,
            int numberOfPlayers
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsEmergencyEventName, new Dictionary<string, object> {
            {"region", region},
            {"lan", playOnLan},
            {"room name", roomName},
            {"number of players", numberOfPlayers},
            {"basic mode", TowerTagSettings.BasicMode},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsEmergencyEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event if a commendation was received
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="playerName">The name of the player who received the commendation</param>
    /// <param name="teamName">The name of the team of the player who received the commendation</param>
    /// <param name="kills">The number of times the player killed an opponent</param>
    /// <param name="deaths">The number of times the player died</param>
    /// <param name="assists">The number of times the player assisted on killing an opponent</param>
    /// <param name="commendationName">The name of the commendation the player received</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool Commendation(
            string roomName,
            string playerName,
            string teamName,
            string commendationName,
            int kills,
            int deaths,
            int assists
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsCommendationEventName, new Dictionary<string, object> {
            {"room name", roomName},
            {"player name", playerName},
            {"team name", teamName},
            {"commendation", commendationName},
            {"kills", kills},
            {"deaths", deaths},
            {"assists", assists},
            {"basic mode", TowerTagSettings.BasicMode},
            {"hologate", TowerTagSettings.Hologate},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsCommendationEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event when a player finished a 1v1 vs a bot
    /// </summary>
    /// <param name="kills">Number of kills from the player</param>
    /// <param name="deaths">Number of deaths of the player</param>
    /// <param name="playerWon"></param>
    /// <param name="mapName"></param>
    /// <param name="gameMode"></param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool TrainingMatchFinished(
            int kills,
            int deaths,
            bool playerWon,
            string mapName,
            string gameMode
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsSendMatchReportEventName, new Dictionary<string, object> {
            {"kills", kills},
            {"deaths", deaths},
            {"player won", playerWon },
            {"map name", mapName},
            {"game mode", gameMode }
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsSendMatchReportEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }

    /// <summary>
    /// Send an analytics event when a match was reported to the VR-Nerds backend
    /// </summary>
    /// <param name="roomName">The room name we are in</param>
    /// <param name="location">The name of the arcade location</param>
    /// <param name="playerCount">Number of participating players</param>
    /// <param name="memberCount">The number of logged in members</param>
    /// <param name="botCount">The number of participating bots</param>
    /// <returns>True if the analytics event was successfully send</returns>>
    public static bool MatchReport(
            string roomName,
            string location,
            int playerCount,
            int memberCount,
            int botCount
        ) {
        AnalyticsResult result = Analytics.CustomEvent(AnalyticsSendMatchReportEventName, new Dictionary<string, object> {
            {"room name", roomName},
            {"location", location},
            {"player count", playerCount},
            {"member count", memberCount},
            {"bot count", botCount},
            {"basic mode", TowerTagSettings.BasicMode},
            {"hologate", TowerTagSettings.Hologate},
            {"version", Version}
        });
        if (result != AnalyticsResult.Ok)
            Debug.LogWarning("Send " + AnalyticsSendMatchReportEventName + " AnalyticsCustomEvent: " + result);
        return result == AnalyticsResult.Ok;
    }
}
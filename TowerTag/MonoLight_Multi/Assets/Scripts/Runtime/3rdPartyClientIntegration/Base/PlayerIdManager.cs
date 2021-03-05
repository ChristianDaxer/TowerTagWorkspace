using System;
using System.Collections.Generic;
#if !UNITY_ANDROID
using Steamworks;
#endif
using TowerTagAPIClient;
using TowerTagAPIClient.Store;
using UnityEngine;
using UnityEngine.Networking;
#if !UNITY_ANDROID
using Viveport;
#endif

public abstract class PlayerIdManager : TTSingleton<PlayerIdManager>
{
    private const string DartsLiveApiKey = "Z6J13wHlPA2rrgLDbh5vd91qWvd6bHKf2hImIfxu";

    protected bool _isReady = false;
    public bool IsReady { get { return _isReady; } }

    protected string _userID;
    protected string _displayName;
    public string GetUserID() => _userID;
    public string GetDisplayName() => _displayName;

    public const string TempDisplayName = "Test User";
    public const string TempUserId = "testuser123";

    // Start is called before the first frame update
    protected void Start()
    {
        TTSceneManager.Instance.HubSceneLoaded += OnHubSceneLoaded;
    }
    
    protected void OnDisable() {
        TTSceneManager.Instance.HubSceneLoaded -= OnHubSceneLoaded;
        Destroy(this);
    }

    protected abstract void OnHubSceneLoaded();

    protected void GetPlayerStatistics(string playerMembershipID) {
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
        request.SendWebRequest();
        PlayerStore.GetPlayer(Authentication.OperatorApiKey, playerMembershipID, true);
    }

    /*public static string GetUserId() {
        try {
            string userId = "";
#if !UNITY_ANDROID
            switch (TowerTagSettings.HomeType) {
                case HomeTypes.Steam:
                    userId = SteamManager.Initialized ? SteamUser.GetSteamID().ToString() : "";
                    break;
                case HomeTypes.Viveport:
                    userId = User.GetUserId();
                    break;
            }
#else
            //TODO QUESTPORT
            userId = "QuestPort";
#endif
            //**Use this, if you want to test TT on the same machine!**
#if UNITY_EDITOR
            if (TowerTagSettings.SteamEditorId)
                userId = "EDITOR" + userId;
#endif
            //**Use this, if you want to test TT on the same machine!**

            return userId;
        }
        catch (Exception e) {
            Debug.LogError("Could not get User id: " + e);
            return "";
        }
    }*/

    public abstract string GetUserId();
}

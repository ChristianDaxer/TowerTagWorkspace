using System.Collections;
using Steamworks;
using Home;
using UnityEngine;
using VRNerdsUtilities;

public class SteamSdkController : BaseSdkController 
{
    public delegate void SteamManagerAction(object sender);

    public event SteamManagerAction SteamManagerInitialized;
    private Callback<GameOverlayActivated_t> _callbackSteamOverlayActivated;
    private CallResult<NumberOfCurrentPlayers_t> _numberOfCurrentPlayers;
    private bool _steamWorksCoroutineRunning;

    protected override void OnAwake() 
    {
        //base.Awake();
        if (!TowerTagSettings.Home) 
        {
            Destroy(gameObject);
            return;
        }

        SteamAPI.Init();
    }

    protected override void OnStart() 
    {
        // Check SteamManager
        if (SteamManager.Initialized) 
        {
            SteamManagerInitialized?.Invoke(this);
            TTSceneManager.Instance.HubSceneLoaded += OnHubSceneLoaded;
            _callbackSteamOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnSteamGameOverlayActivated);
            _numberOfCurrentPlayers = CallResult<NumberOfCurrentPlayers_t>.Create(OnNumberOfCurrentPlayers);
        }

        else 
        {
            Debug.LogWarning("SteamManager not Initialized");
            if (!_steamWorksCoroutineRunning)
                StartCoroutine(InitSteamWorksCoroutine());
        }

        try 
        {
            var steamPlayerName = SteamFriends.GetPersonaName().Length <= BitCompressionConstants.PlayerNameMaxLength
                ? SteamFriends.GetPersonaName()
                : SteamFriends.GetPersonaName().Substring(0, BitCompressionConstants.PlayerNameMaxLength);

            if (string.IsNullOrEmpty(PlayerProfileManager.CurrentPlayerProfile.PlayerName))
                PlayerProfileManager.CurrentPlayerProfile.PlayerName = steamPlayerName;

            if (!SteamSdkPlayerIdManager.GetInstance(out var playerIdManager))
                return;

            Debug.LogFormat("Current account: {0}, ID: {1}", steamPlayerName, playerIdManager.GetUserId());

            TowerTagSettings.LeaderboardManager.InitLeaderboard();
            PlayerAccount.Init(playerIdManager.GetUserId());
        }
        catch 
        {
            PlayerAccount.Init("Bonobo123");
        }
    }

    protected override void OnHubSceneLoaded() 
    {
        //Hub Scene loaded -> local Player is now instantiated
    }

    private void OnDisable() {
        if (TTSceneManager.Instance != null)
            TTSceneManager.Instance.HubSceneLoaded -= OnHubSceneLoaded;
    }

    private void OnNumberOfCurrentPlayers(NumberOfCurrentPlayers_t pCallback, bool bIOFailure) {
        if (pCallback.m_bSuccess != 1 || bIOFailure)
            Debug.Log("There was an error retrieving the NumberOfCurrentPlayers.");
        else
            Debug.Log("The number of players playing your game: " + pCallback.m_cPlayers);
    }

    private static void OnSteamGameOverlayActivated(GameOverlayActivated_t paramCallback) {
        Debug.LogError(paramCallback.m_bActive != 0 ? "Steam Game overlay active" : "Steam Game overlay inactive");
    }

    private IEnumerator InitSteamWorksCoroutine() {
        _steamWorksCoroutineRunning = true;
        yield return new WaitUntil(CheckSteamWorksStatus);
        while (!SteamManager.Initialized)
            _steamWorksCoroutineRunning = false;
        yield return null;
    }

    private static bool CheckSteamWorksStatus() {
        if (SteamManager.Initialized)
            return true;

        SteamAPI.Init();
        return false;
    }
}
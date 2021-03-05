using System.Collections;
using Home;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Viveport;

public class ViveportSDKManager : MonoBehaviour {
    // Get a VIVEPORT ID and VIVEPORT Key from the VIVEPORT Developer Console. Please refer to here:
    // https://developer.viveport.com/documents/sdk/en/viveport_sdk/definition/get_viveportid.html
#if !DEVELOPMENT_BUILD
    static string VIVEPORT_ID = "6fbd6044-c912-49e1-8655-987a13dee10c";           // replace with developer VIVEPORT ID
    static string VIVEPORT_KEY = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDIB+Pi3MzdawGDebdM8JQ8HJWV0lshvyvEAcPUSOWo2Mup3b" +
                                 "XH/SBvDmMOe//drpMpLXaoSE4N4X3prvkBy2GmE6+mymO6KCwkhgUt2CdmRNyNw6eYX5Ih3MLIG7ix9We8/sno" +
                                 "dv8dm0R2WtwXIIlD27/c/zJvv9JnLjqA51IbYwIDAQAB";         // replace with developer VIVEPORT Key
#else
    static string VIVEPORT_ID = "70b065c4-ba99-4623-a643-9f894b96def5"; // replace with developer VIVEPORT ID
    static string VIVEPORT_KEY = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDsMGhztCKhIH5yLrz2wRIWKHtK9T1XQalWeIEogMCzV2" +
                                 "5chRnkuDnhL7UAVzsmF4WjdJPy27qGzDseEwA0/PjsWKniXK04u1hUOGZIkmjqammUCzoPT5jA7V1I5s9" +
                                 "freHhynmyUg55VP2h4y4YcUus48WSTolOBa+PysdZcq8aDwIDAQAB";
#endif

    private const int SUCCESS = 0;
    private static bool bInitComplete, bIsReady, bUserProfileIsReady = false, bArcadeIsReady = false, bTokenIsReady;


    void Awake() {
        MainThreadDispatcher mainThreadDispatcher = gameObject.GetComponent<MainThreadDispatcher>();
        if (!mainThreadDispatcher) {
            gameObject.AddComponent<MainThreadDispatcher>();
        }

        Api.Init(InitStatusHandler, VIVEPORT_ID); // initialize VIVEPORT platform
    }

    void Start() {
        Invoke(nameof(CheckInitStatus), 10); // check that VIVEPORT Init succeeded
    }

    void OnDestroy() {
        Api.Shutdown(ShutdownHandler);
    }

    private static void InitStatusHandler(int nResult) // The callback of Api.init()
    {
        if (nResult == SUCCESS) {
            Debug.Log("VIVEPORT init pass");
            Api.GetLicense(new MyLicenseChecker(), VIVEPORT_ID, VIVEPORT_KEY); // the response of Api.Init() is success, continue using Api.GetLicense() API
            UserStats.IsReady(InitViveportPlayer);
            UserStats.DownloadStats(DownloadStatsHandler);
            bInitComplete = true;
        }
        else {
            bInitComplete = false;
            Debug.Log("VIVEPORT init fail");
            Application.Quit();
        }
    }

    private static void DownloadStatsHandler(int nResult) {
        if (nResult == 0) {
            MainThreadDispatcher.Instance().Enqueue(InitAchievementManager());
        }
        else {
            Viveport.Core.Logger.Log("Failed to download statistics with error code: " + nResult);
        }
    }

    private static IEnumerator InitAchievementManager() {
        AchievementManager.Init(new ViveportAchievementManager());
        yield break;
    }

    private static void IsTokenReadyHandler(int nResult) {
        if (nResult == 0) {
            bTokenIsReady = true;
            Viveport.Core.Logger.Log("IsTokenReadyHandler is successful");
#if !UNITY_ANDROID
            Token.GetSessionToken(GetSessionTokenHandler);
#endif
        }
        else {
            bTokenIsReady = false;
            Viveport.Core.Logger.Log("IsTokenReadyHandler error: " + nResult);
        }
    }

    private static void GetSessionTokenHandler(int nResult, string message) {
        if (nResult == 0) {
            Viveport.Core.Logger.Log("GetSessionTokenHandler is successful, token:" + message);

            // Photon:
            // With the viveport token, we can set the auth values for Photon and connect / auth.
            // We store the token for later use.
            ViveSessionToken = message;
            // if (PhotonNetwork.AuthValues == null)
            // {
            //     PhotonNetwork.AuthValues = new AuthenticationValues();
            // }
            //
            // PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Viveport;
            // PhotonNetwork.AuthValues.AddAuthParameter("userToken", ViveSessionToken);
            // Viveport.Core.Logger.Log("UserID " + PhotonNetwork.AuthValues.UserId);
        }
        else {
            if (message.Length != 0) {
                Viveport.Core.Logger.Log("GetSessionTokenHandler error: " + nResult + ", message:" + message);
            }
            else {
                Viveport.Core.Logger.Log("GetSessionTokenHandler error: " + nResult);
            }
        }
    }

    public static string ViveSessionToken { get; set; }

    private static void ShutdownHandler(int nResult) // The callback of Api.Shutdown()
    {
        if (nResult == SUCCESS) {
            Application.Quit(); // the response of Api.Shutdown() is success, close the content
        }
    }

    private static void InitViveportPlayer(int result) {
        bIsReady = true;
        User.IsReady(InitPlayer);
        MainThreadDispatcher.Instance().Enqueue(InitLeaderboardOnMainThreadDispatcher());
    }

    private static IEnumerator InitLeaderboardOnMainThreadDispatcher() {
        TowerTagSettings.LeaderboardManager.InitLeaderboard();
        yield return null;
    }

    private static void InitPlayer(int nresult) {
        try {
            if (nresult != 0) {
                Viveport.Core.Logger.Log("Not able to init viveport player");
                return;
            }

            var viveportPlayerName = User.GetUserName().Length <= BitCompressionConstants.PlayerNameMaxLength
                ? User.GetUserName()
                : User.GetUserName().Substring(0, BitCompressionConstants.PlayerNameMaxLength);

            if (string.IsNullOrEmpty(PlayerProfileManager.CurrentPlayerProfile.PlayerName))
                PlayerProfileManager.CurrentPlayerProfile.PlayerName = viveportPlayerName;

            if (!PlayerIdManager.GetInstance(out var playerIdManager))
                return;

            Viveport.Core.Logger.Log($"Photon authentication with id {playerIdManager.GetUserId()}");
            Viveport.Core.Logger.Log($"Current Viveport Account: {viveportPlayerName}, ID: {playerIdManager.GetUserId()}");
            MainThreadDispatcher.Instance().Enqueue(AuthenticatePhoton());
            // Get User Data
            if (!PlayerAccount.ReceivedPlayerStatistics) {
                MainThreadDispatcher.Instance().Enqueue(InitPlayerAccount(playerIdManager.GetUserId()));
            }
        }
        catch {
            MainThreadDispatcher.Instance().Enqueue(InitPlayerAccount("FailPlayer"));
        }
    }

    private static IEnumerator AuthenticatePhoton() {

        if (!PlayerIdManager.GetInstance(out var playerIdManager))
            yield break;

        PhotonNetwork.AuthValues = new AuthenticationValues(playerIdManager.GetUserId());
        yield return null;
    }

    private static IEnumerator InitPlayerAccount(string playerId) {
        PlayerAccount.Init(playerId);
        yield return null;
    }

    private void CheckInitStatus() {
        if (!bInitComplete) {
            Debug.LogWarning("Viveport init check fail"); // init requires VIVEPORT app installed and online connection
            Application.Quit();
        }
        else {
            Debug.Log("Viveport init check pass");
        }
    }

    class MyLicenseChecker : Api.LicenseChecker {
        public override void OnSuccess(long issueTime, long expirationTime, int latestVersion, bool updateRequired) {
            // the response of Api.GetLicense() is DRM success, user is allowed to use the content and continue with content flow
            Debug.Log("Viveport DRM pass");
            Debug.Log("issueTime: " + issueTime);
            Debug.Log("expirationTime: " + expirationTime);
            MainThreadDispatcher.Instance().Enqueue(SuccessAction());
#if !UNITY_ANDROID
            Token.IsReady(IsTokenReadyHandler);
#endif
        }

        public override void OnFailure(int errorCode, string errorMessage) {
            // the response of Api.GetLicense() is DRM fail, user is not allowed to use the content
            Debug.LogWarning("Viveport DRM fail:" + errorCode + " Message :" + errorMessage);
            MainThreadDispatcher.Instance().Enqueue(FailAction());
        }

        // Use these methods to call Unity functions from the API callbacks on the main thread
        IEnumerator SuccessAction() {
            yield return null;
        }

        IEnumerator FailAction() {
            Application.Quit();
            yield return null;
        }
    }
}
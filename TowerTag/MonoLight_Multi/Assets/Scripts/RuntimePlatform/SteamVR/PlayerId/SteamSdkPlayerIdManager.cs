using Home;
using Steamworks;
using Viveport;
using TowerTagSOES;

public sealed class SteamSdkPlayerIdManager : PlayerIdManager {
    private new void Start() {
        if (SteamManager.Initialized)
        base.Start();

    }

    protected override void OnHubSceneLoaded() {
        // check if Steam SDK is init and player has valid steam ID -> early return if not

        if (!SteamManager.Initialized || SteamUser.GetSteamID() == CSteamID.NotInitYetGS || SharedControllerType.Spectator)
            return;

        // Check local player instance
        if (PlayerManager.Instance.GetOwnPlayer() == null)
            Debug.LogError("cant find local player");

        PlayerManager.Instance.GetOwnPlayer()?.LogIn(GetUserId());

        // get player statistics
        if (!PlayerAccount.ReceivedPlayerStatistics) {
            PlayerAccount.Init(GetUserId());
            Debug.Log($"Player logged in: {PlayerManager.Instance.GetOwnPlayer()?.IsLoggedIn}");
            Debug.Log($"Player ID: {PlayerManager.Instance.GetOwnPlayer()?.MembershipID}");
        }

        GetPlayerStatistics(GetUserId());
    }

    public override string GetUserId()
    {
        try
        {
            string userId = "";

            switch (TowerTagSettings.HomeType) {
                case HomeTypes.SteamVR:
                    userId = SteamManager.Initialized ? SteamUser.GetSteamID().ToString() : "";
                    break;
                case HomeTypes.Viveport:
                    userId = User.GetUserId();
                    break;
            }

            //**Use this, if you want to test TT on the same machine!**
#if UNITY_EDITOR
            if (TowerTagSettings.SteamEditorId)
                userId = "EDITOR" + userId;
#endif
            //**Use this, if you want to test TT on the same machine!**

            return userId;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Could not get User id: " + e);
            return "";
        }
    }

    protected override void Init()
    {
        _isReady = true;
    }
}
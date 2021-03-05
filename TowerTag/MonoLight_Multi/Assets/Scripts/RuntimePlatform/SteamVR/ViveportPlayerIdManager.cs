using Home;
using System;
using Viveport;
public class ViveportPlayerIdManager : PlayerIdManager
{
    public override string GetUserId()
    {
        try
        {
            string userId = "";
            userId = User.GetUserId();

            //**Use this, if you want to test TT on the same machine!**
#if UNITY_EDITOR
            if (TowerTagSettings.SteamEditorId)
                userId = "EDITOR" + userId;
#endif
            //**Use this, if you want to test TT on the same machine!**

            return userId;
        }
        catch (Exception e)
        {
            Debug.LogError("Could not get User id: " + e);
            return "";
        }
    }

    protected override void Init()
    {
    }

    protected override void OnHubSceneLoaded()
    {

        // check if Steam SDK is init and player has valid steam ID -> early return if not
        if (string.IsNullOrEmpty(GetUserId()))
            return;

        // Check local player instance
        if (PlayerManager.Instance.GetOwnPlayer() == null)
            Debug.LogError("cant find local player");

        PlayerManager.Instance.GetOwnPlayer()?.LogIn(GetUserId());

        // get player statistics
        if (!PlayerAccount.ReceivedPlayerStatistics)
        {
            PlayerAccount.Init(GetUserId());
            Debug.Log($"Player logged in: {PlayerManager.Instance.GetOwnPlayer()?.IsLoggedIn}");
            Debug.Log($"Player ID: {PlayerManager.Instance.GetOwnPlayer()?.MembershipID}");
        }

        GetPlayerStatistics(GetUserId());
    }
}

using Home;

public class OculusSdkController : BaseSdkController
{
    protected override void OnAwake()
    {
    }

    protected override void OnHubSceneLoaded()
    {
    }

    private void OnGetLoggedInUser (string playerName, string userId)
    {
        if (string.IsNullOrEmpty(PlayerProfileManager.CurrentPlayerProfile.PlayerName))
            PlayerProfileManager.CurrentPlayerProfile.PlayerName = playerName;

        Debug.LogFormat("Current account: \"{0}\", ID: \"{1}\".", playerName, userId);

        TowerTagSettings.LeaderboardManager.InitLeaderboard();
        PlayerAccount.Init(userId);
    }

    protected override void OnStart()
    {
        if (!OculusEntitlementChecker.GetInstance(out var entitlementChecker))
            return;

        entitlementChecker.onSkipUserEntitlement += () =>
        {
            if (OculusPlayerIdManager.GetInstance(out var playerIdManager))
                OnGetLoggedInUser(playerIdManager.GetDisplayName(), playerIdManager.GetUserID());
        };

        entitlementChecker.onCompletedUserEntitlement += (entitled, msg) =>
        {
            if (!entitled || !PlayerIdManager.GetInstance(out var playerIdManager))
                return;

            var request = Oculus.Platform.Users.GetLoggedInUser();
            request.OnComplete((userRequest) =>
            {
                if (userRequest.IsError)
                {
                    Debug.LogErrorFormat("Unable to retrieve Oculus player name, the following error occurred: \"{0}\".", userRequest.GetError().Message);
                    return;
                }

                var user = userRequest.GetUser();
                OnGetLoggedInUser(string.IsNullOrEmpty(user.DisplayName) ? user.OculusID : user.DisplayName, playerIdManager.GetUserId()); 
                ///The display name was empty for myself (Paul D), not sure if it's because something is not set on my account
            });
        };
    }
}

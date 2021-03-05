using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamVRPlatformServices : PlatformServices
{
    private ILeaderboardManager leaderboardManager;
    private SteamVRChaperone chaperone;

#if !UNITY_ANDROID
    public override ILeaderboardManager LeaderboardManager
    {
        get
        {
            if (leaderboardManager == null)
            {
                if (SteamLeaderboardManager.GetInstance(out var instance))
                {
                    leaderboardManager = instance;
                }
            }
            return leaderboardManager;
        }
    }
    public override IVRChaperone Chaperone
    {
        get
        {
            if (chaperone == null)
            {
                chaperone = new SteamVRChaperone();
            }
            return chaperone;
        }
    }
#else
    public override ILeaderboardManager LeaderboardManager => throw new System.NotImplementedException();
    public override IVRChaperone Chaperone => throw new System.NotImplementedException();
#endif

    protected override void Init()
    {
    }
}

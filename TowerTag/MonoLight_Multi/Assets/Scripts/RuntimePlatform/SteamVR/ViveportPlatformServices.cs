using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveportPlatformServices : PlatformServices
{
    private ILeaderboardManager leaderboardManager;
    private SteamVRChaperone chaperone;
    public override ILeaderboardManager LeaderboardManager
    {
        get
        {
            if (leaderboardManager == null)
            {
                if (ViveportLeaderboardManager.GetInstance(out var instance))
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

    protected override void Init()
    {
    }
}

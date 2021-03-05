using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.Subsystems;

public class OculusPlatformServices : PlatformServices
{
    private ILeaderboardManager leaderboardManager;
    private OculusChaperone chaperone;
    private XRInputSubsystem inputSubsystem;

    public override ILeaderboardManager LeaderboardManager
    {
        get
        {
            if (leaderboardManager == null)
            {
                if (OculusLeaderboardManager.GetInstance(out var instance))
                {
                    leaderboardManager = instance;
                    return leaderboardManager;
                }

                return null;
            }

            return leaderboardManager;
        }
    }

    public override IVRChaperone Chaperone
    {
        get
        {
            if (chaperone == null)
                chaperone = new OculusChaperone(this);
            return chaperone;
        }

    }

    public XRInputSubsystem InputSubsystem
    {
        get
        {
            if (inputSubsystem == null)
            {
                List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
                SubsystemManager.GetInstances(inputSubsystems);
                for (int i = 0; i < inputSubsystems.Count; i++)
                {
                    if (inputSubsystems[i].running)
                    {
                        inputSubsystem = inputSubsystems[i];
                    }
                }
            }
            return inputSubsystem;
        }
    }

    protected override void Init() {} 
    private void Start()
    {
        if (!OculusEntitlementChecker.GetInstance(out var instance))
            return;

        instance.onSkipUserEntitlement += () => TowerTagSettings.SetPlatformServices(this);
        instance.onCompletedUserEntitlement += (entitled, message) =>
        {
            if (!entitled)
                return;

            AchievementManager.Init(new OculusAchievementManager());
            if (ClientStatistics.GetInstance(out var clientStatistics))
                clientStatistics.StoreStatisticsInDictionary();

            TowerTagSettings.SetPlatformServices(this);
        };
    }
}

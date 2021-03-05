using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamStatCollector : StatCollector
{
    protected override void ResetStatsAndAchievements() {
        if (TowerTagSettings.IsHomeTypeSteam)
            SteamUserStats.ResetAllStats(true);
    }
}

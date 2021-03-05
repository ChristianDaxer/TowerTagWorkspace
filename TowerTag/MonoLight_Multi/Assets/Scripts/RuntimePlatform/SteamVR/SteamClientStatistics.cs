using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class SteamClientStatistics : ClientStatistics
{
    public override void StoreStatisticsInDictionary()
    {
        if(TowerTagSettings.HomeType == HomeTypes.SteamVR)
        {
            SteamUserStats.GetStat(keys.Matches, out int matches);
            Statistics[keys.Matches] = matches;
            SteamUserStats.GetStat(keys.LoginRow, out int logins);
            Statistics[keys.LoginRow] = logins;
            SteamUserStats.GetStat(keys.Win, out int win);
            Statistics[keys.Win] = win;
            SteamUserStats.GetStat(keys.WinStreak, out int winStreak);
            Statistics[keys.WinStreak] = winStreak;
            SteamUserStats.GetStat(keys.Snipe, out int snipe);
            Statistics[keys.Snipe] = snipe;
            SteamUserStats.GetStat(keys.HealedLowPlayer, out int healedLowPlayer);
            Statistics[keys.HealedLowPlayer] = healedLowPlayer;
            SteamUserStats.GetStat(keys.HealthHealed, out int healthHealed);
            Statistics[keys.HealthHealed] = healthHealed;
            SteamUserStats.GetStat(keys.Claims, out int tower);
            Statistics[keys.Claims] = tower;
            SteamUserStats.GetStat(keys.Tele, out int tele);
            Statistics[keys.Tele] = tele;
            SteamUserStats.GetStat(keys.Fire, out int fire);
            Statistics[keys.Fire] = fire;
            SteamUserStats.GetStat(keys.Ice, out int ice);
            Statistics[keys.Ice] = ice;
            SteamUserStats.GetStat(keys.HeadShotsTaken, out int headShotsTaken);
            Statistics[keys.HeadShotsTaken] = headShotsTaken;
            SteamUserStats.GetStat(keys.MVP, out int mvp);
            Statistics[keys.MVP] = mvp;
            SteamUserStats.GetStat(keys.Lvl, out int lvl);
            Statistics[keys.Lvl] = lvl;
        } else if (TowerTagSettings.IsHomeTypeViveport)
        {
            Statistics[keys.Matches] = Viveport.UserStats.GetStat(keys.Matches, 0);
            Statistics[keys.LoginRow] = Viveport.UserStats.GetStat(keys.LoginRow, 0);
            Statistics[keys.Win] = Viveport.UserStats.GetStat(keys.Win, 0);
            Statistics[keys.WinStreak] = Viveport.UserStats.GetStat(keys.WinStreak, 0);
            Statistics[keys.Snipe] = Viveport.UserStats.GetStat(keys.Snipe, 0);
            Statistics[keys.HealedLowPlayer] = Viveport.UserStats.GetStat(keys.HealedLowPlayer, 0);
            Statistics[keys.HealthHealed] = Viveport.UserStats.GetStat(keys.HealthHealed, 0);
            Statistics[keys.Claims] = Viveport.UserStats.GetStat(keys.Claims, 0);
            Statistics[keys.Tele] = Viveport.UserStats.GetStat(keys.Tele, 0);
            Statistics[keys.Fire] = Viveport.UserStats.GetStat(keys.Fire, 0);
            Statistics[keys.Ice] = Viveport.UserStats.GetStat(keys.Ice, 0);
            Statistics[keys.HeadShotsTaken] = Viveport.UserStats.GetStat(keys.HeadShotsTaken, 0);
            Statistics[keys.MVP] = Viveport.UserStats.GetStat(keys.MVP, 0);
            Statistics[keys.Lvl] = Viveport.UserStats.GetStat(keys.Lvl, 0);
        }

        base.StoreStatisticsInDictionary();
    }

}

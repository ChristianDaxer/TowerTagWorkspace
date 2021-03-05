using System;
using System.Collections.Generic;

public class AchievementManagerBase
{
	public virtual void Initialize() { }
	public virtual void SetStatistic(string ID, int value) { }
	public virtual void RaiseStatisticWithOne(string ID) { }
	public virtual void RaiseStatisticWithValue(string ID, int value) { }
	public virtual void SetAchievement(string ID) { }
	public virtual bool IsAchievementAlreadyUnlocked(string ID) { return false; }
	public virtual void AwardAchievementIfNeeded(string id)
	{
		if (!ClientStatistics.GetInstance(out var statistics) || !Achievements.GetInstance(out var achievements))
			return;

        if (id == statistics.keys.Claims)
            if (statistics.Statistics[id] == 100) SetAchievement(achievements.Keys.ControlFreak);
        if (id == statistics.keys.Fire)
            if (statistics.Statistics[id] == 10) SetAchievement(achievements.Keys.HottestFire);
        if (id == statistics.keys.Ice)
            if (statistics.Statistics[id] == 10) SetAchievement(achievements.Keys.ColdestIce);
        if (id == statistics.keys.Lvl)
        {
            if (statistics.Statistics[id] >= 10) SetAchievement(achievements.Keys.Lvl10);
            if (statistics.Statistics[id] >= 20) SetAchievement(achievements.Keys.Lvl20);
            if (statistics.Statistics[id] >= 30) SetAchievement(achievements.Keys.Lvl30);
        }
        if (id == statistics.keys.Matches)
        {
            if (statistics.Statistics[id] == 50) SetAchievement(achievements.Keys.Play50);
            if (statistics.Statistics[id] == 100) SetAchievement(achievements.Keys.Play100);
            if (statistics.Statistics[id] == 200) SetAchievement(achievements.Keys.Play200);
        }
        if (id == statistics.keys.Snipe)
            if (statistics.Statistics[id] == 50) SetAchievement(achievements.Keys.Sniper);
        if (id == statistics.keys.Tele)
            if (statistics.Statistics[id] == 1000) SetAchievement(achievements.Keys.VRLegs);
        if (id == statistics.keys.Win)
            if (statistics.Statistics[id] == 1) SetAchievement(achievements.Keys.WonSingleMatch);
        if (id == statistics.keys.HealthHealed)
            if (statistics.Statistics[id] >= 10000) SetAchievement(achievements.Keys.Parademic);
        if (id == statistics.keys.LoginRow)
            if (statistics.Statistics[id] == 7) SetAchievement(achievements.Keys.LogInRow);
        if (id == statistics.keys.WinStreak)
            if (statistics.Statistics[id] == 5) SetAchievement(achievements.Keys.WinStreak5);
            if (statistics.Statistics[id] == 10) SetAchievement(achievements.Keys.WinStreak10);
        if (id == statistics.keys.HeadShotsTaken)
            if (statistics.Statistics[id] == 10) SetAchievement(achievements.Keys.HeadShotsTaken);
        if (id == statistics.keys.HealedLowPlayer)
            if (statistics.Statistics[id] == 20) SetAchievement(achievements.Keys.HealLowPlayers);
        if (id == statistics.keys.MVP)
            if (statistics.Statistics[id] == 10) SetAchievement(achievements.Keys.MVP);
	}

	public virtual string GetLastError()
	{
		return null;
	}

	public virtual List<AchievementManager.Achievement> GetStatus()
	{
		throw new System.NotImplementedException();
	}
}

#if !UNITY_ANDROID
using Steamworks;
using System.Collections.Generic;
using UnityEngine;

public class SteamAchievementManager : AchievementManagerBase
{
    private Callback<UserStatsReceived_t> _userStatsReceived;

	public override void Initialize()
	{
        SteamUserStats.RequestCurrentStats();
		_userStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
	}

	public override void SetStatistic(string ID, int value)
	{
		if (!ClientStatistics.GetInstance(out var clientStatistics))
			return;

        clientStatistics.Statistics[ID] = value;
        bool success = SteamUserStats.SetStat(ID, clientStatistics.Statistics[ID]);
		SteamUserStats.StoreStats();
		if (!success)
			Debug.LogError($"Could not set Stat {ID} to {clientStatistics.Statistics[ID]}");
	}

	public override void RaiseStatisticWithOne(string ID)
	{
		if (!ClientStatistics.GetInstance(out var clientStatistics))
			return;

        bool success = SteamUserStats.SetStat(ID, ++clientStatistics.Statistics[ID]);
		SteamUserStats.StoreStats();
		if (!success)
			Debug.LogError($"Could not set Stat {ID}");
	}

	public override void RaiseStatisticWithValue(string ID, int value)
	{
		if (!ClientStatistics.GetInstance(out var clientStatistics))
			return;

        bool success = SteamUserStats.SetStat(ID, clientStatistics.Statistics[ID]);
		SteamUserStats.StoreStats();
		if (!success)
			Debug.LogError($"Could not set Stat {ID} to {value}");
	}

	public override void SetAchievement(string ID)
	{
        bool success = SteamUserStats.SetAchievement(ID);
		SteamUserStats.StoreStats();
		if (!success)
			Debug.LogError($"Could not set Achievement {ID}");
	}

	public override bool IsAchievementAlreadyUnlocked(string ID)
	{
		SteamUserStats.GetAchievement(ID, out bool sUnlocked);
		return sUnlocked;
	}

	List<AchievementManager.Achievement> achievementList = null;
	public override List<AchievementManager.Achievement> GetStatus()
	{
		return achievementList;
	}

	private void OnUserStatsReceived(UserStatsReceived_t param) {
		if (!ClientStatistics.GetInstance(out var clientStatistics))
			return;

        if (param.m_eResult == EResult.k_EResultOK) {
            clientStatistics.StoreStatisticsInDictionary();

			var achievementList = new List<AchievementManager.Achievement>();
			
			foreach (var ach in clientStatistics.Statistics)
			{
				bool unlocked = false;
				float percent = 0f;

				SteamUserStats.GetAchievementAchievedPercent(ach.Key, out percent);
				SteamUserStats.GetAchievement(ach.Key, out unlocked);

				achievementList.Add(new AchievementManager.Achievement() { name = ach.Key, count = (ulong)ach.Value, unlocked = unlocked });
			}
		}
	}
}
#endif
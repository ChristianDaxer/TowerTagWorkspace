#if !UNITY_ANDROID
using UnityEngine;

public class ViveportAchievementManager : AchievementManagerBase
{
	public override void Initialize()
	{
		if (!ClientStatistics.GetInstance(out var clientStatistics))
			return;
        clientStatistics.StoreStatisticsInDictionary();
	}

	public override void SetStatistic(string ID, int value)
	{
		if (!ClientStatistics.GetInstance(out var clientStatistics))
			return;
        clientStatistics.Statistics[ID] = value;
		Viveport.UserStats.SetStat(ID, clientStatistics.Statistics[ID]);
		Viveport.UserStats.UploadStats(UploadStatsHandler);
		AwardAchievementIfNeeded(ID);
	}

	public override void RaiseStatisticWithOne(string ID)
	{
		if (!ClientStatistics.GetInstance(out var clientStatistics))
			return;

		Viveport.UserStats.SetStat(ID, ++clientStatistics.Statistics[ID]);
		Viveport.UserStats.UploadStats(UploadStatsHandler);
		AwardAchievementIfNeeded(ID);
	}

	public override void RaiseStatisticWithValue(string ID, int value)
	{
		if (!ClientStatistics.GetInstance(out var clientStatistics))
			return;

        Viveport.UserStats.SetStat(ID, clientStatistics.Statistics[ID]);
		Viveport.UserStats.UploadStats(UploadStatsHandler);
		AwardAchievementIfNeeded(ID);
	}

	public override void SetAchievement(string ID)
	{
        if (!IsAchievementAlreadyUnlocked(ID)) {
			int result = Viveport.UserStats.SetAchievement(ID);
			Viveport.UserStats.UploadStats(UploadStatsHandler);
			if (result != 1)
				Debug.LogError($"Could not set Achievement {ID}");
		}
	}

	public override bool IsAchievementAlreadyUnlocked(string ID)
	{
		return Viveport.UserStats.GetAchievement(ID);
	}

	private void UploadStatsHandler(int nResult)
	{
		if (nResult != 0)
		{
            Viveport.Core.Logger.Log("Failed to upload statistics with error code: " + nResult);
		}
	}
}
#endif
using System;
using System.Collections.Generic;
#if !UNITY_ANDROID
using Steamworks;
#endif

public static class AchievementManager
{
	public class Achievement
	{
		public string name;
		public ulong count;
		public ulong max;
		public bool unlocked;
		public bool counter;
	}

	private static AchievementManagerBase achievementManager;

	public static void Init(AchievementManagerBase manager) {
		achievementManager = manager;
		achievementManager.Initialize();
	}

    public static void SetStatistic(string ID, int value) {
		achievementManager?.SetStatistic(ID, value);
	}
	
	public static void RaiseStatisticWithOne(string ID)
	{
		achievementManager?.RaiseStatisticWithOne(ID);
	}

	public static void RaiseStatisticWithValue(string ID, int value)
	{
		achievementManager?.RaiseStatisticWithValue(ID, value);
	}

	public static void SetAchievement(string ID)
	{
		achievementManager?.SetAchievement(ID);
	}

	private static bool IsAchievementAlreadyUnlocked(string ID)
	{
		return (achievementManager!=null)?achievementManager.IsAchievementAlreadyUnlocked(ID):false;
	}

    private static void AwardAchievementIfNeeded(string ID)
	{
		achievementManager?.AwardAchievementIfNeeded(ID);
	}
	public static List<Achievement> GetStatus()
	{
		return achievementManager?.GetStatus();
	}

	public static string GetLastError()
	{
		return achievementManager?.GetLastError();
	}
}
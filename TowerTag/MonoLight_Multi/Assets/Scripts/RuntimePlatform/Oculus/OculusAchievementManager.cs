using Oculus.Platform;
using Oculus.Platform.Models;
using System.Collections.Generic;
using static AchievementManager;

public class OculusAchievementManager : AchievementManagerBase
{
	private Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();
	private string lastError;

	private ClientStatistics _cachedClientStatistics;
	private ClientStatistics clientStatitics
    {
		get
        {
			if (_cachedClientStatistics == null)
				ClientStatistics.GetInstance(out _cachedClientStatistics);
			return _cachedClientStatistics;
        }
    }

	public override void Initialize()
	{
        if (!Core.IsInitialized())
            Debug.LogError("Oculus API is not initialized!");
        else UpdateWithServerData();
	}

	public override void SetStatistic(string ID, int value)
	{
		if (AchievementExists(ID))
            RaiseStatisticWithValue(ID, value - (int)achievements[ID].count);
		else if (CheckStatistics(ID))
			clientStatitics.Statistics[ID] = value - clientStatitics.Statistics[ID];
	}

	public override void RaiseStatisticWithOne(string ID)
	{
		RaiseStatisticWithValue(ID, 1);
	}

	public override void RaiseStatisticWithValue(string ID, int value)
	{
		if (AchievementExists(ID))
            Oculus.Platform.Achievements.AddCount(ID, (ulong)value);
		else if (CheckStatistics(ID))
			clientStatitics.Statistics[ID] = value - clientStatitics.Statistics[ID];
	}

	public override void SetAchievement(string ID)
	{
		if (!AchievementExists(ID))
			return;

		achievements[ID].unlocked = true;
		Oculus.Platform.Achievements.Unlock(ID);
	}

	public override bool IsAchievementAlreadyUnlocked(string ID)
	{
		if (!AchievementExists(ID))
			return false;

		return achievements[ID].unlocked;
	}

	public override List<Achievement> GetStatus()
	{
		UpdateWithServerData();
		var fullList = new List<Achievement>();
		foreach (var a in achievements)
			fullList.Add(a.Value);
		return fullList;
	}

	public override string GetLastError()
	{
		return lastError;
	}

	private void UpdateWithServerData()
	{
		Oculus.Platform.Achievements.GetAllProgress().OnComplete(
			(Message<AchievementProgressList> msg) =>
			{
				lock (achievements)
				{
					foreach (var achievement in msg.Data)
					{
						Achievement ach = null;
						if (!achievements.TryGetValue(achievement.Name, out ach))
							achievements.Add(achievement.Name, ach = new Achievement());

						ach.name = achievement.Name;
						ach.count = achievement.Count;
						ach.unlocked = achievement.IsUnlocked;
					}
				}
			}
		);
		Oculus.Platform.Achievements.GetAllDefinitions().OnComplete(
			(Message<AchievementDefinitionList> msg) =>
			{
				lock (achievements)
				{
					foreach (var achievement in msg.Data)
					{
						Achievement ach = null;
						if (!achievements.TryGetValue(achievement.Name, out ach))
							achievements.Add(achievement.Name, ach = new Achievement());

						ach.name = achievement.Name;
						ach.max = achievement.Target;
						ach.counter = achievement.Type==AchievementType.Count;
					}
				}
			}
		);
	}

	private bool CheckStatistics(string ID)
	{
		if (clientStatitics == null || clientStatitics.Statistics.Count == 0)
		{
			Debug.LogError("Unknown error occurred regarding client statistics for Oculus platform.");
			return false;
		}

		else if (!clientStatitics.Statistics.ContainsKey(ID))
        {
			Debug.LogErrorFormat("No statistics are being recorded with ID: \"{0}\".", ID);
			return false;
        }

		return true;
	}

	private bool AchievementExists(string ID)
	{
		if (achievements == null)
		{
			lastError = "Oculus API is not initialized!";
			Debug.LogError(lastError);
			return false;
		}

		else if (!achievements.ContainsKey(ID))
        {
			Debug.LogErrorFormat("There is no Oculus achievment with ID: \"{0}\".", ID);
			return false;
        }

		return true;
	}
}

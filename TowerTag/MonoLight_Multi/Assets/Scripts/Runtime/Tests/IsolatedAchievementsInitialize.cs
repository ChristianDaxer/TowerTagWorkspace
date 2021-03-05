using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsolatedAchievementsInitialize : MonoBehaviour
{
	public Text status;
	public RectTransform list;
	private GameObject item;

	private void Awake()
	{
		//PlayerPrefs.SetInt(TowerTag.PlayerPrefKeys.Tutorial, 1);

		ConnectionManager.Instance._onConnectedToMasterDelegate += () => Initialize();
	}

	private void Initialize()
	{
		status.text = "Initializing...";

		item = list.GetChild(0).gameObject;
		item.SetActive(false);
		item.transform.SetParent(null, false);

		StartCoroutine(FillUpTheList());

		var camera = Camera.main;
		var parent = camera.transform.parent;
		while (parent && !parent.name.ToLower().Contains("rig"))
			parent = parent.parent;

		transform.GetChild(0).SetParent(parent, false);
	}

	IEnumerator FillUpTheList()
	{
		int retries = 0;
		while (true)
		{
			var achievements = AchievementManager.GetStatus();
			if (achievements != null && achievements.Count > 0)
			{
				foreach (var i in achievements)
				{
					var newListItem = InstantiateWrapper.InstantiateWithMessage(item);
					newListItem.SetActive(true);
					newListItem.transform.SetParent(list, false);
					newListItem.GetComponentInChildren<Text>().text = 
						i.name + " | " + 
						(i.counter?(i.count+"/"+i.max):"") + " | " + 
						(i.unlocked?"UNLOCKED":"LOCKED");
				}
				break;
			}

			status.text = "Initializing.";
			for (int i = 0; i < retries % 3; ++i)
				status.text += ".";

			status.text += ((achievements == null) ? " [Platform]" : " [Achievements]");

			retries++;

			if (!string.IsNullOrEmpty(AchievementManager.GetLastError()))
				status.text = AchievementManager.GetLastError();

			yield return new WaitForSecondsRealtime(1f);
		}

		status.text = "Achievements";
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.Steam;
using Steamworks;
using UnityEngine;

public class SteamTopListLoader : MonoBehaviour {
    [SerializeField] private List<SteamTopList> _topLists;
    [SerializeField] private List<SteamTopList> _commendationTopLists;

    public List<SteamTopList> TopLists => _topLists;

    public List<SteamTopList> CommendationTopLists => _commendationTopLists;

    private void Awake() {
        SteamTopListManager.Init(this);
    }

#if !UNITY_ANDROID
    private void OnEnable() {
        TowerTagSettings.LeaderboardManager.OnLeaderboardFound += InitTopLists;
    }

    private void OnDisable() {
        TowerTagSettings.LeaderboardManager.OnLeaderboardFound -= InitTopLists;
    }
#endif

    private void InitTopLists() {
        StartCoroutine(CallInitOfTopLists());
    }

    private IEnumerator CallInitOfTopLists() {
        foreach (SteamTopList steamTopList in TopLists) {
            steamTopList.Initialize(this);
            yield return new WaitUntil(() => !steamTopList.Loading);
        }
    }
}

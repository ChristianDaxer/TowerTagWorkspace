using System;
using System.Collections;
using Commendations;
using Network;
using Photon.Pun;
using Rewards;
using TowerTag;
using TowerTagAPIClient.Store;
using UnityEngine;

namespace Runtime.Steam
{

public class SteamTopListManager
{
    private static SteamTopListLoader _loader;
    private static IPlayer _ownPlayer;
    public static void Init(SteamTopListLoader loader)
    {
        _loader = loader;
        HoloPopupStatsInfo.OnStatisticsReceived += OnStatisticsReceived;
        //Non MasterClients do not know the commendations, so we wait for the local player commendation event
        CommendationsController.LocalPlayerCommendationAwarded += OnCommendationReceived;
    }
    
    private static void OnCommendationReceived(Commendation obj) {
        if (!PhotonNetwork.IsMasterClient && IsCurrentMatchValidForTopList())
            _loader.StartCoroutine(ReportLocalCommendation(obj.DisplayName));
    }

    private static void OnStatisticsReceived(StatsInfoData data) {
        StartReportValuesCoroutine(data);
    }

    private static void StartReportValuesCoroutine(StatsInfoData data) {
        if (IsCurrentMatchValidForTopList())
            _loader.StartCoroutine(ReportValues(data));
    }

    private static IEnumerator ReportLocalCommendation(string commendationName) {
        foreach (var toplist in _loader.CommendationTopLists)
        {
            switch (toplist.TopListName)
            {
                case SteamTopList.TopListNames.W_MEDICANGEL:
                    toplist.UploadScore(commendationName.Equals("MEDIC ANGEL") ? 1 : 0);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                case SteamTopList.TopListNames.W_MVP:
                    IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
                    if (ownPlayer == null) break;
                    toplist.UploadScore(
                        GameManager.Instance.CurrentMatch.Stats.WinningTeamID == ownPlayer.TeamID
                        && (commendationName.Equals("COLDEST ICE")
                            || commendationName.Equals("HOTTEST FIRE")
                            || commendationName.Equals("M.V.P."))
                        ? 1 : 0);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static IEnumerator ReportValues(StatsInfoData match)
    {
        IPlayer ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        if (ownPlayer == null) yield break;
        MatchStats currentMatchStats = GameManager.Instance.CurrentMatch.Stats;
        PlayerStats playerStats = currentMatchStats.GetPlayerStats()[ownPlayer.PlayerID];
        foreach (var toplist in _loader.TopLists)
        {
            switch (toplist.TopListName)
            {
                case SteamTopList.TopListNames.W_ASSISTS:
                    toplist.UploadScore(match.AssistsDifference);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                case SteamTopList.TopListNames.W_HEALTHHEALED:
                    toplist.UploadScore(match.HealingDifference);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                case SteamTopList.TopListNames.W_KILLS:
                    toplist.UploadScore(match.KillsDifference);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                case SteamTopList.TopListNames.W_MATCHESPLAYED:
                    toplist.UploadScore(1);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                case SteamTopList.TopListNames.W_MATCHESWON:
                    toplist.UploadScore(currentMatchStats.WinningTeamID == ownPlayer.TeamID 
                        && GameManager.Instance.CurrentMatch.GameMode != GameMode.GoalTower ? 1 : 0);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                case SteamTopList.TopListNames.W_MEDICANGEL:
                    if (string.IsNullOrEmpty(playerStats.Commendation))
                        break;
                    toplist.UploadScore(playerStats.Commendation.Equals("MEDIC ANGEL") ? 1 : 0);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                case SteamTopList.TopListNames.W_MVP:
                    if (string.IsNullOrEmpty(playerStats.Commendation))
                        break;
                    toplist.UploadScore(
                        currentMatchStats.WinningTeamID == ownPlayer.TeamID
                        && (playerStats.Commendation.Equals("COLDEST ICE")
                            || playerStats.Commendation.Equals("HOTTEST FIRE")
                            || playerStats.Commendation.Equals("M.V.P."))
                        ? 1 : 0);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                case SteamTopList.TopListNames.W_TOWERSCLAIMED:
                    toplist.UploadScore(match.ClaimsDifference);
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                case SteamTopList.TopListNames.W_WINSTREAK:
                    if (ClientStatistics.GetInstance(out var instance))
                    {
                        int currentWinStreak = instance.Statistics[instance.keys.WinStreak];
                        #if !UNITY_ANDROID
                        if (toplist.Entry.m_nScore <= currentWinStreak)
                            toplist.UploadScore(currentWinStreak, false);
                        #endif //TODO QuestPORT
                        yield return new WaitUntil(() => !toplist.Uploading);
                    }
                    break;
                case SteamTopList.TopListNames.W_HEADSHOTS:
                    toplist.UploadScore(HeadshotReward.HeadshotRewardsEarned);
                    HeadshotReward.HeadshotRewardsEarned = 0;
                    yield return new WaitUntil(() => !toplist.Uploading);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static bool IsCurrentMatchValidForTopList()
    {
        _ownPlayer = PlayerManager.Instance.GetOwnPlayer();
        return !GameManager.Instance.TrainingVsAI
               && _ownPlayer != null && _ownPlayer.IsParticipating
               && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertyKeys.PIN)
               && string.IsNullOrEmpty((string) PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertyKeys.PIN])
               && RoomConfiguration.GetMaxPlayersForCurrentRoom() == 8;
    }
}}

using System;
using System.Collections;
using System.Collections.Generic;
using Viveport;

public class ViveportLeaderboardManager : LeaderboardManager<ViveportLeaderboardManager> {
    public static Viveport.Leaderboard LocalPlayerEntry;

    public bool Initialized { get; set; }

    public override string LeaderboardHeadline => "VIVEPORT LEADERBOARD (WIP)";

    protected override void UploadRating(string playerName, int roundToInt) {
        UserStats.UploadLeaderboardScore(OnRatingUploaded, _leaderboardName, roundToInt);
    }

    private void OnRatingUploaded(int result) {
        Viveport.Core.Logger.Log("Rating uploaded with result: " + result);
        if (result != 0) {
            Viveport.Core.Logger.Log("Failed to upload score");
        }
    }

    public override void InitLeaderboard() {
        if (Initialized) return;
        GetLeaderboard();
    }

    public void GetLeaderboard() {
        GetLeaderboardGlobal();
    }

    public override bool GetLeaderboardFriends()
    {
        int entries = UserStats.GetLeaderboardScoreCount();
        entries = entries < _maxLeaderboardEntries ? entries : _maxLeaderboardEntries;
        UserStats.DownloadLeaderboardScores(OnLeaderboardDownloaded, _leaderboardName, UserStats.LeaderBoardRequestType.LocalData,
            UserStats.LeaderBoardTimeRange.AllTime, 0, entries - 1);
        return true;
    }

    public override bool GetLeaderboardGlobal()
    {
        int entries = UserStats.GetLeaderboardScoreCount();
        entries = entries < _maxLeaderboardEntries ? entries : _maxLeaderboardEntries;
        UserStats.DownloadLeaderboardScores(OnLeaderboardDownloaded, _leaderboardName, UserStats.LeaderBoardRequestType.GlobalData,
            UserStats.LeaderBoardTimeRange.AllTime, 0, entries - 1);
        return true;
    }

    public override bool GetLeaderboardUsers()
    {
        int entries = UserStats.GetLeaderboardScoreCount();
        entries = entries < _maxLeaderboardEntries ? entries : _maxLeaderboardEntries;
        UserStats.DownloadLeaderboardScores(OnLeaderboardDownloaded, _leaderboardName, UserStats.LeaderBoardRequestType.LocalData,
            UserStats.LeaderBoardTimeRange.AllTime, 0, entries - 1);
        return true;
    }

    public void GetLocalPlayerEntry() {
        UserStats.DownloadLeaderboardScores(SaveLocalStats, _leaderboardName, UserStats.LeaderBoardRequestType.GlobalDataAroundUser,
            UserStats.LeaderBoardTimeRange.AllTime, 0, 0);
    }

    private void SaveLocalStats(int result) {
        if (result != 0) {
            Viveport.Core.Logger.Log("Failed to load Scoreboard: " + result);
            return;
        }

        LocalPlayerEntry = UserStats.GetLeaderboardScore(0);
        MainThreadDispatcher.Instance().Enqueue(TriggerLocalEntryReceivedEvent());
    }

    private IEnumerator TriggerLocalEntryReceivedEvent() {
        MainThreadDispatcher.Instance().Enqueue(() => InvokeOnPlayerDataDownloaded(new LeaderboardEntry(LocalPlayerEntry.UserName, LocalPlayerEntry.Rank, LocalPlayerEntry.Score)));
        yield return null;
    }

    private void OnLeaderboardDownloaded(int result) {
        if (result != 0) {
            Viveport.Core.Logger.Log("Failed to load Scoreboard: " + result);
            return;
        }

        int scoreboardLength = UserStats.GetLeaderboardScoreCount();
        scoreboardLength = scoreboardLength < _maxLeaderboardEntries ? scoreboardLength : _maxLeaderboardEntries;
        LeaderboardEntry[] lbEntries = new LeaderboardEntry[scoreboardLength];
        for (int i = 0; i < scoreboardLength; i++) {
            Viveport.Leaderboard entry = UserStats.GetLeaderboardScore(i);
            lbEntries[i] = new LeaderboardEntry(entry.UserName, entry.Rank, entry.Score);
        }
        
        MainThreadDispatcher.Instance().Enqueue(() => InvokeOnLeaderboardDownloaded(lbEntries));
        Initialized = true;
        GetLocalPlayerEntry();
    }
}
using System;
using System.Collections;
using Home;
using Steamworks;
using UnityEngine;

public sealed class SteamLeaderboardManager : LeaderboardManager<SteamLeaderboardManager> {

    [Header("Upload Settings")] [SerializeField]
    private ELeaderboardUploadScoreMethod _uploadingMethod;

    private CallResult<LeaderboardFindResult_t> _callResultFindLeaderboard;
    private CallResult<LeaderboardScoreUploaded_t> _callResultUploadScore;
    private CallResult<LeaderboardScoresDownloaded_t> _callResultDownloadedEntries;
    private CallResult<LeaderboardScoresDownloaded_t> _callResultPlayerDownloaded;
    private Leaderboard _leaderboard;
    private bool _loading;

    public override string LeaderboardHeadline => "STEAM LEADERBOARD";

    #region Leaderboard Initialization

    public override void InitLeaderboard() {
        _callResultFindLeaderboard = new CallResult<LeaderboardFindResult_t>(OnFindLeaderboard);
        _callResultUploadScore = new CallResult<LeaderboardScoreUploaded_t>(OnUploadScore);
        _callResultDownloadedEntries = new CallResult<LeaderboardScoresDownloaded_t>(OnDownloadedEntries);
        _callResultPlayerDownloaded = new CallResult<LeaderboardScoresDownloaded_t>(OnDownloadedPlayerData);
        if (_leaderboard != null) {
            Debug.LogWarning("Leaderboard already initialized!");
            return;
        }

        if (string.IsNullOrEmpty(_leaderboardName)) {
            Debug.LogWarning("Empty leaderboard name to be loaded!");
            return;
        }

        _leaderboard = new Leaderboard(_leaderboardName);

        SteamAPICall_t steamAPICall = SteamUserStats.FindLeaderboard(_leaderboard.Name);
        _callResultFindLeaderboard.Set(steamAPICall);

        _loading = true;
    }

    private void OnFindLeaderboard(LeaderboardFindResult_t findLeaderboardResult, bool failure) {
        if (failure)
        {
            return;
        }

        if (findLeaderboardResult.m_bLeaderboardFound == 0x00) {
            Debug.LogWarning("Could not find the leaderboard! Is it named correctly?");
            return;
        }

        string leaderboardName = SteamUserStats.GetLeaderboardName(findLeaderboardResult.m_hSteamLeaderboard);
        _leaderboard.Name = leaderboardName;
        _leaderboard.Handle = findLeaderboardResult.m_hSteamLeaderboard;

        _loading = false;

        InvokeOnLeaderboardFound();
    }

    #endregion

    #region Download

    public override bool GetLeaderboardUsers() => GetLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestUsers);
    public override bool GetLeaderboardGlobal() => GetLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal);
    public override bool GetLeaderboardFriends() => GetLeaderboard(ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends);


private bool GetLeaderboard(ELeaderboardDataRequest type) {
    if (!SteamManager.Initialized) return false;

    if (_loading) {
        StartCoroutine(RetryGetLeaderboard(type));
        return false;
    }

    int offset = 0;
    if (type == ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser) {
        offset = -5;
    }

    SteamAPICall_t steamAPIcall =
        SteamUserStats.DownloadLeaderboardEntries(_leaderboard.Handle, type, offset,
            offset + _maxLeaderboardEntries);
    _callResultDownloadedEntries.Set(steamAPIcall);

    return true;
}

    private IEnumerator RetryGetLeaderboard(ELeaderboardDataRequest type) {
        while (_loading) {
            yield return null;
        }

        GetLeaderboard(type);
    }

    private void OnDownloadedEntries(LeaderboardScoresDownloaded_t leaderboardScoresDownloaded, bool failure) {

        if (failure) {
            Debug.LogWarning("Could not download entries for leaderboard!");
            return;
        }

        int entryCount = Mathf.Min(leaderboardScoresDownloaded.m_cEntryCount, _maxLeaderboardEntries);

        LeaderboardEntry[] leaderboardEntries = new LeaderboardEntry[entryCount];

        for (int i = 0; i < entryCount; i++) {
            LeaderboardEntry_t entry;
            SteamUserStats.GetDownloadedLeaderboardEntry(leaderboardScoresDownloaded.m_hSteamLeaderboardEntries, i,
                out entry, null, 0);
            leaderboardEntries[i] = new LeaderboardEntry(SteamFriends.GetFriendPersonaName(entry.m_steamIDUser), entry.m_nGlobalRank, entry.m_nScore);
        }

        InvokeOnLeaderboardDownloaded(leaderboardEntries);

        GetPlayerData(SteamUser.GetSteamID());
    }

    public void GetPlayerData(CSteamID steamID) {
        SteamAPICall_t steamAPIcall =
            SteamUserStats.DownloadLeaderboardEntriesForUsers(_leaderboard.Handle, new[] {steamID}, 1);
        _callResultPlayerDownloaded.Set(steamAPIcall);
    }

    private void OnDownloadedPlayerData(LeaderboardScoresDownloaded_t leaderboardScoresDownloaded, bool failure) {
        if (failure) {
            Debug.LogWarning("Could not download player entry for leaderboard!");
            return;
        }

        SteamUserStats.GetDownloadedLeaderboardEntry(leaderboardScoresDownloaded.m_hSteamLeaderboardEntries, 0,
            out LeaderboardEntry_t entry, null, 0);
        LeaderboardEntry lentry = new LeaderboardEntry(SteamFriends.GetFriendPersonaName(entry.m_steamIDUser), entry.m_nGlobalRank, entry.m_nScore);
        InvokeOnPlayerDataDownloaded(lentry);
    }

#endregion

#region Upload

    protected override void UploadRating(string playerName, int rating) {
        if (!SteamManager.Initialized) return;

        if (_leaderboard == null) {
            // Debug.LogWarning("Leaderboard is null");
            return;
        }

        if (_loading) {
            Debug.LogError("Tried to upload scores but leaderboard is still loading!");
            return;
        }

        float prevRating = PlayerAccount.Statistics != null ? PlayerAccount.Statistics.rating : 0;
        if (Math.Abs(rating - prevRating) < 0.5f) {
            // print("Rating has not changed");
            return;
        }


        SteamAPICall_t
            steamAPIcall =
                SteamUserStats.UploadLeaderboardScore(_leaderboard.Handle, _uploadingMethod, rating, null, 0);

        // print("Try to upload new Score of " + rating + " for " + name + " to leaderboard!");
        _callResultUploadScore.Set(steamAPIcall);
    }

    private void OnUploadScore(LeaderboardScoreUploaded_t scoreUploadedResult, bool failure) {
        if (!failure && scoreUploadedResult.m_bSuccess == 1) {
            print("Uploaded score successfully!");
        }
        else {
            Debug.Log("Failed to upload score!");
        }
    }

#endregion
}
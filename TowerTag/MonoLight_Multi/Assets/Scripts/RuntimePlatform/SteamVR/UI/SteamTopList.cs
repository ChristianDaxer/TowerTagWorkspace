using System.Collections;
using Steamworks;
using UnityEngine;

[CreateAssetMenu(menuName = "TowerTag/TopList")]
public class SteamTopList : ScriptableObject {
    public enum TopListNames { 
        W_ASSISTS,
        W_HEALTHHEALED,
        W_KILLS,
        W_MATCHESPLAYED,
        W_MATCHESWON,
        W_MEDICANGEL,
        W_MVP,
        W_TOWERSCLAIMED,
        W_WINSTREAK,
        W_HEADSHOTS
    }

    [SerializeField] protected TopListNames _topList;
    public string TopListNameString => _topList.ToString();
    public TopListNames TopListName => _topList;


    [Header("Upload Settings")] [SerializeField]
    private ELeaderboardUploadScoreMethod _uploadingMethod;

    private CallResult<LeaderboardFindResult_t> _callResultFindLeaderboard;
    private CallResult<LeaderboardScoreUploaded_t> _callResultUploadScore;
    private CallResult<LeaderboardScoresDownloaded_t> _callResultPlayerDownloaded;
    private Leaderboard _leaderboard;
    public bool Loading { get; private set; }
    public bool Uploading { get; private set; }

    public LeaderboardEntry_t Entry => _entry;


    private LeaderboardEntry_t _entry;
    private SteamTopListLoader _loader;

    public void Initialize(SteamTopListLoader loader) {
        _callResultFindLeaderboard = new CallResult<LeaderboardFindResult_t>(OnFindLeaderboard);
        _callResultUploadScore = new CallResult<LeaderboardScoreUploaded_t>(OnUploadScore);
        _callResultPlayerDownloaded = new CallResult<LeaderboardScoresDownloaded_t>(OnDownloadedPlayerData);
        _loader = loader;
        InitLeaderboard();
    }

    public void InitLeaderboard() {
        if (_leaderboard != null) {
            Debug.LogWarning("Leaderboard already initialized!");
            return;
        }

        if (string.IsNullOrEmpty(TopListNameString)) {
            Debug.LogWarning("Empty leaderboard name to be loaded!");
            return;
        }

        _leaderboard = new Leaderboard(TopListNameString);

        SteamAPICall_t steamAPICall = SteamUserStats.FindLeaderboard(_leaderboard.Name);
        _callResultFindLeaderboard.Set(steamAPICall);

        Loading = true;
    }

    private void OnFindLeaderboard(LeaderboardFindResult_t findLeaderboardResult, bool failure) {
        if (failure) return;

        if (findLeaderboardResult.m_bLeaderboardFound == 0x00) {
            Debug.LogWarning("Could not find the leaderboard! Is it named correctly?");
            return;
        }

        string leaderboardName = SteamUserStats.GetLeaderboardName(findLeaderboardResult.m_hSteamLeaderboard);
        if (!leaderboardName.Equals(TopListNameString))
        {
            Debug.LogError("Yeaaaah im trying to get " + TopListNameString + "but got " + leaderboardName);
            return;
        }

        _leaderboard.Name = leaderboardName;
        _leaderboard.Handle = findLeaderboardResult.m_hSteamLeaderboard;

        Loading = false;

        GetPlayerData(SteamUser.GetSteamID());
    }

    public bool GetLeaderboard(ELeaderboardDataRequest type) {
        if (!SteamManager.Initialized) return false;

        if (Loading) {
            _loader.StartCoroutine(RetryGetLeaderboard(type));
            return false;
        }

        int offset = 0;
        if (type == ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser) {
            offset = -5;
        }

        return true;
    }

    private IEnumerator RetryGetLeaderboard(ELeaderboardDataRequest type) {
        while (Loading) {
            yield return null;
        }

        GetLeaderboard(type);
    }

    private void GetPlayerData(CSteamID steamID) {
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

        if (entry.m_cDetails == 0)
        {
            _entry = entry;
        }
        else
        {
            _entry.m_nGlobalRank = 0;
            _entry.m_nScore = 0;
        }
    }

    public void UploadScore(int value, bool raise = true) {
        if (!SteamManager.Initialized) return;

        if (_leaderboard == null) {
            // Debug.LogWarning("Leaderboard is null");
            return;
        }

        if (Loading) {
            Debug.LogError("Tried to upload scores but leaderboard is still loading!");
            return;
        }

        if (value == 0) {
            // Debug.LogWarning("Rating has not changed in " + _leaderboard.Name);
            return;
        }

        if(raise)
            _entry.m_nScore += value;
        else
        {
            _entry.m_nScore = value;
        }
        SteamAPICall_t
            steamAPIcall =
                SteamUserStats.UploadLeaderboardScore(_leaderboard.Handle, _uploadingMethod, _entry.m_nScore, null, 0);

        global::Debug.LogError("Try to upload new Score of " + _entry.m_nScore + " for " + name + " to leaderboard!");
        _callResultUploadScore.Set(steamAPIcall);
        Uploading = true;
    }

    private void OnUploadScore(LeaderboardScoreUploaded_t scoreUploadedResult, bool failure) {
        if (!failure && scoreUploadedResult.m_bSuccess == 1) {
            Debug.Log($"Uploaded score for {_leaderboard.Name} with score {scoreUploadedResult.m_nScore} successfully!");
        }
        else {
            Debug.Log("Failed to upload score for " + _leaderboard.Name);
        }

        Uploading = false;
    }
}
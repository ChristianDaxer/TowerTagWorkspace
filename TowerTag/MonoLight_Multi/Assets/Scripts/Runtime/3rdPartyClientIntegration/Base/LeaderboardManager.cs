using Home;
using TowerTagAPIClient.Model;
using TowerTagAPIClient.Store;
using UnityEngine;

public delegate void LeaderboardPlayerDownloadDelegate(LeaderboardEntry entry);

public delegate void LeaderboardDownloadDelegate(LeaderboardEntry[] entries);
public delegate void LeaderboardFoundDelegate();
public interface ILeaderboardManager
{

    LeaderboardPlayerDownloadDelegate OnPlayerDataDownloaded { get; set; }
    LeaderboardDownloadDelegate OnLeaderboardDownloaded { get; set; }
    LeaderboardFoundDelegate OnLeaderboardFound { get; set; }

    string LeaderboardHeadline { get; }

    void InitLeaderboard();
    bool GetLeaderboardUsers();
    bool GetLeaderboardGlobal();
    bool GetLeaderboardFriends();
}

public abstract class LeaderboardManager<T> : TTSingleton<T>, ILeaderboardManager where T : TTSingleton<T>
{
    [Header("Leaderboard Settings")]
    [SerializeField] protected string _leaderboardName = "TOPLIST";

    [SerializeField] protected int _maxLeaderboardEntries = 100;

    public event LeaderboardPlayerDownloadDelegate _OnPlayerDataDownloaded;
    public event LeaderboardDownloadDelegate _OnLeaderboardDownloaded;
    public event LeaderboardFoundDelegate _OnLeaderboardFound;

    public LeaderboardPlayerDownloadDelegate OnPlayerDataDownloaded
    {
        get => _OnPlayerDataDownloaded;
        set { _OnPlayerDataDownloaded = value; }
    }
    public LeaderboardDownloadDelegate OnLeaderboardDownloaded
    {
        get => _OnLeaderboardDownloaded;
        set { _OnLeaderboardDownloaded = value; }
    }
    public LeaderboardFoundDelegate OnLeaderboardFound
    {
        get => _OnLeaderboardFound;
        set { _OnLeaderboardFound = value; }
    }

    public abstract string LeaderboardHeadline { get; }

    protected override void Init()
    {
        if (PlayerAccount.ReceivedPlayerStatistics)
            OnPlayerStatisticsReceived(PlayerAccount.Statistics);
    }

    protected void OnEnable()
    {
        PlayerStatisticsStore.PlayerStatisticsReceived += OnPlayerStatisticsReceived;
    }
    protected void OnDisable()
    {
        PlayerStatisticsStore.PlayerStatisticsReceived -= OnPlayerStatisticsReceived;
    }

    private void OnPlayerStatisticsReceived(PlayerStatistics playerStatistics)
    {
        PlayerIdManager.GetInstance(out var playerIdManager);
        if (playerStatistics.id.Equals(playerIdManager.GetUserId()))
            UploadRating(playerStatistics.id, Mathf.RoundToInt(playerStatistics.rating));
    }

    protected abstract void UploadRating(string playerName, int roundToInt);
    public abstract void InitLeaderboard();
    public abstract bool GetLeaderboardUsers();
    public abstract bool GetLeaderboardGlobal();
    public abstract bool GetLeaderboardFriends();

    protected void InvokeOnPlayerDataDownloaded(LeaderboardEntry entry) => _OnPlayerDataDownloaded?.Invoke(entry);
    protected void InvokeOnLeaderboardDownloaded(LeaderboardEntry[] entries) => _OnLeaderboardDownloaded?.Invoke(entries);
    protected void InvokeOnLeaderboardFound() => _OnLeaderboardFound?.Invoke();

    public void TestWriteLeaderBoard() //For testing purposes, need to be deleted.
    {
        UploadRating(_leaderboardName, 1050);
    }

}

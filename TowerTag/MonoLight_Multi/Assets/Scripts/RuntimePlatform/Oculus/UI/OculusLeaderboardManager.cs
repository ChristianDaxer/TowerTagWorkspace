using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;

public sealed class OculusLeaderboardManager : LeaderboardManager<OculusLeaderboardManager>
{
    private SortedDictionary<int, LeaderboardEntry> _leaderboardEntries = new SortedDictionary<int, LeaderboardEntry>();

    public override string LeaderboardHeadline => "OCULUS LEADERBOARD (WIP)";

    public override void InitLeaderboard()
    {
        GetLeaderboardGlobal();
        GetOwnLeaderboardEntry();
    }

    public override bool GetLeaderboardFriends()
    {
        Request<LeaderboardEntryList> entryRequest = Leaderboards.GetEntries(_leaderboardName, _maxLeaderboardEntries,
            LeaderboardFilterType.Friends, LeaderboardStartAt.Top).OnComplete(OnViveportLeaderboardDownloaded);

        if (entryRequest != null)
        {
            entryRequest.OnComplete(OnViveportLeaderboardDownloaded);
            return true;
        }

        Debug.LogError("Unable to get leaderboard friends, the core Oculus platform was not initialized.");
        return false;
    }
    public override bool GetLeaderboardGlobal()
    {
        Request<LeaderboardEntryList> entryRequest = Leaderboards.GetEntries(_leaderboardName, _maxLeaderboardEntries,
            LeaderboardFilterType.None, LeaderboardStartAt.Top);

        if (entryRequest != null)
        {
            entryRequest.OnComplete(OnViveportLeaderboardDownloaded);
            return true;
        }

        Debug.LogError("Unable to get leaderboard friends, the core Oculus platform was not initialized.");
        return false;
    }

    public override bool GetLeaderboardUsers()
    {
        Request<LeaderboardEntryList> entryRequest = Leaderboards.GetEntries(_leaderboardName, _maxLeaderboardEntries,
            LeaderboardFilterType.UserIds, LeaderboardStartAt.Top);

        if (entryRequest != null)
        {
            entryRequest.OnComplete(OnViveportLeaderboardDownloaded);
            return true;
        }

        Debug.LogError("Unable to get leaderboard friends, the core Oculus platform was not initialized.");
        return false;
    }

    public void OnViveportLeaderboardDownloaded(Message<LeaderboardEntryList> msg)
    {
        if (!msg.IsError)
        {
            _leaderboardEntries.Clear();
            foreach (Oculus.Platform.Models.LeaderboardEntry entry in msg.Data)
            {
                _leaderboardEntries[entry.Rank] = new LeaderboardEntry(entry.User.DisplayName, entry.Rank, (int)entry.Score);
            }
            LeaderboardEntry[] entries = new LeaderboardEntry[msg.Data.Count];
            _leaderboardEntries.Values.CopyTo(entries, 0);
            InvokeOnLeaderboardDownloaded(entries);
        }
    }

    private void GetOwnLeaderboardEntry()
    {
        if (!PlayerIdManager.GetInstance(out var playerIdManager))
            return;

        ulong.TryParse(playerIdManager.GetUserId(), out ulong userID);
        ulong[] tmp = { userID };

        if (!Oculus.Platform.Core.IsInitialized())
        {
            Debug.LogError("Unable to get own leaderboard entry, the oculus platform has not been initialized.");
            return;
        }

        Leaderboards.GetEntriesByIds(_leaderboardName, _maxLeaderboardEntries, LeaderboardStartAt.CenteredOnViewer, tmp)
            .OnComplete((msg) => 
            {
                if (!msg.IsError && msg.Data.Count > 0)
                {
                    InvokeOnPlayerDataDownloaded(new LeaderboardEntry(msg.Data[0].User.DisplayName, msg.Data[0].Rank, (int)msg.Data[0].Score));
                }
            });
    }

    protected override void UploadRating(string playerName, int roundToInt)
    {
        Leaderboards.WriteEntry(_leaderboardName, (long)roundToInt);
    }


}

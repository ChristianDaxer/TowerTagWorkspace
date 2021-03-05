using System.Linq;
using Photon.Pun;
using TowerTag;

public static class AbortMatchVotingController
{
    private const string AbortMatchText = "VOTE ABORT MATCH";

    public static void Tick()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PlayerManager.Instance.GetAllParticipatingHumanPlayers(out var players, out var playerCount);
        if (players.Count(player => player != null && player.AbortMatchVote) >= playerCount)
        {
            GameManager.Instance.CurrentMatch.StopMatch();
            for (int i = 0; i < playerCount; i++)
                players[i].AbortMatchVote = false;
            GameManager.Instance.TriggerMatchConfigurationOnMaster();
        }
    }

    public static string GetAbortMatchText()
    {
        PlayerManager.Instance.GetAllParticipatingHumanPlayers(out var players, out var playerCount);
        return AbortMatchText + $" ({players.Count(player => player != null && player.AbortMatchVote)}/{playerCount})";
    }
}

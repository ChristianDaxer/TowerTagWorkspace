using Steamworks;

public class Leaderboard
{
    public string Name;
    public SteamLeaderboard_t Handle;

    public Leaderboard(string name)
    {
        Name = name;
    }
}

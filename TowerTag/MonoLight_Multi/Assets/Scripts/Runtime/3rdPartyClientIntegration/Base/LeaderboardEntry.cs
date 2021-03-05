
public struct LeaderboardEntry
{
    public readonly string Name;
    public readonly int Rank;
    public readonly int Score;
    public LeaderboardEntry(string name, int rank, int score)
    {
        Name = name;
        Rank = rank;
        Score = score;
    }
}

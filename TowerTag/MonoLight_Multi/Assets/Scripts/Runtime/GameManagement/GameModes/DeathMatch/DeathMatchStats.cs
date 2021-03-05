using TowerTag;

public class DeathMatchStats : MatchStats {
    public override GameMode GameMode => GameMode.DeathMatch;
    public DeathMatchStats() { }
    public DeathMatchStats(IPlayer[] players, int playerCount) : base(players, playerCount) { }
}
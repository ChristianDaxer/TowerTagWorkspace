using TowerTag;

public class EliminationMatchStats : MatchStats {
    public override GameMode GameMode => GameMode.Elimination;
    public EliminationMatchStats() { }
    public EliminationMatchStats(IPlayer[] players, int playerCount) : base(players, playerCount) { }
}
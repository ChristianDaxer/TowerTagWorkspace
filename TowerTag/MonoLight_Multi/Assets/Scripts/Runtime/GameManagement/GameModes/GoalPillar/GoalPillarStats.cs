using TowerTag;

public class GoalPillarStats : MatchStats {
    public override GameMode GameMode => GameMode.GoalTower;
    public GoalPillarStats() { }
    public GoalPillarStats(IPlayer[] players, int playerCount) : base(players, playerCount) { }
}
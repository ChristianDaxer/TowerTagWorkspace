using JetBrains.Annotations;

public static class MatchConfigurator {
    /// <summary>
    /// Creates a new match (configured with values) from given MatchDescription.
    /// </summary>
    /// <param name="description"> MatchDescription defines match to create.</param>
    /// <param name="mode">Game mode of the match to create</param>
    /// <param name="photonService">Service for network communication</param>
    /// <returns>New Match corresponding to given MatchDescription</returns>
    [CanBeNull]
    public static IMatch CreateMatch(MatchDescription description, GameMode mode, IPhotonService photonService) {
        if (description == null) {
            Debug.LogError("Failed to create match: MatchDescription is null!");
            return null;
        }

        switch (mode) {
            case GameMode.Elimination:
                return new EliminationMatch(description, photonService);
            case GameMode.DeathMatch:
                return new DeathMatch(description, photonService);
            case GameMode.GoalTower:
                return new GoalPillarMatch(description, photonService);
            default:
                Debug.LogError($"Failed to create match: GameMode({description.GameMode}) is not available!");
                return null;
        }
    }

    /// <summary>
    /// Creates a new match (configured with values) from given MatchID.
    /// </summary>
    /// <param name="matchID"> MatchID defines match to create.</param>
    /// <param name="photonService">Service class for network communication</param>
    /// <returns>New Match corresponding to given MatchID</returns>
    [CanBeNull]
    public static IMatch CreateMatch(int matchID, GameMode mode, IPhotonService photonService) {
        return CreateMatch(MatchDescriptionCollection.Singleton.GetMatchDescription(matchID), mode, photonService);
    }
}
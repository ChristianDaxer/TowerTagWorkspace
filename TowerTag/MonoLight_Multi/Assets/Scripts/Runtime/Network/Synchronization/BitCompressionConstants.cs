

// static class to hold constant values for bit compression
// (don't change at runtime!!! (and only if you can ensure that these values are the same on every client))
public static class BitCompressionConstants {
    // initialize values
    // the smaller the values (count or dist(max - min)) the better (smaller) are the compressed streams
    static BitCompressionConstants() {
        // 32 -> 5 bit per string Array to encode size
        PlayerNameMaxLength = 16;

        // network
        RetryJoinRoomTime = TowerTagSettings.Hologate ? 1 : 10;

        // PlayerIDs (-1 -> no Player)
        MinPlayerID = -1;
        MaxPlayerID = int.MaxValue;
        MaxPlayerCount = 8;

        // TeamIDs (Teams + neutral Team (-1))
        MinTeamID = -1;
        MaxTeamID = 6;
        MaxTeamCount = 8;

        // PillarIDs
        MinPillarID = 900000;
        MaxPillarID = int.MaxValue;
        MaxPillarCount = 128;

        MinMatchID = -1;
        MaxMatchID = 62;

        // min/max countdownTimes (before Match or Round)
        MinCountdownTimeInSec = 0;
        MaxCountdownTimeInSec = 10;

        // 0 - 100 rounds
        MinMatchRoundsToPlay = 0;
        MaxMatchRoundsToPlay = 255;

        // Time for a Match to be played: 0 - 20 min -> 1200s
        MinMatchTime = 0;
        MaxMatchTime = 1200;

        // max Points a Team can achieve in a Match
        MaxTeamPoints = 255;
        // max Kills a Player can achieve in a Match
        MaxKillsPerPlayer = 255;
        // max Assists a Player can achieve in a Match
        MaxAssistsPerPlayer = 255;
        // max Deaths a Player can die in a Match
        MaxDeathsPerPlayer = 255;

        // GameModes with immediate respawn after kill (GoalPillarMatch & TeamDeathMatch)
        MaxRespawnTimeoutInSec = 30;

        // upper limit for points your team gets if winning a round
        MaxPointsForWinningARound = 3;

        // used in GoalPillarMatch
        MaxNumberOfGoalPillarsNeededToWinARound = 7;

        // factor to multiply with claiming times of goal pillars (1x to 5x in whole integer steps)
        // change these if you need finer resolution, with this configuration we need 2 bits to serialize
        // (max - min)/resolution -> (5 - 1)/1 = 4 values -> 2 bits
        MinGoalPillarClaimTimeFactor = 1;
        MaxGoalPillarClaimTimeFactor = 5;
        GoalPillarClaimTimeFactorResolution = 1;
    }

    public static int PlayerNameMaxLength { get; }
    // network
    public static int RetryJoinRoomTime { get; }

    // PlayerIDs
    public static int MinPlayerID { get; }
    public static int MaxPlayerID { get; }
    public static int MaxPlayerCount { get; }

    // TeamIDs (Teams + neutral Team)
    public static int MinTeamID { get; }
    public static int MaxTeamID { get; }
    public static int MaxTeamCount { get; }

    #region Match & GameMode-Serialization
    public static int MinMatchID { get; }
    public static int MaxMatchID { get; }

    public static int MinCountdownTimeInSec { get; }
    public static int MaxCountdownTimeInSec { get; }

    public static int MinMatchRoundsToPlay { get; }
    public static int MaxMatchRoundsToPlay { get; }

    public static int MinMatchTime { get; }
    public static int MaxMatchTime { get; }
    public static int MaxRespawnTimeoutInSec { get; }
    public static int MaxPointsForWinningARound { get; }
    public static int MaxNumberOfGoalPillarsNeededToWinARound { get; }

    public static float MinGoalPillarClaimTimeFactor { get; }
    public static float MaxGoalPillarClaimTimeFactor { get; }
    public static float GoalPillarClaimTimeFactorResolution { get; }



    #region GameModeStats
    // max Points a Team can achieve in a Match
    public static int MaxTeamPoints { get; }
    // max Kills a Player can achieve in a Match
    public static int MaxKillsPerPlayer { get; }
    // max Assists a Player can achieve in a Match
    public static int MaxAssistsPerPlayer { get; }
    // max Deaths a Player can die in a Match
    public static int MaxDeathsPerPlayer { get; }

    #endregion
    #endregion

    #region Pillars

    // PillarIDs
    public static int MinPillarID { get; }
    public static int MaxPillarID { get; }
    public static int MaxPillarCount { get; }

    #endregion
}

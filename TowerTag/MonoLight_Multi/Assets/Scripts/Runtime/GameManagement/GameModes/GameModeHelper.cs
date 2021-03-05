
using Photon.Pun;
using System.Collections.Generic;
using TowerTag;
using UnityEngine;

/// <summary>
/// Class with common helper functions used by GameMode classes
/// </summary>
public static class GameModeHelper {

    #region Goal Pillar Modi

    /// <summary>
    /// Mode to decide how much goal pillar a team has to conquer/hold to win the current round.
    /// For use with CalculateNumberOfGoalPillarsNeededToWinARound function
    /// - All:          all goal pillars in scene have to be claimed by a team (ignoring numberOfGoalPillarsToClaimPercentageValue)
    /// - FixedNumber:  numberOfGoalPillarsToClaimPercentageValue will be interpreted as int value (3 means you have to claim/hold 3 goalPillars to win a round)
    /// - Percentage:   numberOfGoalPillarsToClaimPercentageValue will be interpreted as percent of the number of goal pillars in scene
    ///   ([0..1]: 0.67f means you have to claim/hold 2/3 of the goal pillars in the scene)
    /// </summary>
    public enum GoalPillarPercentageValueMode { All, FixedNumber, Percentage }

    /// <summary>
    /// Calculate how much goal pillar a team has to conquer to win a round.
    /// </summary>
    /// <param name="numGoalPillarsInScene">Number of goal Pillars present in scene.</param>
    /// <param name="mode">Mode how to calculate the number of goal pillars a team needs to conquer/hold (see GoalPillarPercentageValueMode enum).</param>
    /// <param name="value">Can be a fixed number [0..numGoalPillarsInScene] or a percentage value [0..1] dependent of the GoalPillarPercentageValueMode. If GoalPillarPercentageValueMode is All this value will be ignored!</param>
    /// <returns>The number of goal pillar a team has to conquer to win a round.</returns>
    public static int CalculateNumberOfGoalPillarsNeededToWinARound(int numGoalPillarsInScene, GoalPillarPercentageValueMode mode, float value) {
        var result = 1;
        switch (mode) {
            case GoalPillarPercentageValueMode.All:
                result = numGoalPillarsInScene;
                break;
            case GoalPillarPercentageValueMode.FixedNumber:
                result = Mathf.FloorToInt(value);
                break;
            case GoalPillarPercentageValueMode.Percentage:
                result = Mathf.FloorToInt(numGoalPillarsInScene * value);
                break;
        }

        // Debug Log
        string valueMode = mode == GoalPillarPercentageValueMode.Percentage ? "percentage" : "number";
        Debug.Log("GameModeHelper.CalculateNumberOfGoalPillarsNeededToWinARound: Number of goal pillars needed to win a round is " + result
                            + " \n - goalPillars in scene: " + numGoalPillarsInScene
                            + " \n - CapturedGoalPillarsToWinARoundMode: " + mode
                            + " \n - " + valueMode + "  of goal pillars to claim: " + value);

        // something went wrong if no GoalPillar has to be claimed to win
        if (result <= 0) {
            Debug.LogError("Invalid number of pillars calculated. Will return 1 instead.");
            return 1;
        }

        return result;
    }

    #endregion

    #region Calculate number of conquered pillars

    // dataStructure to cache pillars owned by a Team (Dict<teamID, numberOfPillars>) (avoid MemoryAllocations in each iteration (Clear instead of new), to avoid GC)
    // only used temporary in CalculateNumberOfPillarsOwnedByTeams
    private static readonly Dictionary<TeamID, int> _pillarsCountedByTeam = new Dictionary<TeamID, int>();
    // dataStructure to cache pillars owned by a Team (Dict<teamID, numberOfPillars>) (avoid MemoryAllocations in each iteration (Clear instead of new), to avoid GC)
    // only used temporary in CalculateNumberOfPillarsOwnedByTeams
    private static readonly Dictionary<TeamID, int> _goalPillarsCountedByTeam = new Dictionary<TeamID, int>();

    /// <summary>
    /// Calculate distribution of Pillars and write them to game stats.
    /// simple & slow implementation -> iterate over all Pillars and simply count Pillars per Team (including SpawnPillars, excluding SpectatorPillars)
    /// </summary>
    /// <param name="scenePillars">Array of Pillars we want to check/count.</param>
    /// <param name="stats">The stats we should write the result to.</param>
    /// <param name="countGoalPillars">If false we count all given pillars per Team and write result to capturedPillars in given stats. If true we also count goalPillars separately and write the result to capturedGoalPillars in stats. </param>
    public static void WritePillarDistributionToGameModeStats_MasterOnly(Pillar[] scenePillars, MatchStats stats, bool countGoalPillars = false) {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (scenePillars == null) {
            Debug.LogError("Cannot write game mode state: scenePillars is null.");
            return;
        }

        // reset cache data structure to hold pillarCount for Teams
        _pillarsCountedByTeam.Clear();
        _goalPillarsCountedByTeam.Clear();

        // count Pillars owned by each Team
        var numberOfScenePillars = 0;
        foreach (Pillar pillar in scenePillars) {
            if (pillar != null && !pillar.IsSpectatorPillar) {
                numberOfScenePillars++;
                if (pillar.OwningTeamID != TeamID.Neutral) {
                    if (_pillarsCountedByTeam.ContainsKey(pillar.OwningTeamID))
                        _pillarsCountedByTeam[pillar.OwningTeamID] = _pillarsCountedByTeam[pillar.OwningTeamID] + 1;
                    else
                        _pillarsCountedByTeam.Add(pillar.OwningTeamID, 1);

                    if (countGoalPillars && pillar.IsGoalPillar) {
                        if (_goalPillarsCountedByTeam.ContainsKey(pillar.OwningTeamID))
                            _goalPillarsCountedByTeam[pillar.OwningTeamID] = _goalPillarsCountedByTeam[pillar.OwningTeamID] + 1;
                        else
                            _goalPillarsCountedByTeam.Add(pillar.OwningTeamID, 1);
                    }
                }
            }
        }

        // Add PillarCount to Stats for each Team (Team -1 is mapped to number of Pillars in Scene)
        foreach (TeamID key in _pillarsCountedByTeam.Keys) {
            stats.SetNumberOfCapturedPillarsForTeam(key, _pillarsCountedByTeam[key]);
        }

        // add numberOfScenePillars to stats
        stats.SetNumberOfCapturedPillarsForTeam(TeamID.Neutral, numberOfScenePillars);

        if (countGoalPillars) {
            // Add goalPillarCount to Stats for each Team (Team -1 is mapped to number of Pillars in Scene)
            foreach (TeamID key in _goalPillarsCountedByTeam.Keys) {
                stats.SetNumberOfCapturedPillarsForTeam(key, _goalPillarsCountedByTeam[key], true);
            }

            // add numberOfGoalPillarsInScene to stats
            stats.SetNumberOfCapturedPillarsForTeam(TeamID.Neutral, PillarManager.Instance.GetNumberOfGoalPillarsInScene(), true);
        }
    }
    #endregion

    #region Cleanup
    /// <summary>
    /// Cleanup (temporary) cached data
    /// </summary>
    public static void Cleanup() {
        _pillarsCountedByTeam?.Clear();
        _goalPillarsCountedByTeam?.Clear();
    }

    #endregion
}

using System;
using System.Collections.Generic;
using System.Linq;
using TowerTag;
using UnityEngine;

namespace Commendations {
    public interface IPerformanceBasedCommendation : ICommendation {
        int GetBestPlayer(IMatchStats stats);
        bool InGameModeDisabled(GameMode gameMode);
    }

    /// <summary>
    /// A commendation that is awarded based on the performance of the players.
    /// The performance is determined from the match statistics.
    ///
    /// <author>Ole Jürgensen (ole@vrnerds.de)</author>
    /// </summary>
    public abstract class PerformanceBasedCommendation : Commendation, IPerformanceBasedCommendation {
        [SerializeField,
         Tooltip("The value of the best player must be larger than that of the second one by this amount")]
        private float _minimumLead = 0.01f;

        /// <summary>
        /// Determine the player that is awarded this commendation, based on the match statistics.
        /// </summary>
        /// <param name="stats"></param>
        /// <returns></returns>
        public int GetBestPlayer(IMatchStats stats) {
            List<PlayerStats> orderedStats;
            try {
                orderedStats = stats.GetPlayerStats().Values
                    .Where(playerStats => TeamEnabled(playerStats.TeamID))
                    .OrderByDescending(Performance).ToList();
            }
            catch (Exception e) {
                Debug.LogWarning($"Failed to determine best player for commendation {name}: {e}");
                return -1;
            }

            if (orderedStats.Count == 0)
                return -1;
            if (orderedStats.Count > 1 && Performance(orderedStats[0]) - Performance(orderedStats[1]) < _minimumLead) {
                return -1;
            }

            return orderedStats[0].PlayerID;
        }

        /// <summary>
        /// Check if Commendation is disabled for current Game Mode.
        /// </summary>
        /// <param name="gameMode">Current GameMode</param>
        /// <returns></returns>
        public bool InGameModeDisabled(GameMode gameMode) {
            if (DisabledGameModes == null || DisabledGameModes.Length <= 0)
                return false;
            return DisabledGameModes.Any(disabledGameMode => disabledGameMode == gameMode);
        }

        /// <summary>
        /// Compare Commendations enabled Team Id(s) with Player Team
        /// </summary>
        /// <param name="teamId">Player Team Id</param>
        /// <returns></returns>
        private bool TeamEnabled(TeamID teamId) {
            if (TeamID == TeamID.Neutral)
                return true;
            return teamId == TeamID;
        }

        /// <summary>
        /// The commendation is awarded to the player with the highest performance returned by this function.
        /// </summary>
        /// <param name="playerStats">The player statistic for the past match</param>
        /// <returns>A value measuring the performance for this commendation</returns>
        protected abstract float Performance(PlayerStats playerStats);
    }
}
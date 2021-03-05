using System.Collections.Generic;
using System.Linq;
using TowerTag;

namespace Commendations {
    public class CommendationService : ICommendationService {
        private IPlayerManager _playerManager;

        public void SetPlayerManager(IPlayerManager playerManager) {
            _playerManager = playerManager;
        }

        public Dictionary<IPlayer, (ICommendation, int)> AwardCommendations(
            IPerformanceBasedCommendation[] commendations,
            IPlayer[] players,
            int playerCount,
            IMatchStats matchStats,
            ICommendation defaultCommendation = null) 
        {

            var awardedCommendations = new Dictionary<IPlayer, (ICommendation commendation, int place)>();
            var awardedPlayers = new List<IPlayer>();
            var place = 0;

            foreach (IPerformanceBasedCommendation commendation in commendations) {

                // stop when all players are awarded with some commendation
                if (awardedPlayers.Count >= playerCount)
                    break;

                //Check disabled GameModes for Commendation
                if (commendation.InGameModeDisabled(matchStats.GameMode))
                    continue; //Commendation disabled in current GameMode

                // award player
                int playerId = commendation.GetBestPlayer(matchStats);
                if (playerId == -1)
                    continue;

                IPlayer player = _playerManager.GetPlayer(playerId);
                if (player == null || !player.IsParticipating)
                    continue;

                if (awardedPlayers.Contains(player))
                    continue; // player was already awarded a commendation

                awardedCommendations[player] = (commendation, place++);
                awardedPlayers.Add(player);
            }

            // place un-awarded players
            if (defaultCommendation != null) {
                players
                    .Where(player => player != null && !awardedPlayers.Contains(player))
                    .ToList()
                    .ForEach(player => {
                        awardedCommendations[player] = (defaultCommendation, place++);
                    });
            }

            return awardedCommendations;
        }
    }
}
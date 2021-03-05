using System.Collections.Generic;
using TowerTag;

namespace Commendations {
    public interface ICommendationService {
        void SetPlayerManager(IPlayerManager playerManager);
        Dictionary<IPlayer, (ICommendation, int)> AwardCommendations(
            IPerformanceBasedCommendation[] commendations,
            IPlayer[] players,
            int playerCount,
            IMatchStats matchStats,
            ICommendation defaultCommendation = null);
    }
}
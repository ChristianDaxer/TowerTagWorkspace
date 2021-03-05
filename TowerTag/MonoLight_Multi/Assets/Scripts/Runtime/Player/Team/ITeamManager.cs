using JetBrains.Annotations;

namespace TowerTag {
    public interface ITeamManager {
        [NotNull]
        ITeam TeamNeutral { get; }

        [NotNull]
        ITeam TeamFire { get; }

        [NotNull]
        ITeam TeamIce { get; }

        ITeam Get(TeamID id);
    }
}
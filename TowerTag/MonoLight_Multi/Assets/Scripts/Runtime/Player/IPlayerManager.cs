using JetBrains.Annotations;

namespace TowerTag {
    public delegate void PlayerDelegate([NotNull] IPlayer player);

    public interface IPlayerManager {
        event PlayerDelegate PlayerAdded;
        event PlayerDelegate PlayerRemoved;

        /// <summary>
        /// Registers the given player.
        /// </summary>
        void AddPlayer([NotNull] IPlayer player);

        /// <summary>
        /// Returns the local <see cref="IPlayer"/>. Can be null, e.g., for operator.
        /// </summary>
        [CanBeNull]
        IPlayer GetOwnPlayer();

        /// <summary>
        /// Retrieve the player with the given id.
        /// </summary>
        [CanBeNull]
        IPlayer GetPlayer(int playerID);

        /// <summary>
        /// Remove the player with the given id. Also delete the associated gameObject.
        /// </summary>
        void RemovePlayer(int playerID);

        /// <summary>
        /// Remove the given player from the manager. Also delete the associated gameObject.
        /// </summary>
        void RemovePlayer([NotNull] IPlayer player);

        /// <summary>
        /// Get all Players, no separation in participating or not
        /// </summary>
        /// <returns></returns>
        [NotNull]
        void GetAllConnectedPlayers(out IPlayer[] players, out int count);

        /// <summary>
        /// Get all bots
        /// </summary>
        /// <returns></returns>
        [NotNull]
        void GetAllAIPlayers(out IPlayer[] players, out int count);

        /// <summary>
        /// Returns all registered players.
        /// </summary>
        [NotNull]
        void GetParticipatingPlayers(out IPlayer[] players, out int count);

        /// <summary>
        /// Returns all participating players.
        /// </summary>
        [NotNull]
        void GetSpectatingPlayers(out IPlayer[] players, out int count);
    }
}
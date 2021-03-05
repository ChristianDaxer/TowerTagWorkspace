using ExitGames.Client.Photon;

namespace Photon.Realtime {
    public interface IPlayer {
        /// <summary>Identifier of this player in current room. Also known as: actorNumber or actorNumber. It's -1 outside of rooms.</summary>
        /// <remarks>The ID is assigned per room and only valid in that context. It will change even on leave and re-join. IDs are never re-used per room.</remarks>
        int ActorNumber { get; }

        /// <summary>Non-unique nickname of this player. Synced automatically in a room.</summary>
        /// <remarks>
        /// A player might change his own playername in a room (it's only a property).
        /// Setting this value updates the server and other players (using an operation).
        /// </remarks>
        string NickName { get; set; }

        /// <summary>UserId of the player, available when the room got created with RoomOptions.PublishUserId = true.</summary>
        /// <remarks>Useful for PhotonNetwork.FindFriends and blocking slots in a room for expected players (e.g. in PhotonNetwork.CreateRoom).</remarks>
        string UserId { get; }

        /// <summary>
        /// True if this player is the Master Client of the current room.
        /// </summary>
        /// <remarks>
        /// See also: PhotonNetwork.MasterClient.
        /// </remarks>
        bool IsMasterClient { get; }

        /// <summary>If this player is active in the room (and getting events which are currently being sent).</summary>
        /// <remarks>
        /// Inactive players keep their spot in a room but otherwise behave as if offline (no matter what their actual connection status is).
        /// The room needs a PlayerTTL != 0. If a player is inactive for longer than PlayerTTL, the server will remove this player from the room.
        /// For a client "rejoining" a room, is the same as joining it: It gets properties, cached events and then the live events.
        /// </remarks>
        bool IsInactive { get; }

        /// <summary>Read-only cache for custom properties of player. Set via Player.SetCustomProperties.</summary>
        /// <remarks>
        /// Don't modify the content of this Hashtable. Use SetCustomProperties and the
        /// properties of this class to modify values. When you use those, the client will
        /// sync values with the server.
        /// </remarks>
        /// <see cref="SetCustomProperties"/>
        Hashtable CustomProperties { get; set; }

        /// <summary>
        /// Get a Player by ActorNumber (Player.ID).
        /// </summary>
        /// <param name="id">ActorNumber of the a player in this room.</param>
        /// <returns>Player or null.</returns>
        Player Get(int id);

        /// <summary>Gets this Player's next Player, as sorted by ActorNumber (Player.ID). Wraps around.</summary>
        /// <returns>Player or null.</returns>
        Player GetNext();

        /// <summary>Gets a Player's next Player, as sorted by ActorNumber (Player.ID). Wraps around.</summary>
        /// <remarks>Useful when you pass something to the next player. For example: passing the turn to the next player.</remarks>
        /// <param name="currentPlayer">The Player for which the next is being needed.</param>
        /// <returns>Player or null.</returns>
        Player GetNextFor(Player currentPlayer);

        /// <summary>Gets a Player's next Player, as sorted by ActorNumber (Player.ID). Wraps around.</summary>
        /// <remarks>Useful when you pass something to the next player. For example: passing the turn to the next player.</remarks>
        /// <param name="currentPlayerId">The ActorNumber (Player.ID) for which the next is being needed.</param>
        /// <returns>Player or null.</returns>
        Player GetNextFor(int currentPlayerId);

        /// <summary>Caches properties for new Players or when updates of remote players are received. Use SetCustomProperties() for a synced update.</summary>
        /// <remarks>
        /// This only updates the CustomProperties and doesn't send them to the server.
        /// Mostly used when creating new remote players, where the server sends their properties.
        /// </remarks>
        void InternalCacheProperties(Hashtable properties);

        /// <summary>
        /// Updates and synchronizes this Player's Custom Properties. Optionally, expectedProperties can be provided as condition.
        /// </summary>
        /// <remarks>
        /// Custom Properties are a set of string keys and arbitrary values which is synchronized
        /// for the players in a Room. They are available when the client enters the room, as
        /// they are in the response of OpJoin and OpCreate.
        ///
        /// Custom Properties either relate to the (current) Room or a Player (in that Room).
        ///
        /// Both classes locally cache the current key/values and make them available as
        /// property: CustomProperties. This is provided only to read them.
        /// You must use the method SetCustomProperties to set/modify them.
        ///
        /// Any client can set any Custom Properties anytime (when in a room).
        /// It's up to the game logic to organize how they are best used.
        ///
        /// You should call SetCustomProperties only with key/values that are new or changed. This reduces
        /// traffic and performance.
        ///
        /// Unless you define some expectedProperties, setting key/values is always permitted.
        /// In this case, the property-setting client will not receive the new values from the server but
        /// instead update its local cache in SetCustomProperties.
        ///
        /// If you define expectedProperties, the server will skip updates if the server property-cache
        /// does not contain all expectedProperties with the same values.
        /// In this case, the property-setting client will get an update from the server and update it's
        /// cached key/values at about the same time as everyone else.
        ///
        /// The benefit of using expectedProperties can be only one client successfully sets a key from
        /// one known value to another.
        /// As example: Store who owns an item in a Custom Property "ownedBy". It's 0 initally.
        /// When multiple players reach the item, they all attempt to change "ownedBy" from 0 to their
        /// actorNumber. If you use expectedProperties {"ownedBy", 0} as condition, the first player to
        /// take the item will have it (and the others fail to set the ownership).
        ///
        /// Properties get saved with the game state for Turnbased games (which use IsPersistent = true).
        /// </remarks>
        /// <param name="propertiesToSet">Hashtable of Custom Properties to be set. </param>
        /// <param name="expectedValues">If non-null, these are the property-values the server will check as condition for this update.</param>
        /// <param name="webFlags">Defines if this SetCustomProperties-operation gets forwarded to your WebHooks. Client must be in room.</param>
        bool SetCustomProperties(Hashtable propertiesToSet, Hashtable expectedValues = null, WebFlags webFlags = null);
    }
}
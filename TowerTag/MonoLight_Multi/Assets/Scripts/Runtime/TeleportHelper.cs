using TowerTag;
using UnityEngine;

public static class TeleportHelper {
    private static IPhotonService _photonService;

    private static IPhotonService PhotonService =>
        _photonService = _photonService ?? ServiceProvider.Get<IPhotonService>();

    public enum TeleportDurationType {
        Immediate,
        Respawn,
        Teleport
    }

    /// <summary>
    /// Teleports the given Player on a spawnPillar if he isn't already on one and a free spawnPillar can be found.
    /// Call only on Master. Don't use it for teleports requested by clients.
    /// </summary>
    /// <param name="player">The Player who should get teleported.</param>
    /// <param name="teleportDurationType">Type of Teleportation (needed to calculate teleport duration).</param>
    public static void TeleportPlayerOnSpawnPillar(IPlayer player, TeleportDurationType teleportDurationType) {
        if (!PhotonService.IsMasterClient) {
            Debug.LogError("TeleportHelper.TeleportPlayerRequestedByGame: I'm not the MasterClient");
            return;
        }

        if (player == null) {
            Debug.LogError("TeleportHelper.TeleportPlayerOnSpawnPillar: Player is null");
            return;
        }

        PillarManager pillarManager = PillarManager.Instance;

        if (IsPlayerAlreadyOnASpawnPillar(player)) return;

        Pillar spawnPillar = pillarManager.FindSpawnPillarForPlayer(player);

        if (spawnPillar == null)
        {
#if UNITY_EDITOR
            Debug.LogError("Unable to find spawn pillar for player to spawn onto.");
#endif
            return;
        }

        Debug.LogFormat("Found pillar: \"{0}\" for the player to teleport to.", spawnPillar.name);
        TeleportPlayerRequestedByGame(player, spawnPillar, teleportDurationType); 
    }

    /// <summary>
    /// Teleports the given Player to the given Pillar if possible.
    /// Call only on Master. Don't use it for teleports requested by clients.
    /// </summary>
    /// <param name="player">The Player who should get teleported.</param>
    /// <param name="targetPillar">The Pillar we want to teleport to.</param>
    /// <param name="teleportDurationType">Type of Teleportation (needed to calculate teleport duration).</param>
    public static void TeleportPlayerRequestedByGame(
        IPlayer player, Pillar targetPillar, TeleportDurationType teleportDurationType) {
        if (!PhotonService.IsMasterClient) {
            Debug.LogError("TeleportHelper.TeleportPlayerRequestedByGame: I'm not the MasterClient");
            return;
        }

        if (player == null) {
            Debug.LogError("TeleportHelper.TeleportPlayerRequestedByGame: Player is null");
            return;
        }

        if (targetPillar == null) {
            Debug.LogError("TeleportHelper.TeleportPlayerRequestedByGame: Pillar is null");
            return;
        }
        player.TeleportHandler.TriggerTeleportOnMaster(player.CurrentPillar, targetPillar, teleportDurationType, false);
    }

    /// <summary>
    /// Teleports the given Player to the given Pillar if possible.
    /// Call only on Master. Use this only for teleports requested by clients
    /// </summary>
    /// <param name="player">The Player who should get teleported.</param>
    /// <param name="currentPlayerPillarWhenTriggeredTeleport">The Pillar the Player was at when he triggered the teleport request.</param>
    /// <param name="targetPillar">The Pillar we want to teleport to.</param>
    /// <param name="teleportDurationType">Type of Teleportation (needed to calculate teleport duration).</param>
    /// <returns>ID of the Pillar the Player is teleported to (-1 if the teleport request fails).</returns>
    public static void TeleportPlayerRequestedByUser(IPlayer player, Pillar currentPlayerPillarWhenTriggeredTeleport,
        Pillar targetPillar, TeleportDurationType teleportDurationType) {
        if (!PhotonService.IsMasterClient) {
            Debug.LogError("TeleportHelper.TeleportPlayerRequestedByUser: I'm not the MasterClient");
            return;
        }

        if (player == null) {
            Debug.LogError("TeleportHelper.TeleportPlayerRequestedByUser: Player is null");
            return;
        }

        if (player.TeleportHandler == null) {
            Debug.LogError("TeleportHelper.TeleportPlayerRequestedByUser: Players TeleportHandler is null");
            return;
        }

        // if something fails we have to reset the clients predicted teleport (apply old Pillar)
        // if everything is fine we have to sync anyway
        // so either way: force players TeleportHandler to sync
        player.TeleportHandler.TriggerSyncOnNextSerialization();
        player.TeleportHandler.TriggerTeleportOnMaster(
            currentPlayerPillarWhenTriggeredTeleport, targetPillar, teleportDurationType, true);
    }

    /// <summary>
    /// Teleports the given Player to a SpectatorPillar if possible.
    /// Call only on Master.
    /// </summary>
    /// <param name="player">The Player who should get teleported.</param>
    /// <param name="teleportDurationType">Type of Teleportation (needed to calculate teleport duration).</param>
    /// <returns>ID of the Pillar the Player is teleported to (-1 if the teleport request fails).</returns>
    public static void TeleportPlayerToFreeSpectatorPillar(IPlayer player, TeleportDurationType teleportDurationType) {
        Pillar pillar = PillarManager.Instance.FindSpectatorPillarForPlayer(player);
        if (pillar != null) {
            TeleportPlayerRequestedByGame(player, pillar, teleportDurationType);
            return;
        }
        Debug.LogError("TeleportHelper.TeleportPlayer: Could not find a free SpawnPillar.");
    }


    /// <summary>
    /// Convenience function to ease respawns. It teleports player to a spawnPillar (if possible),
    /// resets Players health, sets PlayerState (to immortal, gunDisabled, isInLimbo)
    /// & reactivates the PlayerState after given reactivationTimeout.
    /// Call only on Master. Don't use it for teleports requested by clients.
    /// </summary>
    /// <param name="playerToSpawn">The Player who should get spawned.</param>
    /// <param name="teleportDurationType">Type of Teleportation (needed to calculate teleport duration).</param>
    public static void RespawnPlayerOnSpawnPillar(IPlayer playerToSpawn, TeleportDurationType teleportDurationType) {
        if (playerToSpawn == null) {
            Debug.LogError("TeleportHelper.RespawnPlayerOnPillar: Can't respawn Player, player is null.");
            return;
        }

        TeleportPlayerOnSpawnPillar(playerToSpawn, teleportDurationType);
        playerToSpawn.ResetPlayerHealthOnMaster();
        playerToSpawn.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.Alive);
    }

    /// <summary>
    /// Convenience function to ease respawns. It teleports player to a spawnPillar (if possible),
    /// resets Players health, sets PlayerState (to immortal, gunDisabled, isInLimbo)
    /// & reactivates the PlayerState after given reactivationTimeout.
    /// Call only on Master. Don't use it for teleports requested by clients.
    /// </summary>
    /// <param name="playerToSpawn">The Player who should get spawned.</param>
    /// <param name="spawnPillar">The Pillar we want to spawn on.</param>
    /// <param name="teleportDurationType">Type of Teleportation (needed to calculate teleport duration).</param>
    public static void RespawnPlayerOnPillar(IPlayer playerToSpawn, Pillar spawnPillar,
        TeleportDurationType teleportDurationType) {
        if (playerToSpawn == null) {
            Debug.LogError("TeleportHelper.RespawnPlayerOnPillar: Can't respawn Player, player is null");
            return;
        }

        TeleportPlayerRequestedByGame(playerToSpawn, spawnPillar, teleportDurationType);
        playerToSpawn.ResetPlayerHealthOnMaster();
        playerToSpawn.PlayerStateHandler.SetPlayerStateOnMaster(PlayerState.Dead);
    }

    /// <summary>
    /// Returns if the Players Pillar is a SpawnPillar.
    /// </summary>
    /// <param name="player">Player to check.</param>
    /// <returns>true: if player is on a SpawnPillar, false: else</returns>
    private static bool IsPlayerAlreadyOnASpawnPillar(IPlayer player) {
        if (player == null) {
            Debug.LogError("TeleportHelper.IsPlayerAlreadyOnASpawnPillar: Player is null");
            return false;
        }

        Pillar playerPillar = player.CurrentPillar;
        return playerPillar != null &&
               playerPillar.IsSpawnPillar &&
               playerPillar.OwningTeamID == player.TeamID;
    }

    /// <summary>
    /// Calculates time to teleport from players current position to targetPillar for the given type of teleportation.
    /// </summary>
    /// <param name="player">Player to teleport.</param>
    /// <param name="targetPillar">Pillar to teleport to.</param>
    /// <param name="teleportDurationType">Type of Teleportation.</param>
    /// <returns>Time needed for teleportation.</returns>
    public static float CalculateTeleportDuration(
        IPlayer player, Pillar targetPillar, TeleportDurationType teleportDurationType) {
        if (player == null || targetPillar == null)
            return 0f;

        float teleportSpeed = BalancingConfiguration.Singleton.TeleportSpeed;
        float respawnTeleportSpeed = BalancingConfiguration.Singleton.RespawnTeleportSpeed;
        float maxTeleportDuration = BalancingConfiguration.Singleton.MaxTeleportDuration;

        float teleportDuration;

        switch (teleportDurationType) {
            case TeleportDurationType.Immediate:
                teleportDuration = 0f;
                break;
            case TeleportDurationType.Teleport:
                teleportDuration = teleportSpeed > 0
                    ? Vector3.Distance(
                          player.PlayerAvatar.TeleportTransform.position,
                          targetPillar.TeleportTransform.position) / teleportSpeed
                    : 0f;
                break;
            case TeleportDurationType.Respawn:
                teleportDuration = respawnTeleportSpeed > 0
                    ? Vector3.Distance(
                          player.PlayerAvatar.TeleportTransform.position,
                          targetPillar.TeleportTransform.position) / respawnTeleportSpeed
                    : 0f;
                break;
            default:
                teleportDuration = 0f;
                break;
        }

        teleportDuration = Mathf.Min(teleportDuration, maxTeleportDuration);
        return teleportDuration;
    }
}
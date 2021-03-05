using Photon.Realtime;
using PopUpMenu;
using TowerTag;
using TowerTagSOES;
using UnityEngine.SceneManagement;
using PHashtable = ExitGames.Client.Photon.Hashtable;
using Player = Photon.Realtime.Player;

public class GameStateToRoomPropertySerializer : IInRoomCallbacks {
    private const string SceneNameKey = "SN";
    private const string TeamFireNameKey = "FN";
    private const string TeamIceNameKey = "IN";

    private bool _blockIncomingMessages = true;
    private IPhotonService _photonService;

    ~GameStateToRoomPropertySerializer() {
        _photonService.RemoveCallbackTarget(this);
    }

    /// <summary>
    /// Initialize internal GameState with data from Server.
    /// </summary>
    /// <param name="photonService"></param>
    public void Init(IPhotonService photonService) {
        _photonService = photonService;
        _photonService.AddCallbackTarget(this);
        _blockIncomingMessages = false;

        if (!photonService.IsConnectedAndReady) {
            Debug.LogError("Cannot initialize room property serializer: Not connected");
            return;
        }

        if (photonService.CurrentRoom?.CustomProperties == null) {
            Debug.LogError("Cannot initialize room property serializer: Room or properties null");
            return;
        }

        GameManagerStateSyncHelper.InitGameManagerStateMachineFromRoomProps(
            photonService.CurrentRoom.CustomProperties);
        OnRoomPropertiesUpdate(photonService.CurrentRoom.CustomProperties);
    }

    /// <summary>
    /// Serialize internal GameState (on Master client) and send it to RoomProperties (on Server) so remote clients get informed (by OnRoomPropertiesChanged(hashTable) callback).
    /// Should only be called from Master client (is ignored on remote clients).
    /// </summary>
    public void SendStateFromMasterClient() {
        if (_photonService == null
            || !_photonService.IsConnectedAndReady
            || !_photonService.IsMasterClient
            || _photonService.CurrentRoom == null) {
            return;
        }

        var newRoomProperties = new PHashtable {
            [SceneNameKey] = SceneManager.GetActiveScene().name,
            [TeamFireNameKey] = TeamManager.Singleton.TeamFire.Name,
            [TeamIceNameKey] = TeamManager.Singleton.TeamIce.Name,
            [RoomPropertyKeys.HostPing] = _photonService.RoundTripTime / 2,
        };

        if (SharedControllerType.IsAdmin) {
            newRoomProperties.Add(RoomPropertyKeys.GameMode, AdminController.Instance.CurrentGameMode);
            newRoomProperties.Add(RoomPropertyKeys.AllowTeamChange, AdminController.Instance.AllowTeamChange);
            newRoomProperties.Add(RoomPropertyKeys.UserVote, AdminController.Instance.UserVote);
        }

        // save scene loaded on Master

        // Write TeamManagerState

        // Write Pillar State
        PillarManagerStateSyncHelper.WriteStateToHashtable(newRoomProperties);

        // write GameManagerState & Match
        GameManagerStateSyncHelper.WriteGameStateToHashtable(newRoomProperties);

        // set roomProperties
        _photonService.CurrentRoom.SetCustomProperties(newRoomProperties);

        // update playerProperties (Player-objects have their own receive functions)
        PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
        for (int i = 0; i < count; i++)
            players[i].UpdatePlayerProperties();
    }


    #region Room Callbacks

    public void OnPlayerEnteredRoom(Player newPlayer) {
    }

    public void OnPlayerLeftRoom(Player otherPlayer) {
    }

    public void OnRoomPropertiesUpdate(PHashtable changedProperties) {
        if (_blockIncomingMessages)
            return;

        if (_photonService.IsMasterClient)
            return;

        if (changedProperties == null) {
            Debug.LogError("Failed tp update room properties: changedProperties (PhotonHashtable) is null.");
            return;
        }

        if (changedProperties.ContainsKey(TeamFireNameKey))
            TeamManager.Singleton.TeamFire.SetName((string) changedProperties[TeamFireNameKey]);
        if (changedProperties.ContainsKey(TeamIceNameKey))
            TeamManager.Singleton.TeamIce.SetName((string) changedProperties[TeamIceNameKey]);

        // Update Pillars
        // -> update only if we are in the same scene as the admin
        if (_photonService.CurrentRoom?.CustomProperties != null
            && _photonService.CurrentRoom.CustomProperties.ContainsKey(SceneNameKey)) {
            // grab name of the scene currently active on Master client
            var adminSceneName = (string) _photonService.CurrentRoom.CustomProperties[SceneNameKey];

            // grab name of the scene currently active on this client
            Scene clientScene = SceneManager.GetActiveScene();

            // if we are in the same scene as the Master client
            if (clientScene.name.Equals(adminSceneName)) {
                // Read Pillar State
                PillarManagerStateSyncHelper.ApplyStateFromHashtable(changedProperties);
            }
        }

        if (SharedControllerType.IsAdmin) {
            if (!TTSceneManager.Instance.IsInHubScene) return;
            // Update Admin Settings
            if (changedProperties.ContainsKey(RoomPropertyKeys.AllowTeamChange))
                AdminController.Instance.AllowTeamChange = (bool) changedProperties[RoomPropertyKeys.AllowTeamChange];
            if (changedProperties.ContainsKey(RoomPropertyKeys.UserVote))
                AdminController.Instance.UserVote = (bool) changedProperties[RoomPropertyKeys.UserVote];
            if (changedProperties.ContainsKey(RoomPropertyKeys.GameMode))
                AdminController.Instance.CurrentGameMode = (GameMode) changedProperties[RoomPropertyKeys.GameMode];
        }

        // read GameState from Room properties hashtable and apply it to GameManager
        GameManagerStateSyncHelper.ApplyGameStateFromHashtable(changedProperties);
    }

    public void OnPlayerPropertiesUpdate(Player targetPlayer, PHashtable changedProps) {
    }

    public void OnMasterClientSwitched(Player newMasterClient) {
        Debug.Log("Master client switched. The HubScene will be loaded. Players are reset to spawn pillars.");
        if (newMasterClient.IsLocal) {
            if (TowerTagSettings.Home || TowerTagSettings.BasicMode) return;
            if (TTSceneManager.Instance.IsInHubScene) {

                PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
                for (int i = 0; i < count; i++)
                    ResetPlayer.ResetPlayerOnHubLane(players[i]);

                if(GameManager.Instance.MatchCountdownRunning)
                    GameManager.Instance.AbortMatchCountdown();
            }

            GameManager.Instance.TriggerMatchConfigurationOnMaster();
        }
    }

    #endregion
}
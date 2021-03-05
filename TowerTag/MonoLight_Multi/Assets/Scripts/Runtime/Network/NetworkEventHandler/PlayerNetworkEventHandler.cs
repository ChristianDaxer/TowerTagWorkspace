using System.Collections.Generic;
using System.Linq;
using AI;
using Photon.Pun;
using TowerTag;
using UI;
using Hologate;
using UnityEngine;
using static AI.BotBrain;
using static AI.BotVoiceCommandsManager;
using Hub;

[RequireComponent(typeof(IPlayer))]
public class PlayerNetworkEventHandler : MonoBehaviourPun {
    [SerializeField] private bool _useEncryption = true;
    [SerializeField] private MessageQueue _messageQueue;
    [SerializeField] private PillarWallManager _pillarWallManager;

    private GrapplingHookController _grapplingHook;
    private IPlayer _player;

    private void Awake() {
        _player = GetComponent<IPlayer>();
    }

    public void Start() {
        _player.StatusChanged += SendStatusFromOwner;
        if (_player.TeleportHandler != null)
            _player.TeleportHandler.TeleportRequested += SendTeleportRequestToMaster;
        if (!_player.IsMe) {
            photonView.RpcSecure(nameof(CheckStatus), photonView.Owner, false);
        }
    }

    private void OnDestroy() {
        if (_player != null) {
            _player.StatusChanged -= SendStatusFromOwner;

            if (_player.TeleportHandler != null)
                _player.TeleportHandler.TeleportRequested -= SendTeleportRequestToMaster;
            _player = null;
        }
    }

    #region localToMasterSync
    
    public void SendAbortMatchVoted() {
        photonView.RpcSecure(nameof(AbortMatchVotedReceived), RpcTarget.All, _useEncryption);
    }
    
    [PunRPC]
    private void AbortMatchVotedReceived()
    {
        if (ScoreBoardSoundsPlayer.GetInstance(out var instance))
            instance.PlayAbortMatchVotedSound();
    }
    

    public void SendMicInputReceiveChange(bool received) {
        photonView.RpcSecure(nameof(MicInputChangeReceived), PhotonNetwork.MasterClient, _useEncryption, received);
    }

    [PunRPC]
    private void MicInputChangeReceived(bool receiving) {
        _player.ReceivingMicInput = receiving;
    }

    [PunRPC]
    private void CheckStatus(PhotonMessageInfo info) {
        photonView.RpcSecure(nameof(OnReceivedPlayerStatusFromOwner), info.Sender, _useEncryption,
            _player.Status.StatusText);
    }

    // send Team To Master
    public void SendTeamChangeRequest(TeamID teamID) {
        if (_player.IsMe)
            photonView.RpcSecure(nameof(RequestTeamChange), RpcTarget.MasterClient, _useEncryption, (int) teamID);
    }

    [PunRPC]
    private void RequestTeamChange(int teamID) {
        AdminController.Instance.RequestTeamChange(_player, (TeamID) teamID);
    }

    public void SendGrapplingHookUpdate(bool direction) {
        photonView.RpcSecure(nameof(OnGrapplingHookUpdateReceived), RpcTarget.Others, _useEncryption, direction);
    }

    [PunRPC]
    private void OnGrapplingHookUpdateReceived(bool direction) {
        if (_grapplingHook == null && _player != null)
            _grapplingHook = _player.GameObject.CheckForNull()?.GetComponentInChildren<GrapplingHookController>();

        if (_grapplingHook != null)
            _grapplingHook.TriggerGrapplingAnimation(direction);
    }

    //send playerStatus to Master
    private void SendStatusFromOwner(Status playerStatus) {
        if (_player.IsLocal)
            photonView.RpcSecure(nameof(OnReceivedPlayerStatusFromOwner), RpcTarget.MasterClient, _useEncryption,
                playerStatus.StatusText);
    }

    [PunRPC]
    private void OnReceivedPlayerStatusFromOwner(string statusText) {
        _player.SetPlayerStatusOnMaster(statusText);
    }


    // send Teleport request to Master
    private void SendTeleportRequestToMaster(Pillar currentPillar, Pillar targetPillar, int timestamp) {
        if (currentPillar == null) {
            Debug.LogError("PlayerNetworkEventHandler.SendTeleportRequestToMaster: currentPillar is null!");
            return;
        }

        if (targetPillar == null) {
            Debug.LogError("PlayerNetworkEventHandler.SendTeleportRequestToMaster: targetPillar is null!");
            return;
        }

        photonView.RpcSecure(nameof(TeleportRequestReceivedOnMaster), RpcTarget.MasterClient, _useEncryption,
            currentPillar.ID, targetPillar.ID, timestamp);
    }

    [PunRPC]
    // ReSharper disable once UnusedParameter.Local
    private void TeleportRequestReceivedOnMaster(int currentPillarID, int targetPillarID, int timestamp) {
        if (!PhotonNetwork.IsMasterClient) {
            Debug.LogWarning(
                "PlayerNetworkEventHandler: TeleportRequestReceivedOnMaster was called on a regular client (not Master client)!");
            return;
        }

        Pillar targetPillar = PillarManager.Instance.GetPillarByID(targetPillarID);
        if (targetPillar == null) {
            Debug.LogWarning("Received Teleport request on master, but target pillar is null");
            return;
        }

        TeleportHelper.TeleportPlayerRequestedByUser(_player, PillarManager.Instance.GetPillarByID(currentPillarID),
            targetPillar, TeleportHelper.TeleportDurationType.Teleport);
    }

    public void SendTimerActivation(int startTimeStamp, Match.CountdownType countdownType) {
        if (!PhotonNetwork.IsMasterClient)
            return;

        photonView.RpcSecure(nameof(TimerActivationReceived), _player.PhotonView.Owner, _useEncryption, startTimeStamp,
            (int) countdownType);
    }

    [PunRPC]
    private void TimerActivationReceived(int startTimeStamp, int countdownType) {
        _player.StartCountdown(startTimeStamp, countdownType);
    }

    #endregion

    #region Player Management
    // force player to disconnect
    public void SendDisconnectPlayer(string message = null) {
        if (PhotonNetwork.IsMasterClient) {
            if (_player.IsBot) {
                PhotonNetwork.Destroy(_player.GameObject);
            }
            else {
                photonView.RpcSecure(nameof(DisconnectLocalPlayer), photonView.Owner, _useEncryption, message);
            }
        }
    }

    /// <summary>
    /// Local forced disconnect behaviour
    /// </summary>
    [PunRPC]
    private void DisconnectLocalPlayer(string message) {
        ConnectionManager.Instance.Disconnect();
        if (message != null)
            _messageQueue.AddErrorMessage(message, "Disconnected by Server");

        TTSceneManager.Instance.LoadConnectScene(true);
    }


    public void BroadcastPlayedMatch(string id) {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RpcSecure(nameof(OnPlayedMatchReceived), RpcTarget.Others, true, id);
    }

    [PunRPC]
    private void OnPlayedMatchReceived(string id) {
        HoloPopUp.OnPlayerMatchReceived.Invoke(id);
    }

    [PunRPC]
    private void OnReceiveWallState(Dictionary<string, float> wallDamage) {
        wallDamage
            .Select(kv => (wall: _pillarWallManager.GetPillarWall(kv.Key), damage: kv.Value))
            .Where(tuple => tuple.wall != null)
            .ForEach(tuple => tuple.wall.SetDamage(tuple.damage));
    }
    
    [PunRPC]
    public void SendMemberID(string memberID) {
        if (_player.IsMe)
            photonView.RpcSecure(nameof(OnMemberIDReceived), RpcTarget.All, true, memberID);
    }

    [PunRPC]
    private void OnMemberIDReceived(string memberID) {
        if (!_player.IsMe)
            _player.LogIn(memberID);
    }

    #endregion
    
    #region BotManagement
    /// <summary>
    /// Updates the AI Parameters for the Bot
    /// </summary>
    public void UpdateAIParameters(BotDifficulty difficulty) {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (_player.IsBot) {
            photonView.RpcSecure(nameof(OnUpdateAIParameters), _player.PhotonView.Owner, true, difficulty);
        }
    }

    [PunRPC]
    private void OnUpdateAIParameters(BotDifficulty difficulty) {
        if (_player != null && _player.IsBot) {
            _player.BotDifficulty = difficulty;
            _player.GameObject.CheckForNull()?.GetComponentInChildren<BotBrain>().SetAIParameters(difficulty);
        }
    }
    
    /// <summary>
    /// Issue voice command to the Bots in own team
    /// </summary>
    public void BroadcastBotVoiceCommand(TeamID teamID, VoiceCommands command) {
        if (PhotonNetwork.IsMasterClient)
            return;

        photonView.RpcSecure(nameof(OnReceiveVoiceCommand), RpcTarget.Others, true, (int) teamID, command);
    }

    /// <summary>
    /// Issue command to all alive Bots in own team
    /// </summary>
    [PunRPC]
    private void OnReceiveVoiceCommand(int teamID, VoiceCommands command) {
        if (!_player.IsBot || _player.TeamID != (TeamID) teamID || !_player.PlayerHealth.IsAlive) return;
        var botBrain = _player.GameObject.CheckForNull()?.GetComponentInChildren<BotBrain>();
        if (botBrain == null) return;
        botBrain.RunCommand(command);
    }
    #endregion

    #region HubScene Timer and Shield
    
    // hub shield
    public void ToggleHubShield(int playerId, bool state) {
        IPlayer player = PlayerManager.Instance.GetPlayer(playerId);

        if (player != null) {
            //Debug.LogError($"We are toggling our shield to {state}!");

            if (player.CurrentPillar != null) player.CurrentPillar.transform.parent.GetComponent<HubLaneControllerHome>().SetShieldActive(state, player);
            photonView.RpcSecure(nameof(OnHubShieldToggleReceived), RpcTarget.Others, true, playerId, state);
        }
    }

    [PunRPC]
    private void OnHubShieldToggleReceived(int playerId, bool state, PhotonMessageInfo info) {
        IPlayer player = PlayerManager.Instance.GetPlayer(playerId);

        //Debug.LogError($"{info.Sender.UserId} is sharing his shield state with us ({player.CurrentPillar.transform.parent.GetComponent<HubLaneControllerHome>().IsShieldActive})!");

        //Debug.LogError($"Setting shield state of {info.Sender.UserId} to {state}!");
        if (player?.CurrentPillar != null) player.CurrentPillar.transform.parent.GetComponent<HubLaneControllerHome>().SetShieldActive(state, player);
    }

    public void RequestToggledShields() {
        //Debug.LogError($"We are requesting the shield states of all the other players!");
        photonView.RpcSecure(nameof(OnJoinHubToggleShield), RpcTarget.Others, true); // others to all
    }

    [PunRPC]
    private void OnJoinHubToggleShield(PhotonMessageInfo info) {
        IPlayer player = PlayerManager.Instance.GetOwnPlayer();

        if (player != null) {
            if (player.CurrentPillar == null) return;
            //Debug.LogError($"{info.Sender.UserId} is asking for my shield state ({player.CurrentPillar.transform.parent.GetComponent<HubLaneControllerHome>().IsShieldActive})! Sending!");
            photonView.RpcSecure(nameof(OnHubShieldToggleReceived), info.Sender, true, player.PlayerID,
                player.CurrentPillar.transform.parent.GetComponent<HubLaneControllerHome>().IsShieldActive);
        }
    }
    
    public void RequestHubSceneTimer() {
        if (PhotonNetwork.IsMasterClient) return;

        photonView.RpcSecure(nameof(OnHubSceneTimerRequested), RpcTarget.MasterClient, true);
    }

    [PunRPC]
    private void OnHubSceneTimerRequested(PhotonMessageInfo info) {
        photonView.RpcSecure(nameof(OnHubSceneTimerTimeReceived), info.Sender, true, QueueTimerManager.GetTime());
    }

    [PunRPC]
    private void OnHubSceneTimerTimeReceived(float time) {
        QueueTimerManager.OverrideTimer(time);
        QueueTimerManager.StartQueueTimer();
    }

    public void SendPillarWallState() {
        List<PillarWall> damagedWalls = _pillarWallManager.GetAllWalls()
            .Where(wall => wall.Damage > 0)
            .ToList();
        Dictionary<string, float> wallDamage = damagedWalls.ToDictionary(wall => wall.ID, wall => wall.Damage);
        photonView.RpcSecure(nameof(OnReceiveWallState), _player.PhotonView.Owner, true, wallDamage);
    }
    
    #endregion

    #region Hologate
    public void RequestDeviceID() {
        if (!PhotonNetwork.IsMasterClient)
            return;
        photonView.RpcSecure(nameof(OnDeviceRequestReceived), _player.PhotonView.Owner, true);
    }

    [PunRPC]
    private void OnDeviceRequestReceived() {
        if (PhotonNetwork.IsMasterClient)
            return;
        photonView.RpcSecure(nameof(OnDeviceIDReceived), PhotonNetwork.MasterClient, true, HologateManager.MachineData.Data.ID);
    }

    [PunRPC]
    private void OnDeviceIDReceived(int id) {
        HologateManager.AddPlayerToDeviceList(id, _player);
    }
    
    #endregion

}
using System;
using ExitGames.Client.Photon;
using GameManagement;
using Photon.Realtime;
using TowerTag;
using UnityEngine;

/// <summary>
/// A game action is a set of instructions that can be triggered by a player or the game
/// and that is validated according to the rules of the game and synchronized across the network.
/// Game actions should be used for singular events that have relevance for the game and are subject to certain rules.
/// A game action can be optimistic, so it is executed after local validation without confirmation by the master client.
/// This eliminates latency but comes with the risk of a roll back when the game action is denied by the master client.
/// </summary>
public abstract class GameAction : ScriptableObject, IOnEventCallback {
    protected IPlayerManager PlayerManager { get; private set; }
    protected IPhotonService PhotonService { get; private set; }
    private ISceneService SceneService { get; set; }
    protected IGameManager GameManagerInstance { get; private set; }

    protected bool Initialized { get; set; }

    public void Init(IPhotonService photonService, IGameManager gameManager, IPlayerManager playerManager,
        ISceneService sceneService) {
        PhotonService = photonService;
        SceneService = sceneService;
        PlayerManager = playerManager;
        GameManagerInstance = gameManager;
        PhotonService.AddCallbackTarget(this);
        Initialized = true;
    }

    public abstract void OnEvent(EventData photonEvent);
}

public abstract class GameAction<T> : GameAction where T : GameActionParameter, new() {
    [SerializeField] private GameActionManager _gameActionManager;
    [SerializeField] private bool _optimistic;
    [SerializeField] private SendOptions _sendReliably = SendOptions.SendReliable;
    protected abstract byte EventCode { get; }
    protected abstract byte DenyEventCode { get; }
    private byte ParameterTypeCode => EventCode;

    protected static GameAction<T> Singleton;

    protected void OnEnable() {
        if (_gameActionManager != null)
            _gameActionManager.Register(this, EventCode, DenyEventCode, ParameterTypeCode);

        Debug.LogFormat("OnEnable: {0}", name);
        Singleton = this;
    }

    private void OnDisable() {
        if (_gameActionManager != null)
            _gameActionManager.Unregister(this, EventCode, DenyEventCode, ParameterTypeCode);
        PhotonService?.RemoveCallbackTarget(this);
        Singleton = null;
    }

    public override void OnEvent(EventData photonEvent) {
        OnEventCall(photonEvent.Code, photonEvent.CustomData, photonEvent.Sender);
    }

    private void OnEventCall(byte eventCode, object content, int senderId) {
        if (eventCode != EventCode && eventCode != DenyEventCode)
            return; // not for me

        if (!Initialized) {
            Debug.LogWarning("Received GameAction event call, but GameActions are not initialized!");
            return;
        }

        var parameters = new T();
        parameters.Deserialize((object[]) content);
        if (PhotonService.IsMasterClient) {
            if (eventCode == DenyEventCode) {
                Debug.LogWarning("Received Denial on master!");
                Deny(senderId, parameters);
                return;
            }

            if (IsValid(senderId, parameters)) {
                Execute(senderId, parameters);
                PhotonService.RaiseEvent(EventCode, parameters.Serialize(),
                    new RaiseEventOptions {Receivers = ReceiverGroup.Others}, _sendReliably);
            }
            else {
                PhotonService.RaiseEvent(DenyEventCode, parameters.Serialize(),
                    new RaiseEventOptions {TargetActors = new[] {senderId}}, _sendReliably);
            }
        }
        else {
            if (eventCode == EventCode) {
                if (!_optimistic || parameters.TriggeredBy != PhotonService.LocalPlayer.ActorNumber)
                    Execute(senderId, parameters);
            }
            else if (eventCode == DenyEventCode) {
                if (_optimistic)
                    Rollback(senderId, parameters);
                else
                    Deny(senderId, parameters);
            }
        }
    }

    /// <summary>
    /// Trigger the game action. The game action will be validated and denied if it is invalid.
    /// If it is valid, it is send to the master client for validation and distribution.
    /// If it is also optimistic, it will be executed immediately.
    /// </summary>
    /// <param name="parameters">An object defining the properties of the requested game action.</param>
    protected void Trigger(T parameters) {
        parameters.TriggeredBy = PhotonService.LocalPlayer.ActorNumber;
        if (IsValid(PhotonService.LocalPlayer.ActorNumber, parameters)) {
            if (_optimistic && !PhotonService.IsMasterClient)
                Execute(PhotonService.LocalPlayer.ActorNumber, parameters);
            if (PhotonService.IsMasterClient)
                OnEventCall(EventCode, parameters.Serialize(), parameters.TriggeredBy);
            else
                PhotonService.RaiseEvent(EventCode, parameters.Serialize(),
                    new RaiseEventOptions {Receivers = ReceiverGroup.MasterClient}, _sendReliably);
        }
        else {
            Deny(PhotonService.LocalPlayer.ActorNumber, parameters);
        }
    }

    /// <summary>
    /// Determines the validity of a requested game action and returns true if it it valid and false otherwise.
    /// </summary>
    /// <param name="senderId">The photon owner ID of the player that triggered this action.
    /// A client could potentially manipulate the parameters object,
    /// but not this senderID so this should be used to verify authority.</param>
    /// <param name="parameters">An object defining the properties of the requested game action.</param>
    /// <returns></returns>
    protected abstract bool IsValid(int senderId, T parameters);

    /// <summary>
    /// Executes the game action.
    /// This should trigger all logical and visual effects associated with a valid game action.
    /// </summary>
    /// <param name="senderId">The photon owner ID of the player that triggered this action.</param>
    /// <param name="parameters">An object defining the properties of the requested game action.</param>
    protected abstract void Execute(int senderId, T parameters);

    /// <summary>
    /// Denies the game action.
    /// This should give feedback to the player that the requested action is invalid.
    /// </summary>
    /// <param name="senderId">The photon owner ID of the player that triggered this action.</param>
    /// <param name="parameters">An object defining the properties of the requested game action.</param>
    protected abstract void Deny(int senderId, T parameters);

    /// <summary>
    /// Rolls back a game action that was optimistically executed before, but was then denied by the master client.
    /// This should revert all impacts the execution of the game action caused.
    /// </summary>
    /// <param name="senderId">The photon owner ID of the player that triggered this action.</param>
    /// <param name="parameters">An object defining the properties of the requested game action.</param>
    protected abstract void Rollback(int senderId, T parameters);
}

public abstract class GameActionParameter {
    public int TriggeredBy { get; set; }
    protected abstract object[] SerializeParameters();
    protected abstract void DeserializeParameters(object[] objects);

    public object[] Serialize() {
        object[] parameters = SerializeParameters();
        var result = new object[parameters.Length + 1];
        Array.Copy(parameters, 0, result, 1, parameters.Length);
        result[0] = TriggeredBy;
        return result;
    }

    public void Deserialize(object[] objects) {
        TriggeredBy = (int) objects[0];
        var parameters = new object[objects.Length - 1];
        Array.Copy(objects, 1, parameters, 0, parameters.Length);
        DeserializeParameters(parameters);
    }
}
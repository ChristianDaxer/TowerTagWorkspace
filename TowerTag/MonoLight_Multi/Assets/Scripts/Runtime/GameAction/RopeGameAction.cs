using System;
using Rope;
using System.Linq;
using JetBrains.Annotations;
using TowerTag;
using UnityEngine;

/// <summary>
/// Game action that is triggered when a player tries to connect to or disconnect
/// from a <see cref="Chargeable"/> with the Rope.
/// </summary>
/// <author>Sebastian Krebs (sebastian.krebs@vrnerds.de)</author>
[CreateAssetMenu(menuName = "Game Action/Rope")]
public class RopeGameAction : GameAction<RopeGameAction.Parameter>
{
    private ChargerBeamRenderer _tess;
    protected override byte EventCode => 8;
    protected override byte DenyEventCode => 9;

    public delegate void ChargeableConnect(RopeGameAction sender, IPlayer player, Chargeable pillar);

    public event ChargeableConnect RopeConnectedToChargeable;
    public event ChargeableConnect AttachFailed;

    public delegate void RopeDisconnect(RopeGameAction sender, IPlayer player, Chargeable target, bool onPurpose);

    public event RopeDisconnect Disconnecting;

    public delegate void DenyDelegate(RopeGameAction sender, int senderID, Parameter parameter);

    public event DenyDelegate Denied;

    protected override bool IsValid(int senderId, Parameter parameters)
    {
        IPlayer player = PlayerManager.GetPlayer(parameters.PlayerId);
        if (player == null)
            return false; // player disconnected
        if (!PhotonService.IsMasterClient && senderId != player.OwnerID)
            return false;
        switch (parameters.ActionType)
        {
            case Parameter.RopeActionType.Disconnect:
                return true;
            case Parameter.RopeActionType.Connect:
                if (!player.IsAlive)
                    return false; // dead player cannot connect
                if (GameManagerInstance.MatchTimer.IsPaused || GameManagerInstance.MatchTimer.IsResumingFromPause)
                    return false; // match is paused
                if (player.IsInIngameMenu)
                    return false; // Player is in ingame vr Menu and should not grapple while this time
                if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.LoadMatch)
                    return false;
                if (GameManagerInstance.CurrentState == GameManager.GameManagerStateMachine.State.Countdown)
                    return false;
                if (GameManager.Instance.CurrentState == GameManager.GameManagerStateMachine.State.Commendations)
                    return false; // cannot grapple in commendations scene
                Chargeable target = GetChargeableTarget(parameters);
                if (target == null || !target.CanAttach(player))
                    return false;
                return true;
            case Parameter.RopeActionType.FailedConnectionAttempt:
                if (!player.IsAlive)
                    return false; // dead player cannot connect
                if (player.IsInIngameMenu)
                    return false; // Player is in ingame vr Menu and should not grapple while this time
                if (GameManagerInstance.MatchTimer.IsPaused || GameManagerInstance.MatchTimer.IsResumingFromPause)
                    return false; // match is paused
                if (TTSceneManager.Instance.IsInCommendationsScene)
                    return false; // cannot grapple in commendations scene
                return true;
            default:
                return true;
        }
    }

    protected override void Execute(int senderId, Parameter parameters)
    {
        IPlayer player = PlayerManager.GetPlayer(parameters.PlayerId);
        Chargeable target = GetChargeableTarget(parameters);

        if (target == null || player == null)
            return;

        switch (parameters.ActionType)
        {
            case Parameter.RopeActionType.Connect:
                if (parameters.TargetType == Parameter.RopeTargetType.Undefined)
                {
                    Debug.LogError("Can't connect rope, target type is undefined!");
                    return;
                }

                player.AttachedTo = target;
                target.Attach(player);
                RopeConnectedToChargeable?.Invoke(this, player, target);
                break;
            case Parameter.RopeActionType.Disconnect:
                player.AttachedTo = null;
                target.Detach(player);
                Disconnecting?.Invoke(this, player, target, player.IsMe);
                break;
            case Parameter.RopeActionType.FailedConnectionAttempt:
                AttachFailed?.Invoke(this, player, target);
                break;
        }
    }

    private Chargeable GetChargeableTarget([NotNull] Parameter parameters)
    {
        switch (parameters.TargetType)
        {
            case Parameter.RopeTargetType.Player:
                IPlayer player = PlayerManager.GetPlayer(parameters.TargetId);
                return player?.ChargePlayer;
            case Parameter.RopeTargetType.Pillar:
                return PillarManager.Instance.GetPillarByID(parameters.TargetId);
            case Parameter.RopeTargetType.Option:
                IPlayer playerOnHub = PlayerManager.GetPlayer(parameters.PlayerId);
                if (playerOnHub == null) return null;

                StayLoggedInTrigger loggedInTrigger = playerOnHub.LoggedInTrigger;
                if (loggedInTrigger == null)
                {
                    Debug.LogError("No HubLane or StayLoggedInTrigger found!");
                    return null;
                }

                return loggedInTrigger.Options.FirstOrDefault(option => option.ID == parameters.TargetId);
            case Parameter.RopeTargetType.RotatePlaySpaceHook:
                IPlayer playerTest = PlayerManager.GetPlayer(parameters.PlayerId);
                if (playerTest == null || playerTest.CurrentPillar == null)
                {
                    Debug.LogError("No Player or playspace rotation Hook found!");
                    return null;
                }

                var turningPlateController = playerTest.CurrentPillar.PillarTurningPlateController;
                return turningPlateController.Hooks.FirstOrDefault(hook => hook.ID == parameters.TargetId);
            default:
                Debug.LogError("No Chargeable type found!");
                return null;
        }
    }

    protected override void Deny(int senderId, Parameter parameters)
    {
        IPlayer player = PlayerManager.GetPlayer(parameters.PlayerId);
        if (player != null && !player.IsBot) Debug.LogWarning("Rope Game Action Denied");
        Denied?.Invoke(this, senderId, parameters);
    }

    protected override void Rollback(int senderId, Parameter parameters)
    {
        Chargeable target = GetChargeableTarget(parameters);
        Disconnecting?.Invoke(this, PlayerManager.GetPlayer(parameters.TriggeredBy), target, false);
    }

    public void TryToAttachRopeAndFail([NotNull] Chargeable target, [NotNull] IPlayer player)
    {
        if (target == null || player == null)
        {
            throw new ArgumentException("Need to provide non-null target and player");
        }

        TriggerRopeAction(target, player, Parameter.RopeActionType.FailedConnectionAttempt);
    }

    public void ConnectRope([NotNull] Chargeable target, [NotNull] IPlayer player)
    {
        if (target == null || player == null)
        {
            throw new ArgumentException("Need to provide non-null target and player");
        }

        TriggerRopeAction(target, player, Parameter.RopeActionType.Connect);
    }

    public void DisconnectRope([NotNull] Chargeable target, [NotNull] IPlayer player)
    {
        TriggerRopeAction(target, player, Parameter.RopeActionType.Disconnect);
    }

    private void TriggerRopeAction(Chargeable target, IPlayer player, Parameter.RopeActionType actionType)
    {
        var targetType = Parameter.RopeTargetType.Undefined;

        if (target != null)
        {
            switch (target.ChargeableType)
            {
                case ChargeableType.Player:
                    targetType = Parameter.RopeTargetType.Player;
                    break;
                case ChargeableType.Pillar:
                    targetType = Parameter.RopeTargetType.Pillar;
                    break;
                case ChargeableType.Option:
                    targetType = Parameter.RopeTargetType.Option;
                    break;
                case ChargeableType.RotatePlaySpaceHook:
                    targetType = Parameter.RopeTargetType.RotatePlaySpaceHook;
                    break;
                default:
                    targetType = Parameter.RopeTargetType.Undefined;
                    Debug.LogError("RopeGameAction.RopeAction: Get Unknown TargetType");
                    break;
            }
        }

        if (target != null && player != null) {
            Trigger(new Parameter
            {
                TargetId = target.ID,
                TargetType = targetType,
                PlayerId = player.PlayerID,
                ActionType = actionType
            });
        }
        else
            Debug.LogError("RopeGameAction.TriggerRopeAction: Execute Game Action not allowed because target is null");
    }

    public class Parameter : GameActionParameter
    {
        public int TargetId { get; set; }
        public RopeTargetType TargetType { get; set; }
        public int PlayerId { get; set; }
        public RopeActionType ActionType { get; set; }

        protected override object[] SerializeParameters()
        {
            return new object[] {TargetId, (byte) TargetType, PlayerId, (byte) ActionType};
        }

        protected override void DeserializeParameters(object[] objects)
        {
            TargetId = (int) objects[0];
            TargetType = (RopeTargetType) (byte) objects[1];
            PlayerId = (int) objects[2];
            ActionType = (RopeActionType) (byte) objects[3];
        }

        public enum RopeTargetType
        {
            Player = 0,
            Pillar = 1,
            Option = 2,
            RotatePlaySpaceHook = 3,
            Undefined = 4
        }

        public enum RopeActionType
        {
            Connect = 0,
            Disconnect = 1,
            FailedConnectionAttempt = 2
        }
    }
}
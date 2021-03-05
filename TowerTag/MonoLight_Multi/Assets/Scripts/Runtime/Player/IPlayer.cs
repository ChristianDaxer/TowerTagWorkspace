using System;
using AI;
using ExitGames.Client.Photon;
using JetBrains.Annotations;
using Photon.Pun;
using Runtime.Player;
using UnityEngine;

namespace TowerTag {
    public delegate void PropertyChangedHandler(IPlayer player, bool newValue);

    public interface IPlayer {
        #region properties

        /// <summary>
        /// True iff the player game object still exists.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// The component that identifies this game object across the network.
        /// </summary>
        IPhotonView PhotonView { get; }

        /// <summary>
        /// Number that uniquely identifies this tower-tag player. Do not mistake for the photon owner ID.
        /// </summary>
        int PlayerID { get; }

        /// <summary>
        /// Photon ID of the owner of this player.
        /// A photon user that spawns bots can have multiple tower-tag players that share an owner.
        /// </summary>
        int OwnerID { get; }

        bool IsBot { get; set; }

        /// <summary>
        /// True if this player is taking part in the current match. False if the player is purely spectating.
        /// </summary>
        bool IsParticipating { get; set; }

        /// <summary>
        /// ID of the team that this player is a part of.
        /// </summary>
        TeamID TeamID { get; }

        /// <summary>
        /// True if this player is controlled by the local client. Could be bot that was started locally.
        /// </summary>
        bool IsLocal { get; }

        /// <summary>
        /// True if this is the player associated with the local client, i.e., neither remote nor bot.
        /// </summary>
        bool IsMe { get; }

        /// <summary>
        /// Name as displayed in the game on UI, scoreboards, gun, etc.
        /// </summary>
        string PlayerName { get; }

        /// <summary>
        /// The value that determines the speed of claiming and rapid fire. Between 0 and 1.
        /// </summary>
        float GunEnergy { get; set; }

        /// <summary>
        /// EP in Statistics
        /// </summary>
        int Rank { get; set; }

        /// <summary>
        /// ID that is used to identify the user in the statistics backend.
        /// </summary>
        string MembershipID { get; set; }

        /// <summary>
        /// True if this player has an associated user for capturing statistics in the backend.
        /// </summary>
        bool IsLoggedIn { get; }

        /// <summary>
        /// True if the player has non-zero health.
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// True if the player opened the in-game UI.
        /// </summary>
        bool IsInIngameMenu { get; }

        // ReSharper disable once InconsistentNaming - forced by unity
        [CanBeNull]
        GameObject GameObject { get; }

        /// <summary>
        /// Used to align the VR camera rig to the hierarchy.
        /// </summary>
        AlignmentTarget PlayerAlignmentTarget { get; }

        StayLoggedInTrigger LoggedInTrigger { get; }
        PlayerAvatar PlayerAvatar { get; set; }
        Collider[] GunCollider { get; set; }
        PlayerHealth PlayerHealth { get; }
        RoomOptionsManager RoomOptionsManager { get; }
        GunController GunController { get; }
        ChargePlayer ChargePlayer { get; }
        PlayerNetworkEventHandler PlayerNetworkEventHandler { get; }
        ChargeNetworkEventHandler ChargeNetworkEventHandler { get; }
        bool IsPhotonViewValid { get; }
        TeleportHandler TeleportHandler { get; }
        RotatePlayspaceHandler RotatePlayspaceHandler { get; }

        [CanBeNull]
        Pillar CurrentPillar { get; }

        [NotNull]
        PlayerStateHandler PlayerStateHandler { get; }

        PlayerState PlayerState { get; }
        bool PlayerIsReady { get; set; }
        bool ReceivingMicInput { set; }
        Status Status { get; }
        GameMode VoteGameMode { get; set; }
        bool StartVotum { get; set; }
        bool TeamChangeRequested { get; set; }
        BotBrain.BotDifficulty BotDifficulty { get; set; }
        string SelectedAIParameters { get; set; }
        string DefaultName { get; set; }

        bool HasRopeAttached { get; }

        [CanBeNull]
        Chargeable AttachedTo { get; set; }


        /// <summary>
        /// True if the gun of the player is in the tower
        /// </summary>
        bool IsGunInTower { get; set; }

        bool IsInTower { get; set; }
        bool AbortMatchVote { get; set; }
        bool IsOutOfChaperone { get; set; }
        bool IsLateJoiner { get; set; }

        #endregion

        #region methods

        void Init();
        void InitPlayerFromPlayerProperties();
        void LogIn(string membershipID);
        void LogOut();
        void RestartClient();
        void ResetPlayerHealthOnMaster();
        void ToggleDirectAdminChatOnMaster(bool directChatActive);
        void RequestTeamChange(TeamID teamID);
        void SetName(string newName);
        void InitChaperone(Chaperone chaperone);
        void InitBadaboomHyperactionPointer(BadaboomHyperactionPointer pointer);
        void SetStatus(Status outOfChaperoneStatus);
        void SetTeam(TeamID teamID);
        void UpdatePlayerProperties();
        void UpdateValuesFromPlayerProperties(Hashtable changedProperties);
        void SetPlayerStatusOnMaster(string statusText);
        void StartCountdown(int startTimeStamp, int countdownType);
        void SetRotationOnMaster(Quaternion teleportTransformRotation);
        void ResetButtonStates();

        #endregion

        #region events

        event Action<string> PlayerNameChanged;
        event Action<Status> StatusChanged;
        event Action<IPlayer, TeamID> PlayerTeamChanged;
        event Action<int, int> CountdownStarted;
        event Action<IPlayer, bool> InTowerStateChanged;
        event Action<IPlayer, bool> OutOfChaperoneStateChanged;
        event Action<IPlayer, (GameMode newVote, GameMode previousVote)> GameModeVoted;
        event Action<IPlayer, bool> StartNowVoteChanged;
        event Action<IPlayer, bool> TeamChangeRequestChanged;
        event PropertyChangedHandler ReadyStatusChanged;
        event PropertyChangedHandler ParticipatingStatusChanged;
        event PropertyChangedHandler ReceivingMicInputStatusChanged;

        #endregion
    }
}
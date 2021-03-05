using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public enum ChargeableType {
    Player,
    Pillar,
    Option,
    RotatePlaySpaceHook
}

namespace TowerTag {
    public abstract class Chargeable : MonoBehaviour {
        public abstract ChargeableType ChargeableType { get; }
        public abstract int ID { get; set; }

        [SerializeField] [Tooltip("The anchor point where the rope of the charging player is attached.")]
        private Transform _anchorTransform;

        public Transform AnchorTransform => _anchorTransform;

        [SerializeField,
         Tooltip("Time it takes to charge this with a single player and a gun energy multiplier of one")]
        protected float _timeToCharge = 10;

        protected virtual float TimeToCharge => _timeToCharge;

        [FormerlySerializedAs("_chargeableColliders")] [SerializeField, Tooltip("All chargeable collider linked to this chargeable")]
        private ChargeableCollider[] _chargeableCollider;

        public ChargeableCollider[] ChargeableCollider => _chargeableCollider;

        public List<IPlayer> AttachedPlayers { get; } = new List<IPlayer>();

        private (TeamID teamID, float value) _currentCharge = (teamID: TeamID.Neutral, value: 0);
        protected bool IsBeingCharged => CurrentCharge.value > 0 || CurrentCharge.teamID != TeamID.Neutral;

        protected virtual bool ManageLocally { get; } = false;

        public (TeamID teamID, float value) CurrentCharge {
            get => _currentCharge;
            protected set {
                _currentCharge = value;
                ChargeSet?.Invoke(this, _currentCharge.teamID, _currentCharge.value);
            }
        }

        #region events

        public delegate void ChargeChangeDelegate(Chargeable chargeable, TeamID team, float value);
        public delegate void AttachEvent(Chargeable chargeable, IPlayer player);

        public event ChargeChangeDelegate ChargeSet;
        public event AttachEvent PlayerAttached;
        public event AttachEvent PlayerDetached;

        #endregion

        protected void Awake() {
            ChargeableCollider.ForEach(chargeableCollider => chargeableCollider.Chargeable = this);
        }

        protected void OnEnable() {
            PlayerManager.Instance.PlayerRemoved += Detach;
        }

        protected void OnDisable() {
            PlayerManager.Instance.PlayerRemoved -= Detach;
        }

        protected void Update() {
            ProcessChargeOnManager();
        }

        private void ProcessChargeOnManager() {
            if (!PhotonNetwork.IsMasterClient && !ManageLocally)
                return;

            if (CurrentCharge.value >= 1) FinishChargingOnManager();

            //Slow because ToArray is called each itteration. 
            //// iterate copied array, because finishing healing will auto detach and thereby modify AttachedPlayers list
            //foreach (IPlayer attachedPlayer in AttachedPlayers.ToArray()) {
            //    if(!CanCharge(attachedPlayer)) continue;
            //    ProcessChargeOnManager(attachedPlayer);
            //}
            int max = AttachedPlayers.Count;
            for (int i = 0;i < max; i++)
            {
                IPlayer attachedPlayer = AttachedPlayers[i];
                if (!CanCharge(attachedPlayer)) continue;
                ProcessChargeOnManager(attachedPlayer);
            }
        }

        protected virtual void FinishChargingOnManager() { }

        protected virtual void ProcessChargeOnManager(IPlayer player) {
            CurrentCharge = (player.TeamID,
                CalculateChargeAmount(player));
        }

        private float CalculateChargeAmount(IPlayer player)
        {
            return Mathf.Clamp01(CurrentCharge.value + CalculateChargeChange(player));
        }

        public float CalculateChargeChange(IPlayer player)
        {
            float noGunEnergyMultiplier =
                player.GunEnergy > 0 ? 1 : BalancingConfiguration.Singleton.NoEnergyMultiplier;
            return Time.deltaTime / (TimeToCharge * noGunEnergyMultiplier);
        }

        /// <summary>
        /// Returns whether the given player is allowed to attach his rope to this chargeable.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool CanAttach(IPlayer player);

        /// <summary>
        /// Returns whether the given player can try to attach his rope without any guarantee of success.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool CanTryToAttach(IPlayer player);

        /// <summary>
        /// Returns whether the player can currently charge this chargeable.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool CanCharge(IPlayer player);

        public void Attach(IPlayer player) {
            if (!CanAttach(player)) {
                Debug.LogWarning($"Player {player} cannot start charging {this}");
                return;
            }

            if (AttachedPlayers.Contains(player)) {
                Debug.LogWarning($"Player {player} is already attached {this}");
                return;
            }

            AttachedPlayers.Add(player);

            if (AttachedPlayers.Count == 1 && CanCharge(player)) {
                // start charging
                CurrentCharge = CurrentCharge.teamID == player.TeamID ? CurrentCharge : (player.TeamID, 0);
            }

            PlayerAttached?.Invoke(this, player);
        }

        public void Detach(IPlayer player) {
            if (!AttachedPlayers.Contains(player)) {
//                Debug.LogWarning($"{player} is not charging {this}");
                return;
            }

            AttachedPlayers.Remove(player);
            PlayerDetached?.Invoke(this, player);
        }

        public override string ToString() {
            return $"Chargeable {name} (ID = {ID})";
        }
    }
}
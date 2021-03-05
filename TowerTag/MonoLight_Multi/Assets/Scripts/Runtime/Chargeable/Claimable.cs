using System;
using Photon.Pun;
using UnityEngine;

namespace TowerTag {
    public abstract class Claimable : Chargeable {
        [SerializeField, Tooltip("Time after which the automatic charge fallback sets in")]
        private float _chargeFallbackTimeout;

        protected float ChargeFallbackTimeout {
            private get { return _chargeFallbackTimeout; }
            set { _chargeFallbackTimeout = value; }
        }

        [SerializeField, Tooltip("Amount of charge that is automatically subtracted per " +
                                 "second when nobody is charging for a period of time")]
        private float _chargeFallbackSpeed;

        public float ChargeFallbackSpeed {
            private get { return _chargeFallbackSpeed; }
            set { _chargeFallbackSpeed = value; }
        }

        private float _timeOfLastCharge;

        [SerializeField, Tooltip("Defines the initial owning team of this Chargeable.")]
        private TeamID _owningTeamID;

        [SerializeField]
        [Tooltip("Can this Pillar be claimed? Disabling this prohibits claiming & teleport to this Pillar.")]
        private bool _isClaimable = true;

        public bool IsClaimable {
            get => _isClaimable;
            set {
                if(_isClaimable != value) {
                    _isClaimable = value;
                    ClaimableStatusChanged?.Invoke(this,value);
                }
            }
        }

        [field: SerializeField, Tooltip("Can this Pillar be claimed by enemy Team?")]
        public bool IsTeamBased { get; set; }

        public delegate void OwningTeamChangeDelegate(Claimable claimable, TeamID oldTeam, TeamID newTeam,
            IPlayer[] attachedPlayers);

        public event OwningTeamChangeDelegate OwningTeamChanged;
        public event Action<Claimable, bool> ClaimableStatusChanged;

        public ITeam OwningTeam => TeamManager.Singleton.Get(OwningTeamID);
        public TeamID OwningTeamID {
            get => _owningTeamID;
            set {
                if (value == _owningTeamID) return;
                TeamID oldTeamID = _owningTeamID;
                _owningTeamID = value;
                OwningTeamChanged?.Invoke(this, oldTeamID, _owningTeamID, AttachedPlayers.ToArray());
            }
        }

        protected new void Update() {
            base.Update();
            ProcessChargeFallback();
        }

        protected override void ProcessChargeOnManager(IPlayer player) {
            base.ProcessChargeOnManager(player);
            _timeOfLastCharge = Time.time;
        }

        protected override void FinishChargingOnManager() {
            OwningTeamID = CurrentCharge.teamID;
            CurrentCharge = (teamID: TeamID.Neutral, value: 0);

            base.FinishChargingOnManager();
        }

        private void ProcessChargeFallback() {
            if (!PhotonNetwork.IsMasterClient && !ManageLocally)
                return;

            if (ChargeFallbackSpeed <= 0)
                return;

            // calculate claim fallback
            if (Time.time - _timeOfLastCharge > ChargeFallbackTimeout && IsBeingCharged) {
                CurrentCharge = (CurrentCharge.teamID, CurrentCharge.value - Time.deltaTime * ChargeFallbackSpeed);

                if (CurrentCharge.value <= 0f) {
                    CurrentCharge = (teamID: TeamID.Neutral, value: 0);
                }
            }
        }
    }
}
using System.Collections;
using Photon.Pun;
using TowerTag;
using UnityEngine;
using Player = Photon.Realtime.Player;

namespace Hub {
    public class HubLaneControllerHome : HubLaneControllerBase {
        [SerializeField] private Pillar _spawnPillar;
        [SerializeField] private Pillar _readyPillar;
        [SerializeField] private GameObject _prepareText;
        [SerializeField] private GameObject _startupPart;

        [Header("Shield")] [SerializeField] protected GameObject _shieldPrefab;

        private bool _shieldActive;

        protected override bool IsOccupied => _spawnPillar.IsOccupied || _readyPillar.IsOccupied;

        public bool IsShieldActive {
            get => _shieldActive;
        }

        public override IPlayer Player {
            get {
                if (_spawnPillar.IsOccupied)
                    return _spawnPillar.Owner;
                if (_readyPillar.IsOccupied)
                    return _readyPillar.Owner;
                return null;
            }
        }

        public void SetShieldActive(bool active, IPlayer player) {
            if (active != _shieldActive && _canShieldBeActivated) {
                if (active) ActivateShield(_readyPillar, player);
                else DeactivateShield();
            }
        }

        private void ActivateShield(Pillar pillar, IPlayer player) {
            if (player != null) {
                _shield =InstantiateWrapper.InstantiateWithMessage(_shieldPrefab, pillar.transform);
                var _readyTowerShield = _shield.GetComponent<ReadyTowerShield>();
                _readyTowerShield.Activate(TeamManager.Singleton.Get(player.TeamID), player.IsMe ? ShieldType.Inside : ShieldType.Outside);
                _shieldActive = true;
            }
        }

        private void DeactivateShield() {
            if (_shield) {
                Destroy(_shield);
            }

            _shieldActive = false;
        }


        public override Pillar[] Pillars { get; protected set; }

        private GameObject _shield;
        private bool _canShieldBeActivated;

        protected override void Awake() {
            base.Awake();
            _canShieldBeActivated = true;
        }

        private void OnMissionBriefingStarted(MatchDescription obj, GameMode gameMode) {
            DeactivateShield();
            _canShieldBeActivated = false;
        }

        protected override void Setup() {
            _spawnPillar.OwningTeamID = _teamID;
            _spawnPillar.IsSpawnPillar = true;
            _spawnPillar.ShowOrientationHelp = true;
            _spawnPillar.IsClaimable = false;

            _readyPillar.OwningTeamID = _teamID;
        }

        protected override void InitPillarArray() {
            Pillars = new[] {_spawnPillar, _readyPillar};
        }

        protected override void RegisterEventListeners() {
            GameManager.Instance.MissionBriefingStarted += OnMissionBriefingStarted;

            ConnectionManager.Instance.MasterClientSwitched += OnMasterClientSwitched;
            _spawnPillar.OwnerChanged += OnOwnerChanged;
            _readyPillar.OwnerChanged += OnOwnerChanged;
        }

        private void OnMasterClientSwitched(ConnectionManager arg1, Player newMaster)
        {
            if(newMaster.IsLocal && _spawnPillar.IsOccupied && Player != null)
                OnOwnerChanged(_spawnPillar, null, Player);


        }

        private void Start() {
            if (Player != null)
                OnOwnerChanged(_spawnPillar, null, Player);
        }

        protected override void UnregisterEventListeners() {
            if (GameManager.Instance != null)
                GameManager.Instance.MissionBriefingStarted -= OnMissionBriefingStarted;

            if (ConnectionManager.Instance != null)
                ConnectionManager.Instance.MasterClientSwitched -= OnMasterClientSwitched;

            if (_spawnPillar != null)
                _spawnPillar.OwnerChanged -= OnOwnerChanged;

            if (_readyPillar != null)
                _readyPillar.OwnerChanged -= OnOwnerChanged;
        }

        private void OnOwnerChanged(Pillar pillar, IPlayer previousOwner, IPlayer newOwner) {
            _startupPart.SetActive(IsOccupied && Player.IsMe);

            if (pillar == _spawnPillar) {
                bool isNewOwnerNull = newOwner == null;
                _prepareText.SetActive(!isNewOwnerNull && newOwner.IsMe);

                if (PhotonNetwork.IsMasterClient && !isNewOwnerNull)
                    StartCoroutine(TeleportToReadyPillar());

                if (!isNewOwnerNull && newOwner.IsMe)
                    newOwner.PlayerNetworkEventHandler.RequestToggledShields();
            }
            else if (pillar == _readyPillar) {
                if (newOwner == LocalPlayer) {
                    InstantiateRTUI(newOwner, pillar);
                    LocalPlayer.PlayerIsReady = true;
                }
                else if (previousOwner != null && previousOwner.IsMe && ReadyTowerUiController != null) {
                    Destroy(ReadyTowerUiController.gameObject);
                    LocalPlayer.PlayerIsReady = false;
                }

                DeactivateShield();
                SetShieldActive(true, newOwner);
            }

            _spawnPillar.IsSpawnPillar = !IsOccupied;
        }

        private IEnumerator TeleportToReadyPillar()
        {
            float timer = 0;
            while (timer <= 4)
            {
                if (Player == null) yield break;
                if(Player.TeamID != _teamID)
                {
                    TeleportHelper.RespawnPlayerOnSpawnPillar(
                        Player, TeleportHelper.TeleportDurationType.Immediate);
                    Debug.LogWarning("Player was on the wrong hub lane!");
                    yield break;
                }

                timer += Time.deltaTime;
                yield return null;
            }
            TeleportHelper.TeleportPlayerRequestedByGame(Player, _readyPillar, TeleportHelper.TeleportDurationType.Teleport);
        }
    }
}
using Photon.Pun;
using TowerTag;
using UnityEngine;

#if UNITY_EDITOR
#endif

namespace Hub {
    public class HubLaneController : HubLaneControllerBase {
        [Header("Settings")] [SerializeField] private float _pillarDistance;
        [SerializeField] private string[] _getReadyMessages;
        [SerializeField] private string[] _isReadyMessages;
        [SerializeField] private Material _fireMaterialUI;
        [SerializeField] private Material _iceMaterialUI;

        [Header("References")] [SerializeField]
        private Light _backLight;

        [SerializeField] private Pillar _spawnPillar;
        [SerializeField] private Pillar _tagAndGoPillar;
        [SerializeField] private Pillar _readyPillar;
        [SerializeField] private FlashText _getReadyText;
        [SerializeField] private PillarClaimStateVisualization _explainer;


        protected override bool IsOccupied => SpawnPillar.IsOccupied || TagAndGoPillar.IsOccupied || ReadyPillar.IsOccupied;

        public override IPlayer Player {
            get {
                if (SpawnPillar.IsOccupied)
                    return SpawnPillar.Owner;
                if (TagAndGoPillar.IsOccupied)
                    return TagAndGoPillar.Owner;
                if (ReadyPillar.IsOccupied)
                    return ReadyPillar.Owner;
                return null;
            }
        }

        public override Pillar[] Pillars { get; protected set; }

        public Pillar SpawnPillar => _spawnPillar;
        public Pillar TagAndGoPillar => _tagAndGoPillar;
        public Pillar ReadyPillar => _readyPillar;


        protected override void Setup() {
            Transform spawnPillarTransform = SpawnPillar.transform;
            spawnPillarTransform.localPosition = Vector3.zero;
            spawnPillarTransform.localRotation = Quaternion.identity;
            spawnPillarTransform.localScale = Vector3.one;
            SpawnPillar.OwningTeamID = _teamID;
            SpawnPillar.IsSpawnPillar = true;
            SpawnPillar.ShowOrientationHelp = true;
            SpawnPillar.IsClaimable = false;

            Transform tagAndGoPillarTransform = TagAndGoPillar.transform;
            tagAndGoPillarTransform.localPosition = _pillarDistance * Vector3.forward;
            tagAndGoPillarTransform.localRotation = Quaternion.identity;
            tagAndGoPillarTransform.localScale = Vector3.one;

            Transform readyPillarTransform = ReadyPillar.transform;
            readyPillarTransform.localPosition = 2 * _pillarDistance * Vector3.forward;
            readyPillarTransform.localRotation = Quaternion.identity;
            readyPillarTransform.localScale = Vector3.one;

            _getReadyText.transform.LookAt(spawnPillarTransform);
        }

        protected override void InitPillarArray() {
            Pillars = new[] {_spawnPillar, _tagAndGoPillar, _readyPillar};
        }

        protected override void RegisterEventListeners() {
            if (SpawnPillar != null)
                SpawnPillar.OwnerChanged += OnPillarOwnerChanged;
            if (TagAndGoPillar != null)
                TagAndGoPillar.OwnerChanged += OnPillarOwnerChanged;
            if (ReadyPillar != null)
                ReadyPillar.OwnerChanged += OnPillarOwnerChanged;
            if (LocalPlayer != null)
                LocalPlayer.TeleportHandler.PlayerTeleporting += OnPlayerTeleporting;
        }

        protected override void UnregisterEventListeners() {
            if (SpawnPillar != null)
                SpawnPillar.OwnerChanged -= OnPillarOwnerChanged;
            if (TagAndGoPillar != null)
                TagAndGoPillar.OwnerChanged -= OnPillarOwnerChanged;
            if (ReadyPillar != null)
                ReadyPillar.OwnerChanged -= OnPillarOwnerChanged;
            if (LocalPlayer != null)
                LocalPlayer.TeleportHandler.PlayerTeleporting -= OnPlayerTeleporting;
        }

        private void OnPlayerTeleporting(TeleportHandler sender, Pillar origin, Pillar target, float timeToTeleport) {
            if (_backLight != null) _backLight.enabled = target == SpawnPillar || target == TagAndGoPillar || target == ReadyPillar;
        }

        private void Start() {
            RefreshBlockStatus();
            RefreshGetReadyText();
            RefreshClaimStatus();
            RefreshExplainer();
        }

        private void OnPillarOwnerChanged(Pillar pillar, IPlayer previousOwner, IPlayer newOwner) {
            RefreshBlockStatus();
            RefreshGetReadyText();
            RefreshClaimStatus();
            RefreshExplainer();
            RefreshReadyStatusOnMaster(previousOwner);
            RefreshReadyStatusOnMaster(newOwner);
            if (pillar == ReadyPillar) {
                if (newOwner != null && newOwner.IsMe)
                    InstantiateRTUI(newOwner, pillar);
                else if (previousOwner != null && previousOwner.IsMe && ReadyTowerUiController != null)
                    Destroy(ReadyTowerUiController.gameObject);
            }
        }

        private void RefreshBlockStatus() {
            SpawnPillar.IsSpawnPillar = !IsOccupied;
        }

        private void RefreshGetReadyText() {
            if (_getReadyText != null) {
                _getReadyText.SetTextVisible(ReadyPillar.IsOccupied ||
                                             TagAndGoPillar.IsOccupied && TagAndGoPillar.Owner.IsMe);
                if (ReadyPillar.IsOccupied) {
                    _getReadyText.SetTexts(_isReadyMessages);
                    _getReadyText.SetMaterial(_iceMaterialUI);
                }

                if (TagAndGoPillar.IsOccupied) {
                    _getReadyText.SetTexts(_getReadyMessages);
                    _getReadyText.SetMaterial(_fireMaterialUI);
                }
            }
        }

        private void RefreshClaimStatus() {
            SpawnPillar.PillarVisuals.SetActivatedByNeighbours(false);
            TagAndGoPillar.PillarVisuals.SetActivatedByNeighbours(SpawnPillar.IsOccupied);
            ReadyPillar.PillarVisuals.SetActivatedByNeighbours(TagAndGoPillar.IsOccupied);
            //            ReadyPillar.PillarVisuals.HoloArrow.SetActive(TagAndGoPillar.IsOccupied);

            // toggle claim collider
            SpawnPillar.AnchorTransform.gameObject.SetActive(false);
            TagAndGoPillar.AnchorTransform.gameObject.SetActive(SpawnPillar.IsOccupied);
            ReadyPillar.AnchorTransform.gameObject.SetActive(TagAndGoPillar.IsOccupied);

            // toggle claim value
            TagAndGoPillar.OwningTeamID = TagAndGoPillar.IsOccupied || ReadyPillar.IsOccupied
                ? Player.TeamID
                : TeamID.Neutral;
            ReadyPillar.OwningTeamID = ReadyPillar.IsOccupied
                ? Player.TeamID
                : TeamID.Neutral;
        }

        private void RefreshExplainer() {
            if (_explainer != null) {
                _explainer.gameObject.SetActive(SpawnPillar.IsOccupied && SpawnPillar.Owner.IsMe);
            }
        }

        private void RefreshReadyStatusOnMaster(IPlayer player) {
            if (player == null || !PhotonNetwork.IsMasterClient)
                return;
            if (player == ReadyPillar.Owner)
                player.PlayerIsReady = true;
            if (player == SpawnPillar.Owner)
                player.PlayerIsReady = false;
            if (player == TagAndGoPillar.Owner)
                player.PlayerIsReady = false;
        }
    }
}
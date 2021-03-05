using TowerTag;
using UnityEngine;

namespace Hub
{
    public abstract class HubLaneControllerBase : MonoBehaviour
    {
        [SerializeField] protected TeamID _teamID;
        [SerializeField] protected ReadyTowerUiController _readyTowerUiPrefab;

        [SerializeField, Tooltip("Those walls will fall down when the UI gets toggled")]
        private PillarWall[] _walls;

        public abstract IPlayer Player { get; }
        protected abstract bool IsOccupied { get; }

        public abstract Pillar[] Pillars { get; protected set; }
        protected ReadyTowerUiController ReadyTowerUiController;
        protected IPlayer LocalPlayer;

        protected virtual void Awake()
        {
            LocalPlayer = PlayerManager.Instance.GetOwnPlayer();
            InitPillarArray();
        }

        protected void OnEnable()
        {
            RegisterEventListeners();
        }

        protected void OnDisable()
        {
            UnregisterEventListeners();
            if (ReadyTowerUiController != null)
                Destroy(ReadyTowerUiController.gameObject);
        }

        protected void InstantiateRTUI(IPlayer player, Pillar pillar)
        {
            if (player == null || !player.IsMe || player != pillar.Owner)
                return;

            if (_readyTowerUiPrefab == null)
            {
                Debug.LogError("Oops, can't find RTUI Prefab.");
                return;
            }

            // Instance already exists
            if (ReadyTowerUiController != null)
            {
                return;
            }

            ReadyTowerUiController =InstantiateWrapper.InstantiateWithMessage(_readyTowerUiPrefab, pillar.transform);

            if (_walls != null && _walls.Length >= 1 && ReadyTowerUiController != null)
                ReadyTowerUiController.Walls = _walls;
        }

        protected abstract void Setup();

        protected abstract void InitPillarArray();

        protected abstract void RegisterEventListeners();

        protected abstract void UnregisterEventListeners();

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (gameObject.scene.buildIndex == -1)
                return;
            if (isActiveAndEnabled)
            {
                Setup();
            }
        }
#endif
    }
}
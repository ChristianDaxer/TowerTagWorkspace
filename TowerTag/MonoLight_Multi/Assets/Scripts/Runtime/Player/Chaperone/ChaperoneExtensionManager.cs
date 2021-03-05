using TowerTag;
using UnityEngine;
using UnityEngine.Serialization;

public class ChaperoneExtensionManager : MonoBehaviour {
    [FormerlySerializedAs("_activeWhenOutOfChaperones")] [SerializeField] private GameObject[] _activeWhenOutOfChaperone;
    [FormerlySerializedAs("_activeWhenInChaperones")] [SerializeField] private GameObject[] _activeWhenInChaperone;
    [SerializeField] private GameObject[] _activeWhenInTower;
    private IPlayer _player;

    private void Awake() {
        _player = GetComponentInParent<IPlayer>();
        if (_player == null) {
            Debug.LogWarning("Player not found");
            enabled = false;
        }
    }

    private void OnEnable() {
        _player.OutOfChaperoneStateChanged += OnOutOfChaperoneChanged;
        _player.InTowerStateChanged += OnInTowerStateChanged;
    }

    private void OnDisable() {
        if (_player != null) {
            _player.OutOfChaperoneStateChanged -= OnOutOfChaperoneChanged;
            _player.InTowerStateChanged -= OnInTowerStateChanged;
        }
    }

    private void OnOutOfChaperoneChanged(IPlayer player, bool active) {
        _activeWhenInChaperone.ForEach(obj => obj.SetActive(!active));
        _activeWhenOutOfChaperone.ForEach(obj => obj.SetActive(active));
    }

    private void OnInTowerStateChanged(IPlayer player, bool active) {
        _activeWhenInTower.ForEach(obj => obj.SetActive(active));
    }
}
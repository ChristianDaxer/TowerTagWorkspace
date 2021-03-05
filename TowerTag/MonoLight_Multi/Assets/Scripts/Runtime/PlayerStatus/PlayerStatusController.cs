using Hub;
using TowerTag;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStatusController : MonoBehaviour {
    [Header("Status")] [SerializeField] private Status _towerOneStatus;
    [SerializeField] private Status _towerTwoStatus;
    [SerializeField] private Status _readyStatus;
    [SerializeField] private Status _inTowerStatus;
    [SerializeField] private Status _outOfChaperoneStatus;
    [SerializeField] private Status _inactiveStatus;

    private IPlayer _player;
    private Status _pillarStatus;

    private void OnEnable() {
        _player = GetComponent<IPlayer>();
        if (_player.IsMe) {
            _pillarStatus = _towerOneStatus;
            SetStatusInTower(this, _player.IsInTower);
            SetStatusOutOfChaperone(this, _player.IsOutOfChaperone);
            _player.TeleportHandler.PlayerTeleporting += OnPlayerTeleporting;
            _player.InTowerStateChanged += SetStatusInTower;
            _player.OutOfChaperoneStateChanged += SetStatusOutOfChaperone;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private void OnDisable() {
        if (_player.IsMe) {
            _player.TeleportHandler.PlayerTeleporting -= OnPlayerTeleporting;
            _player.InTowerStateChanged -= SetStatusInTower;
            _player.OutOfChaperoneStateChanged -= SetStatusOutOfChaperone;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnPlayerTeleporting(TeleportHandler sender, Pillar origin, Pillar newPillar, float timeToTeleport) {
        if (GameManager.Instance.IsInConfigureState) {
            var hubLane = newPillar.GetComponentInParent<HubLaneController>();
            if (hubLane == null) return;
            if (newPillar == hubLane.SpawnPillar)
                _pillarStatus = _towerOneStatus;
            else if (newPillar == hubLane.TagAndGoPillar)
                _pillarStatus = _towerTwoStatus;
            else if (newPillar == hubLane.ReadyPillar)
                _pillarStatus = _readyStatus;
        }
        else {
            _pillarStatus = _readyStatus;
        }

        if (!_player.IsOutOfChaperone && !_player.IsInTower)
            _player.SetStatus(_pillarStatus);
    }

    private void SetStatusOutOfChaperone(object sender, bool value) {
        _player.SetStatus(value ? _outOfChaperoneStatus : _pillarStatus);
    }

    private void SetStatusInTower(object sender, bool value) {
        _player.SetStatus(value ? _inTowerStatus : _pillarStatus);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1) {
        if (TTSceneManager.Instance.IsInHubScene) {
            _pillarStatus = _towerOneStatus;
        }
        else {
            _pillarStatus = _readyStatus;
            if (!_player.IsOutOfChaperone && !_player.IsInTower)
                _player.SetStatus(_pillarStatus);
        }
    }

    public Status GetStatusByStatusText(string statusText) {
        if (statusText == _towerOneStatus.StatusText)
            return _towerOneStatus;
        if (statusText == _towerTwoStatus.StatusText)
            return _towerTwoStatus;
        if (statusText == _inTowerStatus.StatusText)
            return _inTowerStatus;
        if (statusText == _outOfChaperoneStatus.StatusText)
            return _outOfChaperoneStatus;
        if (statusText == _inactiveStatus.StatusText)
            return _inactiveStatus;
        if (statusText == _readyStatus.StatusText)
            return _readyStatus;

        return null;
    }
}
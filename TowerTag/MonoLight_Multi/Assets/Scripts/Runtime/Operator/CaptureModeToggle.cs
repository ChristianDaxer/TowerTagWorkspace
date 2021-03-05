using TowerTag;
using TowerTagSOES;
using UnityEngine;

public class CaptureModeToggle : MonoBehaviour {

    [SerializeField] private GameObject _operatorCanvas;
    [SerializeField] private GameObject _spectatorCanvas;
    [SerializeField] private GameObject _gameVersionOverlayPrefab;
    private GameObject _gameVersionOverlay;
    private bool _value = true;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F10)) {
            ToggleCaptureMode();
        }
    }

    private void ToggleCaptureMode() {
        Debug.LogWarning("CaptureMode currently deactivated");
        return;
        if (SharedControllerType.IsAdmin) {
            PlayerManager.Instance.GetAllConnectedPlayers(out var players, out var count);
            for (int i = 0; i < count; i++)
            {
                if (players[i].GameObject == null) continue;
                var visuals = players[i].GameObject.GetComponentInChildren<AvatarVisuals>();
                players[i].GameObject.GetComponentInChildren<NameBadge>().Badge.enabled = _value;
                visuals.ToggleSeeThroughMaterial(_value);
                visuals.SetTeamColor(players[i].TeamID);
            }

            _value = !_value;
            if(_gameVersionOverlay == null)
                _gameVersionOverlay = GameObject.Find(_gameVersionOverlayPrefab.name);
            if(_gameVersionOverlay != null)
                _gameVersionOverlay.SetActive(_value);

            _operatorCanvas.SetActive(_value);
            _spectatorCanvas.SetActive(_value);
        }
    }
}

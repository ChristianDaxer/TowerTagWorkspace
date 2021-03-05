using System.Collections.Generic;
using TowerTag;
using UnityEngine;

public class PillarTurningPlateSlotObject : MonoBehaviour {
    [SerializeField] private PillarTurningPlateController.TurningSlot _turningSlot;

    public PillarTurningPlateController.TurningSlot TurningSlot => _turningSlot;

    [SerializeField] private Renderer _arrowRenderer;
    [SerializeField] private Renderer _sphereRenderer;
    private bool _missingReferences;
    private readonly List<Renderer> _slotRenderers = new List<Renderer>();
    private IPlayer _player;
    private int _tintColorPropertyId;

    private void Awake() {
        _tintColorPropertyId = Shader.PropertyToID("_Color");
    }

    private void OnDisable() {
        if (_player != null) _player.PlayerTeamChanged -= OnTeamChanged;
    }

    private void OnTeamChanged(IPlayer player, TeamID teamID) {
        SetColor(TeamManager.Singleton.Get(teamID).Colors.Effect);
    }

    public void Init(IPlayer player) {
        if (_arrowRenderer == null) {
            Debug.LogWarning("Can't find arrow Renderer.");
            _missingReferences = true;
        }

        if (_sphereRenderer == null) {
            Debug.LogWarning("Can't find sphere Renderer.");
            _missingReferences = true;
        }

        _player = player;

        if (_missingReferences)
            return;

        player.PlayerTeamChanged += OnTeamChanged;

        _slotRenderers.Add(_arrowRenderer);
        _slotRenderers.Add(_sphereRenderer);

        //Debug.LogError("Set Color to: " + TeamManager.Singleton.Get(player.TeamID));
        SetColor(TeamManager.Singleton.Get(player.TeamID).Colors.Effect);
    }

    private void SetColor(Color color) {
        if (_missingReferences) {
            return;
        }

        ColorChanger.ChangeColorInRendererComponents(_slotRenderers.ToArray(), color, _tintColorPropertyId, true);
    }
}
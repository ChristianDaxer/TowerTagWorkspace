using TowerTag;
using UnityEngine;

public class LightBars : FloatVisuals {
    [Header("Grappling Hook Components")] [SerializeField]
    private Renderer[] _particleRenderer;

    [SerializeField] private Renderer _innerGlow;

    [Header("Gun Light Bars")] [SerializeField]
    private Renderer[] _energyRenderer;

    [SerializeField] private string _blendPropName = "_ClaimValue";
    private int _blendPropValueID;

    [SerializeField] private string _lightBarColorPropName = "_ClaimColor";
    private int _lightBarColoredPropValueID;

    [Header("Emissive Spots")] [SerializeField]
    private Material _emissiveGunMat;

    [SerializeField] private Renderer[] _gunRenderer;


    private IPlayer _player;

    private static readonly int _emissionColor = Shader.PropertyToID("_EmissionColor");

    public void Init(IPlayer player) {
        _player = player;
    }

    private void Start() {
        _blendPropValueID = Shader.PropertyToID(_blendPropName);
        _lightBarColoredPropValueID = Shader.PropertyToID(_lightBarColorPropName);

        if (_player != null) {
            OnTeamChanged(_player, _player.TeamID);
        }
    }

    public void OnTeamChanged(IPlayer player, TeamID teamID) {
        SetColor(TeamManager.Singleton.Get(teamID).Colors.Effect);
    }

    private void SetColor(Color color) {
        ColorChanger.ChangeColorInRendererComponentsWithMultipleMaterials(_gunRenderer, _emissiveGunMat, color,
            _emissionColor, true);
        ColorChanger.ChangeColorInRendererComponents(_energyRenderer, color, _lightBarColoredPropValueID, true);
        ColorChanger.ChangeColorInRendererComponents(_particleRenderer, color, true);
        _innerGlow.material.SetColor(_emissionColor, color);
    }

    public override void SetValue(float newValue) {
        if (_energyRenderer != null) {
            ColorChanger.SetCustomFloatPropertyInRenderers(_energyRenderer, _blendPropValueID, newValue);
        }
    }
}
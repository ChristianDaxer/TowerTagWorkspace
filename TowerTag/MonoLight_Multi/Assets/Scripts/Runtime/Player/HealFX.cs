using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class HealFX : MonoBehaviour {
    [SerializeField] private GameObject _particleParent;
    [SerializeField] private ChargePlayer _chargePlayer;
    [SerializeField] private Renderer _shield;
    [SerializeField] private ParticleSystem _crosses;

    [SerializeField, Tooltip("The fill image of the health")]
    private Image _healthFillImage;

    [SerializeField, Tooltip("All particle effects that should be colorized --> All besides ExplosionCore and Crosses")]
    private ParticleSystem[] _particlesToColorize;

    [SerializeField] private RopeGameAction ropeGameAction;


    private PlayerHealth _ownerPlayerHealth;
    private static readonly int RampColorTint = Shader.PropertyToID("_RampColorTint");

    private void Awake() {
        _ownerPlayerHealth = GetComponentInParent<IPlayer>().PlayerHealth;
        ColorizeComponentsByTeamID(_chargePlayer.Owner.TeamID);
    }

    private void OnEnable() {
        ropeGameAction.RopeConnectedToChargeable += OnRopeConnectedToChargeable;
        ropeGameAction.Disconnecting += OnRopeDisconnecting;
        _ownerPlayerHealth.HealthChanged += OnHealthChanged;
        _healthFillImage.fillAmount = _ownerPlayerHealth.HealthFraction;
        _chargePlayer.Owner.PlayerTeamChanged += OnPlayerTeamChanged;
    }

    private void OnDisable() {
        ropeGameAction.RopeConnectedToChargeable -= OnRopeConnectedToChargeable;
        ropeGameAction.Disconnecting -= OnRopeDisconnecting;
        _ownerPlayerHealth.HealthChanged -= OnHealthChanged;
        _chargePlayer.Owner.PlayerTeamChanged -= OnPlayerTeamChanged;
    }

    private void OnPlayerTeamChanged(IPlayer player, TeamID newTeam) {
        ColorizeComponentsByTeamID(newTeam);
    }

    private void ColorizeComponentsByTeamID(TeamID teamId) {
        TeamColors teamColors = TeamManager.Singleton.Get(teamId).Colors;
        _particlesToColorize.ForEach(particle => {
            ParticleSystem.MainModule particleMain = particle.main;
            particleMain.startColor = teamColors.WallCracks;
        });
        _shield.material.SetColor(RampColorTint, teamColors.Main);
        _crosses.GetComponent<Renderer>().material.SetColor(RampColorTint, teamColors.Main);
    }

    private void OnRopeDisconnecting(RopeGameAction sender, IPlayer player, Chargeable target, bool onPurpose) {
        if (target == _chargePlayer) _particleParent.SetActive(false);
    }

    private void OnRopeConnectedToChargeable(RopeGameAction sender, IPlayer player, Chargeable target) {
        if (target != _chargePlayer) return;
        _healthFillImage.fillAmount = _ownerPlayerHealth.HealthFraction;
        _particleParent.SetActive(true);
    }

    private void OnHealthChanged(PlayerHealth playerHealth, int newHType, IPlayer other, byte colliderType) {
        _healthFillImage.fillAmount = playerHealth.HealthFraction;
    }
}
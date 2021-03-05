using TowerTag;
using TowerTagSOES;
using UnityEngine;
using UnityEngine.UI;

public class PillarIconVisuals : MonoBehaviour {
    [SerializeField, Tooltip("The PillarScript of the Parent")]
    private Pillar _owningPillar;

    [SerializeField, Tooltip("The border for the GoalPillar icon")]
    private Image _miniMapGoalPillarIcon;

    private IMatch _currentMatch;
    private Image _miniMapIcon;
    private Color _startColor;

    /// <summary>
    /// Team values
    /// </summary>
    private TeamID? _currentClaimingTeamID;

    private ITeam _currentClaimingTeam;

    private void Awake() {
        //No need for this object if ControllerType isn't admin nor spectator
        if (!SharedControllerType.IsAdmin && !SharedControllerType.Spectator) {
            Destroy(gameObject);
        }
        else {
            _miniMapIcon = GetComponent<Image>();

            //Spawn and spectator pillars are always in color
            if (_owningPillar.IsSpawnPillar || _owningPillar.IsSpectatorPillar) {
                _miniMapIcon.color = TeamManager.Singleton.Get(_owningPillar.OwningTeamID).Colors.UI;
            }

            if (_owningPillar.IsGoalPillar)
                _miniMapGoalPillarIcon.gameObject.SetActive(true);

            _startColor = _miniMapIcon.color;
        }
    }

    private void OnEnable() {
        _owningPillar.ChargeSet += UpdateImageColor;
        _owningPillar.OwningTeamChanged += ChangeTeamColor;
        UpdateImageColor(_owningPillar, _owningPillar.OwningTeamID, _owningPillar.CurrentCharge.value);
        ChangeTeamColor(_owningPillar, TeamID.Neutral, _owningPillar.OwningTeamID, new IPlayer[0]);
    }

    private void OnDisable() {
        _owningPillar.ChargeSet -= UpdateImageColor;
        _owningPillar.OwningTeamChanged -= ChangeTeamColor;
    }

    private void ChangeTeamColor(Claimable claimable, TeamID oldTeamID, TeamID newTeamID, IPlayer[] newOwner) {
        _startColor = TeamManager.Singleton.Get(newTeamID).Colors.UI;
        _miniMapIcon.color = _startColor;
        if (_owningPillar.IsGoalPillar)
            _miniMapGoalPillarIcon.color = _startColor;
    }

    /// <summary>
    /// Lerp colors while claiming value changes
    /// </summary>
    private void UpdateImageColor(Chargeable chargeable, TeamID teamId, float value) {
        if (_currentClaimingTeamID != teamId && teamId != TeamID.Neutral) {
            _currentClaimingTeam = TeamManager.Singleton.Get(teamId);
            _currentClaimingTeamID = teamId;
        }

        if (_currentClaimingTeam != null) {
            Color newColor = Color.Lerp(_startColor, _currentClaimingTeam.Colors.UI, value);
            _miniMapIcon.color = newColor;
            if (_owningPillar.IsGoalPillar)
                _miniMapGoalPillarIcon.color = newColor;
        }
    }
}
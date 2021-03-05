using TowerTag;
using UnityEngine;

public abstract class PillarVisuals : MonoBehaviour {
    [SerializeField] private Pillar _pillar;
    [SerializeField] private Highlighter _highlightTrigger;

    public Pillar Pillar => _pillar;

    private void OnEnable() {
        _pillar.ClaimableStatusChanged += OnClaimableStatusChanged;
        _pillar.ChargeSet += SetCurrentClaim;
        _pillar.PlayerDetached += OnPlayerDetached;
        _pillar.OwningTeamChanged += SetOwningTeam;
        _pillar.OwnerChanged += OnOccupancyChanged;
        _highlightTrigger.Toggled += ShowHighlight;
    }

    private void OnDisable() {
        _pillar.ClaimableStatusChanged -= OnClaimableStatusChanged;
        _pillar.ChargeSet -= SetCurrentClaim;
        _pillar.PlayerDetached -= OnPlayerDetached;
        _pillar.OwningTeamChanged -= SetOwningTeam;
        _pillar.OwnerChanged -= OnOccupancyChanged;
        _highlightTrigger.Toggled -= ShowHighlight;
    }

    protected abstract void OnClaimableStatusChanged(Claimable claimable, bool active);
    protected abstract void OnPlayerDetached(Chargeable chargeable, IPlayer player);
    protected abstract void ShowHighlight(bool isHighlighted);
    protected abstract void SetOwningTeam(Claimable claimable, TeamID oldTeam, TeamID newTeam, IPlayer[] attachedPlayers);
    protected abstract void SetCurrentClaim(Chargeable chargeable, TeamID teamID, float value);
    protected abstract void OnOccupancyChanged(Pillar pillar, IPlayer previousOwner, IPlayer newOwner);
}
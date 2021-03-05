using System;
using Rope;
using TowerTag;
using UnityEngine;
using UnityEngine.UI;

public class PillarClaimStateVisualization : MonoBehaviour {
    private enum PillarClaimStateAnimationStates {
        NotDefined = -1,
        Aim = 0,
        Shoot = 1,
        Hold = 2,
        Pull = 3
    }

    [SerializeField] private TriggerExplainerAnimation _triggerExplainerAniScript;
    [SerializeField] private PillarVisualsExtended _pillarVis;

    [Header("Explainer Help Texts")] [SerializeField]
    private Text _helpTextUI;

    [SerializeField] private string[] _explainerAniTexts;
    [SerializeField] private string _claimText = " % claimed already...";
    [SerializeField] private float _pullFasterTensionThreshold = .1f;
    [SerializeField] private string _pullFasterText = "You have to pull faster!";
    [SerializeField] private Text _claimTextUI;

    private PillarClaimStateAnimationStates _currentAnimationIndex = PillarClaimStateAnimationStates.NotDefined;
    private IPlayer _player;
    private ChargerRopeRenderer _chargerBeam;

    private void OnEnable() {
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
    }

    private void OnDisable() {
        PlayerManager.Instance.PlayerRemoved -= OnPlayerRemoved;
    }

    private void Start() {
        _player = PlayerManager.Instance.GetOwnPlayer();
        if (_player == null) {
            enabled = false;
            return;
        }

		_chargerBeam = gameObject.GetComponentInChildren<ChargerRopeRenderer>(true);

        // Start with aim animation
        SwitchToAnimation(PillarClaimStateAnimationStates.Aim);
    }

    private void OnPlayerRemoved(IPlayer player) {
        if (player.IsMe) enabled = false;
    }

    private void Update() {
        try {
            OnUpdate();
        }
        catch (Exception e) {
            Debug.LogWarning($"Disabling PillarClaimStateVisualization, because an error occured: {e}");
            enabled = false;
        }
    }

    private void OnUpdate() {
        // player does not aim -> aim animation
        bool isAiming = _pillarVis.IsHighlighted || _pillarVis.IsClaiming;
        if (!isAiming) {
            SwitchToAnimation(PillarClaimStateAnimationStates.Aim);
        }
        // the player highlights but hasn't claimed -> connect & claim animation
        else if (_pillarVis.IsHighlighted && !_chargerBeam.IsConnected) {
            SwitchToAnimation(PillarClaimStateAnimationStates.Shoot);
        }
        // the player is claiming the pillar
        else if (_pillarVis.IsClaiming && _pillarVis.Pillar.OwningTeamID != _player.TeamID) {
            SwitchToAnimation(PillarClaimStateAnimationStates.Hold);
        }
        // the player highlights but has finished claiming -> teleport animation
        else if (_pillarVis.Pillar.OwningTeamID == _player.TeamID && _chargerBeam.IsConnected) {
            SwitchToAnimation(PillarClaimStateAnimationStates.Pull);
        }

        // show ClaimText
        if (_currentAnimationIndex == PillarClaimStateAnimationStates.Hold) {
            _claimTextUI.text = (int) (_pillarVis.Pillar.CurrentCharge.value * 100f) + _claimText;
        }

        // show pull faster Text if player does not trigger Teleport
        if (_currentAnimationIndex == PillarClaimStateAnimationStates.Pull) {
            if (_chargerBeam.Tension > _pullFasterTensionThreshold && _chargerBeam.Tension < _chargerBeam.UpperVelo) {
                _claimTextUI.gameObject.SetActive(true);
                _claimTextUI.text = _pullFasterText;
            }
            else {
                _claimTextUI.gameObject.SetActive(false);
            }
        }
    }

    // trigger new Animation if we are not already playing it
    private void SwitchToAnimation(PillarClaimStateAnimationStates animationIndex) {
        if (animationIndex != _currentAnimationIndex) {
            _helpTextUI.text = _explainerAniTexts[(int) animationIndex];

            _claimTextUI.gameObject.SetActive(animationIndex == PillarClaimStateAnimationStates.Hold);
            _claimTextUI.text = (int) (_pillarVis.Pillar.CurrentCharge.value * 100f) + _claimText;

            _currentAnimationIndex = animationIndex;
            _triggerExplainerAniScript.SwitchToAnimation((int) _currentAnimationIndex);
        }
    }
}
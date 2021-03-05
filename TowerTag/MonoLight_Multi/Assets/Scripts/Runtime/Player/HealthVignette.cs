using System.Collections;
using TowerTag;
using UnityEngine;

public sealed class HealthVignette : VignetteEffectController {
    [SerializeField] private ChargePlayer _chargePlayer;

    [Header("FX")] [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _healingSound;

    private bool PlayerAttached => _owner?.ChargePlayer.AttachedPlayers.Count > 0;

    private new void Awake() {
        base.Awake();

        if (_audioSource != null)
            _audioSource.clip = _healingSound;
    }
    private void OnEnable() {
        if (_chargePlayer != null) {
            _chargePlayer.PlayerAttached += OnPlayerAttached;
            _chargePlayer.PlayerDetached += OnPlayerDetached;
        }
    }

    private void OnDisable() {
        if (_chargePlayer != null) {
            _chargePlayer.PlayerAttached -= OnPlayerAttached;
            _chargePlayer.PlayerDetached -= OnPlayerDetached;
        }
    }

    private void OnPlayerAttached(Chargeable chargeable, IPlayer player) {
        if (player?.GameObject == null || chargeable.gameObject == null) {
            return;
        }

        if(!_audioSource.isPlaying)
            _audioSource.Play();
        ResetEffectTime(_frontHitVisuals);
        StartCoroutine(DampenEffect());
        _afterEffect.Material = _vignetteMaterial;
    }

    private void OnPlayerDetached(Chargeable chargeable, IPlayer player) {
        if (chargeable.AttachedPlayers.Count == 0) {
            _audioSource.Stop();
            StopAllCoroutines();
            _afterEffect.enabled = false;
        }
    }

    private IEnumerator DampenEffect() {
        while (PlayerAttached) {
            if (!_afterEffect.enabled)
                _afterEffect.enabled = true;
            CurrentEffectTimeLeft += Time.deltaTime / _hitRadarLifeTime;
            CurrentEffectTimeRight += Time.deltaTime / _hitRadarLifeTime;

            _vignetteMaterial.SetFloat(LeftID,
                CurrentEffectTimeLeft >= 1 ? 0 : _strengthOverTime.Evaluate(CurrentEffectTimeLeft));
            _vignetteMaterial.SetFloat(RightID,
                CurrentEffectTimeRight >= 1 ? 0 : _strengthOverTime.Evaluate(CurrentEffectTimeRight));

            if (CurrentEffectTimeLeft >= 1)
                ResetEffectTime(_frontHitVisuals);
            yield return null;
        }
    }

    public void InitHealthVignette(ChargePlayer charge) {
        IPlayer player = PlayerManager.Instance?.GetOwnPlayer();

        if (player != null && player == charge.Owner) {
            _chargePlayer = charge;
            _chargePlayer.PlayerAttached += OnPlayerAttached;
            _chargePlayer.PlayerDetached += OnPlayerDetached;
            _owner = player;
        }
    }
}
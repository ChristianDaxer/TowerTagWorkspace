using System.Collections;
using TowerTag;
using UnityEngine;

/// <summary>
///
/// <author>Ole Jürgensen</author>
/// <date></date>
/// </summary>
public sealed class HitRadarController : VignetteEffectController {
    [SerializeField] private HitGameAction _hitGameAction;

    private void OnLocalPlayerHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint,
        DamageDetectorBase.ColliderType targetType) {
        if (shotData.Player?.GameObject == null || targetPlayer?.GameObject == null || !targetPlayer.IsMe) {
            return;
        }

        ActivateAfterEffect();
        Vector3 direction = Vector3.ProjectOnPlane(
                shotData.Player.GameObject.transform.position - targetPlayer.GameObject.transform.position, Vector3.up)
            .normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        float fromRight = Vector3.Dot(direction, right); // +1: from right, -1: from left
        float fromFront = Vector3.Dot(direction, forward); // +1: from front, -1: from behind
        if (fromRight > .5f) ResetEffectTime(Direction.Right);
        if (fromRight < -.5f) ResetEffectTime(Direction.Left);
        if (fromFront > .5f) ResetEffectTime(_frontHitVisuals);
        if (fromFront < -.5f) ResetEffectTime(Direction.Bottom);
        StartCoroutine(DampenEffect());
    }

    public void TriggerFrontalVisuals() {
        ActivateAfterEffect();
        ResetEffectTime(_frontHitVisuals);
        StartCoroutine(DampenEffect());
    }

    private void ActivateAfterEffect() {
        _afterEffect.enabled = true;
        _afterEffect.Material = _vignetteMaterial;
    }

    private void OnEnable() {
        _hitGameAction.PlayerGotHit += OnLocalPlayerHit;
    }


    private void OnDisable() {
        _hitGameAction.PlayerGotHit -= OnLocalPlayerHit;
    }


    private IEnumerator DampenEffect() {
        while (enabled) {
            CurrentEffectTimeLeft += Time.deltaTime / _hitRadarLifeTime;
            CurrentEffectTimeRight += Time.deltaTime / _hitRadarLifeTime;
            CurrentEffectTimeTop += Time.deltaTime / _hitRadarLifeTime;
            CurrentEffectTimeBottom += Time.deltaTime / _hitRadarLifeTime;

            _vignetteMaterial.SetFloat(LeftID,
                CurrentEffectTimeLeft >= 1 ? 0 : _strengthOverTime.Evaluate(CurrentEffectTimeLeft));
            _vignetteMaterial.SetFloat(RightID,
                CurrentEffectTimeRight >= 1 ? 0 : _strengthOverTime.Evaluate(CurrentEffectTimeRight));
            _vignetteMaterial.SetFloat(TopID,
                CurrentEffectTimeTop >= 1 ? 0 : _strengthOverTime.Evaluate(CurrentEffectTimeTop));
            _vignetteMaterial.SetFloat(BottomID,
                CurrentEffectTimeBottom >= 1 ? 0 : _strengthOverTime.Evaluate(CurrentEffectTimeBottom));

            if (CurrentEffectTimeLeft > 1 && CurrentEffectTimeRight > 1
                                          && CurrentEffectTimeTop > 1
                                          && CurrentEffectTimeBottom > 1) {
                _afterEffect.enabled = false;
                yield break;
            }

            yield return null;
        }
    }
}
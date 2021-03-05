using Bhaptics.Tact.Unity;
using TowerTag;
using UnityEngine;

public class TactReceiverHGABased : MonoBehaviour {
    [SerializeField] private HitGameAction _hitGameAction;
    [SerializeField] private PositionTag _positionTag = PositionTag.Body;
    [SerializeField] private bool _isActive = true;
    [SerializeField] private float _tactReceiverHeight;

    private void OnEnable() {
        _isActive = ConfigurationManager.Configuration.EnableHapticHitFeedback;
        _hitGameAction.PlayerGotHit += TriggerHapticFeedback;
    }

    private void OnDisable() {
        _hitGameAction.PlayerGotHit -= TriggerHapticFeedback;
    }

    private void TriggerHapticFeedback(
        ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType targetType) {
        if (_isActive && targetPlayer.IsMe) {
            //Only way to handle! if we dont compare position Tag with collider type all the feedback will play at every hit!
            if (targetType == DamageDetectorBase.ColliderType.Body && _positionTag == PositionTag.Body)
                Handle(hitPoint, shotData.TactSender);
            else if (targetType == DamageDetectorBase.ColliderType.Head && _positionTag == PositionTag.Head)
                Handle(hitPoint, shotData.TactSender);
        }
    }

    private void Handle(Vector3 contactPoint, TactSender tactSender) {
        if (tactSender != null) {
            Vector3 targetPosition = transform.position;
            var angle = 0f;
            var offsetY = 0f;

            if (_positionTag == PositionTag.Body)
            {
                Vector3 targetDir = contactPoint - targetPosition;
                angle = BhapticsUtils.Angle(targetDir, transform.forward);
                offsetY = (contactPoint.y - targetPosition.y) / _tactReceiverHeight;
            }

            tactSender.Play(_positionTag, angle, offsetY);
        }
    }
}
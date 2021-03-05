using TowerTag;
using UnityEngine;

public class AvatarHit : MonoBehaviour {
    public Animator HitAnimator { get; set; }
    public IPlayer Owner { get; set; }
    [SerializeField] private HitGameAction _hga;
    private static readonly int _headFrontHit = Animator.StringToHash("HeadFrontHit");
    private static readonly int _headBackHit = Animator.StringToHash("HeadBackHit");

    private void OnEnable() {
        _hga.PlayerGotHit += OnPlayerGotHit;
    }
    private void OnDisable() {
        _hga.PlayerGotHit -= OnPlayerGotHit;
    }

    private void OnPlayerGotHit(ShotData shotData, IPlayer targetPlayer, Vector3 hitPoint, DamageDetectorBase.ColliderType targetType) {
        if (Owner.IsMe || targetPlayer != Owner)
            return;
        Vector3 direction = targetPlayer.PlayerAvatar.AvatarMovement.HeadSourceTransform.position - hitPoint;
        Vector3 shotDirection = Vector3.ProjectOnPlane(shotData.SpawnPosition - hitPoint, Vector3.up);
        float angle = Vector3.Angle(shotDirection, Vector3.ProjectOnPlane(targetPlayer.PlayerAvatar.AvatarMovement.HeadSourceTransform.forward, Vector3.up));
        //Front hit
        if (targetType == DamageDetectorBase.ColliderType.Head) {
            HitAnimator.SetTrigger(angle <= 90 ? _headFrontHit : _headBackHit);
            return;
        }

        if (angle <= 90) {
            HitAnimator.SetTrigger(direction.x <= 0 ? "LeftFrontHit" : "RightFrontHit");
        } else {
            HitAnimator.SetTrigger(direction.x <= 0 ? "RightBackHit" : "LeftBackHit");
        }
    }
}
